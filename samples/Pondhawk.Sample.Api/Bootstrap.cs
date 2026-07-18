using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pondhawk.Api.Endpoints;
using Pondhawk.Api.Middleware;
using Pondhawk.Configuration;
using Pondhawk.Sample.Api.Data;
using Pondhawk.Sample.Api.Modules;

namespace Pondhawk.Sample.Api;

/// <summary>
/// Owns application startup. It registers the config-bound service modules, builds the app, prepares the
/// database, and wires the middleware pipeline in the required order. <c>Program.cs</c> does nothing but
/// call <see cref="Build"/> and run the result.
/// </summary>
public static class Bootstrap
{
    /// <summary>Builds the fully configured web application, ready to run.</summary>
    /// <param name="args">The command-line arguments.</param>
    public static WebApplication Build(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        ConfigureConfiguration(builder);
        var configuration = builder.Configuration;

        // Registration lives in explicit, config-bound modules — no hidden mega-method.
        builder.Services.AddServiceModule<PersistenceModule>(configuration);
        builder.Services.AddServiceModule<ApiModule>(configuration);
        builder.Services.AddServiceModule<ApplicationModule>(configuration);

        var app = builder.Build();

        EnsureDatabase(app);
        ConfigurePipeline(app);

        return app;
    }

    // The application's configuration sources, in precedence order (later overrides earlier): a required
    // base file, an optional environment file and mission file the deployment can drop in, and finally
    // environment variables prefixed APP_. Replaces the default appsettings/unprefixed-env sources.
    private static void ConfigureConfiguration(WebApplicationBuilder builder)
    {
        var configuration = builder.Configuration;

        ((IConfigurationBuilder)configuration).Sources.Clear();

        configuration
            .AddJsonFile("configuration.json", optional: false)
            .AddJsonFile("environment.json", optional: true)
            .AddJsonFile("mission.json", optional: true)
            .AddEnvironmentVariables(prefix: "APP_");
    }

    // Sample convenience: create the SQLite schema on startup. A real app uses EF migrations.
    private static void EnsureDatabase(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        database.Database.EnsureCreated();
    }

    // The middleware pipeline, explicit and ordered: monitor early, authentication before authorization,
    // enrichment after auth, request logging deep, endpoints last.
    private static void ConfigurePipeline(WebApplication app)
    {
        app.UseDiagnosticsMonitor();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseDiagnosticsEnrichment();
        app.UseRequestLogging();
        app.MapEndpointModules("/api");
    }
}
