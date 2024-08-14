using RobotAppLibrary.Api.Connector.Tcp;
using RobotAppLibrary.Api.Interfaces;
using RobotAppLibrary.Api.Modeles;
using RobotAppLibrary.Modeles;

namespace RobotAppLibrary.Api.Executor;

public class TcpCommandExecutor : ICommandExecutor
{
    protected readonly ICommandCreator CommandCreator;
    protected readonly IReponseAdapter ResponseAdapter;
    protected readonly ITcpConnector TcpClient;
    protected readonly ITcpStreamingConnector TcpStreamingClient;
    
    protected TcpCommandExecutor(ITcpConnector tcpClient, ITcpStreamingConnector tcpStreamingClient,
        ICommandCreator commandCreator, IReponseAdapter responseAdapter)
    {
        TcpClient = tcpClient;
        TcpStreamingClient = tcpStreamingClient;
        CommandCreator = commandCreator;
        ResponseAdapter = responseAdapter;
        tcpClient.Connected += (sender, args) => Connected?.Invoke(sender, args);
        tcpClient.Disconnected += (sender, args) => Disconnected?.Invoke(sender, args);
        tcpStreamingClient.Connected += (sender, args) => Connected?.Invoke(sender, args);
        tcpStreamingClient.Disconnected += (sender, args) => Disconnected?.Invoke(sender, args);
        tcpStreamingClient.TickRecordReceived += tick => TickRecordReceived?.Invoke(tick);
        tcpStreamingClient.TradeRecordReceived += position => TradeRecordReceived?.Invoke(position);
        tcpStreamingClient.BalanceRecordReceived += balance => BalanceRecordReceived?.Invoke(balance);
        tcpStreamingClient.ProfitRecordReceived += profit => ProfitRecordReceived?.Invoke(profit);
        tcpStreamingClient.NewsRecordReceived += news => NewsRecordReceived?.Invoke(news);
        tcpStreamingClient.KeepAliveRecordReceived += () => KeepAliveRecordReceived?.Invoke();
        tcpStreamingClient.CandleRecordReceived += candle => CandleRecordReceived?.Invoke(candle);
    }

    public event Action<Tick>? TickRecordReceived;
    public event Action<Position>? TradeRecordReceived;
    public event Action<AccountBalance>? BalanceRecordReceived;
    public event Action<Position>? ProfitRecordReceived;
    public event Action<News>? NewsRecordReceived;
    public event Action? KeepAliveRecordReceived;
    public event Action<Candle>? CandleRecordReceived;
    public event EventHandler? Connected;
    public event EventHandler? Disconnected;


    public virtual async Task ExecuteLoginCommand(Credentials credentials)
    {
        await TcpClient.ConnectAsync();
        var command = CommandCreator.CreateLoginCommand(credentials);
        await TcpClient.SendAndReceiveAsync(command);
        await TcpStreamingClient.ConnectAsync();
    }

    public async Task ExecuteLogoutCommand()
    {
        var command = CommandCreator.CreateLogOutCommand();
        await TcpClient.SendAndReceiveAsync(command);
    }

    public virtual async Task<List<SymbolInfo>> ExecuteAllSymbolsCommand()
    {
        var command = CommandCreator.CreateAllSymbolsCommand();
        var rsp = await TcpClient.SendAndReceiveAsync(command, false);
        return ResponseAdapter.AdaptAllSymbolsResponse(rsp);
    }

    public virtual async Task<List<CalendarEvent>> ExecuteCalendarCommand()
    {
        var command = CommandCreator.CreateCalendarCommand();
        var rsp = await TcpClient.SendAndReceiveAsync(command);
        return ResponseAdapter.AdaptCalendarResponse(rsp);
    }

    public virtual async Task<List<Candle>> ExecuteFullChartCommand(Timeframe timeframe, DateTime start, string symbol)
    {
        var command = CommandCreator.CreateFullChartCommand(timeframe, start, symbol);
        var rsp = await TcpClient.SendAndReceiveAsync(command, false);
        return ResponseAdapter.AdaptFullChartResponse(rsp);
    }

    public virtual async Task<List<Candle>> ExecuteRangeChartCommand(Timeframe timeframe, DateTime start, DateTime end,
        string symbol)
    {
        var command = CommandCreator.CreateRangeChartCommand(timeframe, start, end, symbol);
        var rsp = await TcpClient.SendAndReceiveAsync(command, false);
        return ResponseAdapter.AdaptRangeChartResponse(rsp);
    }

    public virtual async Task<AccountBalance> ExecuteBalanceAccountCommand()
    {
        var command = CommandCreator.CreateBalanceAccountCommand();
        var rsp = await TcpClient.SendAndReceiveAsync(command);
        return ResponseAdapter.AdaptBalanceAccountResponse(rsp);
    }

    public virtual async Task<List<News>> ExecuteNewsCommand(DateTime? start, DateTime? end)
    {
        var command = CommandCreator.CreateNewsCommand(start, end);
        var rsp = await TcpClient.SendAndReceiveAsync(command);
        return ResponseAdapter.AdaptNewsResponse(rsp);
    }

    public virtual async Task<string> ExecuteCurrentUserDataCommand()
    {
        var command = CommandCreator.CreateCurrentUserDataCommand();
        var rsp = await TcpClient.SendAndReceiveAsync(command);
        return ResponseAdapter.AdaptCurrentUserDataResponse(rsp);
    }

    public virtual async Task<bool> ExecutePingCommand()
    {
        var command = CommandCreator.CreatePingCommand();
        var rsp = await TcpClient.SendAndReceiveAsync(command);
        return ResponseAdapter.AdaptPingResponse(rsp);
    }

    public virtual async Task<SymbolInfo> ExecuteSymbolCommand(string symbol)
    {
        var command = CommandCreator.CreateSymbolCommand(symbol);
        var rsp = await TcpClient.SendAndReceiveAsync(command);
        return ResponseAdapter.AdaptSymbolResponse(rsp);
    }

    public virtual async Task<Tick> ExecuteTickCommand(string symbol)
    {
        var command = CommandCreator.CreateTickCommand(symbol);
        var rsp = await TcpClient.SendAndReceiveAsync(command);
        return ResponseAdapter.AdaptTickResponse(rsp);
    }


    public virtual async Task<List<Position>?> ExecuteTradesHistoryCommand(string tradeCom)
    {
        var command = CommandCreator.CreateTradesHistoryCommand();
        var rsp = await TcpClient.SendAndReceiveAsync(command, false);
        return ResponseAdapter.AdaptTradesHistoryResponse(rsp, tradeCom);
    }

    public virtual async Task<Position?> ExecuteTradesOpenedTradesCommand(string tradeCom)
    {
        var command = CommandCreator.CreateTradesOpenedTradesCommand();
        var rsp = await TcpClient.SendAndReceiveAsync(command, false);
        return ResponseAdapter.AdaptTradesOpenedTradesResponse(rsp, tradeCom);
    }

    public virtual async Task<TradeHourRecord> ExecuteTradingHoursCommand(string symbol)
    {
        var command = CommandCreator.CreateTradingHoursCommand(symbol);
        var rsp = await TcpClient.SendAndReceiveAsync(command);
        return ResponseAdapter.AdaptTradingHoursResponse(rsp);
    }

    public virtual async Task<Position> ExecuteOpenTradeCommand(Position position, decimal price)
    {
        var command = CommandCreator.CreateOpenTradeCommande(position, price);
        var rsp = await TcpClient.SendAndReceiveAsync(command);
        return ResponseAdapter.AdaptOpenTradeResponse(rsp);
    }

    public virtual async Task<Position> ExecuteUpdateTradeCommand(Position position, decimal price)
    {
        var command = CommandCreator.CreateUpdateTradeCommande(position, price);
        var rsp = await TcpClient.SendAndReceiveAsync(command);
        return ResponseAdapter.AdaptUpdateTradeResponse(rsp);
    }

    public virtual async Task<Position> ExecuteCloseTradeCommand(Position position, decimal price)
    {
        var command = CommandCreator.CreateCloseTradeCommande(position, price);
        var rsp = await TcpClient.SendAndReceiveAsync(command);
        return ResponseAdapter.AdaptCloseTradeResponse(rsp);
    }

    public bool ExecuteIsConnected()
    {
        return TcpClient.IsConnected && TcpStreamingClient.IsConnected;
    }

    public virtual async void ExecuteSubscribeBalanceCommandStreaming()
    {
        var command = CommandCreator.CreateSubscribeBalanceCommandStreaming();
        await TcpStreamingClient.SendAsync(command);
    }

    public virtual async void ExecuteStopBalanceCommandStreaming()
    {
        var command = CommandCreator.CreateStopBalanceCommandStreaming();
        await TcpStreamingClient.SendAsync(command);
    }

    public virtual async void ExecuteSubscribeCandleCommandStreaming(string symbol)
    {
        var command = CommandCreator.CreateSubscribeCandleCommandStreaming(symbol);
        await TcpStreamingClient.SendAsync(command);
    }

    public virtual async void ExecuteStopCandleCommandStreaming(string symbol)
    {
        var command = CommandCreator.CreateStopCandleCommandStreaming(symbol);
        await TcpStreamingClient.SendAsync(command);
    }

    public virtual async void ExecuteSubscribeKeepAliveCommandStreaming()
    {
        var command = CommandCreator.CreateSubscribeKeepAliveCommandStreaming();
        await TcpStreamingClient.SendAsync(command);
    }

    public virtual async void ExecuteStopKeepAliveCommandStreaming()
    {
        var command = CommandCreator.CreateStopKeepAliveCommandStreaming();
        await TcpStreamingClient.SendAsync(command);
    }

    public virtual async void ExecuteSubscribeNewsCommandStreaming()
    {
        var command = CommandCreator.CreateSubscribeNewsCommandStreaming();
        await TcpStreamingClient.SendAsync(command);
    }

    public virtual async void ExecuteStopNewsCommandStreaming()
    {
        var command = CommandCreator.CreateStopNewsCommandStreaming();
        await TcpStreamingClient.SendAsync(command);
    }

    public virtual async void ExecuteSubscribeProfitsCommandStreaming()
    {
        var command = CommandCreator.CreateSubscribeProfitsCommandStreaming();
        await TcpStreamingClient.SendAsync(command);
    }

    public virtual async void ExecuteStopProfitsCommandStreaming()
    {
        var command = CommandCreator.CreateStopProfitsCommandStreaming();
        await TcpStreamingClient.SendAsync(command);
    }

    public virtual async void ExecuteTickPricesCommandStreaming(string symbol)
    {
        var command = CommandCreator.CreateTickPricesCommandStreaming(symbol);
        await TcpStreamingClient.SendAsync(command);
    }

    public virtual async void ExecuteStopTickPriceCommandStreaming(string symbol)
    {
        var command = CommandCreator.CreateStopTickPriceCommandStreaming(symbol);
        await TcpStreamingClient.SendAsync(command);
    }

    public virtual async void ExecuteTradesCommandStreaming()
    {
        var command = CommandCreator.CreateTradesCommandStreaming();
        await TcpStreamingClient.SendAsync(command);
    }

    public virtual async void ExecuteStopTradesCommandStreaming()
    {
        var command = CommandCreator.CreateStopTradesCommandStreaming();
        await TcpStreamingClient.SendAsync(command);
    }

    public virtual async void ExecuteTradeStatusCommandStreaming()
    {
        var command = CommandCreator.CreateTradeStatusCommandStreaming();
        await TcpStreamingClient.SendAsync(command);
    }

    public virtual async void ExecuteStopTradeStatusCommandStreaming()
    {
        var command = CommandCreator.CreateStopTradeStatusCommandStreaming();
        await TcpStreamingClient.SendAsync(command);
    }

    public virtual async void ExecutePingCommandStreaming()
    {
        var command = CommandCreator.CreatePingCommandStreaming();
        await TcpStreamingClient.SendAsync(command);
    }

    public virtual async void ExecuteStopPingCommandStreaming()
    {
        var command = CommandCreator.CreateStopPingCommandStreaming();
        await TcpStreamingClient.SendAsync(command);
    }

    public void Dispose()
    {
        TcpStreamingClient.Close();
        TcpClient.Close();
    }
}