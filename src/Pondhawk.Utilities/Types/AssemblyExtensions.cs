// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;
using CommunityToolkit.Diagnostics;

namespace Pondhawk.Utilities.Types;

/// <summary>
/// Extension methods for <see cref="System.Reflection.Assembly"/> to query embedded resources and filter types.
/// </summary>
public static class AssemblyExtensions
{

    /// <summary>
    /// Gets an embedded manifest resource stream by name from the specified assembly.
    /// </summary>
    /// <param name="target">The assembly to retrieve the resource from.</param>
    /// <param name="name">The case-sensitive name of the manifest resource.</param>
    /// <returns>The resource stream, or <c>null</c> if no resource with the specified name is found.</returns>
    public static Stream? GetResource(this Assembly target, string name)
    {

        Guard.IsNotNullOrEmpty(name);

        return target.GetManifestResourceStream(name);

    }

    /// <summary>
    /// Returns the names of embedded manifest resources that match the specified filter predicate.
    /// </summary>
    /// <param name="target">The assembly to query.</param>
    /// <param name="filter">A predicate to filter resource names.</param>
    /// <returns>An enumerable of matching resource names.</returns>
    public static IEnumerable<string> GetResourceNames(this Assembly target, Func<string, bool> filter)
    {

        Guard.IsNotNull(filter);

        var results = target.GetManifestResourceNames().Where(filter);
        return results;

    }

    /// <summary>
    /// Returns the names of embedded manifest resources whose names start with the specified path prefix.
    /// </summary>
    /// <param name="target">The assembly to query.</param>
    /// <param name="path">The path prefix to match against resource names.</param>
    /// <returns>An enumerable of matching resource names.</returns>
    public static IEnumerable<string> GetResourceNamesByPath(this Assembly target, string path)
    {

        Guard.IsNotNullOrEmpty(path);


        bool Filter(string r) => r.StartsWith(path, StringComparison.Ordinal);

        var results = target.GetManifestResourceNames().Where(Filter);
        return results;

    }

    /// <summary>
    /// Returns the names of embedded manifest resources whose names end with the specified file extension.
    /// </summary>
    /// <param name="target">The assembly to query.</param>
    /// <param name="extension">The file extension suffix to match (e.g. <c>.json</c>).</param>
    /// <returns>An enumerable of matching resource names.</returns>
    public static IEnumerable<string> GetResourceNamesByExt(this Assembly target, string extension)
    {

        Guard.IsNotNullOrEmpty(extension);

        bool Filter(string r) => r.EndsWith(extension, StringComparison.Ordinal);

        var results = target.GetManifestResourceNames().Where(Filter);
        return results;

    }


    /// <summary>
    /// Returns the names of embedded manifest resources whose names start with the specified path prefix and end with the specified extension.
    /// </summary>
    /// <param name="target">The assembly to query.</param>
    /// <param name="path">The path prefix to match against resource names.</param>
    /// <param name="extension">The file extension suffix to match.</param>
    /// <returns>An enumerable of matching resource names.</returns>
    public static IEnumerable<string> GetResourceNamesByPathAndExt(this Assembly target, string path, string extension)
    {

        Guard.IsNotNullOrEmpty(path);
        Guard.IsNotNullOrEmpty(extension);

        bool Filter(string r) => r.StartsWith(path, StringComparison.Ordinal) && r.EndsWith(extension, StringComparison.Ordinal);

        var results = target.GetManifestResourceNames().Where(Filter);
        return results;

    }


    /// <summary>
    /// Returns all types from the assembly that match the specified filter predicate.
    /// </summary>
    /// <param name="target">The assembly to query.</param>
    /// <param name="filter">A predicate to filter types.</param>
    /// <returns>An enumerable of matching types.</returns>
    public static IEnumerable<Type> GetFilteredTypes(this Assembly target, Func<Type, bool> filter)
    {

        Guard.IsNotNull(filter);


        return target.GetTypes().Where(filter);

    }

    /// <summary>
    /// Returns all types from the assembly that implement or derive from the specified type.
    /// </summary>
    /// <param name="target">The assembly to query.</param>
    /// <param name="implements">The base type or interface that returned types must implement.</param>
    /// <returns>An enumerable of types that implement the specified type.</returns>
    public static IEnumerable<Type> GetImplementations(this Assembly target, Type implements)
    {

        Guard.IsNotNull(implements);


        bool Filter(Type t) => (t != implements) && (implements.IsAssignableFrom(t));

        return target.GetTypes().Where(Filter);

    }


    /// <summary>
    /// Returns all types from the assembly that are decorated with the specified attribute type.
    /// </summary>
    /// <param name="target">The assembly to query.</param>
    /// <param name="attribute">The attribute type to search for.</param>
    /// <returns>An enumerable of types decorated with the specified attribute.</returns>
    public static IEnumerable<Type> GetTypesWithAttribute(this Assembly target, Type attribute)
    {

        Guard.IsNotNull(attribute);

        bool Filter(Type t) => t.GetCustomAttributes(attribute, false).Length > 0;

        return target.GetTypes().Where(Filter);

    }

    /// <summary>
    /// Returns all types from the assembly that implement the specified type and are decorated with the specified attribute.
    /// </summary>
    /// <param name="target">The assembly to query.</param>
    /// <param name="implements">The base type or interface that returned types must implement.</param>
    /// <param name="attribute">The attribute type that returned types must be decorated with.</param>
    /// <returns>An enumerable of types that match both the implementation and attribute criteria.</returns>
    public static IEnumerable<Type> GetImplementationsWithAttribute(this Assembly target, Type implements, Type attribute)
    {

        Guard.IsNotNull(implements);
        Guard.IsNotNull(attribute);

        bool Predicate(Type t) => (t != implements) && (implements.IsAssignableFrom(t)) && (t.GetCustomAttributes(attribute, false).Length > 0);

        return target.GetTypes().Where(Predicate);

    }


}
