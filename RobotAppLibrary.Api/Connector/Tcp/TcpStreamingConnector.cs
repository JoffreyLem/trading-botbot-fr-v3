using System.Text.Json;
using RobotAppLibrary.Api.Interfaces;
using RobotAppLibrary.Api.Modeles;
using RobotAppLibrary.Modeles;
using Serilog;

namespace RobotAppLibrary.Api.Connector.Tcp;

public interface ITcpStreamingConnector : IConnectorBase, IStreamingEvent
{
}

public abstract class TcpStreamingConnector(Server server, ILogger logger)
    : TcpClientBase(server.Address, server.StreamingPort, logger), ITcpStreamingConnector
{
    public event Action<Tick>? TickRecordReceived;
    public event Action<Position>? TradeRecordReceived;
    public event Action<AccountBalance>? BalanceRecordReceived;
    public event Action<Position>? ProfitRecordReceived;
    public event Action<News>? NewsRecordReceived;
    public event Action? KeepAliveRecordReceived;
    public event Action<Candle>? CandleRecordReceived;

    public override async Task ConnectAsync()
    {
        await base.ConnectAsync();
        
        _ = Task.Run(async () =>
        {
            while (IsConnected) 
            {
                await ReadStreamMessage();
            }
        });
    }

    public override async Task SendAsync(string messageToSend)
    {
        Logger.Information("Streaming message to send {Message}", messageToSend);
        await base.SendAsync(messageToSend).ConfigureAwait(false);
    }

    protected abstract void HandleMessage(JsonDocument message);

    protected virtual async Task ReadStreamMessage()
    {
        try
        {
            using var message = await ReceiveAsync().ConfigureAwait(false);
            if (message is not null)
            {
                Logger.Verbose("New stream message received {@message}", message);
                HandleMessage(message);
            }
        }
        catch (Exception e)
        {
            Logger.Error(e, "Error on read stream message");
        }
    }

    protected virtual void OnTickRecordReceived(Tick obj)
    {
        Logger.Verbose("New tick event {@Tick}", obj);
        TickRecordReceived?.Invoke(obj);
    }

    protected virtual void OnTradeRecordReceived(Position obj)
    {
        Logger.Information("Position event {@obj}", obj);
        TradeRecordReceived?.Invoke(obj);
    }

    protected virtual void OnBalanceRecordReceived(AccountBalance obj)
    {
        Logger.Verbose("Account balance event {@obj}", obj);
        BalanceRecordReceived?.Invoke(obj);
    }


    protected virtual void OnProfitRecordReceived(Position obj)
    {
        Logger.Verbose("Profit record event {@obj}", obj);
        ProfitRecordReceived?.Invoke(obj);
    }

    protected virtual void OnNewsRecordReceived(News obj)
    {
        Logger.Verbose("News event {@obj}", obj);
        NewsRecordReceived?.Invoke(obj);
    }

    protected virtual void OnKeepAliveRecordReceived()
    {
        Logger.Verbose("Keep alive event");
        KeepAliveRecordReceived?.Invoke();
    }

    protected virtual void OnCandleRecordReceived(Candle obj)
    {
        Logger.Verbose("Candle event {@obj}", obj);
        CandleRecordReceived?.Invoke(obj);
    }
}