// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Pondhawk.Utilities.Hosting;

/// <summary>
/// Provides application lifecycle management via file-based signaling, such as detecting start, stop,
/// and custom shutdown scenarios using specific flag files. Implements the <see cref="IHostedService"/>
/// interface for integration with the .NET Generic Host lifecycle.
/// </summary>
public partial class AppLifecycleService : IHostedService, IDisposable
{
    private readonly IHostApplicationLifetime _lifetime;
    private readonly ILogger<AppLifecycleService> _logger;
    private readonly string _flagDirectory;
    private FileSystemWatcher? _watcher;

    private const string StartedFlag = "started.flag";
    private const string MustStopFlag = "muststop.flag";
    private const string StoppedFlag = "stopped.flag";

    /// <summary>
    /// Manages the lifecycle of an application through file-based signaling.
    /// Enables the detection of key application events such as start and stop,
    /// and facilitates custom shutdown behavior through the use of flag files.
    /// Implements the <see cref="IHostedService"/> interface for integration
    /// with the .NET Generic Host lifecycle.
    /// </summary>
    public AppLifecycleService(
        IHostApplicationLifetime lifetime,
        ILoggerFactory loggerFactory,
        string? flagDirectory = null)
    {
        _lifetime = lifetime;
        _logger = loggerFactory.CreateLogger<AppLifecycleService>();
        _flagDirectory = flagDirectory ?? AppContext.BaseDirectory;
    }

    /// <summary>
    /// Starts the application lifecycle service by cleaning up stale flags,
    /// registering lifecycle callbacks, and initiating the monitoring of
    /// file-based shutdown signals.
    /// </summary>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <return>A completed <see cref="Task"/> once startup operations are initialized.</return>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Clean up stale flags from previous runs
        CleanupStaleFlags();

        // Register lifecycle callbacks
        _lifetime.ApplicationStarted.Register(OnStarted);
        _lifetime.ApplicationStopped.Register(OnStopped);

        // Start watching for muststop.flag
        StartWatching();

        return Task.CompletedTask;
    }

    /// <summary>
    /// Stops the application lifecycle service by halting the monitoring of
    /// file-based shutdown signals and releasing associated resources.
    /// </summary>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <return>A completed <see cref="Task"/> once shutdown operations are finalized.</return>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        StopWatching();
        return Task.CompletedTask;
    }

    private void CleanupStaleFlags()
    {
        // Clean up all stale flags from previous runs
        DeleteFlag(StartedFlag);
        DeleteFlag(StoppedFlag);
        DeleteFlag(MustStopFlag);
    }

    private void OnStarted()
    {
        CreateFlag(StartedFlag);
    }

    private void OnStopped()
    {
        CreateFlag(StoppedFlag);
    }

    private void StartWatching()
    {
        // Dispose existing watcher if StartWatching called multiple times
        _watcher?.Dispose();

        _watcher = new FileSystemWatcher(_flagDirectory, MustStopFlag)
        {
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime
        };
        _watcher.Created += OnMustStopCreated;
        _watcher.EnableRaisingEvents = true;
    }

    private void StopWatching()
    {
        _watcher?.Dispose();
        _watcher = null;
    }

    private void OnMustStopCreated(object sender, FileSystemEventArgs e)
    {
        LogMustStopDetected(MustStopFlag);
        _lifetime.StopApplication();
    }

    private void CreateFlag(string flagName)
    {
        var path = Path.Combine(_flagDirectory, flagName);
        File.WriteAllText(path, DateTime.UtcNow.ToString("O"));
    }

    private void DeleteFlag(string flagName)
    {
        var path = Path.Combine(_flagDirectory, flagName);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    /// <summary>
    /// Releases the file system watcher used to monitor for shutdown signals.
    /// </summary>
    public void Dispose()
    {
        _watcher?.Dispose();
        GC.SuppressFinalize(this);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Detected {Flag} - initiating graceful shutdown")]
    private partial void LogMustStopDetected(string flag);
}
