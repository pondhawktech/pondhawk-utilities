// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Pondhawk.Context;

/// <summary>Registration for <see cref="IOperationContext"/>.</summary>
public static class OperationContextExtensions
{
    /// <summary>
    /// Registers the empty context as the scoped default, so anything depending on
    /// <see cref="IOperationContext"/> resolves even where nothing populates it — background
    /// hosts, the migrator, tests.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    /// <remarks>
    /// Uses <c>TryAdd</c>, so an edge that registers a populated context first wins. Without this
    /// default, every consumer would have to treat the context as optional, and "optional" is how
    /// a missing tenant becomes a silent no-filter.
    /// </remarks>
    public static IServiceCollection AddOperationContext(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddScoped(_ => OperationContext.None);

        return services;
    }
}
