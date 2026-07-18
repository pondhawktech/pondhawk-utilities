// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pondhawk.Api.Filters;
using Pondhawk.Mediator;

namespace Pondhawk.Api.Endpoints;

/// <summary>
/// Base class for a slice endpoint's nested <c>Dispatcher</c>. It holds the invariant plumbing —
/// the <see cref="IMediator"/>, the <see cref="HttpContext"/>, and the request cancellation token —
/// so a slice's dispatcher declares only its bound inputs and a one-line <c>Send()</c>. Minimal APIs
/// bind it via <c>[AsParameters]</c>, which populates these inherited <c>required</c> properties
/// alongside the derived type's own bound properties.
/// </summary>
public abstract class EndpointDispatcher
{
    /// <summary>Gets the mediator used to dispatch the slice's request. Resolved from DI.</summary>
    [FromServices]
    public required IMediator Mediator { get; init; }

    /// <summary>Gets the current HTTP context, bound by the framework.</summary>
    public required HttpContext Http { get; init; }

    /// <summary>Gets the token that is cancelled when the request is aborted.</summary>
    protected CancellationToken Cancel => Http.RequestAborted;

    /// <summary>Dispatches <paramref name="request"/> and renders success as 200, failure as ProblemDetails.</summary>
    /// <typeparam name="T">The response value type.</typeparam>
    /// <param name="request">The request to dispatch.</param>
    protected async Task<IResult> Ok<T>(IRequest<T> request)
    {
        var response = await Mediator.SendAsync(request, Cancel).ConfigureAwait(false);
        var result = response.ToOk(Http);
        return result;
    }

    /// <summary>Dispatches <paramref name="request"/> and renders success as 201 Created, failure as ProblemDetails.</summary>
    /// <typeparam name="T">The response value type.</typeparam>
    /// <param name="request">The request to dispatch.</param>
    /// <param name="location">Builds the <c>Location</c> header value from the created value.</param>
    protected async Task<IResult> Created<T>(IRequest<T> request, Func<T, string> location)
    {
        var response = await Mediator.SendAsync(request, Cancel).ConfigureAwait(false);
        var result = response.ToCreated(Http, location);
        return result;
    }

    /// <summary>Dispatches <paramref name="request"/> and renders success as 204 No Content, failure as ProblemDetails.</summary>
    /// <typeparam name="T">The response value type.</typeparam>
    /// <param name="request">The request to dispatch.</param>
    protected async Task<IResult> NoContent<T>(IRequest<T> request)
    {
        var response = await Mediator.SendAsync(request, Cancel).ConfigureAwait(false);
        var result = response.ToNoContent(Http);
        return result;
    }
}
