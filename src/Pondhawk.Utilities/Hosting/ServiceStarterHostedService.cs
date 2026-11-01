// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Pondhawk.Utilities.Hosting;

internal sealed partial class ServiceStarterHostedService : IHostedService
{
    private readonly IServiceProvider _provider;
    private readonly IEnumerable<ServiceStartDescriptor> _descriptors;
    private readonly ILogger _logger;

    public ServiceStarterHostedService(
        IServiceProvider provider,
        IEnumerable<ServiceStartDescriptor> descriptors,
        ILoggerFactory loggerFactory)
    {
        _provider = provider;
        _descriptors = descriptors;
        _logger = loggerFactory.CreateLogger<ServiceStarterHostedService>();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var descriptors = _descriptors.ToList();
        LogStarting(descriptors.Count);

        foreach (var descriptor in descriptors)
        {
            var serviceName = descriptor.ServiceType.Name;
            var service = _provider.GetService(descriptor.ServiceType);

            if (service is null)
            {
                LogServiceNotResolved(serviceName);
                continue;
            }

            LogServiceStarting(serviceName);
            try
            {
                await descriptor.StartAction(service, cancellationToken).ConfigureAwait(false);
                LogServiceStarted(serviceName);
            }
            catch (Exception ex)
            {
                LogServiceStartFailed(serviceName, ex);
            }
        }

        LogStarted(descriptors.Count);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        var descriptors = _descriptors.Reverse().ToList();
        LogStopping(descriptors.Count);

        foreach (var descriptor in descriptors)
        {
            var serviceName = descriptor.ServiceType.Name;
            var service = _provider.GetService(descriptor.ServiceType);

            if (service is null)
            {
                LogServiceNotResolved(serviceName);
                continue;
            }

            LogServiceStopping(serviceName);
            try
            {
                await descriptor.StopAction(service, cancellationToken).ConfigureAwait(false);
                LogServiceStopped(serviceName);
            }
            catch (Exception ex)
            {
                LogServiceStopFailed(serviceName, ex);
            }
        }

        LogStopped(descriptors.Count);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Starting {Count} registered service(s)")]
    private partial void LogStarting(int count);

    [LoggerMessage(Level = LogLevel.Information, Message = "Started {Count} registered service(s)")]
    private partial void LogStarted(int count);

    [LoggerMessage(Level = LogLevel.Information, Message = "Starting service {ServiceName}")]
    private partial void LogServiceStarting(string serviceName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Started service {ServiceName}")]
    private partial void LogServiceStarted(string serviceName);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Service {ServiceName} could not be resolved")]
    private partial void LogServiceNotResolved(string serviceName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Stopping {Count} registered service(s)")]
    private partial void LogStopping(int count);

    [LoggerMessage(Level = LogLevel.Information, Message = "Stopping service {ServiceName}")]
    private partial void LogServiceStopping(string serviceName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Stopped service {ServiceName}")]
    private partial void LogServiceStopped(string serviceName);

    [LoggerMessage(Level = LogLevel.Error, Message = "Service {ServiceName} failed to start")]
    private partial void LogServiceStartFailed(string serviceName, Exception ex);

    [LoggerMessage(Level = LogLevel.Error, Message = "Service {ServiceName} failed to stop")]
    private partial void LogServiceStopFailed(string serviceName, Exception ex);

    [LoggerMessage(Level = LogLevel.Information, Message = "Stopped {Count} registered service(s)")]
    private partial void LogStopped(int count);
}
