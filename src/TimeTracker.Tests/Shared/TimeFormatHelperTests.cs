using TimeTracker.Web.Shared;

namespace TimeTracker.Tests.Shared;

public class TimeFormatHelperTests
{
    [Fact]
    public void FormatMinutes_Null_ReturnsDash() =>
        Assert.Equal("—", TimeFormatHelper.FormatMinutes(null));

    [Fact]
    public void FormatMinutes_Zero_ReturnsZeroMinutes() =>
        Assert.Equal("0m", TimeFormatHelper.FormatMinutes(0));

    [Fact]
    public void FormatMinutes_Under60_ReturnsMinutesOnly()
    {
        Assert.Equal("1m", TimeFormatHelper.FormatMinutes(1));
        Assert.Equal("42m", TimeFormatHelper.FormatMinutes(42));
        Assert.Equal("59m", TimeFormatHelper.FormatMinutes(59));
    }

    [Fact]
    public void FormatMinutes_Exactly60_ReturnsOneHourZeroMinutes() =>
        Assert.Equal("1h 00m", TimeFormatHelper.FormatMinutes(60));

    [Fact]
    public void FormatMinutes_Over60_ReturnsHoursAndPaddedMinutes()
    {
        Assert.Equal("1h 02m", TimeFormatHelper.FormatMinutes(62));
        Assert.Equal("1h 30m", TimeFormatHelper.FormatMinutes(90));
        Assert.Equal("2h 05m", TimeFormatHelper.FormatMinutes(125));
    }

    [Fact]
    public void FormatMinutes_LargeValue_ReturnsCorrectBreakdown() =>
        Assert.Equal("8h 15m", TimeFormatHelper.FormatMinutes(495));

    [Fact]
    public void FormatMinutes_NonNullableOverload_MatchesNullableOverload()
    {
        Assert.Equal(TimeFormatHelper.FormatMinutes((int?)42), TimeFormatHelper.FormatMinutes(42));
        Assert.Equal(TimeFormatHelper.FormatMinutes((int?)90), TimeFormatHelper.FormatMinutes(90));
    }
}
