// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Pondhawk.Utilities.Hosting;

/// <summary>
/// Extension methods for registering singletons with co-located startup and shutdown logic.
/// </summary>
/// <remarks>
/// Each call registers the service as a singleton plus a <see cref="ServiceStartDescriptor"/>.
/// A single <see cref="ServiceStarterHostedService"/> (auto-registered via <c>TryAddEnumerable</c>)
/// resolves all descriptors on host startup and invokes start actions, then stop actions in reverse order on shutdown.
/// </remarks>
/// <example>
/// <code>
/// // Synchronous start
/// services.AddSingletonWithStart&lt;RuleSetFactory&gt;(f =&gt; f.Start());
///
/// // Synchronous start + stop
/// services.AddSingletonWithStart&lt;RuleSetFactory&gt;(f =&gt; f.Start(), f =&gt; f.Stop());
///
/// // Async start + stop with CancellationToken
/// services.AddSingletonWithStart&lt;MyService&gt;(
///     (svc, ct) =&gt; svc.InitializeAsync(ct),
///     (svc, ct) =&gt; svc.ShutdownAsync(ct));
/// </code>
/// </example>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers <typeparamref name="TService"/> as a singleton and invokes <paramref name="startAction"/> when the host starts.
    /// </summary>
    /// <typeparam name="TService">The service type to register.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="startAction">The synchronous action to run on host startup.</param>
    public static IServiceCollection AddSingletonWithStart<TService>(
        this IServiceCollection services,
        Action<TService> startAction)
        where TService : class
    {
        return services.AddSingletonWithStart<TService>(
            (svc, _) => { startAction(svc); return Task.CompletedTask; },
            null);
    }

    /// <summary>
    /// Registers <typeparamref name="TService"/> as a singleton with synchronous start and stop actions.
    /// </summary>
    /// <typeparam name="TService">The service type to register.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="startAction">The synchronous action to run on host startup.</param>
    /// <param name="stopAction">The synchronous action to run on host shutdown.</param>
    public static IServiceCollection AddSingletonWithStart<TService>(
        this IServiceCollection services,
        Action<TService> startAction,
        Action<TService> stopAction)
        where TService : class
    {
        return services.AddSingletonWithStart<TService>(
            (svc, _) => { startAction(svc); return Task.CompletedTask; },
            (svc, _) => { stopAction(svc); return Task.CompletedTask; });
    }

    /// <summary>
    /// Registers <typeparamref name="TService"/> as a singleton and invokes an async <paramref name="startAction"/> when the host starts.
    /// </summary>
    /// <typeparam name="TService">The service type to register.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="startAction">The async function to run on host startup.</param>
    public static IServiceCollection AddSingletonWithStart<TService>(
        this IServiceCollection services,
        Func<TService, CancellationToken, Task> startAction)
        where TService : class
    {
        return services.AddSingletonWithStart(startAction, null);
    }

    /// <summary>
    /// Registers <typeparamref name="TService"/> as a singleton with async start and stop functions.
    /// </summary>
    /// <typeparam name="TService">The service type to register.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="startAction">The async function to run on host startup.</param>
    /// <param name="stopAction">The async function to run on host shutdown, or <c>null</c> for no-op.</param>
    public static IServiceCollection AddSingletonWithStart<TService>(
        this IServiceCollection services,
        Func<TService, CancellationToken, Task> startAction,
        Func<TService, CancellationToken, Task>? stopAction)
        where TService : class
    {
        services.AddSingleton<TService>();
        EnsureHostedService(services);

        services.AddSingleton(new ServiceStartDescriptor
        {
            ServiceType = typeof(TService),
            StartAction = (svc, ct) => startAction((TService)svc, ct),
            StopAction = stopAction is not null
                ? (svc, ct) => stopAction((TService)svc, ct)
                : (_, _) => Task.CompletedTask
        });

        return services;
    }

    private static void EnsureHostedService(IServiceCollection services)
    {
        services.TryAddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IHostedService, ServiceStarterHostedService>());
    }
}
