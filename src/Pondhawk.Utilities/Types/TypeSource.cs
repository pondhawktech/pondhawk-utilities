// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;
using CommunityToolkit.Diagnostics;

namespace Pondhawk.Utilities.Types;

/// <summary>
/// Collects and filters types from assemblies for discovery-based registration patterns.
/// </summary>
public class TypeSource
{

    private static Func<Type, bool> DefaultPredicate { get; } = _ => true;

    /// <summary>
    /// Gets the predicate used to filter types when adding them to the source. Override to customize filtering.
    /// </summary>
    /// <returns>A predicate that returns <c>true</c> for types that should be included.</returns>
    protected virtual Func<Type, bool> GetPredicate()
    {
        return DefaultPredicate;
    }


    /// <summary>
    /// Adds all types from the specified assemblies that pass the predicate filter.
    /// </summary>
    /// <param name="assemblies">The assemblies whose types should be added.</param>
    public void AddTypes(params Assembly[] assemblies)
    {

        Guard.IsNotNull(assemblies);

        foreach (var type in assemblies.SelectMany(a => a.GetTypes()).Where(GetPredicate()))
            Types.Add(type);
    }


    /// <summary>
    /// Adds the specified types that pass the predicate filter.
    /// </summary>
    /// <param name="types">The types to add.</param>
    public void AddTypes(params Type[] types)
    {

        Guard.IsNotNull(types);

        foreach (var type in types.Where(GetPredicate()))
            Types.Add(type);
    }


    /// <summary>
    /// Adds the specified candidate types that pass the predicate filter.
    /// </summary>
    /// <param name="candidates">The candidate types to evaluate and add.</param>
    public void AddTypes(IEnumerable<Type> candidates)
    {

        Guard.IsNotNull(candidates);

        foreach (var type in candidates.Where(GetPredicate()))
            Types.Add(type);
    }


    private HashSet<Type> Types { get; } = new();

    /// <summary>
    /// Returns all types that have been collected by this source.
    /// </summary>
    /// <returns>An enumerable of the collected types.</returns>
    public IEnumerable<Type> GetTypes()
    {
        return Types;
    }


}
