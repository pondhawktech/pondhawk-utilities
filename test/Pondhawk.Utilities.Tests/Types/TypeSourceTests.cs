// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Pondhawk.Utilities.Types;
using Shouldly;
using Xunit;

namespace Pondhawk.Utilities.Tests.Types;

public class TypeSourceTests
{

    // ── AddTypes from assemblies ──

    [Fact]
    public void AddTypes_FromAssembly_CollectsTypes()
    {
        var source = new TypeSource();

        source.AddTypes(typeof(TypeSourceTests).Assembly);

        source.GetTypes().ShouldNotBeEmpty();
        source.GetTypes().ShouldContain(typeof(TypeSourceTests));
    }

    [Fact]
    public void AddTypes_NullAssemblies_Throws()
    {
        var source = new TypeSource();

        Should.Throw<ArgumentNullException>(
            () => source.AddTypes((System.Reflection.Assembly[])null));
    }

    // ── AddTypes from Type[] ──

    [Fact]
    public void AddTypes_FromTypeArray_CollectsTypes()
    {
        var source = new TypeSource();

        source.AddTypes(typeof(string), typeof(int));

        source.GetTypes().Count().ShouldBe(2);
        source.GetTypes().ShouldContain(typeof(string));
        source.GetTypes().ShouldContain(typeof(int));
    }

    [Fact]
    public void AddTypes_NullTypeArray_Throws()
    {
        var source = new TypeSource();

        Should.Throw<ArgumentNullException>(
            () => source.AddTypes((Type[])null));
    }

    // ── AddTypes from IEnumerable ──

    [Fact]
    public void AddTypes_FromEnumerable_CollectsTypes()
    {
        var source = new TypeSource();
        var candidates = new List<Type> { typeof(string), typeof(int) };

        source.AddTypes(candidates);

        source.GetTypes().Count().ShouldBe(2);
    }

    [Fact]
    public void AddTypes_NullEnumerable_Throws()
    {
        var source = new TypeSource();

        Should.Throw<ArgumentNullException>(
            () => source.AddTypes((IEnumerable<Type>)null));
    }

    // ── Deduplication ──

    [Fact]
    public void AddTypes_DuplicateType_IsDeduped()
    {
        var source = new TypeSource();

        source.AddTypes(typeof(string));
        source.AddTypes(typeof(string));

        source.GetTypes().Count().ShouldBe(1);
    }

    // ── Custom predicate ──

    private class FilteredTypeSource : TypeSource
    {
        protected override Func<Type, bool> GetPredicate()
        {
            return t => t.IsPublic;
        }
    }

    [Fact]
    public void AddTypes_WithCustomPredicate_FiltersTypes()
    {
        var source = new FilteredTypeSource();

        source.AddTypes(typeof(TypeSourceTests).Assembly);

        // All collected types should be public
        foreach (var type in source.GetTypes())
        {
            type.IsPublic.ShouldBeTrue();
        }
    }

    // ── Empty source ──

    [Fact]
    public void GetTypes_EmptySource_ReturnsEmpty()
    {
        var source = new TypeSource();

        source.GetTypes().ShouldBeEmpty();
    }

}
