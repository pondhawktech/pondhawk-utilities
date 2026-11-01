// Copyright (c) Pond Hawk Technologies Inc. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Pondhawk.Utilities.Types;
using Shouldly;
using Xunit;

namespace Pondhawk.Utilities.Tests.Types;

public class DateTimeHelpersTests
{

    // ── Static collections ──

    [Fact]
    public void PastModels_HasExpectedCount()
    {
        DateTimeHelpers.PastModels.Count.ShouldBe(19);
    }

    [Fact]
    public void FutureModels_HasExpectedCount()
    {
        DateTimeHelpers.FutureModels.Count.ShouldBe(19);
    }

    [Fact]
    public void AllModels_IsPastPlusFuture()
    {
        DateTimeHelpers.AllModels.Count.ShouldBe(38);
    }

    [Fact]
    public void RecentModels_IsFirst13PastModels()
    {
        DateTimeHelpers.RecentModels.Count.ShouldBe(13);
    }

    [Fact]
    public void Epoch_IsJan1_1970_Utc()
    {
        DateTimeHelpers.Epoch.ShouldBe(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    }

    // ── Timestamp conversions ──

    [Fact]
    public void ToTimestamp_KnownDate_ReturnsExpected()
    {
        var date = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var ts = DateTimeHelpers.ToTimestamp(date);
        ts.ShouldBe(1704067200);
    }

    [Fact]
    public void FromTimestamp_RoundTrips()
    {
        var original = new DateTime(2024, 6, 15, 12, 30, 0, DateTimeKind.Local);
        var ts = DateTimeHelpers.ToTimestamp(original);
        var roundTripped = DateTimeHelpers.FromTimestamp(ts);

        roundTripped.Year.ShouldBe(original.Year);
        roundTripped.Month.ShouldBe(original.Month);
        roundTripped.Day.ShouldBe(original.Day);
        roundTripped.Hour.ShouldBe(original.Hour);
        roundTripped.Minute.ShouldBe(original.Minute);
    }

    [Fact]
    public void ToTimestamp_Default_UsesNow()
    {
        var before = DateTimeHelpers.ToTimestamp(DateTime.UtcNow.AddSeconds(-1));
        var ts = DateTimeHelpers.ToTimestamp(default);
        var after = DateTimeHelpers.ToTimestamp(DateTime.UtcNow.AddSeconds(1));

        ts.ShouldBeGreaterThanOrEqualTo(before);
        ts.ShouldBeLessThanOrEqualTo(after);
    }

    [Fact]
    public void ToTimestampMilli_KnownDate_ReturnsExpected()
    {
        var date = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var ts = DateTimeHelpers.ToTimestampMilli(date);
        ts.ShouldBe(1704067200000);
    }

    [Fact]
    public void FromTimestampMilli_RoundTrips()
    {
        var original = new DateTime(2024, 6, 15, 12, 30, 0, DateTimeKind.Local);
        var ts = DateTimeHelpers.ToTimestampMilli(original);
        var roundTripped = DateTimeHelpers.FromTimestampMilli(ts);

        roundTripped.Year.ShouldBe(original.Year);
        roundTripped.Month.ShouldBe(original.Month);
        roundTripped.Day.ShouldBe(original.Day);
        roundTripped.Hour.ShouldBe(original.Hour);
        roundTripped.Minute.ShouldBe(original.Minute);
    }

    [Fact]
    public void ToTimestampMilli_Default_UsesNow()
    {
        var before = DateTimeHelpers.ToTimestampMilli(DateTime.UtcNow.AddSeconds(-1));
        var ts = DateTimeHelpers.ToTimestampMilli();
        var after = DateTimeHelpers.ToTimestampMilli(DateTime.UtcNow.AddSeconds(1));

        ts.ShouldBeGreaterThanOrEqualTo(before);
        ts.ShouldBeLessThanOrEqualTo(after);
    }

    // ── StartOfWeek ──

    [Fact]
    public void StartOfWeek_Sunday_ReturnsCorrectDate()
    {
        // Wednesday June 19, 2024
        var wed = new DateTime(2024, 6, 19);
        var start = wed.StartOfWeek(DayOfWeek.Sunday);
        start.ShouldBe(new DateTime(2024, 6, 16)); // Sunday
    }

    [Fact]
    public void StartOfWeek_Monday_ReturnsCorrectDate()
    {
        var wed = new DateTime(2024, 6, 19);
        var start = wed.StartOfWeek(DayOfWeek.Monday);
        start.ShouldBe(new DateTime(2024, 6, 17)); // Monday
    }

    [Fact]
    public void StartOfWeek_OnStartDay_ReturnsSameDate()
    {
        var sun = new DateTime(2024, 6, 16); // Sunday
        var start = sun.StartOfWeek(DayOfWeek.Sunday);
        start.ShouldBe(new DateTime(2024, 6, 16));
    }

    // ── StartOfMonth ──

    [Fact]
    public void StartOfMonth_MidMonth_ReturnsFirstDay()
    {
        var date = new DateTime(2024, 6, 15, 14, 30, 0);
        var start = date.StartOfMonth();
        start.ShouldBe(new DateTime(2024, 6, 1));
    }

    [Fact]
    public void StartOfMonth_FirstDay_ReturnsSameDate()
    {
        var date = new DateTime(2024, 6, 1);
        var start = date.StartOfMonth();
        start.ShouldBe(new DateTime(2024, 6, 1));
    }

    // ── CalculateRange ──

    private static readonly DateTime Origin = new(2024, 6, 15, 10, 30, 0);

    [Theory]
    [InlineData(DateTimeRange.Prev1Min, -1, 0)]
    [InlineData(DateTimeRange.Prev5Min, -5, 0)]
    [InlineData(DateTimeRange.Prev15Min, -15, 0)]
    [InlineData(DateTimeRange.Prev30Min, -30, 0)]
    public void CalculateRange_PrevMinutes_ReturnsCorrectSpan(DateTimeRange range, int beginOffsetMin, int endOffsetMin)
    {
        var expected = new DateTime(2024, 6, 15, 10, 30, 0); // origin truncated to minute
        var (begin, end) = DateTimeHelpers.CalculateRange(range, Origin);

        begin.ShouldBe(expected.AddMinutes(beginOffsetMin));
        end.ShouldBe(expected.AddMinutes(endOffsetMin));
    }

    [Theory]
    [InlineData(DateTimeRange.Prev1Hour, -1)]
    [InlineData(DateTimeRange.Prev2Hour, -2)]
    [InlineData(DateTimeRange.Prev4Hour, -4)]
    [InlineData(DateTimeRange.Prev8Hour, -8)]
    [InlineData(DateTimeRange.Prev12Hour, -12)]
    [InlineData(DateTimeRange.Prev24Hour, -24)]
    public void CalculateRange_PrevHours_ReturnsCorrectSpan(DateTimeRange range, int beginOffsetHours)
    {
        var expected = new DateTime(2024, 6, 15, 10, 30, 0);
        var (begin, end) = DateTimeHelpers.CalculateRange(range, Origin);

        begin.ShouldBe(expected.AddHours(beginOffsetHours));
        end.ShouldBe(expected);
    }

    [Theory]
    [InlineData(DateTimeRange.Next1Min, 0, 1)]
    [InlineData(DateTimeRange.Next5Min, 0, 5)]
    [InlineData(DateTimeRange.Next15Min, 0, 15)]
    [InlineData(DateTimeRange.Next30Min, 0, 30)]
    public void CalculateRange_NextMinutes_ReturnsCorrectSpan(DateTimeRange range, int beginOffsetMin, int endOffsetMin)
    {
        var expected = new DateTime(2024, 6, 15, 10, 30, 0);
        var (begin, end) = DateTimeHelpers.CalculateRange(range, Origin);

        begin.ShouldBe(expected.AddMinutes(beginOffsetMin));
        end.ShouldBe(expected.AddMinutes(endOffsetMin));
    }

    [Theory]
    [InlineData(DateTimeRange.Next1Hour, 1)]
    [InlineData(DateTimeRange.Next2Hour, 2)]
    [InlineData(DateTimeRange.Next4Hour, 4)]
    [InlineData(DateTimeRange.Next8Hour, 8)]
    [InlineData(DateTimeRange.Next12Hour, 12)]
    [InlineData(DateTimeRange.Next24Hour, 24)]
    public void CalculateRange_NextHours_ReturnsCorrectSpan(DateTimeRange range, int endOffsetHours)
    {
        var expected = new DateTime(2024, 6, 15, 10, 30, 0);
        var (begin, end) = DateTimeHelpers.CalculateRange(range, Origin);

        begin.ShouldBe(expected);
        end.ShouldBe(expected.AddHours(endOffsetHours));
    }

    [Fact]
    public void CalculateRange_Today_ReturnsMidnightToMidnight()
    {
        var (begin, end) = DateTimeHelpers.CalculateRange(DateTimeRange.Today, Origin);

        begin.ShouldBe(new DateTime(2024, 6, 15));
        end.ShouldBe(new DateTime(2024, 6, 16));
    }

    [Fact]
    public void CalculateRange_Yesterday_ReturnsPreviousDay()
    {
        var (begin, end) = DateTimeHelpers.CalculateRange(DateTimeRange.Yesterday, Origin);

        begin.ShouldBe(new DateTime(2024, 6, 14));
        end.ShouldBe(new DateTime(2024, 6, 15));
    }

    [Fact]
    public void CalculateRange_Tomorrow_ReturnsNextDay()
    {
        var (begin, end) = DateTimeHelpers.CalculateRange(DateTimeRange.Tommorrow, Origin);

        begin.ShouldBe(new DateTime(2024, 6, 16));
        end.ShouldBe(new DateTime(2024, 6, 17));
    }

    [Fact]
    public void CalculateRange_ThisWeek_ReturnsSundayToSunday()
    {
        // June 15, 2024 is Saturday; week starts Sunday June 9
        var (begin, end) = DateTimeHelpers.CalculateRange(DateTimeRange.ThisWeek, Origin);

        begin.ShouldBe(new DateTime(2024, 6, 9));
        end.ShouldBe(new DateTime(2024, 6, 16));
    }

    [Fact]
    public void CalculateRange_LastWeek_ReturnsPreviousWeek()
    {
        var (begin, end) = DateTimeHelpers.CalculateRange(DateTimeRange.LastWeek, Origin);

        begin.ShouldBe(new DateTime(2024, 6, 2));
        end.ShouldBe(new DateTime(2024, 6, 9));
    }

    [Fact]
    public void CalculateRange_NextWeek_ReturnsFollowingWeek()
    {
        var (begin, end) = DateTimeHelpers.CalculateRange(DateTimeRange.NextWeek, Origin);

        begin.ShouldBe(new DateTime(2024, 6, 16));
        end.ShouldBe(new DateTime(2024, 6, 23));
    }

    [Fact]
    public void CalculateRange_ThisMonth_ReturnsJuneRange()
    {
        var (begin, end) = DateTimeHelpers.CalculateRange(DateTimeRange.ThisMonth, Origin);

        begin.ShouldBe(new DateTime(2024, 6, 1));
        end.ShouldBe(new DateTime(2024, 7, 1));
    }

    [Fact]
    public void CalculateRange_LastMonth_ReturnsMayRange()
    {
        // Use 1st-of-month origin; impl subtracts 1 day then takes StartOfMonth
        var firstOfJune = new DateTime(2024, 6, 1, 10, 30, 0);
        var (begin, end) = DateTimeHelpers.CalculateRange(DateTimeRange.LastMonth, firstOfJune);

        begin.ShouldBe(new DateTime(2024, 5, 1));
        end.ShouldBe(new DateTime(2024, 6, 1));
    }

    [Fact]
    public void CalculateRange_NextMonth_ReturnsJulyRange()
    {
        var (begin, end) = DateTimeHelpers.CalculateRange(DateTimeRange.NextMonth, Origin);

        begin.ShouldBe(new DateTime(2024, 7, 1));
        end.ShouldBe(new DateTime(2024, 8, 1));
    }

    [Fact]
    public void CalculateRange_ThisYear_Returns2024()
    {
        var (begin, end) = DateTimeHelpers.CalculateRange(DateTimeRange.ThisYear, Origin);

        begin.ShouldBe(new DateTime(2024, 1, 1));
        end.ShouldBe(new DateTime(2025, 1, 1));
    }

    [Fact]
    public void CalculateRange_LastYear_Returns2023()
    {
        var (begin, end) = DateTimeHelpers.CalculateRange(DateTimeRange.LastYear, Origin);

        begin.ShouldBe(new DateTime(2023, 1, 1));
        end.ShouldBe(new DateTime(2024, 1, 1));
    }

    [Fact]
    public void CalculateRange_NextYear_Returns2025()
    {
        var (begin, end) = DateTimeHelpers.CalculateRange(DateTimeRange.NextYear, Origin);

        begin.ShouldBe(new DateTime(2025, 1, 1));
        end.ShouldBe(new DateTime(2026, 1, 1));
    }

    [Fact]
    public void CalculateRange_DefaultOrigin_UsesNow()
    {
        var (begin, end) = DateTimeHelpers.CalculateRange(DateTimeRange.Today);

        begin.ShouldBe(DateTime.Now.Date);
        end.ShouldBe(DateTime.Now.Date.AddDays(1));
    }

    [Fact]
    public void CalculateRange_InvalidEnum_Throws()
    {
        Should.Throw<ArgumentOutOfRangeException>(
            () => DateTimeHelpers.CalculateRange((DateTimeRange)999));
    }

    // ── CalculateRange by ID ──

    [Fact]
    public void CalculateRange_ById_ReturnsCorrectRange()
    {
        // ID 1 = PastModels[0] = Prev1Min
        var (begin, end) = DateTimeHelpers.CalculateRange(1, Origin);
        var expected = new DateTime(2024, 6, 15, 10, 30, 0);

        begin.ShouldBe(expected.AddMinutes(-1));
        end.ShouldBe(expected);
    }

    [Fact]
    public void CalculateRange_ById_ZeroId_Throws()
    {
        Should.Throw<ArgumentOutOfRangeException>(
            () => DateTimeHelpers.CalculateRange(0));
    }

    [Fact]
    public void CalculateRange_ById_TooLargeId_Throws()
    {
        Should.Throw<ArgumentOutOfRangeException>(
            () => DateTimeHelpers.CalculateRange(999));
    }

    // ── CalculateTimestamps ──

    [Fact]
    public void CalculateTimestamps_ByRange_ReturnsUnixSeconds()
    {
        var (beginTs, endTs) = DateTimeHelpers.CalculateTimestamps(DateTimeRange.Today, Origin);
        var (beginDt, endDt) = DateTimeHelpers.CalculateRange(DateTimeRange.Today, Origin);

        beginTs.ShouldBe(DateTimeHelpers.ToTimestamp(beginDt));
        endTs.ShouldBe(DateTimeHelpers.ToTimestamp(endDt));
    }

    [Fact]
    public void CalculateTimestamps_ById_ReturnsUnixSeconds()
    {
        var (beginTs, endTs) = DateTimeHelpers.CalculateTimestamps(1, Origin);
        var (beginDt, endDt) = DateTimeHelpers.CalculateRange(1, Origin);

        beginTs.ShouldBe(DateTimeHelpers.ToTimestamp(beginDt));
        endTs.ShouldBe(DateTimeHelpers.ToTimestamp(endDt));
    }

    [Fact]
    public void CalculateTimestamps_ById_InvalidId_Throws()
    {
        Should.Throw<ArgumentOutOfRangeException>(
            () => DateTimeHelpers.CalculateTimestamps(0));
    }

    // ── From ──

    [Fact]
    public void From_PastRange_ReturnsModel()
    {
        var model = DateTimeHelpers.From(DateTimeRange.Prev1Hour);

        model.ShouldNotBeNull();
        model.RangeKind.ShouldBe(DateTimeRange.Prev1Hour);
        model.Label.ShouldBe("Last 1 Hour");
    }

    [Fact]
    public void From_FutureRange_ReturnsModel()
    {
        var model = DateTimeHelpers.From(DateTimeRange.Next1Hour);

        model.ShouldNotBeNull();
        model.RangeKind.ShouldBe(DateTimeRange.Next1Hour);
    }

    // ── DateTimeRangeModel ──

    [Fact]
    public void DateTimeRangeModel_Begin_CalculatesFromRangeKind()
    {
        var model = new DateTimeRangeModel { RangeKind = DateTimeRange.Today };

        model.Begin.ShouldBe(DateTime.Now.Date);
    }

    [Fact]
    public void DateTimeRangeModel_End_CalculatesFromRangeKind()
    {
        var model = new DateTimeRangeModel { RangeKind = DateTimeRange.Today };

        model.End.ShouldBe(DateTime.Now.Date.AddDays(1));
    }

    [Fact]
    public void DateTimeRangeModel_InterfaceTimestamps_AreConsistent()
    {
        var model = new DateTimeRangeModel { Id = 1, Label = "Today", RangeKind = DateTimeRange.Today };
        IDateTimeRange iface = model;

        iface.Id.ShouldBe(1);
        iface.Label.ShouldBe("Today");
        iface.BeginTimestamp.ShouldBe(DateTimeHelpers.ToTimestamp(model.Begin));
        iface.EndTimestamp.ShouldBe(DateTimeHelpers.ToTimestamp(model.End));
    }

    [Fact]
    public void DateTimeRangeModel_Defaults_AreToday()
    {
        var model = new DateTimeRangeModel();

        model.Label.ShouldBe("Today");
        model.RangeKind.ShouldBe(DateTimeRange.Today);
    }

    // ── Prev2Min (cover the remaining inline) ──

    [Fact]
    public void CalculateRange_Prev2Min()
    {
        var expected = new DateTime(2024, 6, 15, 10, 30, 0);
        var (begin, end) = DateTimeHelpers.CalculateRange(DateTimeRange.Prev2Min, Origin);

        begin.ShouldBe(expected.AddMinutes(-2));
        end.ShouldBe(expected);
    }

    [Fact]
    public void CalculateRange_Next2Min()
    {
        var expected = new DateTime(2024, 6, 15, 10, 30, 0);
        var (begin, end) = DateTimeHelpers.CalculateRange(DateTimeRange.Next2Min, Origin);

        begin.ShouldBe(expected);
        end.ShouldBe(expected.AddMinutes(2));
    }

}
