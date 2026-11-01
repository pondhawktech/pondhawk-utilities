// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace Pondhawk.Utilities.Pipeline;

/// <summary>
/// Extension methods for registering pipeline infrastructure with <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{

    /// <summary>
    /// Registers the pipeline factory with the service collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> used to register the pipeline factory.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining registrations.</returns>
    public static IServiceCollection AddPipelineFactory(this IServiceCollection services)
    {

        services.AddScoped<IPipelineFactory, PipelineFactory>();

        return services;

    }




    /// <summary>
    /// Registers a pipeline builder and its steps with the given service collection.
    /// </summary>
    /// <typeparam name="TContext">The type of the pipeline context that implements <see cref="IPipelineContext"/>.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> used to register pipeline components.</param>
    /// <param name="steps">An action to configure and register the pipeline steps.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining registrations.</returns>
    public static IServiceCollection AddPipeline<TContext>(this IServiceCollection services, Action<IRegisterPipelineStep<TContext>> steps) where TContext : class, IPipelineContext
    {

        Guard.IsNotNull(services, nameof(services));
        Guard.IsNotNull(steps, nameof(steps));

        steps(new RegisterPipelineStep<TContext>(services));

        services.AddTransient<IPipelineBuilder<TContext>>(sp =>
            {

                var list = sp.GetServices<IPipelineStep<TContext>>();
                var comp = new PipelineBuilder<TContext>();

                foreach (var step in list)
                {
                    comp.AddStep(step);
                }

                return comp;

            });

        return services;

    }


}
