using RobotAppLibrary.Modeles;
using RobotAppLibrary.TradingManager.Interfaces;

namespace RobotAppLibrary.Backtest;

public class PositionHandlerBacktest : IPositionHandlerBase
{
    public int DefaultSl { get; set; }
    public int DefaultTp { get; set; }
    public Position? PositionOpened { get; }
    public bool PositionInProgress { get; }

    public Task OpenPositionAsync(string symbol, TypeOperation typePosition, double volume,
        decimal sl = 0M,
        decimal tp = 0M, double? risk = null, long? expiration = 0L)
    {
        throw new NotImplementedException();
    }

    public Task UpdatePositionAsync(Position position)
    {
        throw new NotImplementedException();
    }

    public Task ClosePositionAsync(Position position)
    {
        throw new NotImplementedException();
    }

    public decimal CalculateStopLoss(decimal pips, TypeOperation positionType)
    {
        throw new NotImplementedException();
    }

    public decimal CalculateTakeProfit(decimal pips, TypeOperation positionType)
    {
        throw new NotImplementedException();
    }
}