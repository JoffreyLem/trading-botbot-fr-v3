using System.Text;
using System.Text.Json;
using RobotAppLibrary.Api.Interfaces;
using RobotAppLibrary.Api.Modeles;
using RobotAppLibrary.Api.Providers.Exceptions;
using RobotAppLibrary.Api.Providers.Xtb.Assembler;
using RobotAppLibrary.Api.Providers.Xtb.Code;
using RobotAppLibrary.Api.Providers.Xtb.Utils;
using RobotAppLibrary.Modeles;
using RobotAppLibrary.Utils;

namespace RobotAppLibrary.Api.Providers.Xtb;

public interface ICommandCreatorXtb : ICommandCreator
{
    public string? StreamingSessionId { get; set; }
}

public class CommandCreatorXtb : ICommandCreatorXtb
{
    private string? _streamingSessionId;

    public string? StreamingSessionId
    {
        get
        {
            if (string.IsNullOrEmpty(_streamingSessionId))
                throw new ArgumentException("The streaming session id is empty");

            return _streamingSessionId;
        }
        set => _streamingSessionId = value;
    }

    public string CreateLoginCommand(Credentials credentials)
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        writer.WriteStartObject();
        writer.WriteString("userId", credentials.User);
        writer.WriteString("password", credentials.Password);
        writer.WriteString("appId", "botbot");
        writer.WriteString("appName", "botbot");
        writer.WriteEndObject();
        writer.Flush();

        using var doc = JsonDocument.Parse(stream.ToArray());
        var args = doc.RootElement;

        return WriteBaseCommand("login", args);
    }

    public string CreateAllSymbolsCommand()
    {
        return WriteBaseCommand("getAllSymbols", null);
    }

    public string CreateCalendarCommand()
    {
        return WriteBaseCommand("getCalendar", null);
    }

    public string CreateFullChartCommand(Timeframe timeframe, DateTime start, string symbol)
    {
        var chartLastInfoRecordJson = JsonSerializer.Serialize(new
        {
            symbol,
            period = ToXtbAssembler.ToPeriodCode(timeframe),
            start = ToXtbAssembler.SetDateTimeForChart(timeframe).ConvertToUnixTime()
        });

        var fullJson = $"{{\"arguments\": {{\"info\": {chartLastInfoRecordJson}}}}}";

        using var doc = JsonDocument.Parse(fullJson);
        var argumentsElement = doc.RootElement.GetProperty("arguments");

        return WriteBaseCommand("getChartLastRequest", argumentsElement);
    }

    public string CreateRangeChartCommand(Timeframe timeframe, DateTime start, DateTime end, string symbol)
    {
        var chartLastInfoRecordJson = JsonSerializer.Serialize(new
        {
            symbol,
            period = ToXtbAssembler.ToPeriodCode(timeframe),
            start = start.ConvertToUnixTime(),
            end = end.ConvertToUnixTime()
        });

        var fullJson = $"{{\"arguments\": {{\"info\": {chartLastInfoRecordJson}}}}}";

        using var doc = JsonDocument.Parse(fullJson);
        var argumentsElement = doc.RootElement.GetProperty("arguments");

        return WriteBaseCommand("getChartRangeRequest", argumentsElement);
    }

    public string CreateLogOutCommand()
    {
        return WriteBaseCommand("logout", null);
    }

    public string CreateBalanceAccountCommand()
    {
        return WriteBaseCommand("getMarginLevel", null);
    }

    public string CreateNewsCommand(DateTime? start, DateTime? end)
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        writer.WriteStartObject();
        writer.WriteNumber("start", start.GetValueOrDefault().ConvertToUnixTime());
        writer.WriteNumber("end", end.GetValueOrDefault().ConvertToUnixTime());
        writer.WriteEndObject();

        writer.Flush();

        using var doc = JsonDocument.Parse(stream.ToArray());

        return WriteBaseCommand("getNews", doc.RootElement);
    }


    public string CreateCurrentUserDataCommand()
    {
        return WriteBaseCommand("getCurrentUserData", null);
    }

    public string CreatePingCommand()
    {
        return WriteBaseCommand("ping", null);
    }

    public string CreateSymbolCommand(string symbol)
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        writer.WriteStartObject();
        writer.WriteString("symbol", symbol);
        writer.WriteEndObject();

        writer.Flush();

        using var doc = JsonDocument.Parse(stream.ToArray());

        return WriteBaseCommand("getSymbol", doc.RootElement);
    }

    public string CreateTickCommand(string symbol)
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        writer.WriteStartObject();
        writer.WriteStartArray("symbols");
        writer.WriteStringValue(symbol);
        writer.WriteEndArray();
        writer.WriteNumber("timestamp", 0);
        writer.WriteNumber("level", 0);
        writer.WriteEndObject();

        writer.Flush();

        using var doc = JsonDocument.Parse(stream.ToArray());

        return WriteBaseCommand("getTickPrices", doc.RootElement);
    }

    public string CreateTradesHistoryCommand()
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        writer.WriteStartObject();

        writer.WriteNumber("start", new DateTime(2000, 01, 01).ConvertToUnixTime());
        writer.WriteNumber("end", 0);
        writer.WriteEndObject();

        writer.Flush();

        using var doc = JsonDocument.Parse(stream.ToArray());

        return WriteBaseCommand("getTradesHistory", doc.RootElement);
    }

    public string CreateTradesOpenedTradesCommand()
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        writer.WriteStartObject();
        writer.WriteBoolean("openedOnly", true);
        writer.WriteEndObject();

        writer.Flush();

        using var doc = JsonDocument.Parse(stream.ToArray());

        return WriteBaseCommand("getTrades", doc.RootElement);
    }

    public string CreateTradingHoursCommand(string symbol)
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        writer.WriteStartObject();
        writer.WriteStartArray("symbols");
        writer.WriteStringValue(symbol);
        writer.WriteEndArray();
        writer.WriteEndObject();

        writer.Flush();

        using var doc = JsonDocument.Parse(stream.ToArray());

        return WriteBaseCommand("getTradingHours", doc.RootElement);
    }

    public string CreateOpenTradeCommande(Position position)
    {
        return CreateTradeTransactionCommand(position, position.OpenPrice, TRADE_TRANSACTION_TYPE.ORDER_OPEN.Code);
    }

    public string CreateUpdateTradeCommande(Position position)
    {
        return CreateTradeTransactionCommand(position, position.CurrentPrice.GetValueOrDefault(), TRADE_TRANSACTION_TYPE.ORDER_MODIFY.Code);
    }

    public string CreateCloseTradeCommande(Position position)
    {
        return CreateTradeTransactionCommand(position, position.ClosePrice.GetValueOrDefault(), TRADE_TRANSACTION_TYPE.ORDER_CLOSE.Code);
    }

    public string CreateSubscribeBalanceCommandStreaming()
    {
        using var memoryStream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(memoryStream))
        {
            writer.WriteStartObject();

            writer.WriteString("command", "getBalance");
            writer.WriteString("streamSessionId", StreamingSessionId);

            writer.WriteEndObject();
            writer.Flush();
        }

        return Encoding.UTF8.GetString(memoryStream.ToArray());
    }

    public string CreateStopBalanceCommandStreaming()
    {
        using var memoryStream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(memoryStream))
        {
            writer.WriteStartObject();

            writer.WriteString("command", "stopBalance");

            writer.WriteEndObject();
            writer.Flush();
        }

        return Encoding.UTF8.GetString(memoryStream.ToArray());
    }

    public string CreateSubscribeCandleCommandStreaming(string symbol)
    {
        using var memoryStream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(memoryStream))
        {
            writer.WriteStartObject();

            writer.WriteString("command", "getCandles");
            writer.WriteString("streamSessionId", StreamingSessionId);
            writer.WriteString("symbol", symbol);

            writer.WriteEndObject();
            writer.Flush();
        }

        return Encoding.UTF8.GetString(memoryStream.ToArray());
    }

    public string CreateStopCandleCommandStreaming(string symbol)
    {
        using var memoryStream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(memoryStream))
        {
            writer.WriteStartObject();

            writer.WriteString("command", "getCandles");
            writer.WriteString("streamSessionId", StreamingSessionId);
            writer.WriteString("symbol", symbol);

            writer.WriteEndObject();
            writer.Flush();
        }

        return Encoding.UTF8.GetString(memoryStream.ToArray());
    }

    public string CreateSubscribeKeepAliveCommandStreaming()
    {
        using var memoryStream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(memoryStream))
        {
            writer.WriteStartObject();

            writer.WriteString("command", "getKeepAlive");
            writer.WriteString("streamSessionId", StreamingSessionId);


            writer.WriteEndObject();
            writer.Flush();
        }

        return Encoding.UTF8.GetString(memoryStream.ToArray());
    }

    public string CreateStopKeepAliveCommandStreaming()
    {
        using var memoryStream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(memoryStream))
        {
            writer.WriteStartObject();

            writer.WriteString("command", "stopKeepAlive");
            writer.WriteString("streamSessionId", StreamingSessionId);

            writer.WriteEndObject();
            writer.Flush();
        }

        return Encoding.UTF8.GetString(memoryStream.ToArray());
    }

    public string CreateSubscribeNewsCommandStreaming()
    {
        using var memoryStream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(memoryStream))
        {
            writer.WriteStartObject();

            writer.WriteString("command", "getNews");
            writer.WriteString("streamSessionId", StreamingSessionId);


            writer.WriteEndObject();
            writer.Flush();
        }

        return Encoding.UTF8.GetString(memoryStream.ToArray());
    }

    public string CreateStopNewsCommandStreaming()
    {
        using var memoryStream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(memoryStream))
        {
            writer.WriteStartObject();

            writer.WriteString("command", "stopNews");

            writer.WriteEndObject();
            writer.Flush();
        }

        return Encoding.UTF8.GetString(memoryStream.ToArray());
    }

    public string CreateSubscribeProfitsCommandStreaming()
    {
        using var memoryStream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(memoryStream))
        {
            writer.WriteStartObject();

            writer.WriteString("command", "getProfits");
            writer.WriteString("streamSessionId", StreamingSessionId);


            writer.WriteEndObject();
            writer.Flush();
        }

        return Encoding.UTF8.GetString(memoryStream.ToArray());
    }

    public string CreateStopProfitsCommandStreaming()
    {
        using var memoryStream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(memoryStream))
        {
            writer.WriteStartObject();

            writer.WriteString("command", "stopProfits");
            writer.WriteString("streamSessionId", StreamingSessionId);


            writer.WriteEndObject();
            writer.Flush();
        }

        return Encoding.UTF8.GetString(memoryStream.ToArray());
    }

    public string CreateTickPricesCommandStreaming(string symbol)
    {
        using var memoryStream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(memoryStream))
        {
            writer.WriteStartObject();

            writer.WriteString("command", "getTickPrices");
            writer.WriteString("streamSessionId", StreamingSessionId);
            writer.WriteString("symbol", symbol);
            writer.WriteNumber("minArrivalTime", 0);
            writer.WriteNumber("maxLevel", 0);

            writer.WriteEndObject();
            writer.Flush();
        }


        return Encoding.UTF8.GetString(memoryStream.ToArray());
    }

    public string CreateStopTickPriceCommandStreaming(string symbol)
    {
        using var memoryStream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(memoryStream))
        {
            writer.WriteStartObject();

            writer.WriteString("command", "stopTickPrices");
            writer.WriteString("symbol", symbol);
            writer.WriteString("streamSessionId", StreamingSessionId);


            writer.WriteEndObject();
            writer.Flush();
        }

        return Encoding.UTF8.GetString(memoryStream.ToArray());
    }

    public string CreateTradesCommandStreaming()
    {
        using var memoryStream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(memoryStream))
        {
            writer.WriteStartObject();

            writer.WriteString("command", "getTrades");
            writer.WriteString("streamSessionId", StreamingSessionId);

            writer.WriteEndObject();
            writer.Flush();
        }

        return Encoding.UTF8.GetString(memoryStream.ToArray());
    }

    public string CreateStopTradesCommandStreaming()
    {
        using var memoryStream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(memoryStream))
        {
            writer.WriteStartObject();

            writer.WriteString("command", "stopTrades");

            writer.WriteEndObject();
            writer.Flush();
        }

        return Encoding.UTF8.GetString(memoryStream.ToArray());
    }

    public string CreateTradeStatusCommandStreaming()
    {
        using var memoryStream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(memoryStream))
        {
            writer.WriteStartObject();

            writer.WriteString("command", "getTradeStatus");
            writer.WriteString("streamSessionId", StreamingSessionId);

            writer.WriteEndObject();
            writer.Flush();
        }

        return Encoding.UTF8.GetString(memoryStream.ToArray());
    }

    public string CreateStopTradeStatusCommandStreaming()
    {
        using var memoryStream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(memoryStream))
        {
            writer.WriteStartObject();

            writer.WriteString("command", "stopTradeStatus");

            writer.WriteEndObject();
            writer.Flush();
        }

        return Encoding.UTF8.GetString(memoryStream.ToArray());
    }

    public string CreatePingCommandStreaming()
    {
        using var memoryStream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(memoryStream))
        {
            writer.WriteStartObject();

            writer.WriteString("command", "ping");
            writer.WriteString("streamSessionId", StreamingSessionId);
            writer.WriteEndObject();
            writer.Flush();
        }

        return Encoding.UTF8.GetString(memoryStream.ToArray());
    }

    public string CreateStopPingCommandStreaming()
    {
        throw new NotImplementedException();
    }


    private string CreateTradeTransactionCommand(Position position, decimal price, long? typeCode)
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        var order = position.Order?.Split('|');

        if (order is null && typeCode != TRADE_TRANSACTION_TYPE.ORDER_OPEN.Code)
            throw new ApiProvidersException($"Order is not defined in {position}");

        writer.WriteStartObject();
        writer.WriteStartObject("tradeTransInfo");

        writer.WriteNumber("cmd", ToXtbAssembler.ToTradeOperationCode(position.TypePosition));
        writer.WriteNumber("type", typeCode.GetValueOrDefault());
        writer.WriteNumber("price", price);
        writer.WriteNumber("sl", position.StopLoss);
        writer.WriteNumber("tp", position.TakeProfit);
        writer.WriteString("symbol", position.Symbol);
        writer.WriteNumber("volume", position.Volume);
        writer.WriteNumber("order",
            typeCode != TRADE_TRANSACTION_TYPE.ORDER_OPEN.Code ? long.Parse(order[0]) : 0);
        writer.WriteString("customComment", position.PositionStrategyReferenceId);
        writer.WriteNumber("expiration", 0);

        writer.WriteEndObject();
        writer.WriteEndObject();

        writer.Flush();

        using var doc = JsonDocument.Parse(stream.ToArray());

        return WriteBaseCommand("tradeTransaction", doc.RootElement);
    }

    private string WriteBaseCommand(string commandName, JsonElement? arguments, bool prettyPrint = true)
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        writer.WriteStartObject();
        writer.WriteString("command", commandName);
        writer.WriteBoolean("prettyPrint", prettyPrint);

        if (arguments.HasValue && arguments.Value.ValueKind != JsonValueKind.Undefined &&
            arguments.Value.ValueKind != JsonValueKind.Null)
        {
            writer.WriteStartObject("arguments");
            foreach (var property in arguments.Value.EnumerateObject()) property.WriteTo(writer);
            writer.WriteEndObject();
        }

        writer.WriteString("customTag", CustomTagUtils.Next());
        writer.WriteEndObject();
        writer.Flush();

        return Encoding.UTF8.GetString(stream.ToArray());
    }
}