// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Pondhawk.Persistence;

/// <summary>
/// DI registration for the persistence seam.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers <typeparamref name="TContext"/> as the EF Core context and exposes the same scoped
    /// instance through <see cref="IDbContext"/>, so slice handlers depend on the seam rather than the
    /// concrete context type.
    /// </summary>
    /// <typeparam name="TContext">The application's EF Core context, which must also implement <see cref="IDbContext"/>.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Configures the <see cref="DbContextOptionsBuilder"/> (provider, connection string, etc.).</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPondhawkDbContext<TContext>(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> configure)
        where TContext : DbContext, IDbContext
    {
        services.AddDbContext<TContext>(configure);
        services.AddScoped<IDbContext>(static sp => sp.GetRequiredService<TContext>());

        return services;
    }
}
