// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text.Json.Serialization;

namespace Pondhawk.Api;

/// <summary>Cross-cutting metadata carried by <see cref="ApiEnvelope.Meta"/>.</summary>
/// <param name="CorrelationId">The request correlation id, echoed on success for tracing.</param>
/// <param name="Pagination">Pagination metadata; present only for paged results, omitted otherwise.</param>
public sealed record ResponseMeta(
    string? CorrelationId,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] PaginationMeta? Pagination);
