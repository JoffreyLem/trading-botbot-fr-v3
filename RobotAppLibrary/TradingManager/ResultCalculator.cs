using RobotAppLibrary.Modeles;
using RobotAppLibrary.TradingManager.Exceptions;

namespace RobotAppLibrary.TradingManager;

public class ResultCalculator
{
    public Result CalculateResults(List<Position> positions)
    {
        try
        {
            var result = new Result();

            if (positions.Exists(x => x.Profit > 0))
            {
                result.GainMax = positions.Max(x => x.Profit);
                result.ProfitPositif = positions.Where(x => x.Profit > 0).Sum(x => x.Profit);
                result.TotalPositionPositive = positions.Count(x => x.Profit > 0);
                result.MoyennePositive = positions.Where(x => x.Profit > 0).Average(x => x.Profit);
            }

            if (positions.Exists(x => x.Profit < 0))
            {
                result.PerteMax = positions.Min(x => x.Profit);
                result.ProfitNegatif = positions.Where(x => x.Profit < 0).Sum(x => x.Profit);
                result.TotalPositionNegative = positions.Count(x => x.Profit < 0);
                result.MoyenneNegative = positions.Where(x => x.Profit < 0).Average(x => x.Profit);
            }

            result.Profit = positions.Sum(x => x.Profit);
            result.TotalPositions = positions.Count;
            result.MoyenneProfit = positions.Count > 0 ? positions.Average(x => x.Profit) : 0;
            if (result.MoyenneNegative != 0)
                result.RatioMoyennePositifNegatif =
                    result.MoyennePositive / result.MoyenneNegative;


            if (result.ProfitNegatif != 0)
                result.ProfitFactor = Math.Abs(result.ProfitPositif / result.ProfitNegatif);

            if (result.TotalPositions != 0)
                result.TauxReussite =
                    result.TotalPositionPositive / result.TotalPositions * 100;

            CalculateDrawdowns(positions, result);

            return result;
        }
        catch (Exception e)
        {
            throw new ResultException("Error on calculating result", e);
        }
    }

    private void CalculateDrawdowns(List<Position> positions, Result result)
    {
        if (positions.Count > 1 && positions.Any(p => p.Profit < 0))
        {
            var orderedPositions = positions.OrderBy(x => x.DateClose).ToList();
            var peakValue = orderedPositions[0].Profit;
            decimal drawdownMax = 0;
            decimal lastDrawdown = 0;

            foreach (var position in orderedPositions)
            {
                var profit = position.Profit;
                if (profit > peakValue) peakValue = profit;

                var drawdown = peakValue - profit;

                lastDrawdown = drawdown;
                if (drawdown > drawdownMax) drawdownMax = drawdown;
            }

            result.Drawndown = lastDrawdown;
            result.DrawndownMax = drawdownMax;
        }
        else
        {
            result.Drawndown = 0;
            result.DrawndownMax = 0;
        }
    }
}