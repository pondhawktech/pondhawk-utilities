// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Pondhawk.Context;
using Shouldly;
using Xunit;

namespace Pondhawk.Core.Tests.Context;

public class AmbientContextTests
{
    [Fact]
    public void A_value_stored_in_a_scope_reads_back_in_that_scope()
    {
        using var scope = AmbientContext.CreateScope();

        AmbientContext.Set("Tenant", "acme");

        AmbientContext.Get<string>("Tenant").ShouldBe("acme");
    }

    [Fact]
    public void Storing_over_an_existing_value_replaces_it()
    {
        using var scope = AmbientContext.CreateScope();

        AmbientContext.Set("Tenant", "acme");
        AmbientContext.Set("Tenant", "other");

        AmbientContext.Get<string>("Tenant").ShouldBe(
            "other",
            "overwriting is the application's policy to forbid, not this store's");
    }

    [Fact]
    public void An_absent_value_reads_as_default()
    {
        using var scope = AmbientContext.CreateScope();

        AmbientContext.Get<string>("Tenant").ShouldBeNull();
        AmbientContext.Get<int>("Attempt").ShouldBe(0);
    }

    [Fact]
    public void A_stored_null_reads_as_default_rather_than_throwing()
    {
        using var scope = AmbientContext.CreateScope();

        AmbientContext.Set<string>("Tenant", null);

        AmbientContext.Get<string>("Tenant").ShouldBeNull();
    }

    [Fact]
    public void Value_types_round_trip()
    {
        using var scope = AmbientContext.CreateScope();
        var stamp = new DateTimeOffset(2026, 8, 10, 9, 0, 0, TimeSpan.Zero);

        AmbientContext.Set("Attempt", 3);
        AmbientContext.Set("Retryable", true);
        AmbientContext.Set("StartedAt", stamp);

        AmbientContext.Get<int>("Attempt").ShouldBe(3);
        AmbientContext.Get<bool>("Retryable").ShouldBeTrue();
        AmbientContext.Get<DateTimeOffset>("StartedAt").ShouldBe(stamp);
    }

    [Fact]
    public void Reading_a_value_as_the_wrong_type_says_so()
    {
        using var scope = AmbientContext.CreateScope();

        AmbientContext.Set("Attempt", 3);

        var error = Should.Throw<InvalidOperationException>(() => AmbientContext.Get<string>("Attempt"));

        error.Message.ShouldContain("Attempt");
        error.Message.ShouldContain("Int32");
        error.Message.ShouldContain("String");
    }

    [Fact]
    public void Reading_outside_a_scope_yields_default_rather_than_throwing()
    {
        AmbientContext.IsActive.ShouldBeFalse();

        AmbientContext.Get<string>("Tenant").ShouldBeNull(
            "library code runs both inside a request and outside one, and should not have to ask which");
    }

    [Fact]
    public void Writing_outside_a_scope_throws()
    {
        AmbientContext.IsActive.ShouldBeFalse();

        var error = Should.Throw<InvalidOperationException>(() => AmbientContext.Set("Tenant", "acme"));

        error.Message.ShouldContain("Tenant");
        error.Message.ShouldContain("CreateScope");
    }

    [Fact]
    public void IsActive_tracks_the_scope()
    {
        AmbientContext.IsActive.ShouldBeFalse();

        using (var scope = AmbientContext.CreateScope())
        {
            AmbientContext.IsActive.ShouldBeTrue();
        }

        AmbientContext.IsActive.ShouldBeFalse();
    }

    [Fact]
    public void Values_do_not_survive_the_scope_that_held_them()
    {
        using (var first = AmbientContext.CreateScope())
        {
            AmbientContext.Set("Tenant", "acme");
        }

        using var second = AmbientContext.CreateScope();

        AmbientContext.Get<string>("Tenant").ShouldBeNull(
            "a value left over from the last unit of work is a plausible wrong tenant");
    }

    [Fact]
    public void A_nested_scope_starts_from_the_outer_values()
    {
        using var outer = AmbientContext.CreateScope();

        AmbientContext.Set("Tenant", "acme");

        using var inner = AmbientContext.CreateScope();

        AmbientContext.Get<string>("Tenant").ShouldBe("acme");
    }

    [Fact]
    public void A_nested_scope_reverts_its_writes_on_dispose()
    {
        using var outer = AmbientContext.CreateScope();

        AmbientContext.Set("Tenant", "acme");

        using (var inner = AmbientContext.CreateScope())
        {
            AmbientContext.Set("Tenant", "other");
            AmbientContext.Get<string>("Tenant").ShouldBe("other");
        }

        AmbientContext.Get<string>("Tenant").ShouldBe(
            "acme",
            "a temporary override must not escape the block that made it");
    }

    [Fact]
    public void Disposing_a_scope_twice_is_harmless()
    {
        var outer = AmbientContext.CreateScope();
        var inner = AmbientContext.CreateScope();

        inner.Dispose();
        inner.Dispose();

        AmbientContext.IsActive.ShouldBeTrue("the second dispose must not close the outer scope");

        outer.Dispose();

        AmbientContext.IsActive.ShouldBeFalse();
    }

    [Fact]
    public async Task A_value_flows_into_awaited_work()
    {
        using var scope = AmbientContext.CreateScope();

        AmbientContext.Set("Tenant", "acme");

        var seen = await ReadTenantAsync();

        seen.ShouldBe("acme");
    }

    [Fact]
    public async Task A_write_inside_awaited_work_is_visible_to_the_caller()
    {
        using var scope = AmbientContext.CreateScope();

        await WriteTenantAsync();

        AmbientContext.Get<string>("Tenant").ShouldBe(
            "acme",
            "the scope holds values by reference so a write is not lost when the execution context is copied");
    }

    [Fact]
    public async Task Parallel_writes_in_one_scope_all_survive()
    {
        using var scope = AmbientContext.CreateScope();
        var writes = Enumerable.Range(0, 64).Select(i => Task.Run(() => AmbientContext.Set($"Key{i}", i)));

        await Task.WhenAll(writes);

        for (var i = 0; i < 64; i++)
        {
            AmbientContext.Get<int>($"Key{i}").ShouldBe(i);
        }
    }

    [Fact]
    public async Task A_scope_opened_inside_a_task_does_not_leak_out_of_it()
    {
        using var scope = AmbientContext.CreateScope();

        AmbientContext.Set("Tenant", "acme");

        await Task.Run(() =>
        {
            using var nested = AmbientContext.CreateScope();
            AmbientContext.Set("Tenant", "other");
        });

        AmbientContext.Get<string>("Tenant").ShouldBe("acme");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void A_blank_name_is_rejected(string name)
    {
        using var scope = AmbientContext.CreateScope();

        Should.Throw<ArgumentException>(() => AmbientContext.Get<string>(name));
        Should.Throw<ArgumentException>(() => AmbientContext.Set(name, "acme"));
    }

    private static async Task<string> ReadTenantAsync()
    {
        await Task.Yield();

        var tenant = AmbientContext.Get<string>("Tenant");

        return tenant;
    }

    private static async Task WriteTenantAsync()
    {
        await Task.Yield();

        AmbientContext.Set("Tenant", "acme");
    }
}
