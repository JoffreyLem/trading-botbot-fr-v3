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
        CommandExecutor.NewsRecordReceived += OnCommandExecutorOnNewsRecordReceived;
        CommandExecutor.TickRecordReceived += OnCommandExecutorOnTickRecordReceived;
        CommandExecutor.TradeRecordReceived += TcpStreamingConnectorOnTradeRecordReceived;
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
            CommandExecutor.ExecuteSubscribeNewsCommandStreaming();
            CommandExecutor.ExecuteSubscribeKeepAliveCommandStreaming();

            void TimerCallback(object? _)
            {
                PingAsync().GetAwaiter().GetResult();
            }

            _pingTimer = new Timer(TimerCallback, null, 0, PingInterval.Ticks / TimeSpan.TicksPerMillisecond);
        }
        catch (ApiProvidersException)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new ApiProvidersException($"Error on  {nameof(ConnectAsync)}", e);
        }
    }

    public async Task DisconnectAsync()
    {
        try
        {
            await CommandExecutor.ExecuteLogoutCommand();
        }
        catch (ApiProvidersException)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new ApiProvidersException($"Error on  {nameof(DisconnectAsync)}",e);
        }
    }

    public bool IsConnected()
    {
        try
        {
            return CommandExecutor.ExecuteIsConnected();
        }
        catch (ApiProvidersException)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new ApiProvidersException($"Error on  {nameof(IsConnected)}", e);
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
        catch (ApiProvidersException)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new ApiProvidersException($"Error on  {nameof(GetBalanceAsync)}",e);
        }
    }


    public async Task<List<CalendarEvent>> GetCalendarAsync()
    {
        try
        {
            return await CommandExecutor.ExecuteCalendarCommand();
        }
        catch (ApiProvidersException)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new ApiProvidersException($"Error on  {nameof(GetCalendarAsync)}",e);
        }
    }

    public async Task<List<SymbolInfo>> GetAllSymbolsAsync()
    {
        try
        {
            if (AllSymbols is { Count: 0 }) AllSymbols = await CommandExecutor.ExecuteAllSymbolsCommand();

            return AllSymbols.ToList();
        }
        catch (ApiProvidersException)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new ApiProvidersException($"Error on  {nameof(GetAllSymbolsAsync)}",e);
        }
    }

    public async Task<Position?> GetOpenedTradesAsync(string comment)
    {
        try
        {
            return await CommandExecutor.ExecuteTradesOpenedTradesCommand(comment);
        }
        catch (ApiProvidersException)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new ApiProvidersException($"Error on  {nameof(GetOpenedTradesAsync)}",e);
        }
    }

    public async Task<List<Position>?> GetAllPositionsByCommentAsync(string comment)
    {
        try
        {
            return await CommandExecutor.ExecuteTradesHistoryCommand(comment);
        }
        catch (ApiProvidersException)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new ApiProvidersException($"Error on  {nameof(GetAllPositionsByCommentAsync)}", e);
        }
    }

    public async Task<SymbolInfo> GetSymbolInformationAsync(string symbol)
    {
        try
        {
            if (AllSymbols is { Count: >= 1 }) return AllSymbols.First(x => x.Symbol == symbol);

            return await CommandExecutor.ExecuteSymbolCommand(symbol);
        }
        catch (ApiProvidersException)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new ApiProvidersException($"Error on  {nameof(GetSymbolInformationAsync)}",e);
        }
    }

    public async Task<TradeHourRecord> GetTradingHoursAsync(string symbol)
    {
        try
        {
            return await CommandExecutor.ExecuteTradingHoursCommand(symbol);
        }
        catch (ApiProvidersException)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new ApiProvidersException($"Error on  {nameof(GetTradingHoursAsync)}",e);
        }
    }

    public async Task<List<Candle>> 
        GetChartAsync(ChartRequest chartRequest)
    {
        try
        {
            return await CommandExecutor.ExecuteFullChartCommand(chartRequest);
        }
        catch (ApiProvidersException)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new ApiProvidersException($"Error on  {nameof(GetChartAsync)}",e);
        }
    }

    public async Task<List<Candle>> GetChartByDateAsync(ChartRequest chartRequest)
    {
        try
        {
            return await CommandExecutor.ExecuteRangeChartCommand(chartRequest);
        }
        catch (ApiProvidersException)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new ApiProvidersException($"Error on  {nameof(GetChartByDateAsync)}",e);
        }
    }

    public async Task<Tick> GetTickPriceAsync(string symbol)
    {
        try
        {
            return await CommandExecutor.ExecuteTickCommand(symbol);
        }
        catch (ApiProvidersException)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new ApiProvidersException($"Error on  {nameof(GetTickPriceAsync)}",e);
        }
    }

    public virtual async Task<Position?> OpenPositionAsync(Position position)
    {
        try
        {
            var pos = await CommandExecutor.ExecuteOpenTradeCommand(position);
            position.Order = pos.Order;
            CachePosition.Add(position);
            return pos;
        }
        catch (ApiProvidersException)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new ApiProvidersException($"Error on  {nameof(OpenPositionAsync)}",e);
        }
    }

    public virtual async Task UpdatePositionAsync(Position position)
    {
        try
        {
            await CommandExecutor.ExecuteUpdateTradeCommand(position);
        }
        catch (ApiProvidersException)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new ApiProvidersException($"Error on  {nameof(UpdatePositionAsync)}",e);
        }
    }

    public virtual async Task ClosePositionAsync(Position position)
    {
        try
        {
            await CommandExecutor.ExecuteCloseTradeCommand(position);
        }
        catch (ApiProvidersException)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new ApiProvidersException($"Error on  {nameof(ClosePositionAsync)}",e);
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
        catch (ApiProvidersException)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new ApiProvidersException($"Error on  {nameof(SubscribePrice)}",e);
        }
    }

    public void UnsubscribePrice(string symbol)
    {
        try
        {
            CommandExecutor.ExecuteStopTickPriceCommandStreaming(symbol);
        }
        catch (ApiProvidersException)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new ApiProvidersException($"Error on  {nameof(UnsubscribePrice)}",e);
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
    
    
    private void OnCommandExecutorOnTickRecordReceived(Tick tick)
    {
        TickEvent?.Invoke(this, tick);
        Logger.Verbose("New tick {@tick}",tick);
    }

    private void OnCommandExecutorOnNewsRecordReceived(News news)
    {
        NewsEvent?.Invoke(this, news);
        Logger.Verbose("New api event {@news}", news);
    }


    private void TcpStreamingConnectorOnTradeRecordReceived(Position? obj)
    {
        if (obj is not null)
        {
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
            Logger.Verbose("Position data received {@position}", obj);
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
            Logger.Verbose("New account balance data {@Account}", AccountBalance);
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