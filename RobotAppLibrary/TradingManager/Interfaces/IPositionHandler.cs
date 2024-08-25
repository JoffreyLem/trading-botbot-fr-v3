using RobotAppLibrary.Modeles;

namespace RobotAppLibrary.TradingManager.Interfaces;

public interface IPositionHandlerBase
{
    int DefaultSl { get; set; }
    int DefaultTp { get; set; }
    Position PositionOpened { get; }
    bool PositionInProgress { get; }


    Task OpenPositionAsync(string symbol, TypeOperation typePosition, double volume,
        decimal sl = 0M,
        decimal tp = 0M, double? risk = null, long? expiration = 0L);

    Task UpdatePositionAsync(Position position);
    Task ClosePositionAsync(Position position);
    decimal CalculateStopLoss(decimal pips, TypeOperation positionType);
    decimal CalculateTakeProfit(decimal pips, TypeOperation positionType);
}

public interface IPositionHandlerEvent
{
    event EventHandler<Position>? PositionOpenedEvent;
    event EventHandler<Position>? PositionUpdatedEvent;
    event EventHandler<Position>? PositionRejectedEvent;
    event EventHandler<Position>? PositionClosedEvent;
}

public interface IPositionHandler : IPositionHandlerBase, IPositionHandlerEvent, IDisposable
{
}