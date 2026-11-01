// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Pondhawk.Utilities.Hosting;
using Shouldly;
using Xunit;

namespace Pondhawk.Utilities.Tests.Hosting;

public sealed class AppLifecycleServiceTests : IDisposable
{
    // Flag file names mirror the constants in AppLifecycleService.
    private const string StartedFlag = "started.flag";
    private const string MustStopFlag = "muststop.flag";
    private const string StoppedFlag = "stopped.flag";

    private readonly string _dir;

    public AppLifecycleServiceTests()
    {
        _dir = Path.Combine(Path.GetTempPath(), "pondhawk-applifecycle-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_dir);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_dir))
                Directory.Delete(_dir, recursive: true);
        }
        catch
        {
            // Best effort; FileSystemWatcher handles may briefly linger on some platforms.
        }
    }


    // ========== Test double ==========

    private sealed class FakeLifetime : IHostApplicationLifetime, IDisposable
    {
        private readonly CancellationTokenSource _started = new();
        private readonly CancellationTokenSource _stopping = new();
        private readonly CancellationTokenSource _stopped = new();
        private readonly TaskCompletionSource _stopRequested = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public CancellationToken ApplicationStarted => _started.Token;
        public CancellationToken ApplicationStopping => _stopping.Token;
        public CancellationToken ApplicationStopped => _stopped.Token;

        public int StopApplicationCallCount { get; private set; }

        /// <summary>Completes the first time <see cref="StopApplication"/> is invoked.</summary>
        public Task StopRequested => _stopRequested.Task;

        public void StopApplication()
        {
            StopApplicationCallCount++;
            _stopRequested.TrySetResult();
        }

        // CancellationToken callbacks run synchronously on Cancel(), so these fire the
        // service's registered handlers inline.
        public void TriggerStarted() => _started.Cancel();
        public void TriggerStopped() => _stopped.Cancel();

        public void Dispose()
        {
            _started.Dispose();
            _stopping.Dispose();
            _stopped.Dispose();
        }
    }

    private string FlagPath(string name) => Path.Combine(_dir, name);

    private AppLifecycleService CreateService(FakeLifetime lifetime)
        => new(lifetime, NullLoggerFactory.Instance, _dir);


    // ========== Construction ==========

    [Fact]
    public void Constructor_NullDirectory_DoesNotThrow()
    {
        using var lifetime = new FakeLifetime();

        // Defaults to AppContext.BaseDirectory; no file IO happens until StartAsync.
        var act = () => new AppLifecycleService(lifetime, NullLoggerFactory.Instance);

        act.ShouldNotThrow();
    }


    // ========== Stale flag cleanup ==========

    [Fact]
    public async Task StartAsync_RemovesStaleFlags()
    {
        File.WriteAllText(FlagPath(StartedFlag), "stale");
        File.WriteAllText(FlagPath(StoppedFlag), "stale");
        File.WriteAllText(FlagPath(MustStopFlag), "stale");

        using var lifetime = new FakeLifetime();
        using var service = CreateService(lifetime);

        await service.StartAsync(CancellationToken.None);

        File.Exists(FlagPath(StartedFlag)).ShouldBeFalse();
        File.Exists(FlagPath(StoppedFlag)).ShouldBeFalse();
        File.Exists(FlagPath(MustStopFlag)).ShouldBeFalse();
    }

    [Fact]
    public async Task StartAsync_StaleMustStopFlag_DoesNotRequestStop()
    {
        // A leftover muststop.flag from a previous run is cleaned up before watching
        // begins, so it must not trigger a shutdown.
        File.WriteAllText(FlagPath(MustStopFlag), "stale");

        using var lifetime = new FakeLifetime();
        using var service = CreateService(lifetime);

        await service.StartAsync(CancellationToken.None);

        lifetime.StopApplicationCallCount.ShouldBe(0);
    }


    // ========== Lifecycle flag creation ==========

    [Fact]
    public async Task ApplicationStarted_CreatesStartedFlag()
    {
        using var lifetime = new FakeLifetime();
        using var service = CreateService(lifetime);
        await service.StartAsync(CancellationToken.None);

        lifetime.TriggerStarted();

        File.Exists(FlagPath(StartedFlag)).ShouldBeTrue();
    }

    [Fact]
    public async Task ApplicationStopped_CreatesStoppedFlag()
    {
        using var lifetime = new FakeLifetime();
        using var service = CreateService(lifetime);
        await service.StartAsync(CancellationToken.None);

        lifetime.TriggerStopped();

        File.Exists(FlagPath(StoppedFlag)).ShouldBeTrue();
    }


    // ========== muststop.flag watching ==========

    [Fact]
    public async Task MustStopFlagCreated_RequestsApplicationStop()
    {
        using var lifetime = new FakeLifetime();
        using var service = CreateService(lifetime);
        await service.StartAsync(CancellationToken.None);

        File.WriteAllText(FlagPath(MustStopFlag), "stop");

        // FileSystemWatcher fires asynchronously; wait with a generous timeout.
        await lifetime.StopRequested.WaitAsync(TimeSpan.FromSeconds(10));

        lifetime.StopApplicationCallCount.ShouldBe(1);
    }


    // ========== Shutdown / disposal ==========

    [Fact]
    public async Task StopAsync_AfterStart_DoesNotThrow()
    {
        using var lifetime = new FakeLifetime();
        using var service = CreateService(lifetime);
        await service.StartAsync(CancellationToken.None);

        var act = async () => await service.StopAsync(CancellationToken.None);

        await act.ShouldNotThrowAsync();
    }

    [Fact]
    public async Task StopAsync_WithoutStart_DoesNotThrow()
    {
        using var lifetime = new FakeLifetime();
        using var service = CreateService(lifetime);

        var act = async () => await service.StopAsync(CancellationToken.None);

        await act.ShouldNotThrowAsync();
    }

    [Fact]
    public async Task Dispose_IsIdempotent()
    {
        using var lifetime = new FakeLifetime();
        var service = CreateService(lifetime);
        await service.StartAsync(CancellationToken.None);

        service.Dispose();
        var act = service.Dispose;

        act.ShouldNotThrow();
    }
}
