// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using CommunityToolkit.Diagnostics;

namespace Pondhawk.Utilities.Types;

/// <summary>
/// Extension methods for byte arrays, DateTime formatting, and human-readable type name generation.
/// </summary>
public static class TypeExtensions
{


    /// <summary>
    /// Converts a byte array to its lowercase hexadecimal string representation.
    /// </summary>
    /// <param name="bytes">The byte array to convert.</param>
    /// <returns>A lowercase hexadecimal string.</returns>
    public static string ToHexString(this byte[] bytes)
    {

        Guard.IsNotNull(bytes);

        var hex = Convert.ToHexStringLower(bytes);
        return hex;

    }


    /// <summary>
    /// Converts a <see cref="DateTime"/> to a sortable UTC timestamp string in the format YYYYMMDDTicks.
    /// </summary>
    /// <param name="source">The <see cref="DateTime"/> to convert.</param>
    /// <returns>A sortable timestamp string.</returns>
    public static string ToTimestampString(this DateTime source)
    {

        var utc = source.ToUniversalTime();

        var y = utc.Year.ToString(System.Globalization.CultureInfo.InvariantCulture).PadLeft(4, '0');
        var m = utc.Month.ToString(System.Globalization.CultureInfo.InvariantCulture).PadLeft(2, '0');
        var d = utc.Day.ToString(System.Globalization.CultureInfo.InvariantCulture).PadLeft(2, '0');
        var t = utc.TimeOfDay.Ticks.ToString(System.Globalization.CultureInfo.InvariantCulture).PadLeft(20, '0');

        var ts = string.Join("", y, m, d, t);
        return ts;

    }

    /// <summary>
    /// Gets a human-readable short name for a type, including generic type arguments (e.g. <c>Repository&lt;Order&gt;</c>).
    /// </summary>
    /// <param name="type">The type to get the concise name for.</param>
    /// <returns>A concise type name with generic arguments expanded.</returns>
    public static string GetConciseName(this Type type)
    {

        var conciseName = type.Name;
        if (!type.IsGenericType)
            return conciseName;

        var iBacktick = conciseName.IndexOf('`');
        if (iBacktick > 0) conciseName =
            conciseName.Remove(iBacktick);

        var genericParameters = type.GetGenericArguments().Select(x => x.GetConciseName());
        conciseName += "<" + string.Join(", ", genericParameters) + ">";


        return conciseName;


    }

    /// <summary>
    /// Gets a human-readable fully qualified name for a type, including generic type arguments (e.g. <c>MyApp.Services.Repository&lt;Order&gt;</c>).
    /// </summary>
    /// <param name="type">The type to get the concise full name for.</param>
    /// <returns>A concise fully qualified type name with generic arguments expanded, or an empty string if the full name is unavailable.</returns>
    public static string GetConciseFullName(this Type type)
    {

        var conciseName = type.FullName;
        if (string.IsNullOrWhiteSpace(conciseName))
            return "";

        if (!type.IsGenericType)
            return conciseName;

        var iBacktick = conciseName.IndexOf('`');
        if (iBacktick > 0) conciseName =
            conciseName.Remove(iBacktick);

        var genericParameters = type.GetGenericArguments().Select(x => x.GetConciseName());
        conciseName += "<" + string.Join(", ", genericParameters) + ">";


        return conciseName;


    }



}
