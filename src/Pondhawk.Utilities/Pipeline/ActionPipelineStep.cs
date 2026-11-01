// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Pondhawk.Utilities.Pipeline;

internal sealed class ActionPipelineStep<TContext>(Func<TContext, Task> action) : IPipelineStep<TContext> where TContext : class, IPipelineContext
{

    public bool ContinueAfterFailure { get; set; }

    public async Task InvokeAsync(TContext context, Func<TContext, Task> continuation)
    {

        await action(context).ConfigureAwait(false);
        context.Phase = PipelinePhase.After;

    }


}
