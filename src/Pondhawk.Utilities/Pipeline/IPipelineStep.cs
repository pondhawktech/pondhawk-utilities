// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Pondhawk.Utilities.Pipeline;

/// <summary>
/// A single step in a pipeline that wraps the next step in the chain.
/// </summary>
public interface IPipelineStep<TContext> where TContext : class
{

    /// <summary>
    /// Gets a value indicating whether the pipeline should continue executing subsequent steps after a failure.
    /// </summary>
    bool ContinueAfterFailure { get; }

    /// <summary>
    /// Invokes this pipeline step, optionally calling the next step via the provided continuation.
    /// </summary>
    /// <param name="context">The pipeline context shared across all steps.</param>
    /// <param name="continuation">The delegate to invoke the next step in the pipeline chain.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task InvokeAsync(TContext context, Func<TContext, Task> continuation);

}
