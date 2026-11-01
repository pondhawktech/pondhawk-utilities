// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pondhawk.Utilities.Configuration;
using Shouldly;
using Xunit;

namespace Pondhawk.Utilities.Tests.Configuration;

public class ServiceModuleExtensionsTests
{

    // ── Test doubles ──

    public class TestModule : IServiceModule
    {
        public string ConnectionString { get; set; } = "";
        public int MaxRetries { get; set; }
        public bool BuildWasCalled { get; private set; }
        public IConfiguration CapturedConfiguration { get; private set; }

        public void Build(IServiceCollection services, IConfiguration configuration)
        {
            BuildWasCalled = true;
            CapturedConfiguration = configuration;
            services.AddSingleton(this);
        }
    }

    public class MinimalModule : IServiceModule
    {
        public bool BuildWasCalled { get; private set; }

        public void Build(IServiceCollection services, IConfiguration configuration)
        {
            BuildWasCalled = true;
        }
    }

    private static IConfiguration BuildConfig(Dictionary<string, string> values = null)
    {
        var builder = new ConfigurationBuilder();
        if (values != null)
        {
            builder.AddInMemoryCollection(values);
        }

        return builder.Build();
    }

    // ── AddServiceModule<TModule>(configuration) ──

    [Fact]
    public void AddServiceModule_CallsBuild()
    {
        var services = new ServiceCollection();
        var config = BuildConfig();

        services.AddServiceModule<TestModule>(config);

        var provider = services.BuildServiceProvider();
        var module = provider.GetRequiredService<TestModule>();
        module.BuildWasCalled.ShouldBeTrue();
    }

    [Fact]
    public void AddServiceModule_PassesConfiguration()
    {
        var services = new ServiceCollection();
        var config = BuildConfig();

        services.AddServiceModule<TestModule>(config);

        var provider = services.BuildServiceProvider();
        var module = provider.GetRequiredService<TestModule>();
        module.CapturedConfiguration.ShouldBeSameAs(config);
    }

    [Fact]
    public void AddServiceModule_BindsConfigurationValues()
    {
        var services = new ServiceCollection();
        var config = BuildConfig(new Dictionary<string, string>
        {
            ["ConnectionString"] = "Server=db;Database=test",
            ["MaxRetries"] = "5"
        });

        services.AddServiceModule<TestModule>(config);

        var provider = services.BuildServiceProvider();
        var module = provider.GetRequiredService<TestModule>();
        module.ConnectionString.ShouldBe("Server=db;Database=test");
        module.MaxRetries.ShouldBe(5);
    }

    [Fact]
    public void AddServiceModule_EmptyConfig_UsesDefaults()
    {
        var services = new ServiceCollection();
        var config = BuildConfig();

        services.AddServiceModule<TestModule>(config);

        var provider = services.BuildServiceProvider();
        var module = provider.GetRequiredService<TestModule>();
        module.ConnectionString.ShouldBe("");
        module.MaxRetries.ShouldBe(0);
    }

    [Fact]
    public void AddServiceModule_ReturnsSameServiceCollection()
    {
        var services = new ServiceCollection();
        var config = BuildConfig();

        var result = services.AddServiceModule<TestModule>(config);

        result.ShouldBeSameAs(services);
    }

    // ── AddServiceModule<TModule>(configuration, configure) ──

    [Fact]
    public void AddServiceModule_WithConfigure_AppliesOverrides()
    {
        var services = new ServiceCollection();
        var config = BuildConfig(new Dictionary<string, string>
        {
            ["ConnectionString"] = "Server=db;Database=test",
            ["MaxRetries"] = "3"
        });

        services.AddServiceModule<TestModule>(config, module =>
        {
            module.MaxRetries = 10;
        });

        var provider = services.BuildServiceProvider();
        var module = provider.GetRequiredService<TestModule>();
        module.ConnectionString.ShouldBe("Server=db;Database=test");
        module.MaxRetries.ShouldBe(10);
    }

    [Fact]
    public void AddServiceModule_WithConfigure_OverridesRunAfterBinding()
    {
        var services = new ServiceCollection();
        var config = BuildConfig(new Dictionary<string, string>
        {
            ["ConnectionString"] = "from-config"
        });

        services.AddServiceModule<TestModule>(config, module =>
        {
            module.ConnectionString = "overridden";
        });

        var provider = services.BuildServiceProvider();
        var module = provider.GetRequiredService<TestModule>();
        module.ConnectionString.ShouldBe("overridden");
    }

    [Fact]
    public void AddServiceModule_WithConfigure_CallsBuild()
    {
        var services = new ServiceCollection();
        var config = BuildConfig();

        services.AddServiceModule<TestModule>(config, _ => { });

        var provider = services.BuildServiceProvider();
        var module = provider.GetRequiredService<TestModule>();
        module.BuildWasCalled.ShouldBeTrue();
    }

    [Fact]
    public void AddServiceModule_WithConfigure_ReturnsSameServiceCollection()
    {
        var services = new ServiceCollection();
        var config = BuildConfig();

        var result = services.AddServiceModule<TestModule>(config, _ => { });

        result.ShouldBeSameAs(services);
    }

    // ── Module registers services correctly ──

    [Fact]
    public void AddServiceModule_ModuleCanRegisterArbitraryServices()
    {
        var services = new ServiceCollection();
        var config = BuildConfig();

        services.AddServiceModule<TestModule>(config);

        var provider = services.BuildServiceProvider();
        // TestModule registers itself as a singleton in Build()
        provider.GetService<TestModule>().ShouldNotBeNull();
    }

}
