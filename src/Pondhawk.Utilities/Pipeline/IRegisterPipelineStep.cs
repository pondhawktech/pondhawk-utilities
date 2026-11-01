// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Pondhawk.Utilities.Pipeline;

/// <summary>
/// Provides a fluent API for registering pipeline steps during pipeline configuration.
/// </summary>
/// <typeparam name="TContext">The pipeline context type that implements <see cref="IPipelineContext"/>.</typeparam>
public interface IRegisterPipelineStep<TContext> where TContext : class, IPipelineContext
{
    /// <summary>
    /// Adds a pipeline step of the specified type to the current pipeline registration.
    /// </summary>
    /// <typeparam name="TStep">The type of the pipeline step to be added, which must implement <see cref="IPipelineStep{TContext}"/>.</typeparam>
    /// <returns>The current <see cref="IRegisterPipelineStep{TContext}"/> instance for chaining further step registrations.</returns>
    IRegisterPipelineStep<TContext> Add<TStep>() where TStep : class, IPipelineStep<TContext>;
}
