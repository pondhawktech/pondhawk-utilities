// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Pondhawk.Utilities.Types;

/// <summary>
/// Enumerates predefined relative date/time range kinds (e.g. Prev1Hour, Today, ThisWeek).
/// </summary>
public enum DateTimeRange
{
    /// <summary>Previous 1 minute.</summary>
    Prev1Min,

    /// <summary>Previous 2 minutes.</summary>
    Prev2Min,

    /// <summary>Previous 5 minutes.</summary>
    Prev5Min,

    /// <summary>Previous 15 minutes.</summary>
    Prev15Min,

    /// <summary>Previous 30 minutes.</summary>
    Prev30Min,

    /// <summary>Previous 1 hour.</summary>
    Prev1Hour,

    /// <summary>Previous 2 hours.</summary>
    Prev2Hour,

    /// <summary>Previous 4 hours.</summary>
    Prev4Hour,

    /// <summary>Previous 8 hours.</summary>
    Prev8Hour,

    /// <summary>Previous 12 hours.</summary>
    Prev12Hour,

    /// <summary>Previous 24 hours.</summary>
    Prev24Hour,

    /// <summary>Next 1 minute.</summary>
    Next1Min,

    /// <summary>Next 2 minutes.</summary>
    Next2Min,

    /// <summary>Next 5 minutes.</summary>
    Next5Min,

    /// <summary>Next 15 minutes.</summary>
    Next15Min,

    /// <summary>Next 30 minutes.</summary>
    Next30Min,

    /// <summary>Next 1 hour.</summary>
    Next1Hour,

    /// <summary>Next 2 hours.</summary>
    Next2Hour,

    /// <summary>Next 4 hours.</summary>
    Next4Hour,

    /// <summary>Next 8 hours.</summary>
    Next8Hour,

    /// <summary>Next 12 hours.</summary>
    Next12Hour,

    /// <summary>Next 24 hours.</summary>
    Next24Hour,

    /// <summary>The current day from midnight to midnight.</summary>
    Today,

    /// <summary>The previous day from midnight to midnight.</summary>
    Yesterday,

    /// <summary>The next day from midnight to midnight.</summary>
    Tommorrow,

    /// <summary>The current week from the start-of-week day.</summary>
    ThisWeek,

    /// <summary>The previous week.</summary>
    LastWeek,

    /// <summary>The next week.</summary>
    NextWeek,

    /// <summary>The current calendar month.</summary>
    ThisMonth,

    /// <summary>The previous calendar month.</summary>
    LastMonth,

    /// <summary>The next calendar month.</summary>
    NextMonth,

    /// <summary>The current calendar year.</summary>
    ThisYear,

    /// <summary>The previous calendar year.</summary>
    LastYear,

    /// <summary>The next calendar year.</summary>
    NextYear
}
