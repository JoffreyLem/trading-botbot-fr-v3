using System.Runtime.CompilerServices;
using RobotAppLibrary.Api.Interfaces;
using RobotAppLibrary.Api.Modeles;
using RobotAppLibrary.Api.Providers.Exceptions;
using RobotAppLibrary.Modeles;
using Serilog;

[assembly: InternalsVisibleTo("RobotAppLibrary")]

namespace RobotAppLibrary.Api.Providers.Base;

//TODO : test des states event avec les order ?
public abstract class ApiProviderBase : IApiProviderBase
{
    internal readonly List<Position> CachePosition = [];
    protected readonly ILogger Logger;
    private Timer? _pingTimer;
    public DateTime LastPing;

    protected ApiProviderBase(ICommandExecutor commandExecutor, ILogger logger, TimeSpan pingInterval)
    {
        CommandExecutor = commandExecutor;
        Logger = logger.ForContext<ApiProviderBase>();
        CommandExecutor.BalanceRecordReceived += TcpStreamingConnectorOnBalanceRecordReceived;
        CommandExecutor.NewsRecordReceived += news => NewsEvent?.Invoke(this, news);
        CommandExecutor.TickRecordReceived += tick => TickEvent?.Invoke(this, tick);
        CommandExecutor.TradeRecordReceived += TcpStreamingConnectorOnTradeRecordReceived;
        CommandExecutor.ProfitRecordReceived += TcpStreamingConnectorOnProfitRecordReceived;
        CommandExecutor.Disconnected += TcpConnectorOnDisconnected;
        PingInterval = pingInterval;
    }

    public TimeSpan PingInterval { get; }

    public string Name => GetType().Name;

    internal ICommandExecutor CommandExecutor { get; }

    public abstract ApiProviderEnum ApiProviderName { get; }

    public List<SymbolInfo> AllSymbols { get; set; } = new();
    public AccountBalance AccountBalance { get; } = new();
    public event EventHandler? Connected;
    public event EventHandler? Disconnected;
    public event EventHandler<Tick>? TickEvent;
    public event EventHandler<Position>? PositionOpenedEvent;
    public event EventHandler<Position>? PositionUpdatedEvent;
    public event EventHandler<Position>? PositionRejectedEvent;
    public event EventHandler<Position>? PositionClosedEvent;
    public event EventHandler<AccountBalance>? NewBalanceEvent;
    public event EventHandler<News>? NewsEvent;

    public async Task ConnectAsync(Credentials credentials)
    {
        try
        {
            Logger.Information("Connecting to handler.");
            await CommandExecutor.ExecuteLoginCommand(credentials);
            CommandExecutor.ExecuteSubscribeBalanceCommandStreaming();
            CommandExecutor.ExecuteTradesCommandStreaming();
            CommandExecutor.ExecuteTradeStatusCommandStreaming();
            CommandExecutor.ExecuteSubscribeProfitsCommandStreaming();
            CommandExecutor.ExecuteSubscribeNewsCommandStreaming();
            CommandExecutor.ExecuteSubscribeKeepAliveCommandStreaming();

            void TimerCallback(object? _)
            {
                PingAsync().GetAwaiter().GetResult();
            }

            _pingTimer = new Timer(TimerCallback, null, 0, PingInterval.Ticks / TimeSpan.TicksPerMillisecond);
        }
        catch (Exception e)
        {
            Logger.Error(e, $"Error on  {nameof(ConnectAsync)}");
            throw new ApiProvidersException($"Error on  {nameof(ConnectAsync)}", e);
        }
    }

    public async Task DisconnectAsync()
    {
        try
        {
            await CommandExecutor.ExecuteLogoutCommand();
        }
        catch (Exception e)
        {
            Logger.Error(e, $"Error on  {nameof(DisconnectAsync)}");
            throw new ApiProvidersException($"Error on  {nameof(DisconnectAsync)}");
        }
    }

    public bool IsConnected()
    {
        try
        {
            return CommandExecutor.ExecuteIsConnected();
        }
        catch (Exception e)
        {
            Logger.Error(e, $"Error on  {nameof(IsConnected)}");
            throw new ApiProvidersException($"Error on  {nameof(IsConnected)}");
        }
    }

    public async Task PingAsync()
    {
        try
        {
            await CommandExecutor.ExecutePingCommand();
            CommandExecutor.ExecutePingCommandStreaming();
            LastPing = DateTime.Now;
        }
        catch (Exception e)
        {
            Logger.Error(e, $"Error on  {nameof(PingAsync)}");
        }
    }

    public async Task<AccountBalance> GetBalanceAsync()
    {
        try
        {
            return await CommandExecutor.ExecuteBalanceAccountCommand();
        }
        catch (Exception e)
        {
            Logger.Error(e, $"Error on  {nameof(GetBalanceAsync)}");
            throw new ApiProvidersException($"Error on  {nameof(GetBalanceAsync)}");
        }
    }


    public async Task<List<CalendarEvent>> GetCalendarAsync()
    {
        try
        {
            return await CommandExecutor.ExecuteCalendarCommand();
        }
        catch (Exception e)
        {
            Logger.Error(e, $"Error on  {nameof(GetCalendarAsync)}");
            throw new ApiProvidersException($"Error on  {nameof(GetCalendarAsync)}");
        }
    }

    public async Task<List<SymbolInfo>> GetAllSymbolsAsync()
    {
        try
        {
            if (AllSymbols is { Count: 0 }) AllSymbols = await CommandExecutor.ExecuteAllSymbolsCommand();

            return AllSymbols.ToList();
        }
        catch (Exception e)
        {
            Logger.Error(e, $"Error on  {nameof(GetAllSymbolsAsync)}");
            throw new ApiProvidersException($"Error on  {nameof(GetAllSymbolsAsync)}");
        }
    }

    public async Task<Position?> GetOpenedTradesAsync(string comment)
    {
        try
        {
            return await CommandExecutor.ExecuteTradesOpenedTradesCommand(comment);
        }
        catch (Exception e)
        {
            Logger.Error(e, $"Error on  {nameof(GetOpenedTradesAsync)}");
            throw new ApiProvidersException($"Error on  {nameof(GetOpenedTradesAsync)}");
        }
    }

    public async Task<List<Position>?> GetAllPositionsByCommentAsync(string comment)
    {
        try
        {
            return await CommandExecutor.ExecuteTradesHistoryCommand(comment);
        }
        catch (Exception e)
        {
            Logger.Error(e, $"Error on  {nameof(GetAllPositionsByCommentAsync)}");
            throw new ApiProvidersException($"Error on  {nameof(GetAllPositionsByCommentAsync)}");
        }
    }

    public async Task<SymbolInfo> GetSymbolInformationAsync(string symbol)
    {
        try
        {
            if (AllSymbols is { Count: >= 1 }) return AllSymbols.First(x => x.Symbol == symbol);

            return await CommandExecutor.ExecuteSymbolCommand(symbol);
        }
        catch (Exception e)
        {
            Logger.Error(e, $"Error on  {nameof(GetSymbolInformationAsync)}");
            throw new ApiProvidersException($"Error on  {nameof(GetSymbolInformationAsync)}");
        }
    }

    public async Task<TradeHourRecord> GetTradingHoursAsync(string symbol)
    {
        try
        {
            return await CommandExecutor.ExecuteTradingHoursCommand(symbol);
        }
        catch (Exception e)
        {
            Logger.Error(e, $"Error on  {nameof(GetTradingHoursAsync)}");
            throw new ApiProvidersException($"Error on  {nameof(GetTradingHoursAsync)}");
        }
    }

    public async Task<List<Candle>> GetChartAsync(string symbol, Timeframe timeframe)
    {
        try
        {
            return await CommandExecutor.ExecuteFullChartCommand(timeframe, new DateTime(), symbol);
        }
        catch (Exception e)
        {
            Logger.Error(e, $"Error on  {nameof(GetChartAsync)}");
            throw new ApiProvidersException($"Error on  {nameof(GetChartAsync)}");
        }
    }

    public async Task<List<Candle>> GetChartByDateAsync(string symbol, Timeframe periodCodeStr, DateTime start,
        DateTime end)
    {
        try
        {
            return await CommandExecutor.ExecuteRangeChartCommand(periodCodeStr, start, end, symbol);
        }
        catch (Exception e)
        {
            Logger.Error(e, $"Error on  {nameof(GetChartByDateAsync)}");
            throw new ApiProvidersException($"Error on  {nameof(GetChartByDateAsync)}");
        }
    }

    public async Task<Tick> GetTickPriceAsync(string symbol)
    {
        try
        {
            return await CommandExecutor.ExecuteTickCommand(symbol);
        }
        catch (Exception e)
        {
            Logger.Error(e, $"Error on  {nameof(GetTickPriceAsync)}");
            throw new ApiProvidersException($"Error on  {nameof(GetTickPriceAsync)}");
        }
    }

    public virtual async Task<Position> OpenPositionAsync(Position position, decimal price)
    {
        try
        {
            var pos = await CommandExecutor.ExecuteOpenTradeCommand(position, price);
            position.Order = pos.Order;
            CachePosition.Add(position);
            return pos;
        }
        catch (Exception e)
        {
            Logger.Error(e, $"Error on  {nameof(OpenPositionAsync)}");
            throw new ApiProvidersException($"Error on  {nameof(OpenPositionAsync)}");
        }
    }

    public virtual async Task UpdatePositionAsync(decimal price, Position position)
    {
        try
        {
            await CommandExecutor.ExecuteUpdateTradeCommand(position, price);
        }
        catch (Exception e)
        {
            Logger.Error(e, $"Error on  {nameof(UpdatePositionAsync)}");
            throw new ApiProvidersException($"Error on  {nameof(UpdatePositionAsync)}");
        }
    }

    public virtual async Task ClosePositionAsync(decimal price, Position position)
    {
        try
        {
            await CommandExecutor.ExecuteCloseTradeCommand(position, price);
        }
        catch (Exception e)
        {
            Logger.Error(e, $"Error on  {nameof(ClosePositionAsync)}");
            throw new ApiProvidersException($"Error on  {nameof(ClosePositionAsync)}");
        }
    }

    public Task<bool> CheckIfSymbolExistAsync(string symbol)
    {
        throw new NotImplementedException();
    }

    public void SubscribePrice(string symbol)
    {
        try
        {
            CommandExecutor.ExecuteTickPricesCommandStreaming(symbol);
        }
        catch (Exception e)
        {
            Logger.Error(e, $"Error on  {nameof(SubscribePrice)}");
            throw new ApiProvidersException($"Error on  {nameof(SubscribePrice)}");
        }
    }

    public void UnsubscribePrice(string symbol)
    {
        try
        {
            CommandExecutor.ExecuteStopTickPriceCommandStreaming(symbol);
        }
        catch (Exception e)
        {
            Logger.Error(e, $"Error on  {nameof(UnsubscribePrice)}");
            throw new ApiProvidersException($"Error on  {nameof(UnsubscribePrice)}");
        }
    }

    // TODO : Peut être refacto ce fonctionnement ? 
    public void RestoreSession(Position position)
    {
        CachePosition.Add(position);
    }

    public void Dispose()
    {
        CommandExecutor.Dispose();
        _pingTimer?.Dispose();
    }

    private void TcpConnectorOnDisconnected(object? sender, EventArgs e)
    {
        Logger.Information("Disconnected from {Connector}", sender);
        Disconnected?.Invoke(this, EventArgs.Empty);
    }

    private void TcpStreamingConnectorOnProfitRecordReceived(Position? obj)
    {
        OnPositionUpdatedEvent(obj);
    }

    private void TcpStreamingConnectorOnTradeRecordReceived(Position? obj)
    {
        if (obj is not null)
            switch (obj.StatusPosition)
            {
                case StatusPosition.Pending:
                    //TODO : jsp quoi faire encore ici ??
                    break;
                case StatusPosition.Rejected:
                    OnPositionRejectedEvent(obj);
                    break;
                case StatusPosition.Open:
                    OnPositionOpenedEvent(obj);
                    break;
                case StatusPosition.Updated:
                    OnPositionUpdatedEvent(obj);
                    break;
                case StatusPosition.Close:
                    OnPositionClosedEvent(obj);
                    break;
            }
    }


    private void TcpStreamingConnectorOnBalanceRecordReceived(AccountBalance? obj)
    {
        if (obj is not null)
        {
            AccountBalance.Balance = obj.Balance;
            AccountBalance.Credit = obj.Credit;
            AccountBalance.Equity = obj.Equity;
            AccountBalance.Margin = obj.Margin;
            AccountBalance.MarginFree = obj.MarginFree;
            AccountBalance.MarginLevel = obj.MarginLevel;

            NewBalanceEvent?.Invoke(this, AccountBalance);
        }
    }

    protected virtual void OnPositionUpdatedEvent(Position? e)
    {
        if (e is not null)
        {
            var posSelected = CachePosition.FirstOrDefault(x => x.Id == e.Id || x.Order == e.Order);
            if (posSelected is not null)
            {
                posSelected.Profit = e.Profit;
                posSelected.StopLoss = (e.StopLoss != posSelected.StopLoss && e.StopLoss is not 0) ? e.StopLoss : posSelected.StopLoss;
                posSelected.TakeProfit = (e.TakeProfit != posSelected.TakeProfit && e.TakeProfit is not 0) ? e.TakeProfit : posSelected.TakeProfit;
                posSelected.StatusPosition = StatusPosition.Updated;
                PositionUpdatedEvent?.Invoke(this, posSelected);
            }
        }
    }

    protected virtual void OnPositionOpenedEvent(Position? e)
    {
        if (e is not null)
        {
            var posSelected = CachePosition.FirstOrDefault(x => x.Id == e.Id || x.Order == e.Order);
            if (posSelected is { Opened: false })
            {
                posSelected.Opened = true;
                posSelected.Order = e.Order;
                posSelected.DateOpen = e.DateOpen;
                posSelected.StopLoss = e.StopLoss;
                posSelected.TakeProfit = e.TakeProfit;
                posSelected.StatusPosition = StatusPosition.Open;
                posSelected.OpenPrice = e.OpenPrice;
                PositionOpenedEvent?.Invoke(this, posSelected);
            }
        }
    }

    protected virtual void OnPositionRejectedEvent(Position? e)
    {
        if (e is not null)
        {
            var posSelected = CachePosition.FirstOrDefault(x => x.Id == e.Id || x.Order == e.Order);
            if (posSelected is not null)
            {
                CachePosition.Remove(posSelected);
                posSelected.Opened = false;
                posSelected.StatusPosition = StatusPosition.Rejected;
                PositionRejectedEvent?.Invoke(this, posSelected);
            }
        }
    }

    protected virtual void OnPositionClosedEvent(Position? e)
    {
        if (e is not null)
        {
            var posSelected = CachePosition.FirstOrDefault(x => x.Id == e.Id || x.Order == e.Order);
            if (posSelected is not null)
            {
                CachePosition.Remove(posSelected);
                posSelected.StatusPosition = StatusPosition.Close;
                posSelected.Profit = e.Profit;
                posSelected.StopLoss = e.StopLoss;
                posSelected.TakeProfit = e.TakeProfit;
                posSelected.DateClose = e.DateClose;
                posSelected.ClosePrice = e.ClosePrice;
                posSelected.ReasonClosed = e.ReasonClosed;
                posSelected.Opened = false;
                PositionClosedEvent?.Invoke(this, posSelected);
            }
        }
    }
}