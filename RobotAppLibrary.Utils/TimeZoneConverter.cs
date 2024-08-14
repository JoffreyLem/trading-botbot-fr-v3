using System.Runtime.InteropServices;

namespace RobotAppLibrary.Utils;

public static class TimeZoneConverter
{
    private static readonly string CentralEuropeanTimeZoneId =
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Romance Standard Time" : "Europe/Paris";

    public static DateTime ConvertCetCestToUtc(DateTime cetDateTime)
    {
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(CentralEuropeanTimeZoneId);
        return TimeZoneInfo.ConvertTimeToUtc(cetDateTime, timeZone);
    }

    public static DateTime ConvertMillisecondsToUtc(long milliseconds)
    {
        return DateTimeOffset.FromUnixTimeMilliseconds(milliseconds).UtcDateTime;
    }

    public static DateTime ConvertMidnightCetCestMillisecondsToUtc(long milliseconds)
    {
        // Convertir les millisecondes en DateTime
        var cetDateTime = DateTimeOffset.FromUnixTimeMilliseconds(milliseconds).DateTime;

        // Ajuster à minuit dans le fuseau horaire CET/CEST
        var midnightCetDateTime = new DateTime(cetDateTime.Year, cetDateTime.Month, cetDateTime.Day, 0, 0, 0);
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(CentralEuropeanTimeZoneId);

        // Convertir en UTC
        return TimeZoneInfo.ConvertTimeToUtc(midnightCetDateTime, timeZone);
    }

    public static TimeSpan ConvertMidnightCetCestMillisecondsToUtcOffset(long milliseconds)
    {
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(CentralEuropeanTimeZoneId);

        // Calculer l'heure et la date en CET/CEST à partir des millisecondes depuis minuit
        var timeSinceMidnightCetCest = TimeSpan.FromMilliseconds(milliseconds);
        var today = DateTime.UtcNow.Date;
        var cetCestDateTime = new DateTime(today.Year, today.Month, today.Day).Add(timeSinceMidnightCetCest);

        // Convertir cette heure et cette date en heure UTC
        var utcDateTime = TimeZoneInfo.ConvertTimeToUtc(cetCestDateTime, timeZone);

        // Obtenir le TimeSpan depuis minuit UTC
        return utcDateTime.TimeOfDay;
    }
}