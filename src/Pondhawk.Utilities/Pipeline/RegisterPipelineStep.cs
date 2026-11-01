// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace Pondhawk.Utilities.Pipeline;

internal sealed class RegisterPipelineStep<TContext>(IServiceCollection services) : IRegisterPipelineStep<TContext> where TContext : class, IPipelineContext
{

    IRegisterPipelineStep<TContext> IRegisterPipelineStep<TContext>.Add<TStep>() => Add<TStep>();

    public IRegisterPipelineStep<TContext> Add<TStep>() where TStep : class, IPipelineStep<TContext>
    {

        Guard.IsNotNull(services, nameof(services));

        services.AddTransient<IPipelineStep<TContext>, TStep>();

        return this;

    }

}
