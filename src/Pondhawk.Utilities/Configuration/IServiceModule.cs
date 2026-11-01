// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Pondhawk.Utilities.Configuration;

/// <summary>
/// A module that bundles related service registrations.
/// Implementing classes declare public properties that are populated
/// via <see cref="IConfiguration"/> model binding from the configuration root,
/// then use those values in <see cref="Build"/> to register services.
/// </summary>
public interface IServiceModule
{
    /// <summary>
    /// Registers services into the DI container using configuration values.
    /// </summary>
    void Build(IServiceCollection services, IConfiguration configuration);
}
