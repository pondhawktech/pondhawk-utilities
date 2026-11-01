// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Pondhawk.Utilities.Configuration;

/// <summary>
/// Extension methods for registering <see cref="IServiceModule"/> implementations.
/// </summary>
public static class ServiceModuleExtensions
{
    /// <summary>
    /// Binds a <typeparamref name="TModule"/> from the configuration root
    /// and calls <see cref="IServiceModule.Build"/> to register its services.
    /// </summary>
    public static IServiceCollection AddServiceModule<TModule>(
        this IServiceCollection services,
        IConfiguration configuration)
        where TModule : class, IServiceModule, new()
    {
        var module = configuration.Get<TModule>() ?? new TModule();
        module.Build(services, configuration);
        return services;
    }

    /// <summary>
    /// Binds a <typeparamref name="TModule"/> from the configuration root,
    /// applies post-binding overrides, then calls <see cref="IServiceModule.Build"/>
    /// to register its services.
    /// </summary>
    public static IServiceCollection AddServiceModule<TModule>(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<TModule> configure)
        where TModule : class, IServiceModule, new()
    {
        var module = configuration.Get<TModule>() ?? new TModule();
        configure(module);
        module.Build(services, configuration);
        return services;
    }
}
