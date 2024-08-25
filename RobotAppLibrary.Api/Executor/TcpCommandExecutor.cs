using System.Text.Json;
using RobotAppLibrary.Api.Connector.Tcp;
using RobotAppLibrary.Api.Consts;
using RobotAppLibrary.Api.Interfaces;
using RobotAppLibrary.Api.Modeles;
using RobotAppLibrary.Modeles;
using Serilog;
using Serilog.Core;

namespace RobotAppLibrary.Api.Executor;

public class TcpCommandExecutor : ICommandExecutor
{
    protected readonly ICommandCreator CommandCreator;
    protected readonly IReponseAdapter ResponseAdapter;
    protected readonly ITcpConnector TcpClient;
    protected readonly ITcpStreamingConnector TcpStreamingClient;
    protected readonly ILogger Logger;

    protected TcpCommandExecutor(ITcpConnector tcpClient, ITcpStreamingConnector tcpStreamingClient,
        ICommandCreator commandCreator, IReponseAdapter responseAdapter, ILogger logger)
    {
        TcpClient = tcpClient;
        TcpStreamingClient = tcpStreamingClient;
        CommandCreator = commandCreator;
        ResponseAdapter = responseAdapter;
        Logger = logger.ForContext<TcpCommandExecutor>();
        tcpClient.Connected += (sender, args) => Connected?.Invoke(sender, args);
        tcpClient.Disconnected += (sender, args) => Disconnected?.Invoke(sender, args);
        tcpStreamingClient.Connected += (sender, args) => Connected?.Invoke(sender, args);
        tcpStreamingClient.Disconnected += (sender, args) => Disconnected?.Invoke(sender, args);
        tcpStreamingClient.TickRecordReceived += tick => TickRecordReceived?.Invoke(tick);
        tcpStreamingClient.TradeRecordReceived += position => TradeRecordReceived?.Invoke(position);
        tcpStreamingClient.BalanceRecordReceived += balance => BalanceRecordReceived?.Invoke(balance);
        tcpStreamingClient.NewsRecordReceived += news => NewsRecordReceived?.Invoke(news);
        tcpStreamingClient.KeepAliveRecordReceived += () => KeepAliveRecordReceived?.Invoke();
        tcpStreamingClient.CandleRecordReceived += candle => CandleRecordReceived?.Invoke(candle);
    }

    public event Action<Tick>? TickRecordReceived;
    public event Action<Position>? TradeRecordReceived;
    public event Action<AccountBalance>? BalanceRecordReceived;
    public event Action<News>? NewsRecordReceived;
    public event Action? KeepAliveRecordReceived;
    public event Action<Candle>? CandleRecordReceived;
    public event EventHandler? Connected;
    public event EventHandler? Disconnected;


    public virtual async Task ExecuteLoginCommand(Credentials credentials)
    {
        Logger.Information("Execute login command");
        await TcpClient.ConnectAsync();
        var command = CommandCreator.CreateLoginCommand(credentials);
        await TcpClient.SendAndReceiveAsync(command);
        await TcpStreamingClient.ConnectAsync();
    }

    public async Task ExecuteLogoutCommand()
    {
        Logger.Information("Execute logout command");
        var command = CommandCreator.CreateLogOutCommand();
        await TcpClient.SendAndReceiveAsync(command);
    }

    public virtual async Task<List<SymbolInfo>> ExecuteAllSymbolsCommand()
    {
        Logger.Information("Execute all symbols command");
        var command = CommandCreator.CreateAllSymbolsCommand();
        using var rsp = await TcpClient.SendAndReceiveAsync(command);
        return ResponseAdapter.AdaptAllSymbolsResponse(rsp);
    }

    public virtual async Task<List<CalendarEvent>> ExecuteCalendarCommand()
    {
        Logger.Information("Execute calendar command");
        var command = CommandCreator.CreateCalendarCommand();
        using var rsp = await TcpClient.SendAndReceiveAsync(command);
        return ResponseAdapter.AdaptCalendarResponse(rsp);
    }

    public virtual async Task<List<Candle>> ExecuteFullChartCommand(ChartRequest chartRequest)
    {
        Logger.Information("Execute full chart command with params {@ChartRequest}", chartRequest);
        var command = CommandCreator.CreateFullChartCommand(chartRequest.Timeframe, chartRequest.Start, chartRequest.Symbol);
        using var rsp = await TcpClient.SendAndReceiveAsync(command);
        return ResponseAdapter.AdaptFullChartResponse(rsp);
    }

    public virtual async Task<List<Candle>> ExecuteRangeChartCommand(ChartRequest chartRequest)
    {
        Logger.Information("Execute chart range command with params {@ChartRequest}", chartRequest);
        var command = CommandCreator.CreateRangeChartCommand(chartRequest.Timeframe, chartRequest.Start, chartRequest.End.GetValueOrDefault(), chartRequest.Symbol);
        using var rsp = await TcpClient.SendAndReceiveAsync(command);
        return ResponseAdapter.AdaptRangeChartResponse(rsp);
    }

    public virtual async Task<AccountBalance> ExecuteBalanceAccountCommand()
    {
        Logger.Information("Execute balance account command");
        var command = CommandCreator.CreateBalanceAccountCommand();
        using var rsp = await TcpClient.SendAndReceiveAsync(command);
        return ResponseAdapter.AdaptBalanceAccountResponse(rsp);
    }

    public virtual async Task<List<News>> ExecuteNewsCommand(NewsRequest newsRequest)
    {
        Logger.Information("Execute news command with params {@NewsRequest}", newsRequest); 
        var command = CommandCreator.CreateNewsCommand(newsRequest.Start, newsRequest.End);
        using var rsp = await TcpClient.SendAndReceiveAsync(command);
        return ResponseAdapter.AdaptNewsResponse(rsp);
    }

    public virtual async Task<string> ExecuteCurrentUserDataCommand()
    {
        Logger.Information("Get current user data");
        var command = CommandCreator.CreateCurrentUserDataCommand();
        using var rsp = await TcpClient.SendAndReceiveAsync(command);
        return ResponseAdapter.AdaptCurrentUserDataResponse(rsp);
    }

    public virtual async Task<bool> ExecutePingCommand()
    {
        Logger.Information("Execute ping command");
        var command = CommandCreator.CreatePingCommand();
        using var rsp = await TcpClient.SendAndReceiveAsync(command);
        return ResponseAdapter.AdaptPingResponse(rsp);
    }

    public virtual async Task<SymbolInfo> ExecuteSymbolCommand(string symbol)
    {
        Logger.Information("Execute symbol command for symbol {Symbol}", symbol);
        var command = CommandCreator.CreateSymbolCommand(symbol);
        using var rsp = await TcpClient.SendAndReceiveAsync(command);
        return ResponseAdapter.AdaptSymbolResponse(rsp);
    }

    public virtual async Task<Tick> ExecuteTickCommand(string symbol)
    {
        Logger.Information("Execute tick command for symbol {Symbol}", symbol);
        var command = CommandCreator.CreateTickCommand(symbol);
        using var rsp = await TcpClient.SendAndReceiveAsync(command);
        return ResponseAdapter.AdaptTickResponse(rsp);
    }


    public virtual async Task<List<Position>?> ExecuteTradesHistoryCommand(string tradeCom)
    {
        Logger.Information("Execute trade history command for id {Id}", tradeCom);
        var command = CommandCreator.CreateTradesHistoryCommand();
        using var rsp = await TcpClient.SendAndReceiveAsync(command);
        return ResponseAdapter.AdaptTradesHistoryResponse(rsp, tradeCom);
    }

    public virtual async Task<Position?> ExecuteTradesOpenedTradesCommand(string tradeCom)
    {
        Logger.Information("Execute trade Opened command for id {Id}", tradeCom);
        var command = CommandCreator.CreateTradesOpenedTradesCommand();
        using var rsp = await TcpClient.SendAndReceiveAsync(command);
        return ResponseAdapter.AdaptTradesOpenedTradesResponse(rsp, tradeCom);
    }

    public virtual async Task<TradeHourRecord> ExecuteTradingHoursCommand(string symbol)
    {
        Logger.Information("Execute trade hours record for symbol {Symbol}", symbol);
        var command = CommandCreator.CreateTradingHoursCommand(symbol);
        using var rsp = await TcpClient.SendAndReceiveAsync(command);
        return ResponseAdapter.AdaptTradingHoursResponse(rsp);
    }

    public virtual async Task<Position?> ExecuteOpenTradeCommand(Position? position)
    {
        var command = string.Empty;
        Position? positionResponse = null;
        try
        {
            command = CommandCreator.CreateOpenTradeCommande(position);
            using var rsp = await TcpClient.SendAndReceiveAsync(command);
            positionResponse = ResponseAdapter.AdaptOpenTradeResponse(rsp);
            return positionResponse;
        }
        finally
        {
            Logger.ForContext(LoggerConst.PositionId,position?.Id).Information("Open trade command executed {@Position}", new LogObject()
            {
                Command = command,
                Result = positionResponse
            });
        }
        
    }

    public virtual async Task<Position?> ExecuteUpdateTradeCommand(Position? position)
    {
        var command = string.Empty;
        Position? positionResponse = null;
        try
        {
            command = CommandCreator.CreateUpdateTradeCommande(position);
            using var rsp = await TcpClient.SendAndReceiveAsync(command);
            positionResponse = ResponseAdapter.AdaptUpdateTradeResponse(rsp);
            return positionResponse;
        }
        finally
        {
            Logger.ForContext(LoggerConst.PositionId,position?.Id).Information("Update trade command executed {@Position}", new LogObject()
            {
                Command = command,
                Result = positionResponse
            });
        }

    }

    public virtual async Task<Position?> ExecuteCloseTradeCommand(Position? position)
    {
        var command = string.Empty;
        Position? positionResponse = null;
        try
        {
          
            command = CommandCreator.CreateCloseTradeCommande(position);
            using var rsp = await TcpClient.SendAndReceiveAsync(command);
            positionResponse = ResponseAdapter.AdaptCloseTradeResponse(rsp);
            return positionResponse;
        }
        finally
        {
            Logger.ForContext(LoggerConst.PositionId,position?.Id).Information("Close trade command executed {@Position}", new LogObject()
            {
                Command = command,
                Result = positionResponse
            });
        }

    }

    public bool ExecuteIsConnected()
    {
        Logger.Information("Execute is connected command");
        return TcpClient.IsConnected && TcpStreamingClient.IsConnected;
    }

    public virtual async void ExecuteSubscribeBalanceCommandStreaming()
    {
        Logger.Information("Execute subscribe balance command streaming");
        var command = CommandCreator.CreateSubscribeBalanceCommandStreaming();
        await TcpStreamingClient.SendAsync(command);
    }

    public virtual async void ExecuteStopBalanceCommandStreaming()
    {
        Logger.Information("Execute stop balance command streaming");
        var command = CommandCreator.CreateStopBalanceCommandStreaming();
        await TcpStreamingClient.SendAsync(command);
    }

    public virtual async void ExecuteSubscribeCandleCommandStreaming(string symbol)
    {
        Logger.Information("Execute subscribe candle command streaming");
        var command = CommandCreator.CreateSubscribeCandleCommandStreaming(symbol);
        await TcpStreamingClient.SendAsync(command);
    }

    public virtual async void ExecuteStopCandleCommandStreaming(string symbol)
    {
        Logger.Information("Execute stop candle command streaming");
        var command = CommandCreator.CreateStopCandleCommandStreaming(symbol);
        await TcpStreamingClient.SendAsync(command);
    }

    public virtual async void ExecuteSubscribeKeepAliveCommandStreaming()
    {
        Logger.Information("Execute subscribe keep alive command stream");
        var command = CommandCreator.CreateSubscribeKeepAliveCommandStreaming();
        await TcpStreamingClient.SendAsync(command);
    }

    public virtual async void ExecuteStopKeepAliveCommandStreaming()
    {
        Logger.Information("Execute stop keep alive command stream");
        var command = CommandCreator.CreateStopKeepAliveCommandStreaming();
        await TcpStreamingClient.SendAsync(command);
    }

    public virtual async void ExecuteSubscribeNewsCommandStreaming()
    {
        Logger.Information("Execute subscribe news command stream");
        var command = CommandCreator.CreateSubscribeNewsCommandStreaming();
        await TcpStreamingClient.SendAsync(command);
    }

    public virtual async void ExecuteStopNewsCommandStreaming()
    {
        Logger.Information("Execute stop news command stream");
        var command = CommandCreator.CreateStopNewsCommandStreaming();
        await TcpStreamingClient.SendAsync(command);
    }

    public virtual async void ExecuteTickPricesCommandStreaming(string symbol)
    {
        Logger.Information("Execute subscribe tick price command stream");
        var command = CommandCreator.CreateTickPricesCommandStreaming(symbol);
        await TcpStreamingClient.SendAsync(command);
    }

    public virtual async void ExecuteStopTickPriceCommandStreaming(string symbol)
    {
        Logger.Information("Execute stop tick price command stream");
        var command = CommandCreator.CreateStopTickPriceCommandStreaming(symbol);
        await TcpStreamingClient.SendAsync(command);
    }

    public virtual async void ExecuteTradesCommandStreaming()
    {
        Logger.Information("Execute subscribe trades command stream");
        var command = CommandCreator.CreateTradesCommandStreaming();
        await TcpStreamingClient.SendAsync(command);
        var command2 = CommandCreator.CreateSubscribeProfitsCommandStreaming();
        await TcpStreamingClient.SendAsync(command2);
        var command3 = CommandCreator.CreateTradeStatusCommandStreaming();
        await TcpStreamingClient.SendAsync(command3);
    }

    public virtual async void ExecuteStopTradesCommandStreaming()
    {
        Logger.Information("Execute stop trades command stream");
        var command = CommandCreator.CreateStopTradesCommandStreaming();
        await TcpStreamingClient.SendAsync(command);
        var command2 = CommandCreator.CreateStopProfitsCommandStreaming();
        await TcpStreamingClient.SendAsync(command2);
        var command3 = CommandCreator.CreateStopTradeStatusCommandStreaming();
        await TcpStreamingClient.SendAsync(command3);
    }


    public virtual async void ExecutePingCommandStreaming()
    {
        Logger.Information("Execute  ping command stream");
        var command = CommandCreator.CreatePingCommandStreaming();
        await TcpStreamingClient.SendAsync(command);
    }

    public virtual async void ExecuteStopPingCommandStreaming()
    {
        Logger.Information("Execute stop ping command stream");
        var command = CommandCreator.CreateStopPingCommandStreaming();
        await TcpStreamingClient.SendAsync(command);
    }

    public void Dispose()
    {
        TcpStreamingClient.Close();
        TcpClient.Close();
    }
}