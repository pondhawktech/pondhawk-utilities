// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using Pondhawk.Context;
using Shouldly;
using Xunit;

namespace Pondhawk.Core.Tests.Context;

public class OperationContextTests
{
    [Fact]
    public void None_supplies_a_safe_absence_rather_than_a_permissive_default()
    {
        var context = OperationContext.None;

        context.SubjectUid.ShouldBeEmpty();
        context.SubjectDescription.ShouldBeEmpty();
        context.TenantCode.ShouldBeNull();
        context.AllowedTenants.ShouldBeEmpty();
        context.OnBehalfOf.ShouldBeNull();
    }

    [Fact]
    public void Allows_grants_only_what_is_in_the_allowed_set()
    {
        var context = new OperationContext { AllowedTenants = ["ACME-WEST", "ACME-EAST"] };

        context.Allows("ACME-WEST").ShouldBeTrue();
        context.Allows("ACME-EAST").ShouldBeTrue();
        context.Allows("CONTOSO").ShouldBeFalse();
    }

    [Fact]
    public void Allows_is_case_sensitive_and_rejects_nothing_gracefully()
    {
        var context = new OperationContext { AllowedTenants = ["ACME"] };

        context.Allows("acme").ShouldBeFalse("tenant codes are canonical uppercase; a near miss is a miss");
        context.Allows("").ShouldBeFalse();
        context.Allows("   ").ShouldBeFalse();
    }

    [Fact]
    public void An_empty_context_allows_nothing()
    {
        ((OperationContext)OperationContext.None).Allows("ACME").ShouldBeFalse(
            "an unattended operation has no tenant grants, so it must not pass a tenant check");
    }

    [Fact]
    public void Impersonation_is_marked_by_OnBehalfOf()
    {
        new OperationContext().IsImpersonating.ShouldBeFalse();
        new OperationContext { OnBehalfOf = "staff-01" }.IsImpersonating.ShouldBeTrue();
    }

    [Fact]
    public void AddOperationContext_registers_the_empty_context_as_the_scoped_default()
    {
        var services = new ServiceCollection();
        services.AddOperationContext();

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        scope.ServiceProvider.GetRequiredService<IOperationContext>().ShouldBeSameAs(OperationContext.None);
    }

    [Fact]
    public void An_edge_that_registers_first_wins()
    {
        var populated = new OperationContext { SubjectUid = "user-7", TenantCode = "ACME" };

        var services = new ServiceCollection();
        services.AddScoped<IOperationContext>(_ => populated);
        services.AddOperationContext();

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        scope.ServiceProvider.GetRequiredService<IOperationContext>().ShouldBeSameAs(populated);
    }
}
