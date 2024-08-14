using RobotAppLibrary.Modeles;
using RobotAppLibrary.Utils;

namespace RobotAppLibrary.Tests;

public static class TestUtils
{
    public static List<Candle> GenerateCandle(Timeframe timeframe, int nombre = 1000, DateTime? start = null)
    {
        var interval = TimeSpan.FromMinutes(timeframe.GetMinuteFromTimeframe());
        var random = new Random();
        var candles = new List<Candle>();
        var dateDebut = start ?? new DateTime(2021, 1, 1, 0, 0, 0);

        if (timeframe == Timeframe.Weekly)
        {
            var daysUntilMonday = (int)DayOfWeek.Monday - (int)dateDebut.DayOfWeek;
            if (daysUntilMonday > 0) daysUntilMonday -= 7;

            dateDebut = dateDebut.AddDays(daysUntilMonday);
        }

        if (timeframe == Timeframe.Daily || timeframe < Timeframe.Daily) dateDebut = dateDebut.AddDays(1);
        if (timeframe < Timeframe.FourHour) dateDebut = dateDebut.AddHours(1);

        for (var i = 0; i < nombre; i++)
        {
            var open = (decimal)random.NextDouble() * 100;
            var close = (decimal)random.NextDouble() * 100;
            var candle = new Candle()
            {
                Open = open,
                Close = close,
                High = Math.Max(open, close) + (decimal)(random.NextDouble() * 100),
                Low = Math.Max(open, close) + (decimal)(random.NextDouble() * 100),
                Date = dateDebut
            };

            candles.Add(candle);

            do
            {
                if (start is not null)
                    dateDebut += interval;
                else
                    dateDebut -= interval;
            } while (dateDebut.DayOfWeek == DayOfWeek.Saturday || dateDebut.DayOfWeek == DayOfWeek.Sunday);
        }

        return candles.Distinct().OrderBy(candle => candle.Date).ToList();
    }
    
    public static List<Position> GeneratePositions(DateTime startDate)
    {
        var random = new Random();
        var positions = new List<Position>();
        DateTime endDate = startDate.AddMonths(6);

        for (DateTime date = startDate; date < endDate; date = date.AddDays(1))
        {
            positions.Add(new Position
            {
                Profit = random.Next(-100, 101), 
                DateClose = date
            });
        }

        return positions;
    }

}