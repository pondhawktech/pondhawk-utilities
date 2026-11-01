// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Pondhawk.Utilities.Types;

/// <summary>
/// Default implementation of <see cref="IDateTimeRange"/> that calculates begin/end from a <see cref="DateTimeRange"/> kind.
/// </summary>
public class DateTimeRangeModel : IDateTimeRange
{


    /// <summary>
    /// Gets or sets the unique identifier for this date/time range model.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the human-readable label for this date/time range (e.g. "Last 1 Hour").
    /// </summary>
    public string Label { get; set; } = "Today";

    /// <summary>
    /// Gets or sets the <see cref="DateTimeRange"/> kind that defines this range.
    /// </summary>
    public DateTimeRange RangeKind { get; set; } = DateTimeRange.Today;

    /// <summary>
    /// Gets the calculated begin date/time for this range.
    /// </summary>
    public DateTime Begin => DateTimeHelpers.CalculateRange(RangeKind).begin;

    /// <summary>
    /// Gets the calculated end date/time for this range.
    /// </summary>
    public DateTime End => DateTimeHelpers.CalculateRange(RangeKind).end;


    int IDateTimeRange.Id => Id;
    string IDateTimeRange.Label => Label;
    DateTime IDateTimeRange.Begin => Begin;
    long IDateTimeRange.BeginTimestamp => (long)(Begin.ToUniversalTime() - DateTimeHelpers.Epoch).TotalSeconds;

    DateTime IDateTimeRange.End => End;
    long IDateTimeRange.EndTimestamp => (long)(End.ToUniversalTime() - DateTimeHelpers.Epoch).TotalSeconds;

}
