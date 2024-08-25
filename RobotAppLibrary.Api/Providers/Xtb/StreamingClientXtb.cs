using System.Text.Json;
using RobotAppLibrary.Api.Connector.Exceptions;
using RobotAppLibrary.Api.Connector.Tcp;
using RobotAppLibrary.Api.Interfaces;
using RobotAppLibrary.Api.Modeles;
using Serilog;

namespace RobotAppLibrary.Api.Providers.Xtb;

public class StreamingClientXtb(Server server, ILogger logger, IReponseAdapter adapter)
    : TcpStreamingConnector(server, logger)
{
    protected override void HandleMessage(JsonDocument message)
    {
        try
        {
            var root = message.RootElement;
            var commandName = root.GetProperty("command").GetString();

            switch (commandName)
            {
                case "tickPrices":
                    OnTickRecordReceived(adapter.AdaptTickRecordStreaming(message));
                    break;
                case "trade":
                    OnTradeRecordReceived(adapter.AdaptTradeRecordStreaming(message));
                    break;
                case "balance":
                    OnBalanceRecordReceived(adapter.AdaptBalanceRecordStreaming(message));
                    break;
                case "tradeStatus":
                    OnTradeRecordReceived(adapter.AdaptTradeStatusRecordStreaming(message));
                    break;
                case "profit":
                    OnTradeRecordReceived(adapter.AdaptProfitRecordStreaming(message));
                    break;
                case "news":
                    OnNewsRecordReceived(adapter.AdaptNewsRecordStreaming(message));
                    break;
                case "keepAlive":
                    OnKeepAliveRecordReceived();
                    break;
                case "candle":
                    OnCandleRecordReceived(adapter.AdaptCandleRecordStreaming(message));
                    break;
                default:
                    throw new ApiCommunicationException("Unknown streaming record received");
            }
        }
        catch (Exception e)
        {
            Logger.Error(e, "error on message reception {@message}", message);
        }
    }
}