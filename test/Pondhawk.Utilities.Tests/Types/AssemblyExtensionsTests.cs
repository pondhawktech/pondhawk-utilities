// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;
using Pondhawk.Utilities.Types;
using Shouldly;
using Xunit;

namespace Pondhawk.Utilities.Tests.Types;

public class AssemblyExtensionsTests
{

    private static readonly Assembly TestAssembly = typeof(AssemblyExtensionsTests).Assembly;

    // ── GetResource ──

    [Fact]
    public void GetResource_NonExistent_ReturnsNull()
    {
        var result = TestAssembly.GetResource("NonExistent.Resource");

        result.ShouldBeNull();
    }

    [Fact]
    public void GetResource_EmptyName_Throws()
    {
        Should.Throw<ArgumentException>(
            () => TestAssembly.GetResource(""));
    }

    [Fact]
    public void GetResource_NullName_Throws()
    {
        Should.Throw<ArgumentNullException>(
            () => TestAssembly.GetResource(null));
    }

    // ── GetResourceNames ──

    [Fact]
    public void GetResourceNames_WithFilter_ReturnsFilteredResults()
    {
        var results = TestAssembly.GetResourceNames(n => n.Contains("NonExistent"));

        results.ShouldBeEmpty();
    }

    [Fact]
    public void GetResourceNames_NullFilter_Throws()
    {
        Should.Throw<ArgumentNullException>(
            () => TestAssembly.GetResourceNames(null));
    }

    // ── GetResourceNamesByPath ──

    [Fact]
    public void GetResourceNamesByPath_NonExistent_ReturnsEmpty()
    {
        var results = TestAssembly.GetResourceNamesByPath("NoSuchPath.");

        results.ShouldBeEmpty();
    }

    [Fact]
    public void GetResourceNamesByPath_NullPath_Throws()
    {
        Should.Throw<ArgumentNullException>(
            () => TestAssembly.GetResourceNamesByPath(null));
    }

    // ── GetResourceNamesByExt ──

    [Fact]
    public void GetResourceNamesByExt_NonExistent_ReturnsEmpty()
    {
        var results = TestAssembly.GetResourceNamesByExt(".nosuchext");

        results.ShouldBeEmpty();
    }

    [Fact]
    public void GetResourceNamesByExt_NullExtension_Throws()
    {
        Should.Throw<ArgumentNullException>(
            () => TestAssembly.GetResourceNamesByExt(null));
    }

    // ── GetResourceNamesByPathAndExt ──

    [Fact]
    public void GetResourceNamesByPathAndExt_NonExistent_ReturnsEmpty()
    {
        var results = TestAssembly.GetResourceNamesByPathAndExt("NoPath.", ".nosuchext");

        results.ShouldBeEmpty();
    }

    [Fact]
    public void GetResourceNamesByPathAndExt_NullPath_Throws()
    {
        Should.Throw<ArgumentNullException>(
            () => TestAssembly.GetResourceNamesByPathAndExt(null, ".txt"));
    }

    [Fact]
    public void GetResourceNamesByPathAndExt_NullExtension_Throws()
    {
        Should.Throw<ArgumentNullException>(
            () => TestAssembly.GetResourceNamesByPathAndExt("Path.", null));
    }

    // ── GetFilteredTypes ──

    [Fact]
    public void GetFilteredTypes_ReturnsMatchingTypes()
    {
        var results = TestAssembly.GetFilteredTypes(t => t.Name == nameof(AssemblyExtensionsTests));

        results.ShouldContain(typeof(AssemblyExtensionsTests));
    }

    [Fact]
    public void GetFilteredTypes_NullFilter_Throws()
    {
        Should.Throw<ArgumentNullException>(
            () => TestAssembly.GetFilteredTypes(null));
    }

    // ── GetImplementations ──

    public interface ITestMarker { }
    public class TestMarkerImpl : ITestMarker { }

    [Fact]
    public void GetImplementations_FindsImplementors()
    {
        var results = TestAssembly.GetImplementations(typeof(ITestMarker));

        results.ShouldContain(typeof(TestMarkerImpl));
        results.ShouldNotContain(typeof(ITestMarker));
    }

    [Fact]
    public void GetImplementations_NullType_Throws()
    {
        Should.Throw<ArgumentNullException>(
            () => TestAssembly.GetImplementations(null));
    }

    // ── GetTypesWithAttribute ──

    [AttributeUsage(AttributeTargets.Class)]
    public class TestMarkerAttribute : Attribute { }

    [TestMarker]
    public class DecoratedClass { }

    [Fact]
    public void GetTypesWithAttribute_FindsDecoratedTypes()
    {
        var results = TestAssembly.GetTypesWithAttribute(typeof(TestMarkerAttribute));

        results.ShouldContain(typeof(DecoratedClass));
    }

    [Fact]
    public void GetTypesWithAttribute_NullAttribute_Throws()
    {
        Should.Throw<ArgumentNullException>(
            () => TestAssembly.GetTypesWithAttribute(null));
    }

    // ── GetImplementationsWithAttribute ──

    [TestMarker]
    public class DecoratedMarkerImpl : ITestMarker { }

    [Fact]
    public void GetImplementationsWithAttribute_FindsMatchingTypes()
    {
        var results = TestAssembly.GetImplementationsWithAttribute(
            typeof(ITestMarker), typeof(TestMarkerAttribute));

        results.ShouldContain(typeof(DecoratedMarkerImpl));
        results.ShouldNotContain(typeof(TestMarkerImpl));
    }

    [Fact]
    public void GetImplementationsWithAttribute_NullImplements_Throws()
    {
        Should.Throw<ArgumentNullException>(
            () => TestAssembly.GetImplementationsWithAttribute(null, typeof(TestMarkerAttribute)));
    }

    [Fact]
    public void GetImplementationsWithAttribute_NullAttribute_Throws()
    {
        Should.Throw<ArgumentNullException>(
            () => TestAssembly.GetImplementationsWithAttribute(typeof(ITestMarker), null));
    }

}
