// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace Pondhawk.Persistence;

/// <summary>
/// The persistence seam the application layer (slice handlers) depends on instead of a concrete
/// <see cref="DbContext"/>. A concrete EF Core context satisfies this simply by declaring it —
/// <c>public class AppDbContext : DbContext, IDbContext</c> — because <see cref="DbContext"/> already
/// exposes matching <c>Set&lt;TEntity&gt;()</c> and <c>SaveChangesAsync</c> members. Handlers inject
/// <see cref="IDbContext"/> so they stay testable against a fake and are not welded to a specific
/// context type.
/// </summary>
public interface IDbContext
{
    /// <summary>Returns the queryable/insertable set for <typeparamref name="TEntity"/>.</summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    [SuppressMessage("Naming", "CA1716:Identifiers should not match keywords",
        Justification = "Named to mirror DbContext.Set<T>() exactly, so a DbContext satisfies IDbContext by merely declaring it.")]
    DbSet<TEntity> Set<TEntity>() where TEntity : class;

    /// <summary>Persists pending changes and returns the number of state entries written to the store.</summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
