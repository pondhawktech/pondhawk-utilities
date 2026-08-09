// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Pondhawk.Context;

/// <summary>
/// Who is doing the work, and which tenant it is for. Scoped to one operation — a request, a
/// queue message, a scheduled run — and populated at the edge that knows those facts.
/// </summary>
/// <remarks>
/// <para>
/// It deliberately carries <b>no correlation identifier</b>. <c>Pondhawk.Logging</c>'s
/// <c>CorrelationManager</c> already owns that, flowing it through <see cref="System.Diagnostics.Activity"/>
/// baggage so it crosses async boundaries and reaches background work with nothing threaded
/// through. Two competing correlation identifiers is how one trace ends up split across two ids.
/// </para>
/// <para>
/// It also carries <b>no principal</b>. The subject is exposed as flat values because the
/// consumers need identity, not authorization: a persistence layer that can see a
/// <c>ClaimsPrincipal</c> is a persistence layer that will eventually make an authorization
/// decision. The principal stays at the edge that authenticates.
/// </para>
/// </remarks>
public interface IOperationContext
{
    /// <summary>
    /// The acting subject's stable identifier, from the identity provider. Empty when the work
    /// has no user — a scheduled job, a migration, a test.
    /// </summary>
    string SubjectUid { get; }

    /// <summary>A human-readable name for the subject, for audit journals and log events.</summary>
    string SubjectDescription { get; }

    /// <summary>
    /// The tenant this operation is for, or <see langword="null"/> when it is not tenant-scoped.
    /// A consumer that requires one must fail rather than assume: an absent tenant returning
    /// everything is how one customer sees another's data.
    /// </summary>
    string? TenantCode { get; }

    /// <summary>
    /// Every tenant the subject may reach. A single-tenant user has one; a regional manager has
    /// several; an unattended job has none. Used to validate a requested tenant and to bound a
    /// multi-tenant read.
    /// </summary>
    IReadOnlyCollection<string> AllowedTenants { get; }

    /// <summary>
    /// Set when a staff member is acting inside a customer's tenant rather than their own. Its
    /// presence marks the operation read-only and turns on read auditing.
    /// </summary>
    string? OnBehalfOf { get; }
}
