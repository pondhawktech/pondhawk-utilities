using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pondhawk.Api;
using Pondhawk.Api.Behaviors;
using Pondhawk.Api.Endpoints;
using Pondhawk.Api.Json;
using Pondhawk.Configuration;
using Pondhawk.Hosting;
using Pondhawk.Mediator;
using Pondhawk.Rules;
using Pondhawk.Rules.Factory;

namespace Pondhawk.Sample.Api.Modules;

/// <summary>
/// The Pondhawk.Api framework wiring, spelled out so nothing is hidden: request context, JSON
/// conventions, ProblemDetails, the mediator and its logging/validation behaviors, endpoint and
/// validation-rule discovery (with the rule factory started on host startup), and the fail-closed
/// authorization default. Copy and edit as the app needs.
/// </summary>
public sealed class ApiModule : IServiceModule
{
    public void Build(IServiceCollection services, IConfiguration configuration)
    {
        var assembly = typeof(ApiModule).Assembly;

        services.AddPondhawkApi();
        services.AddPondhawkJson();
        services.AddProblemDetails();

        services.AddMediator(assembly);
        services.AddPipelineBehavior(typeof(LoggingBehavior<,>));      // outermost: wraps validation
        services.AddPipelineBehavior(typeof(ValidationBehavior<,>));
        services.AddEndpointModules(assembly);

        services.AddRules(assembly);
        services.UseRules();
        services.AddStart<RuleSetFactory>(f => f.Start(), f => f.Stop());   // compile the rules on host startup

        // Fail closed: every endpoint requires an authenticated caller unless it opts out with AllowAnonymous.
        var policy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();

        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = policy;
        });
    }
}
