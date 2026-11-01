// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace Pondhawk.Utilities.Types
{


    /// <summary>
    /// Represents a named date/time range with begin and end boundaries and Unix timestamps.
    /// </summary>
    [SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "End is a domain property name for date range boundaries")]
    public interface IDateTimeRange
    {

        /// <summary>
        /// Gets the unique identifier for this date/time range.
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Gets the <see cref="DateTimeRange"/> kind that defines this range.
        /// </summary>
        DateTimeRange RangeKind { get; }

        /// <summary>
        /// Gets the human-readable label for this date/time range.
        /// </summary>
        string Label { get; }

        /// <summary>
        /// Gets the begin date/time of this range.
        /// </summary>
        DateTime Begin { get; }

        /// <summary>
        /// Gets the begin date/time as a Unix timestamp in seconds.
        /// </summary>
        long BeginTimestamp { get; }

        /// <summary>
        /// Gets the end date/time of this range.
        /// </summary>
        DateTime End { get; }

        /// <summary>
        /// Gets the end date/time as a Unix timestamp in seconds.
        /// </summary>
        long EndTimestamp { get; }
    }


}
