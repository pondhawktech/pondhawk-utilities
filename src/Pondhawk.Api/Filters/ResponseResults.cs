// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Pondhawk.Api.Context;
using Pondhawk.Api.Exceptions;
using Pondhawk.Exceptions;
using Pondhawk.Mediator;

namespace Pondhawk.Api.Filters;

/// <summary>
/// Extension helpers that render a mediator <see cref="Response{T}"/> to an <see cref="IResult"/>. On
/// success the payload is wrapped in the standard <see cref="ApiEnvelope"/> (<c>{ data, meta }</c>); a
/// paged result (<see cref="IPagedResult"/>) has its items placed in <c>data</c> and its paging metadata
/// lifted into <c>meta.pagination</c>. On failure the response renders as a <see cref="ProblemDetail"/>
/// whose status is mapped through <see cref="ErrorStatusMap"/> — the one place error mapping lives, so a
/// slice <c>Dispatcher</c> can return <see cref="IResult"/> directly.
/// </summary>
public static class ResponseResults
{
    /// <summary>
    /// Renders success as <c>200 OK</c> with the enveloped payload (or a raw stream), and failure as a
    /// mapped <see cref="ProblemDetail"/>.
    /// </summary>
    /// <typeparam name="T">The response value type.</typeparam>
    /// <param name="response">The envelope to render.</param>
    /// <param name="http">The current HTTP context (for JSON options and correlation).</param>
    public static IResult ToOk<T>(this Response<T> response, HttpContext http)
    {
        if (!response.Ok)
        {
            var problem = response.ToProblem(http);
            return problem;
        }

        if (response.Value is Stream stream)
        {
            var streamResult = Results.Stream(stream);
            return streamResult;
        }

        var envelope = BuildEnvelope(response.Value, http);
        var options = JsonOptions(http);
        var result = Results.Json(envelope, options);
        return result;
    }

    /// <summary>
    /// Renders success as <c>201 Created</c> with a <c>Location</c> header and the enveloped payload, and
    /// failure as a mapped <see cref="ProblemDetail"/>.
    /// </summary>
    /// <typeparam name="T">The response value type.</typeparam>
    /// <param name="response">The envelope to render.</param>
    /// <param name="http">The current HTTP context.</param>
    /// <param name="location">Builds the <c>Location</c> header value from the created value.</param>
    public static IResult ToCreated<T>(this Response<T> response, HttpContext http, Func<T, string> location)
    {
        ArgumentNullException.ThrowIfNull(location);

        if (!response.Ok)
        {
            var problem = response.ToProblem(http);
            return problem;
        }

        var value = response.AsEntity;
        var uri = location(value);
        var envelope = BuildEnvelope(value, http);
        var result = Results.Created(uri, envelope);
        return result;
    }

    /// <summary>
    /// Renders success as <c>204 No Content</c> and failure as a mapped <see cref="ProblemDetail"/>.
    /// Suited to mutations that return a <see cref="Receipt"/>.
    /// </summary>
    /// <typeparam name="T">The response value type.</typeparam>
    /// <param name="response">The envelope to render.</param>
    /// <param name="http">The current HTTP context.</param>
    public static IResult ToNoContent<T>(this Response<T> response, HttpContext http)
    {
        if (!response.Ok)
        {
            var problem = response.ToProblem(http);
            return problem;
        }

        var result = Results.NoContent();
        return result;
    }

    /// <summary>
    /// Renders a failed response as a <see cref="ProblemDetail"/> (<c>application/problem+json</c>) whose
    /// status is mapped from the error's <see cref="ErrorKind"/> via <see cref="ErrorStatusMap"/>.
    /// </summary>
    /// <param name="response">The failed (or, defensively, any) response.</param>
    /// <param name="http">The current HTTP context.</param>
    public static IResult ToProblem(this IResponse response, HttpContext http)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentNullException.ThrowIfNull(http);

        var error = response.Error!;
        var status = ErrorStatusMap.ToStatus(error.Kind);
        var correlationId = Correlation(http);

        var problem = new ProblemDetail
        {
            Type = error.Kind.ToString(),
            Title = error.ErrorCode,
            Detail = error.Explanation,
            StatusCode = status,
            Instance = http.Request.Path.Value ?? string.Empty,
            CorrelationId = correlationId ?? string.Empty,
            Segments = [.. error.Details],
        };

        var options = JsonOptions(http);
        var result = Results.Json(problem, options, "application/problem+json", status);
        return result;
    }

    // Wraps the payload in the standard envelope. A paged result has its items placed in `data` and its
    // paging metadata lifted into `meta.pagination`; anything else goes into `data` as-is.
    private static ApiEnvelope BuildEnvelope(object? value, HttpContext http)
    {
        var correlationId = Correlation(http);

        if (value is IPagedResult paged)
        {
            var pagination = new PaginationMeta(paged.Page, paged.PageSize, paged.TotalCount, paged.TotalPages);
            var pagedMeta = new ResponseMeta(correlationId, pagination);
            var pagedEnvelope = new ApiEnvelope(paged.Items, pagedMeta);
            return pagedEnvelope;
        }

        var meta = new ResponseMeta(correlationId, null);
        var envelope = new ApiEnvelope(value, meta);
        return envelope;
    }

    private static string? Correlation(HttpContext http)
    {
        var context = http.RequestServices.GetService<IRequestContext>();
        var correlationId = context?.CorrelationId;
        return correlationId;
    }

    private static JsonSerializerOptions? JsonOptions(HttpContext http)
    {
        var options = http.RequestServices.GetService<JsonSerializerOptions>();
        return options;
    }
}
