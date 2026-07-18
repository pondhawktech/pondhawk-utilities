using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pondhawk.Api.Identity;
using Pondhawk.Configuration;

namespace Pondhawk.Sample.Api.Modules;

/// <summary>
/// The application's own module: identity and app services. Config-bound: <see cref="AuthMode"/> selects
/// the gateway authentication handler. Defaults to the deployed <c>header</c> mode; local development
/// overrides it to <c>development</c> (authenticate every request as a fixed identity).
/// </summary>
public sealed class ApplicationModule : IServiceModule
{
    public string AuthMode { get; set; } = "header";

    public void Build(IServiceCollection services, IConfiguration configuration)
    {
        if (string.Equals(AuthMode, "development", StringComparison.OrdinalIgnoreCase))
        {
            var developer = new ClaimSet
            {
                UserId = "dev",
                UserName = "developer",
                Email = "developer@pondhawk.local",
                Roles = { "admin" },
            };

            services.AddGatewayDevelopmentAuthentication(developer);
        }
        else
        {
            services.AddGatewayHeaderAuthentication();
        }

        // App-specific services would be registered here.
    }
}
