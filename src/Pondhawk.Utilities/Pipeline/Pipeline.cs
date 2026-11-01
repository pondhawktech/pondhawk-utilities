// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Diagnostics;
using Pondhawk.Utilities.Types;

namespace Pondhawk.Utilities.Pipeline;

/// <summary>
/// Executes a chain of pipeline steps around an action, with automatic failure tracking and short-circuiting.
/// </summary>
[SuppressMessage("Design", "MA0049:Type name should not match containing namespace", Justification = "Pipeline is the canonical name for this type")]
public class Pipeline<TContext> where TContext : class, IPipelineContext
{

    private readonly ICollection<IPipelineStep<TContext>> _steps;

    internal Pipeline(ICollection<IPipelineStep<TContext>> steps)
    {
        _steps = steps;
    }

    /// <summary>
    /// Executes the pipeline by running all registered steps around the specified action, with automatic failure tracking.
    /// </summary>
    /// <param name="context">The pipeline context shared across all steps.</param>
    /// <param name="action">The core action to execute within the pipeline.</param>
    /// <returns>A task representing the asynchronous pipeline execution.</returns>
    public async Task ExecuteAsync(TContext context, Func<TContext, Task> action)
    {

        Guard.IsNotNull(context, nameof(context));

        var innerSteps = _steps.ToList();
        innerSteps.Add(new ActionPipelineStep<TContext>(action));

        Func<TContext, Task> nextAction = (_) => Task.CompletedTask;

        foreach (var step in innerSteps.AsEnumerable().Reverse())
        {
            var currentStep = step;
            var capturedNext = nextAction;
            nextAction = async (ctx) => await InvokeWrapper(currentStep, ctx, capturedNext).ConfigureAwait(false);
        }

        await nextAction(context).ConfigureAwait(false);

        async Task InvokeWrapper(IPipelineStep<TContext> step, TContext ctx, Func<TContext, Task> continuation)
        {
            try
            {
                await step.InvokeAsync(ctx, continuation).ConfigureAwait(false);
            }
            catch (Exception cause)
            {
                if (ctx.Success)
                {
                    ctx.Success = false;
                    ctx.FailedStep = step.GetType().GetConciseFullName();
                    ctx.Cause = cause;
                }

                if (!step.ContinueAfterFailure)
                    throw;
            }
        }

    }

}
