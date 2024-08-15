using RobotAppLibrary.Api.Modeles;
using RobotAppLibrary.Modeles;

namespace RobotAppLibrary.Api.Providers.Base;

public interface IApiProviderBase : IDisposable
{
    public ApiProviderEnum ApiProviderName { get; }
    public List<SymbolInfo> AllSymbols { get; set; }
    public AccountBalance AccountBalance { get; }
    public event EventHandler Connected;
    public event EventHandler Disconnected;
    public event EventHandler<Tick> TickEvent;
    public event EventHandler<Position>? PositionOpenedEvent;
    public event EventHandler<Position>? PositionUpdatedEvent;
    public event EventHandler<Position> PositionRejectedEvent;
    public event EventHandler<Position> PositionClosedEvent;
    public event EventHandler<AccountBalance> NewBalanceEvent;
    public event EventHandler<News> NewsEvent;
    public Task ConnectAsync(Credentials credentials);
    public Task DisconnectAsync();
    public bool IsConnected();
    public Task PingAsync();
    public Task<AccountBalance> GetBalanceAsync();
    public Task<List<CalendarEvent>> GetCalendarAsync();
    public Task<List<SymbolInfo>> GetAllSymbolsAsync();
    public Task<Position?> GetOpenedTradesAsync(string strategyPositionId);
    public Task<List<Position>?> GetAllPositionsByCommentAsync(string strategyPositionId);
    public Task<SymbolInfo> GetSymbolInformationAsync(string symbol);
    public Task<TradeHourRecord> GetTradingHoursAsync(string symbol);
    public Task<List<Candle>> GetChartAsync(string symbol, Timeframe timeframe);

    public Task<List<Candle>> GetChartByDateAsync(string symbol, Timeframe periodCodeStr, DateTime start,
        DateTime end);

    public Task<Tick> GetTickPriceAsync(string symbol);
    public Task<Position> OpenPositionAsync(Position position, decimal price);
    public Task UpdatePositionAsync(decimal price, Position position);
    public Task ClosePositionAsync(decimal price, Position position);
    public Task<bool> CheckIfSymbolExistAsync(string symbol);
    public void SubscribePrice(string symbol);
    public void UnsubscribePrice(string symbol);
    void RestoreSession(Position position);
}