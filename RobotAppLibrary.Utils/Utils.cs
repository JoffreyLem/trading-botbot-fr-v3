using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using RobotAppLibrary.Modeles;
using Skender.Stock.Indicators;

namespace RobotAppLibrary.Utils;

public static class Utils
{
    [ExcludeFromCodeCoverage]
    public static string? GetEnumDescription(this Enum? enumVal)
    {
        var memInfo = enumVal?.GetType().GetMember(enumVal.ToString());
        var attribute = (memInfo?[0] ?? throw new InvalidOperationException())
            .GetCustomAttribute<DescriptionAttribute>();
        return attribute?.Description;
    }

    /// <summary>
    ///     Return utc date time
    /// </summary>
    /// <param name="timestamp">timestamp in millisecondes</param>
    /// <returns>The datetime utc</returns>
    public static DateTime ConvertToDatetime(this long timestamp)
    {
        var dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(timestamp);
        return dateTimeOffset.UtcDateTime;
    }


    public static long ConvertToUnixTime(this DateTime dateTime)
    {
        var datetimeOFfset = new DateTimeOffset(dateTime);
        return datetimeOFfset.ToUnixTimeMilliseconds();
    }


    public static DayOfWeek GetDay(long day)
    {
        return day switch
        {
            1 => DayOfWeek.Monday,
            2 => DayOfWeek.Tuesday,
            3 => DayOfWeek.Wednesday,
            4 => DayOfWeek.Thursday,
            5 => DayOfWeek.Friday,
            6 => DayOfWeek.Saturday,
            7 => DayOfWeek.Sunday,
            _ => throw new Exception()
        };
    }


    public static int GetMinuteFromTimeframe(this Timeframe timeFrame)
    {
        return timeFrame switch
        {
            Timeframe.OneMinute => 1,
            Timeframe.FiveMinutes => 5,
            Timeframe.FifteenMinutes => 15,
            Timeframe.ThirtyMinutes => 30,
            Timeframe.OneHour => 60,
            Timeframe.FourHour => 240,
            Timeframe.Daily => 1440,
            Timeframe.Weekly => 10080,
            Timeframe.Monthly => 43800,
            _ => throw new Exception()
        };
    }


    public static PeriodSize ToPeriodSize(this Timeframe timeframe)
    {
        return timeframe switch
        {
            Timeframe.OneMinute => PeriodSize.OneMinute,
            Timeframe.FiveMinutes => PeriodSize.FiveMinutes,
            Timeframe.FifteenMinutes => PeriodSize.FifteenMinutes,
            Timeframe.ThirtyMinutes => PeriodSize.ThirtyMinutes,
            Timeframe.OneHour => PeriodSize.OneHour,
            Timeframe.FourHour => PeriodSize.FourHours,
            Timeframe.Daily => PeriodSize.Day,
            Timeframe.Weekly => PeriodSize.Week,
            Timeframe.Monthly => PeriodSize.Month,
            _ => throw new ArgumentException("Invalid Timeframe")
        };
    }
}