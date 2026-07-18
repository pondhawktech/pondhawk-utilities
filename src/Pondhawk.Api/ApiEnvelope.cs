// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Pondhawk.Api;

/// <summary>
/// The standard success response envelope every successful endpoint renders: the payload in
/// <see cref="Data"/> and cross-cutting metadata in <see cref="Meta"/>. Failures are deliberately
/// <em>not</em> enveloped — they render as RFC 7807 ProblemDetails. Giving every success one shape lets
/// clients parse uniformly and lets the API add <see cref="Meta"/> fields without breaking callers.
/// </summary>
/// <param name="Data">The response payload — an entity, or the items of a paged result.</param>
/// <param name="Meta">Cross-cutting metadata (correlation id, and pagination for paged results).</param>
public sealed record ApiEnvelope(object? Data, ResponseMeta Meta);
