// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pondhawk.Utilities.Hosting;
using Shouldly;
using Xunit;

namespace Pondhawk.Utilities.Tests.Hosting;

public class ServiceStarterTests
{

    // ========== Test service ==========

    private class CounterService
    {
        public int StartCount { get; set; }
        public int StopCount { get; set; }

        public void Start() => StartCount++;
        public void Stop() => StopCount++;
    }

    private class AsyncService
    {
        public bool Started { get; set; }
        public bool Stopped { get; set; }
        public CancellationToken ReceivedToken { get; set; }

        public async Task StartAsync(CancellationToken ct)
        {
            ReceivedToken = ct;
            await Task.Delay(1, ct);
            Started = true;
        }

        public async Task StopAsync(CancellationToken ct)
        {
            await Task.Delay(1, ct);
            Stopped = true;
        }
    }


    // ========== Registration ==========

    [Fact]
    public void AddSingletonWithStart_RegistersSingleton()
    {
        var services = new ServiceCollection();

        services.AddSingletonWithStart<CounterService>(s => s.Start());

        var provider = services.BuildServiceProvider();
        var svc = provider.GetService<CounterService>();

        svc.ShouldNotBeNull();
    }

    [Fact]
    public void AddSingletonWithStart_RegistersHostedService()
    {
        var services = new ServiceCollection();

        services.AddSingletonWithStart<CounterService>(s => s.Start());

        var provider = services.BuildServiceProvider();
        var hosted = provider.GetServices<IHostedService>();

        hosted.ShouldContain(h => h is ServiceStarterHostedService);
    }

    [Fact]
    public void AddSingletonWithStart_MultipleServices_RegistersOneHostedService()
    {
        var services = new ServiceCollection();

        services.AddSingletonWithStart<CounterService>(s => s.Start());
        services.AddSingletonWithStart<AsyncService>((s, ct) => s.StartAsync(ct));

        var provider = services.BuildServiceProvider();
        var hosted = provider.GetServices<IHostedService>()
            .Where(h => h is ServiceStarterHostedService)
            .ToList();

        hosted.Count.ShouldBe(1);
    }


    // ========== Start ==========

    [Fact]
    public async Task Start_CallsStartAction()
    {
        var services = new ServiceCollection();
        services.AddSingletonWithStart<CounterService>(s => s.Start());

        var provider = services.BuildServiceProvider();
        var hosted = provider.GetServices<IHostedService>().Single(h => h is ServiceStarterHostedService);

        await hosted.StartAsync(CancellationToken.None);

        var svc = provider.GetRequiredService<CounterService>();
        svc.StartCount.ShouldBe(1);
    }

    [Fact]
    public async Task Start_AsyncAction_CallsStartAction()
    {
        var services = new ServiceCollection();
        services.AddSingletonWithStart<AsyncService>((s, ct) => s.StartAsync(ct));

        var provider = services.BuildServiceProvider();
        var hosted = provider.GetServices<IHostedService>().Single(h => h is ServiceStarterHostedService);

        await hosted.StartAsync(CancellationToken.None);

        var svc = provider.GetRequiredService<AsyncService>();
        svc.Started.ShouldBeTrue();
    }

    [Fact]
    public async Task Start_MultipleServices_StartsAll()
    {
        var services = new ServiceCollection();
        services.AddSingletonWithStart<CounterService>(s => s.Start());
        services.AddSingletonWithStart<AsyncService>((s, ct) => s.StartAsync(ct));

        var provider = services.BuildServiceProvider();
        var hosted = provider.GetServices<IHostedService>().Single(h => h is ServiceStarterHostedService);

        await hosted.StartAsync(CancellationToken.None);

        provider.GetRequiredService<CounterService>().StartCount.ShouldBe(1);
        provider.GetRequiredService<AsyncService>().Started.ShouldBeTrue();
    }

    [Fact]
    public async Task Start_PassesCancellationToken()
    {
        var services = new ServiceCollection();
        using var cts = new CancellationTokenSource();
        services.AddSingletonWithStart<AsyncService>((s, ct) => s.StartAsync(ct));

        var provider = services.BuildServiceProvider();
        var hosted = provider.GetServices<IHostedService>().Single(h => h is ServiceStarterHostedService);

        await hosted.StartAsync(cts.Token);

        provider.GetRequiredService<AsyncService>().ReceivedToken.ShouldBe(cts.Token);
    }


    // ========== Stop ==========

    [Fact]
    public async Task Stop_CallsStopAction()
    {
        var services = new ServiceCollection();
        services.AddSingletonWithStart<CounterService>(s => s.Start(), s => s.Stop());

        var provider = services.BuildServiceProvider();
        var hosted = provider.GetServices<IHostedService>().Single(h => h is ServiceStarterHostedService);

        await hosted.StartAsync(CancellationToken.None);
        await hosted.StopAsync(CancellationToken.None);

        var svc = provider.GetRequiredService<CounterService>();
        svc.StartCount.ShouldBe(1);
        svc.StopCount.ShouldBe(1);
    }

    [Fact]
    public async Task Stop_AsyncAction_CallsStopAction()
    {
        var services = new ServiceCollection();
        services.AddSingletonWithStart<AsyncService>(
            (s, ct) => s.StartAsync(ct),
            (s, ct) => s.StopAsync(ct));

        var provider = services.BuildServiceProvider();
        var hosted = provider.GetServices<IHostedService>().Single(h => h is ServiceStarterHostedService);

        await hosted.StartAsync(CancellationToken.None);
        await hosted.StopAsync(CancellationToken.None);

        var svc = provider.GetRequiredService<AsyncService>();
        svc.Started.ShouldBeTrue();
        svc.Stopped.ShouldBeTrue();
    }

    [Fact]
    public async Task Stop_NoStopAction_DoesNotThrow()
    {
        var services = new ServiceCollection();
        services.AddSingletonWithStart<CounterService>(s => s.Start());

        var provider = services.BuildServiceProvider();
        var hosted = provider.GetServices<IHostedService>().Single(h => h is ServiceStarterHostedService);

        await hosted.StartAsync(CancellationToken.None);
        await hosted.StopAsync(CancellationToken.None); // should be no-op
    }

    [Fact]
    public async Task Stop_ReverseOrder()
    {
        var order = new List<string>();

        var services = new ServiceCollection();
        services.AddSingletonWithStart<CounterService>(
            s => { },
            s => order.Add("counter"));
        services.AddSingletonWithStart<AsyncService>(
            (s, ct) => Task.CompletedTask,
            (s, ct) => { order.Add("async"); return Task.CompletedTask; });

        var provider = services.BuildServiceProvider();
        var hosted = provider.GetServices<IHostedService>().Single(h => h is ServiceStarterHostedService);

        await hosted.StartAsync(CancellationToken.None);
        await hosted.StopAsync(CancellationToken.None);

        order.ShouldBe(["async", "counter"]);
    }


    // ========== Host integration ==========

    [Fact]
    public async Task FullHost_StartsAndStopsServices()
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Services.AddSingletonWithStart<CounterService>(s => s.Start(), s => s.Stop());

        using var host = builder.Build();

        await host.StartAsync();

        var svc = host.Services.GetRequiredService<CounterService>();
        svc.StartCount.ShouldBe(1);

        await host.StopAsync();

        svc.StopCount.ShouldBe(1);
    }
}
