// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using CommunityToolkit.Diagnostics;

namespace Pondhawk.Utilities.Pipeline;

/// <summary>
/// Base pipeline step with before/after hooks and automatic failure short-circuiting.
/// </summary>
public abstract class BasePipelineStep<TContext> where TContext : class, IPipelineContext
{

    /// <summary>
    /// Gets or sets a value indicating whether the pipeline should continue executing subsequent steps after a failure.
    /// </summary>
    public bool ContinueAfterFailure { get; set; }

    /// <summary>
    /// Invokes this step's before/after hooks around the given continuation, skipping execution if the context has failed and <see cref="ContinueAfterFailure"/> is false.
    /// </summary>
    /// <param name="context">The pipeline context shared across all steps.</param>
    /// <param name="continuation">The next step in the pipeline chain to invoke.</param>
    public async Task InvokeAsync(TContext context, Func<TContext, Task> continuation)
    {

        Guard.IsNotNull(context, nameof(context));
        Guard.IsNotNull(continuation, nameof(continuation));

        if (!ContinueAfterFailure && !context.Success)
            return;


        await Before(context).ConfigureAwait(false);

        await continuation(context).ConfigureAwait(false);

        if (!ContinueAfterFailure && !context.Success)
            return;

        await After(context).ConfigureAwait(false);

    }

    /// <summary>
    /// Called before the continuation step executes. Override to add pre-processing logic.
    /// </summary>
    /// <param name="context">The pipeline context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected virtual Task Before(TContext context)
    {

        Guard.IsNotNull(context, nameof(context));

        return Task.CompletedTask;

    }

    /// <summary>
    /// Called after the continuation step executes. Override to add post-processing logic.
    /// </summary>
    /// <param name="context">The pipeline context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected virtual Task After(TContext context)
    {

        Guard.IsNotNull(context, nameof(context));

        return Task.CompletedTask;

    }


}
