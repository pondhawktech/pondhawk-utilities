// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Pondhawk.Utilities.Types;

/// <summary>
/// Provides pre-built date/time range models and methods for calculating date ranges and Unix timestamps.
/// </summary>
public static class DateTimeHelpers
{

    static DateTimeHelpers()
    {

        var id = 0;

        PastModels = new List<IDateTimeRange>
        {
            new DateTimeRangeModel {Id=++id, Label = "Last 1 Minute",  RangeKind = DateTimeRange.Prev1Min},
            new DateTimeRangeModel {Id=++id,Label = "Last 2 Minutes",  RangeKind = DateTimeRange.Prev2Min},
            new DateTimeRangeModel {Id=++id,Label = "Last 5 Minutes",  RangeKind = DateTimeRange.Prev5Min},
            new DateTimeRangeModel {Id=++id,Label = "Last 15 Minutes", RangeKind = DateTimeRange.Prev15Min},
            new DateTimeRangeModel {Id=++id,Label = "Last 30 Minutes", RangeKind = DateTimeRange.Prev30Min},
            new DateTimeRangeModel {Id=++id,Label = "Last 1 Hour",     RangeKind = DateTimeRange.Prev1Hour},
            new DateTimeRangeModel {Id=++id,Label = "Last 2 Hours",    RangeKind = DateTimeRange.Prev2Hour},
            new DateTimeRangeModel {Id=++id,Label = "Last 4 Hours",    RangeKind = DateTimeRange.Prev4Hour},
            new DateTimeRangeModel {Id=++id,Label = "Last 8 Hours",    RangeKind = DateTimeRange.Prev8Hour},
            new DateTimeRangeModel {Id=++id,Label = "Last 12 Hours",   RangeKind = DateTimeRange.Prev12Hour},
            new DateTimeRangeModel {Id=++id,Label = "Last 24 Hours",   RangeKind = DateTimeRange.Prev24Hour},
            new DateTimeRangeModel {Id=++id,Label = "Today",           RangeKind = DateTimeRange.Today},
            new DateTimeRangeModel {Id=++id,Label = "Yesterday",       RangeKind = DateTimeRange.Yesterday},
            new DateTimeRangeModel {Id=++id,Label = "This Week",       RangeKind = DateTimeRange.ThisWeek},
            new DateTimeRangeModel {Id=++id,Label = "Last Week",       RangeKind = DateTimeRange.LastWeek},
            new DateTimeRangeModel {Id=++id,Label = "This Month",      RangeKind = DateTimeRange.ThisMonth},
            new DateTimeRangeModel {Id=++id,Label = "Last Month",      RangeKind = DateTimeRange.LastMonth},
            new DateTimeRangeModel {Id=++id,Label = "This Year",       RangeKind = DateTimeRange.ThisYear},
            new DateTimeRangeModel {Id=++id,Label = "Last Year",       RangeKind = DateTimeRange.LastYear}
        };


        FutureModels = new List<IDateTimeRange>
        {
            new DateTimeRangeModel {Id=++id,Label = "Next 1 Minute",   RangeKind  = DateTimeRange.Next1Min},
            new DateTimeRangeModel {Id=++id,Label = "Next 2 Minutes",  RangeKind = DateTimeRange.Next2Min},
            new DateTimeRangeModel {Id=++id,Label = "Next 5 Minutes",  RangeKind = DateTimeRange.Next5Min},
            new DateTimeRangeModel {Id=++id,Label = "Next 15 Minutes", RangeKind = DateTimeRange.Next15Min},
            new DateTimeRangeModel {Id=++id,Label = "Next 30 Minutes", RangeKind = DateTimeRange.Next30Min},
            new DateTimeRangeModel {Id=++id,Label = "Next 1 Hour",     RangeKind = DateTimeRange.Next1Hour},
            new DateTimeRangeModel {Id=++id,Label = "Next 2 Hours",    RangeKind = DateTimeRange.Next2Hour},
            new DateTimeRangeModel {Id=++id,Label = "Next 4 Hours",    RangeKind = DateTimeRange.Next4Hour},
            new DateTimeRangeModel {Id=++id,Label = "Next 8 Hours",    RangeKind = DateTimeRange.Next8Hour},
            new DateTimeRangeModel {Id=++id,Label = "Next 12 Hours",   RangeKind = DateTimeRange.Next12Hour},
            new DateTimeRangeModel {Id=++id,Label = "Next 24 Hours",   RangeKind = DateTimeRange.Next24Hour},
            new DateTimeRangeModel {Id=++id,Label = "Today",           RangeKind = DateTimeRange.Today},
            new DateTimeRangeModel {Id=++id,Label = "Tomorrow",        RangeKind = DateTimeRange.Tommorrow},
            new DateTimeRangeModel {Id=++id,Label = "This Week",       RangeKind = DateTimeRange.ThisWeek},
            new DateTimeRangeModel {Id=++id,Label = "Next Week",       RangeKind = DateTimeRange.NextWeek},
            new DateTimeRangeModel {Id=++id,Label = "This Month",      RangeKind = DateTimeRange.ThisMonth},
            new DateTimeRangeModel {Id=++id,Label = "Next Month",      RangeKind = DateTimeRange.NextMonth},
            new DateTimeRangeModel {Id=++id,Label = "This Year",       RangeKind = DateTimeRange.ThisYear},
            new DateTimeRangeModel {Id=++id,Label = "Next Year",       RangeKind = DateTimeRange.NextYear}
        };


        AllModels = PastModels.Concat(FutureModels).ToList();
        RecentModels = PastModels.Take(13).ToList();

    }

    /// <summary>
    /// Gets a collection of recent past date/time range models (up to the last 24 hours and today).
    /// </summary>
    public static IReadOnlyCollection<IDateTimeRange> RecentModels { get; }

    /// <summary>
    /// Gets a collection of all past-oriented date/time range models.
    /// </summary>
    public static IReadOnlyCollection<IDateTimeRange> PastModels { get; }

    /// <summary>
    /// Gets a collection of all future-oriented date/time range models.
    /// </summary>
    public static IReadOnlyCollection<IDateTimeRange> FutureModels { get; }

    /// <summary>
    /// Gets a collection of all date/time range models (past and future combined).
    /// </summary>
    public static IReadOnlyCollection<IDateTimeRange> AllModels { get; }

    /// <summary>
    /// Gets the Unix epoch (January 1, 1970 00:00:00 UTC).
    /// </summary>
    public static DateTime Epoch { get; } = new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// Converts a <see cref="DateTime"/> to a Unix timestamp in seconds.
    /// </summary>
    /// <param name="target">The date/time to convert. If <c>default</c>, uses <see cref="DateTime.Now"/>.</param>
    /// <returns>The Unix timestamp in seconds.</returns>
    public static long ToTimestamp(DateTime target)
    {

        if (target == default)
            target = DateTime.Now;

        var ts = (long)(target.ToUniversalTime() - Epoch).TotalSeconds;

        return ts;

    }

    /// <summary>
    /// Converts a Unix timestamp in seconds to a local <see cref="DateTime"/>.
    /// </summary>
    /// <param name="ts">The Unix timestamp in seconds.</param>
    /// <returns>The corresponding local <see cref="DateTime"/>.</returns>
    public static DateTime FromTimestamp(long ts) => Epoch.AddSeconds(ts).ToLocalTime();

    /// <summary>
    /// Converts a <see cref="DateTime"/> to a Unix timestamp in milliseconds.
    /// </summary>
    /// <param name="target">The date/time to convert. If <c>default</c>, uses <see cref="DateTime.Now"/>.</param>
    /// <returns>The Unix timestamp in milliseconds.</returns>
    public static long ToTimestampMilli(DateTime target = default)
    {

        if (target == default)
            target = DateTime.Now;

        var ts = (long)(target.ToUniversalTime() - Epoch).TotalMilliseconds;

        return ts;

    }

    /// <summary>
    /// Converts a Unix timestamp in milliseconds to a local <see cref="DateTime"/>.
    /// </summary>
    /// <param name="ts">The Unix timestamp in milliseconds.</param>
    /// <returns>The corresponding local <see cref="DateTime"/>.</returns>
    public static DateTime FromTimestampMilli(long ts) => Epoch.AddMilliseconds(ts).ToLocalTime();



    /// <summary>
    /// Returns the date of the start of the week containing the specified date.
    /// </summary>
    /// <param name="dt">The date to calculate from.</param>
    /// <param name="startOfWeek">The day that starts the week (defaults to <see cref="DayOfWeek.Sunday"/>).</param>
    /// <returns>The <see cref="DateTime"/> representing the start of the week.</returns>
    public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek = DayOfWeek.Sunday)
    {

        int diff = dt.DayOfWeek - startOfWeek;
        if (diff < 0)
        {
            diff += 7;
        }

        return dt.AddDays(-1 * diff).Date;

    }


    /// <summary>
    /// Returns the first day of the month containing the specified date.
    /// </summary>
    /// <param name="dt">The date to calculate from.</param>
    /// <returns>The <see cref="DateTime"/> representing the first day of the month.</returns>
    public static DateTime StartOfMonth(this DateTime dt)
    {


        var year = dt.Year;
        var month = dt.Month;

        var start = new DateTime(year, month, 1, 0, 0, 0);

        return start;

    }

    /// <summary>
    /// Calculates the begin and end <see cref="DateTime"/> for a predefined date/time range model identified by its 1-based ID.
    /// </summary>
    /// <param name="id">The 1-based ID of the date/time range model from <see cref="AllModels"/>.</param>
    /// <param name="origin">The reference date/time to calculate from. If <c>default</c>, uses <see cref="DateTime.Now"/>.</param>
    /// <returns>A tuple containing the begin and end <see cref="DateTime"/>.</returns>
    public static (DateTime begin, DateTime end) CalculateRange(int id, DateTime origin = default)
    {

        if (id < 1 || id > AllModels.Count)
            throw new ArgumentOutOfRangeException(nameof(id), id, "Id must be between 1 and " + AllModels.Count);

        var range = AllModels.ElementAt(id - 1);

        return CalculateRange(range.RangeKind, origin);

    }


    /// <summary>
    /// Calculates the begin and end Unix timestamps (in seconds) for a predefined date/time range model identified by its 1-based ID.
    /// </summary>
    /// <param name="id">The 1-based ID of the date/time range model from <see cref="AllModels"/>.</param>
    /// <param name="origin">The reference date/time to calculate from. If <c>default</c>, uses <see cref="DateTime.Now"/>.</param>
    /// <returns>A tuple containing the begin and end Unix timestamps in seconds.</returns>
    public static (long begin, long end) CalculateTimestamps(int id, DateTime origin = default)
    {

        if (id < 1 || id > AllModels.Count)
            throw new ArgumentOutOfRangeException(nameof(id), id, "Id must be between 1 and " + AllModels.Count);

        var range = AllModels.ElementAt(id - 1);
        var (begin, end) = CalculateRange(range.RangeKind, origin);

        return ((long)(begin.ToUniversalTime() - Epoch).TotalSeconds, (long)(end.ToUniversalTime() - Epoch).TotalSeconds);

    }


    /// <summary>
    /// Calculates the begin and end Unix timestamps (in seconds) for the specified <see cref="DateTimeRange"/>.
    /// </summary>
    /// <param name="range">The date/time range kind to calculate.</param>
    /// <param name="origin">The reference date/time to calculate from. If <c>default</c>, uses <see cref="DateTime.Now"/>.</param>
    /// <returns>A tuple containing the begin and end Unix timestamps in seconds.</returns>
    public static (long begin, long end) CalculateTimestamps(DateTimeRange range, DateTime origin = default)
    {

        var (begin, end) = CalculateRange(range, origin);

        return ((long)(begin.ToUniversalTime() - Epoch).TotalSeconds, (long)(end.ToUniversalTime() - Epoch).TotalSeconds);

    }



    /// <summary>
    /// Calculates the begin and end <see cref="DateTime"/> for the specified <see cref="DateTimeRange"/>.
    /// </summary>
    /// <param name="range">The date/time range kind to calculate.</param>
    /// <param name="origin">The reference date/time to calculate from. If <c>default</c>, uses <see cref="DateTime.Now"/>.</param>
    /// <returns>A tuple containing the begin and end <see cref="DateTime"/>.</returns>
    public static (DateTime begin, DateTime end) CalculateRange(DateTimeRange range, DateTime origin = default)
    {
        var now = origin == default ? DateTime.Now : origin;
        origin = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, 0);

        return range switch
        {
            // Previous time ranges (begin adjusted, end = origin)
            DateTimeRange.Prev1Min => (origin - TimeSpan.FromMinutes(1), origin),
            DateTimeRange.Prev2Min => (origin - TimeSpan.FromMinutes(2), origin),
            DateTimeRange.Prev5Min => (origin - TimeSpan.FromMinutes(5), origin),
            DateTimeRange.Prev15Min => (origin - TimeSpan.FromMinutes(15), origin),
            DateTimeRange.Prev30Min => (origin - TimeSpan.FromMinutes(30), origin),
            DateTimeRange.Prev1Hour => (origin - TimeSpan.FromHours(1), origin),
            DateTimeRange.Prev2Hour => (origin - TimeSpan.FromHours(2), origin),
            DateTimeRange.Prev4Hour => (origin - TimeSpan.FromHours(4), origin),
            DateTimeRange.Prev8Hour => (origin - TimeSpan.FromHours(8), origin),
            DateTimeRange.Prev12Hour => (origin - TimeSpan.FromHours(12), origin),
            DateTimeRange.Prev24Hour => (origin - TimeSpan.FromHours(24), origin),

            // Next time ranges (begin = origin, end adjusted)
            DateTimeRange.Next1Min => (origin, origin + TimeSpan.FromMinutes(1)),
            DateTimeRange.Next2Min => (origin, origin + TimeSpan.FromMinutes(2)),
            DateTimeRange.Next5Min => (origin, origin + TimeSpan.FromMinutes(5)),
            DateTimeRange.Next15Min => (origin, origin + TimeSpan.FromMinutes(15)),
            DateTimeRange.Next30Min => (origin, origin + TimeSpan.FromMinutes(30)),
            DateTimeRange.Next1Hour => (origin, origin + TimeSpan.FromHours(1)),
            DateTimeRange.Next2Hour => (origin, origin + TimeSpan.FromHours(2)),
            DateTimeRange.Next4Hour => (origin, origin + TimeSpan.FromHours(4)),
            DateTimeRange.Next8Hour => (origin, origin + TimeSpan.FromHours(8)),
            DateTimeRange.Next12Hour => (origin, origin + TimeSpan.FromHours(12)),
            DateTimeRange.Next24Hour => (origin, origin + TimeSpan.FromHours(24)),

            // Day ranges
            DateTimeRange.Today => (origin.Date, origin.Date + TimeSpan.FromHours(24)),
            DateTimeRange.Yesterday => (origin.Date - TimeSpan.FromHours(24), origin.Date),
            DateTimeRange.Tommorrow => (origin.Date + TimeSpan.FromHours(24), origin.Date + TimeSpan.FromHours(48)),

            // Week ranges
            DateTimeRange.ThisWeek => (origin.Date.StartOfWeek(), origin.Date.StartOfWeek() + TimeSpan.FromDays(7)),
            DateTimeRange.LastWeek => (origin.Date.StartOfWeek() - TimeSpan.FromDays(7), origin.Date.StartOfWeek()),
            DateTimeRange.NextWeek => (origin.Date.StartOfWeek() + TimeSpan.FromDays(7), origin.Date.StartOfWeek() + TimeSpan.FromDays(14)),

            // Month ranges
            DateTimeRange.ThisMonth => CalculateMonthRange(origin.Date.StartOfMonth()),
            DateTimeRange.LastMonth => CalculateMonthRange((origin.Date - TimeSpan.FromDays(1)).StartOfMonth()),
            DateTimeRange.NextMonth => CalculateMonthRange((origin.Date + TimeSpan.FromDays(32)).StartOfMonth()),

            // Year ranges
            DateTimeRange.ThisYear => (new DateTime(origin.Year, 1, 1), new DateTime(origin.Year + 1, 1, 1)),
            DateTimeRange.LastYear => (new DateTime(origin.Year - 1, 1, 1), new DateTime(origin.Year, 1, 1)),
            DateTimeRange.NextYear => (new DateTime(origin.Year + 1, 1, 1), new DateTime(origin.Year + 2, 1, 1)),

            _ => throw new ArgumentOutOfRangeException(nameof(range), range, null)
        };
    }

    private static (DateTime begin, DateTime end) CalculateMonthRange(DateTime monthStart) =>
        (monthStart, (monthStart + TimeSpan.FromDays(32)).StartOfMonth());


    /// <summary>
    /// Looks up and returns the <see cref="IDateTimeRange"/> model for the specified range kind.
    /// </summary>
    /// <param name="range">The date/time range kind to look up.</param>
    /// <returns>The matching <see cref="IDateTimeRange"/> model.</returns>
    public static IDateTimeRange From(DateTimeRange range)
    {
        return PastModels.SingleOrDefault(r => r.RangeKind == range) ?? FutureModels.Single(r => r.RangeKind == range);
    }

}
