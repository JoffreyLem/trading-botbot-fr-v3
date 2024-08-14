using RobotAppLibrary.Api.Modeles;
using RobotAppLibrary.Modeles;

namespace RobotAppLibrary.Api.Interfaces;

public interface ICommandExecutor : IStreamingEvent, IConnectionEvent, IDisposable
{
    Task ExecuteLoginCommand(Credentials credentials);
    Task ExecuteLogoutCommand();
    Task<List<SymbolInfo>> ExecuteAllSymbolsCommand();
    Task<List<CalendarEvent>> ExecuteCalendarCommand();
    Task<List<Candle>> ExecuteFullChartCommand(Timeframe timeframe, DateTime start, string symbol);
    Task<List<Candle>> ExecuteRangeChartCommand(Timeframe timeframe, DateTime start, DateTime end, string symbol);
    Task<AccountBalance> ExecuteBalanceAccountCommand();
    Task<List<News>> ExecuteNewsCommand(DateTime? start, DateTime? end);
    Task<string> ExecuteCurrentUserDataCommand();
    Task<bool> ExecutePingCommand();
    Task<SymbolInfo> ExecuteSymbolCommand(string symbol);
    Task<Tick> ExecuteTickCommand(string symbol);
    Task<List<Position>?> ExecuteTradesHistoryCommand(string positionReference);
    Task<Position?> ExecuteTradesOpenedTradesCommand(string positionReference);
    Task<TradeHourRecord> ExecuteTradingHoursCommand(string symbol);
    Task<Position> ExecuteOpenTradeCommand(Position position, decimal price);
    Task<Position> ExecuteUpdateTradeCommand(Position position, decimal price);
    Task<Position> ExecuteCloseTradeCommand(Position position, decimal price);
    bool ExecuteIsConnected();
    void ExecuteSubscribeBalanceCommandStreaming();
    void ExecuteStopBalanceCommandStreaming();
    void ExecuteSubscribeCandleCommandStreaming(string symbol);
    void ExecuteStopCandleCommandStreaming(string symbol);
    void ExecuteSubscribeKeepAliveCommandStreaming();
    void ExecuteStopKeepAliveCommandStreaming();
    void ExecuteSubscribeNewsCommandStreaming();
    void ExecuteStopNewsCommandStreaming();
    void ExecuteSubscribeProfitsCommandStreaming();
    void ExecuteStopProfitsCommandStreaming();
    void ExecuteTickPricesCommandStreaming(string symbol);
    void ExecuteStopTickPriceCommandStreaming(string symbol);
    void ExecuteTradesCommandStreaming();
    void ExecuteStopTradesCommandStreaming();
    void ExecuteTradeStatusCommandStreaming();
    void ExecuteStopTradeStatusCommandStreaming();
    void ExecutePingCommandStreaming();
    void ExecuteStopPingCommandStreaming();
}