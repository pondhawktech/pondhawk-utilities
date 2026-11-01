// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using Pondhawk.Utilities.Pipeline;
using Shouldly;
using Xunit;

namespace Pondhawk.Utilities.Tests.Pipeline;

public class PipelineTests
{

    // ── Test doubles ──

    private class TestContext : BasePipelineContext, IPipelineContext
    {
        public List<string> Log { get; } = [];
    }

    private class LoggingStep(string name) : BasePipelineStep<TestContext>, IPipelineStep<TestContext>
    {
        protected override Task Before(TestContext context)
        {
            context.Log.Add($"{name}:Before");
            return Task.CompletedTask;
        }

        protected override Task After(TestContext context)
        {
            context.Log.Add($"{name}:After");
            return Task.CompletedTask;
        }
    }

    private class FailingStep : IPipelineStep<TestContext>
    {
        public bool ContinueAfterFailure { get; set; }

        public Task InvokeAsync(TestContext context, Func<TestContext, Task> continuation)
        {
            throw new InvalidOperationException("Step failed");
        }
    }

    private class ContinueAfterFailureStep : BasePipelineStep<TestContext>, IPipelineStep<TestContext>
    {
        public ContinueAfterFailureStep() { ContinueAfterFailure = true; }

        protected override Task Before(TestContext context)
        {
            context.Log.Add("ContinueStep:Before");
            return Task.CompletedTask;
        }

        protected override Task After(TestContext context)
        {
            context.Log.Add("ContinueStep:After");
            return Task.CompletedTask;
        }
    }

    // Uses the base BasePipelineStep.Before/After no-op hooks without overriding them.
    private sealed class PlainStep : BasePipelineStep<TestContext>, IPipelineStep<TestContext>;

    // Parameterless for DI registration
    private class DiLoggingStep() : LoggingStep("DI");

    // ── Pipeline execution ──

    [Fact]
    public async Task ExecuteAsync_RunsActionAndSteps()
    {
        var builder = new PipelineBuilder<TestContext>();
        builder.AddStep(new LoggingStep("S1"));
        var pipeline = builder.Build();
        var ctx = new TestContext();

        await pipeline.ExecuteAsync(ctx, c =>
        {
            c.Log.Add("Action");
            return Task.CompletedTask;
        });

        ctx.Log.ShouldBe(["S1:Before", "Action", "S1:After"]);
        ctx.Success.ShouldBeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_MultipleSteps_ExecuteInOrder()
    {
        var builder = new PipelineBuilder<TestContext>();
        builder.AddStep(new LoggingStep("S1"));
        builder.AddStep(new LoggingStep("S2"));
        var pipeline = builder.Build();
        var ctx = new TestContext();

        await pipeline.ExecuteAsync(ctx, c =>
        {
            c.Log.Add("Action");
            return Task.CompletedTask;
        });

        ctx.Log.ShouldBe(["S1:Before", "S2:Before", "Action", "S2:After", "S1:After"]);
    }

    [Fact]
    public async Task ExecuteAsync_SetsPhaseToAfter_AfterAction()
    {
        var builder = new PipelineBuilder<TestContext>();
        var pipeline = builder.Build();
        var ctx = new TestContext();

        await pipeline.ExecuteAsync(ctx, _ => Task.CompletedTask);

        ctx.Phase.ShouldBe(PipelinePhase.After);
    }

    [Fact]
    public async Task ExecuteAsync_NullContext_Throws()
    {
        var builder = new PipelineBuilder<TestContext>();
        var pipeline = builder.Build();

        await Should.ThrowAsync<ArgumentNullException>(
            () => pipeline.ExecuteAsync(null, _ => Task.CompletedTask));
    }

    [Fact]
    public async Task ExecuteAsync_StepFailure_SetsContextFailure()
    {
        var builder = new PipelineBuilder<TestContext>();
        builder.AddStep(new FailingStep());
        var pipeline = builder.Build();
        var ctx = new TestContext();

        await Should.ThrowAsync<InvalidOperationException>(
            () => pipeline.ExecuteAsync(ctx, _ => Task.CompletedTask));

        ctx.Success.ShouldBeFalse();
        ctx.FailedStep.ShouldNotBeEmpty();
        ctx.Cause.ShouldNotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_StepFailure_ContinueAfterFailure_DoesNotThrow()
    {
        var builder = new PipelineBuilder<TestContext>();
        builder.AddStep(new FailingStep { ContinueAfterFailure = true });
        var pipeline = builder.Build();
        var ctx = new TestContext();

        await pipeline.ExecuteAsync(ctx, _ => Task.CompletedTask);

        ctx.Success.ShouldBeFalse();
    }

    // ── BasePipelineStep ──

    [Fact]
    public async Task BasePipelineStep_SkipsExecution_WhenContextFailed()
    {
        var step = new LoggingStep("S1");
        var ctx = new TestContext { Success = false };

        await ((IPipelineStep<TestContext>)step).InvokeAsync(ctx, _ => Task.CompletedTask);

        ctx.Log.ShouldBeEmpty();
    }

    [Fact]
    public async Task BasePipelineStep_ContinueAfterFailure_StillRuns()
    {
        var step = new ContinueAfterFailureStep();
        var ctx = new TestContext { Success = false };

        await ((IPipelineStep<TestContext>)step).InvokeAsync(ctx, _ => Task.CompletedTask);

        ctx.Log.ShouldContain("ContinueStep:Before");
    }

    [Fact]
    public async Task BasePipelineStep_SkipsAfter_WhenContextFailsDuringContinuation()
    {
        var step = new LoggingStep("S1");
        var ctx = new TestContext();

        await ((IPipelineStep<TestContext>)step).InvokeAsync(ctx, c =>
        {
            c.Success = false;
            return Task.CompletedTask;
        });

        ctx.Log.ShouldContain("S1:Before");
        ctx.Log.ShouldNotContain("S1:After");
    }

    [Fact]
    public async Task BasePipelineStep_NullContext_Throws()
    {
        var step = new LoggingStep("S1");

        await Should.ThrowAsync<ArgumentNullException>(
            () => ((IPipelineStep<TestContext>)step).InvokeAsync(null, _ => Task.CompletedTask));
    }

    [Fact]
    public async Task BasePipelineStep_NullContinuation_Throws()
    {
        var step = new LoggingStep("S1");
        var ctx = new TestContext();

        await Should.ThrowAsync<ArgumentNullException>(
            () => ((IPipelineStep<TestContext>)step).InvokeAsync(ctx, null));
    }

    [Fact]
    public async Task BasePipelineStep_DefaultHooks_PassThroughToContinuation()
    {
        // A step that does not override Before/After exercises the base no-op hooks.
        var step = new PlainStep();
        var ctx = new TestContext();
        var continued = false;

        await ((IPipelineStep<TestContext>)step).InvokeAsync(ctx, c =>
        {
            continued = true;
            c.Log.Add("Action");
            return Task.CompletedTask;
        });

        continued.ShouldBeTrue();
        ctx.Log.ShouldBe(["Action"]);
        ctx.Success.ShouldBeTrue();
    }

    // ── ActionPipelineStep ──

    [Fact]
    public async Task ActionPipelineStep_InvokesAction_AndSetsPhaseToAfter()
    {
        var ran = false;
        var step = new ActionPipelineStep<TestContext>(c =>
        {
            ran = true;
            c.Log.Add("Action");
            return Task.CompletedTask;
        });
        var ctx = new TestContext();

        await step.InvokeAsync(ctx, _ => Task.CompletedTask);

        ran.ShouldBeTrue();
        ctx.Log.ShouldBe(["Action"]);
        ctx.Phase.ShouldBe(PipelinePhase.After);
    }

    [Fact]
    public void ActionPipelineStep_ContinueAfterFailure_DefaultsFalse_AndIsSettable()
    {
        var step = new ActionPipelineStep<TestContext>(_ => Task.CompletedTask);

        step.ContinueAfterFailure.ShouldBeFalse();

        step.ContinueAfterFailure = true;

        step.ContinueAfterFailure.ShouldBeTrue();
    }

    // ── BasePipelineContext ──

    [Fact]
    public void BasePipelineContext_DefaultValues()
    {
        var ctx = new TestContext();

        ctx.Success.ShouldBeTrue();
        ctx.Phase.ShouldBe(PipelinePhase.Before);
        ctx.FailedStep.ShouldBe(string.Empty);
        ctx.Cause.ShouldBeNull();
        ctx.ExceptionType.ShouldBe(string.Empty);
        ctx.ExceptionMessage.ShouldBe(string.Empty);
    }

    [Fact]
    public void BasePipelineContext_WithException_ExposesTypeAndMessage()
    {
        var ctx = new TestContext
        {
            Cause = new InvalidOperationException("test error")
        };

        ctx.ExceptionType.ShouldBe("InvalidOperationException");
        ctx.ExceptionMessage.ShouldBe("test error");
    }

    // ── PipelineBuilder ──

    [Fact]
    public void PipelineBuilder_AddStep_NullStep_Throws()
    {
        var builder = new PipelineBuilder<TestContext>();

        Should.Throw<ArgumentNullException>(() => builder.AddStep(null));
    }

    // ── ServiceCollectionExtensions ──

    [Fact]
    public void AddPipelineFactory_RegistersFactory()
    {
        var services = new ServiceCollection();

        services.AddPipelineFactory();

        var provider = services.BuildServiceProvider();
        provider.GetService<IPipelineFactory>().ShouldNotBeNull();
    }

    [Fact]
    public void AddPipeline_RegistersBuilderAndSteps()
    {
        var services = new ServiceCollection();
        services.AddPipeline<TestContext>(steps =>
        {
            steps.Add<DiLoggingStep>();
        });

        var provider = services.BuildServiceProvider();
        var builder = provider.GetService<IPipelineBuilder<TestContext>>();
        builder.ShouldNotBeNull();
    }

    [Fact]
    public void AddPipeline_NullServices_Throws()
    {
        IServiceCollection services = null;

        Should.Throw<ArgumentNullException>(
            () => services.AddPipeline<TestContext>(_ => { }));
    }

    [Fact]
    public void AddPipeline_NullSteps_Throws()
    {
        var services = new ServiceCollection();

        Should.Throw<ArgumentNullException>(
            () => services.AddPipeline<TestContext>(null));
    }

    // ── PipelineFactory ──

    [Fact]
    public void PipelineFactory_Create_ResolvesPipeline()
    {
        var services = new ServiceCollection();
        services.AddPipelineFactory();
        services.AddPipeline<TestContext>(_ => { });
        var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IPipelineFactory>();

        var pipeline = factory.Create<TestContext>();

        pipeline.ShouldNotBeNull();
    }

}
