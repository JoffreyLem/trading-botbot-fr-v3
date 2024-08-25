using RobotAppLibrary.Modeles;

namespace RobotAppLibrary.Api.Interfaces;

public interface IStreamingEvent
{
    public event Action<Tick> TickRecordReceived;
    public event Action<Position> TradeRecordReceived;
    public event Action<AccountBalance> BalanceRecordReceived;
    public event Action<News> NewsRecordReceived;
    public event Action KeepAliveRecordReceived;
    public event Action<Candle> CandleRecordReceived;
}