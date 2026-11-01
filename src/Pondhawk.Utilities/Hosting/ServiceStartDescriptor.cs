// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Pondhawk.Utilities.Hosting;

internal sealed class ServiceStartDescriptor
{
    public required Type ServiceType { get; init; }
    public required Func<object, CancellationToken, Task> StartAction { get; init; }
    public required Func<object, CancellationToken, Task> StopAction { get; init; }
}
