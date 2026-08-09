// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Pondhawk.Context;

/// <summary>
/// The populated form of <see cref="IOperationContext"/>. The edge that authenticates builds one
/// of these and registers it for the scope.
/// </summary>
public sealed class OperationContext : IOperationContext
{
    /// <summary>
    /// An operation with no subject and no tenant: scheduled work, migrations, tests. Consumers
    /// that require a tenant still fail against it, which is the intended behaviour — it supplies
    /// a safe absence, not a permissive default.
    /// </summary>
    public static IOperationContext None { get; } = new OperationContext();

    /// <inheritdoc />
    public string SubjectUid { get; init; } = string.Empty;

    /// <inheritdoc />
    public string SubjectDescription { get; init; } = string.Empty;

    /// <inheritdoc />
    public string? TenantCode { get; init; }

    /// <inheritdoc />
    public IReadOnlyCollection<string> AllowedTenants { get; init; } = [];

    /// <inheritdoc />
    public string? OnBehalfOf { get; init; }

    /// <summary>Whether a staff member is acting inside another tenant.</summary>
    public bool IsImpersonating => !string.IsNullOrEmpty(OnBehalfOf);

    /// <summary>Whether the subject may act for the given tenant.</summary>
    /// <param name="tenantCode">The tenant being requested.</param>
    /// <returns>Whether it is granted.</returns>
    /// <remarks>
    /// A tenant supplied by a caller is a <em>selection</em>, never an authorization. This is what
    /// turns the one into the other.
    /// </remarks>
    public bool Allows(string tenantCode) =>
        !string.IsNullOrWhiteSpace(tenantCode) &&
        AllowedTenants.Contains(tenantCode, StringComparer.Ordinal);
}
