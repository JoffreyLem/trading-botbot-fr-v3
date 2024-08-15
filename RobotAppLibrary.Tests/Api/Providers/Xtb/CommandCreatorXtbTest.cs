using System.Text.Json;
using FluentAssertions;
using RobotAppLibrary.Api.Modeles;
using RobotAppLibrary.Api.Providers.Xtb;
using RobotAppLibrary.Api.Providers.Xtb.Assembler;
using RobotAppLibrary.Api.Providers.Xtb.Code;
using RobotAppLibrary.Modeles;
using RobotAppLibrary.Utils;

namespace RobotAppLibrary.Tests.Api.Providers.Xtb;

public class CommandCreatorXtbTests
{
    [Fact]
    public void StreamingSessionId_ShouldThrowArgumentException_WhenEmpty()
    {
        // Arrange
        var commandCreator = new CommandCreatorXtb();

        // Act
        var act = () =>
        {
            var id = commandCreator.StreamingSessionId;
        };

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("The streaming session id is empty");
    }

    [Fact]
    public void StreamingSessionId_ShouldReturnSetValue()
    {
        // Arrange
        var commandCreator = new CommandCreatorXtb();
        var expectedSessionId = "test-session-id";

        // Act
        commandCreator.StreamingSessionId = expectedSessionId;
        var actualSessionId = commandCreator.StreamingSessionId;

        // Assert
        actualSessionId.Should().Be(expectedSessionId);
    }

    [Fact]
    public void CreateAllSymbolsCommand_ShouldReturnExpectedJsonString()
    {
        // Arrange
        var commandCreator = new CommandCreatorXtb();

        // Act
        var actualJson = commandCreator.CreateAllSymbolsCommand();

        // Assert
        using var doc = ValidateCommandJson(actualJson, "getAllSymbols");
        // Additional assertions can be added here if needed
    }

    [Fact]
    public void CreateCalendarCommand_ShouldReturnExpectedJsonString()
    {
        // Arrange
        var commandCreator = new CommandCreatorXtb();

        // Act
        var actualJson = commandCreator.CreateCalendarCommand();

        // Assert
        using var doc = ValidateCommandJson(actualJson, "getCalendar");
        // Additional assertions can be added here if needed
    }

    [Fact]
    public void CreateLoginCommand_ShouldReturnExpectedJsonString()
    {
        // Arrange
        var commandCreator = new CommandCreatorXtb();
        var credentials = new Credentials
        {
            User = "testuser",
            Password = "testpassword"
        };

        // Act
        var actualJson = commandCreator.CreateLoginCommand(credentials);

        // Assert
        using var doc = ValidateCommandJson(actualJson, "login");

        var root = doc.RootElement;

        var arguments = root.GetProperty("arguments");
        arguments.GetProperty("userId").GetString().Should().Be(credentials.User);
        arguments.GetProperty("password").GetString().Should().Be(credentials.Password);
        arguments.GetProperty("appId").GetString().Should().Be("botbot");
        arguments.GetProperty("appName").GetString().Should().Be("botbot");
    }

    [Fact]
    public void CreateFullChartCommand_ShouldReturnExpectedJsonString()
    {
        // Arrange
        var commandCreator = new CommandCreatorXtb();
        var timeframe = Timeframe.OneMinute;
        var start = new DateTime(2023, 6, 1);
        var symbol = "EURUSD";
        var expectedPeriodCode = ToXtbAssembler.ToPeriodCode(timeframe);
        var expectedStartUnixTime = ToXtbAssembler.SetDateTimeForChart(timeframe).ConvertToUnixTime();

        // Act
        var actualJson = commandCreator.CreateFullChartCommand(timeframe, start, symbol);

        // Assert
        using var doc = ValidateCommandJson(actualJson, "getChartLastRequest");

        var arguments = doc.RootElement.GetProperty("arguments");
        var info = arguments.GetProperty("info");
        info.GetProperty("symbol").GetString().Should().Be(symbol);
        info.GetProperty("period").GetInt64().Should().BeCloseTo(expectedPeriodCode, 2);
        info.GetProperty("start").GetInt64().Should().BeCloseTo(expectedStartUnixTime, 2);
    }

    [Fact]
    public void CreateRangeChartCommand_ShouldReturnExpectedJsonString()
    {
        // Arrange
        var commandCreator = new CommandCreatorXtb();
        var timeframe = Timeframe.OneMinute;
        var start = new DateTime(2023, 6, 1);
        var end = new DateTime(2023, 6, 2);
        var symbol = "EURUSD";
        var expectedPeriodCode = ToXtbAssembler.ToPeriodCode(timeframe);
        var expectedStartUnixTime = start.ConvertToUnixTime();
        var expectedEndUnixTime = end.ConvertToUnixTime();

        // Act
        var actualJson = commandCreator.CreateRangeChartCommand(timeframe, start, end, symbol);

        // Assert
        using var doc = ValidateCommandJson(actualJson, "getChartRangeRequest");

        var arguments = doc.RootElement.GetProperty("arguments");
        var info = arguments.GetProperty("info");
        info.GetProperty("symbol").GetString().Should().Be(symbol);
        info.GetProperty("period").GetInt64().Should().Be(expectedPeriodCode);
        info.GetProperty("start").GetInt64().Should().Be(expectedStartUnixTime);
        info.GetProperty("end").GetInt64().Should().Be(expectedEndUnixTime);
    }

    [Fact]
    public void CreateLogOutCommand_ShouldReturnExpectedJsonString()
    {
        // Arrange
        var commandCreator = new CommandCreatorXtb();

        // Act
        var actualJson = commandCreator.CreateLogOutCommand();

        // Assert
        using var doc = ValidateCommandJson(actualJson, "logout");
        // Additional assertions can be added here if needed
    }

    [Fact]
    public void CreateBalanceAccountCommand_ShouldReturnExpectedJsonString()
    {
        // Arrange
        var commandCreator = new CommandCreatorXtb();

        // Act
        var actualJson = commandCreator.CreateBalanceAccountCommand();

        // Assert
        using var doc = ValidateCommandJson(actualJson, "getMarginLevel");
        // Additional assertions can be added here if needed
    }

    [Fact]
    public void CreateNewsCommand_ShouldReturnExpectedJsonString()
    {
        // Arrange
        var commandCreator = new CommandCreatorXtb();
        var start = new DateTime(2023, 6, 1);
        var end = new DateTime(2023, 6, 2);
        var expectedStartUnixTime = start.ConvertToUnixTime();
        var expectedEndUnixTime = end.ConvertToUnixTime();

        // Act
        var actualJson = commandCreator.CreateNewsCommand(start, end);

        // Assert
        using var doc = ValidateCommandJson(actualJson, "getNews");

        var root = doc.RootElement;
        var arguments = doc.RootElement.GetProperty("arguments");
        arguments.GetProperty("start").GetInt64().Should().Be(expectedStartUnixTime);
        arguments.GetProperty("end").GetInt64().Should().Be(expectedEndUnixTime);
    }

    [Fact]
    public void CreateCurrentUserDataCommand_ShouldReturnExpectedJsonString()
    {
        // Arrange
        var commandCreator = new CommandCreatorXtb();

        // Act
        var actualJson = commandCreator.CreateCurrentUserDataCommand();

        // Assert
        using var doc = ValidateCommandJson(actualJson, "getCurrentUserData");
        // Additional assertions can be added here if needed
    }

    [Fact]
    public void CreatePingCommand_ShouldReturnExpectedJsonString()
    {
        // Arrange
        var commandCreator = new CommandCreatorXtb();

        // Act
        var actualJson = commandCreator.CreatePingCommand();

        // Assert
        using var doc = ValidateCommandJson(actualJson, "ping");
        // Additional assertions can be added here if needed
    }


    [Fact]
    public void CreateSymbolCommand_ShouldReturnExpectedJsonString()
    {
        // Arrange
        var commandCreator = new CommandCreatorXtb();
        var symbol = "EURUSD";

        // Act
        var actualJson = commandCreator.CreateSymbolCommand(symbol);

        // Assert
        using var doc = ValidateCommandJson(actualJson, "getSymbol");

        var arguments = doc.RootElement.GetProperty("arguments");
        arguments.GetProperty("symbol").GetString().Should().Be(symbol);
    }

    [Fact]
    public void CreateTickCommand_ShouldReturnExpectedJsonString()
    {
        // Arrange
        var commandCreator = new CommandCreatorXtb();
        var symbol = "EURUSD";

        // Act
        var actualJson = commandCreator.CreateTickCommand(symbol);

        // Assert
        using var doc = ValidateCommandJson(actualJson, "getTickPrices");

        var arguments = doc.RootElement.GetProperty("arguments");
        var symbols = arguments.GetProperty("symbols");
        symbols[0].GetString().Should().Be(symbol);
        arguments.GetProperty("timestamp").GetInt64().Should().Be(0);
        arguments.GetProperty("level").GetInt32().Should().Be(0);
    }

    [Fact]
    public void CreateTradesHistoryCommand_ShouldReturnExpectedJsonString()
    {
        // Arrange
        var commandCreator = new CommandCreatorXtb();
        var expectedStartUnixTime = new DateTime(2000, 01, 01).ConvertToUnixTime();
        var expectedEndUnixTime = 0;

        // Act
        var actualJson = commandCreator.CreateTradesHistoryCommand();

        // Assert
        using var doc = ValidateCommandJson(actualJson, "getTradesHistory");

        var arguments = doc.RootElement.GetProperty("arguments");
        arguments.GetProperty("start").GetInt64().Should().Be(expectedStartUnixTime);
        arguments.GetProperty("end").GetInt64().Should().Be(expectedEndUnixTime);
    }

    [Fact]
    public void CreateTradesOpenedTradesCommand_ShouldReturnExpectedJsonString()
    {
        // Arrange
        var commandCreator = new CommandCreatorXtb();

        // Act
        var actualJson = commandCreator.CreateTradesOpenedTradesCommand();

        // Assert
        using var doc = ValidateCommandJson(actualJson, "getTrades");

        var arguments = doc.RootElement.GetProperty("arguments");
        arguments.GetProperty("openedOnly").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public void CreateTradingHoursCommand_ShouldReturnExpectedJsonString()
    {
        // Arrange
        var commandCreator = new CommandCreatorXtb();
        var symbol = "EURUSD";

        // Act
        var actualJson = commandCreator.CreateTradingHoursCommand(symbol);

        // Assert
        using var doc = ValidateCommandJson(actualJson, "getTradingHours");

        var arguments = doc.RootElement.GetProperty("arguments");
        var symbols = arguments.GetProperty("symbols");
        symbols[0].GetString().Should().Be(symbol);
    }


    [Fact]
    public void CreateOpenTradeCommande_ShouldReturnExpectedJsonString()
    {
        // Arrange
        var commandCreator = new CommandCreatorXtb();
        var position = new Position
        {
            TypePosition = TypeOperation.Buy,
            Order = "12345|otherinfo",
            StopLoss = 1.1000m,
            TakeProfit = 1.2000m,
            Symbol = "EURUSD",
            Volume = 1000,
            StrategyId = "strategy123",
            Id = "1"
        };
        var price = 1.2345m;

        // Act
        var actualJson = commandCreator.CreateOpenTradeCommande(position, price);

        // Assert
        using var doc = ValidateCommandJson(actualJson, "tradeTransaction");

        var tradeTransInfo = doc.RootElement.GetProperty("arguments").GetProperty("tradeTransInfo");
        tradeTransInfo.GetProperty("cmd").GetInt64().Should()
            .Be(ToXtbAssembler.ToTradeOperationCode(position.TypePosition));
        tradeTransInfo.GetProperty("type").GetInt64().Should().Be(TRADE_TRANSACTION_TYPE.ORDER_OPEN.Code);
        tradeTransInfo.GetProperty("price").GetDecimal().Should().Be(price);
        tradeTransInfo.GetProperty("sl").GetDecimal().Should().Be(position.StopLoss);
        tradeTransInfo.GetProperty("tp").GetDecimal().Should().Be(position.TakeProfit);
        tradeTransInfo.GetProperty("symbol").GetString().Should().Be(position.Symbol);
        tradeTransInfo.GetProperty("volume").GetDouble().Should().Be(position.Volume);
        tradeTransInfo.GetProperty("order").GetInt64().Should().Be(0);
        tradeTransInfo.GetProperty("customComment").GetString().Should().Be(position.PositionStrategyReferenceId);
        tradeTransInfo.GetProperty("expiration").GetInt64().Should().Be(0);
    }


    [Fact]
    public void CreateUpdateTradeCommande_ShouldReturnExpectedJsonString()
    {
        // Arrange
        var commandCreator = new CommandCreatorXtb();
        var position = new Position
        {
            TypePosition = TypeOperation.Buy,
            Order = "12345|otherinfo",
            StopLoss = 1.1000m,
            TakeProfit = 1.2000m,
            Symbol = "EURUSD",
            Volume = 1000
        };
        var price = 1.2345m;

        // Act
        var actualJson = commandCreator.CreateUpdateTradeCommande(position, price);

        // Assert
        using var doc = ValidateCommandJson(actualJson, "tradeTransaction");

        var tradeTransInfo = doc.RootElement.GetProperty("arguments").GetProperty("tradeTransInfo");
        tradeTransInfo.GetProperty("cmd").GetInt64().Should()
            .Be(ToXtbAssembler.ToTradeOperationCode(position.TypePosition));
        tradeTransInfo.GetProperty("type").GetInt64().Should().Be(TRADE_TRANSACTION_TYPE.ORDER_MODIFY.Code);
        tradeTransInfo.GetProperty("price").GetDecimal().Should().Be(price);
        tradeTransInfo.GetProperty("sl").GetDecimal().Should().Be(position.StopLoss);
        tradeTransInfo.GetProperty("tp").GetDecimal().Should().Be(position.TakeProfit);
        tradeTransInfo.GetProperty("symbol").GetString().Should().Be(position.Symbol);
        tradeTransInfo.GetProperty("volume").GetDouble().Should().Be(position.Volume);
        tradeTransInfo.GetProperty("order").GetInt64().Should().Be(12345);
        tradeTransInfo.GetProperty("customComment").GetString().Should().Be(position.PositionStrategyReferenceId);
        tradeTransInfo.GetProperty("expiration").GetInt64().Should().Be(0);
    }

    [Fact]
    public void CreateCloseTradeCommande_ShouldReturnExpectedJsonString()
    {
        // Arrange
        var commandCreator = new CommandCreatorXtb();
        var position = new Position
        {
            TypePosition = TypeOperation.Buy,
            Order = "12345|otherinfo",
            StopLoss = 1.1000m,
            TakeProfit = 1.2000m,
            Symbol = "EURUSD",
            Volume = 1000
        };
        var price = 1.2345m;

        // Act
        var actualJson = commandCreator.CreateCloseTradeCommande(position, price);

        // Assert
        using var doc = ValidateCommandJson(actualJson, "tradeTransaction");

        var tradeTransInfo = doc.RootElement.GetProperty("arguments").GetProperty("tradeTransInfo");
        tradeTransInfo.GetProperty("cmd").GetInt64().Should()
            .Be(ToXtbAssembler.ToTradeOperationCode(position.TypePosition));
        tradeTransInfo.GetProperty("type").GetInt64().Should().Be(TRADE_TRANSACTION_TYPE.ORDER_CLOSE.Code);
        tradeTransInfo.GetProperty("price").GetDecimal().Should().Be(price);
        tradeTransInfo.GetProperty("sl").GetDecimal().Should().Be(position.StopLoss);
        tradeTransInfo.GetProperty("tp").GetDecimal().Should().Be(position.TakeProfit);
        tradeTransInfo.GetProperty("symbol").GetString().Should().Be(position.Symbol);
        tradeTransInfo.GetProperty("volume").GetDouble().Should().Be(position.Volume);
        tradeTransInfo.GetProperty("order").GetInt64().Should().Be(12345);
        tradeTransInfo.GetProperty("customComment").GetString().Should().Be(position.PositionStrategyReferenceId);
        tradeTransInfo.GetProperty("expiration").GetInt64().Should().Be(0);
    }


    [Fact]
    public void CreateSubscribeBalanceCommandStreaming_ShouldReturnExpectedJsonString()
    {
        // Arrange
        var commandCreator = new CommandCreatorXtb();
        commandCreator.StreamingSessionId = "test-session";

        // Act
        var actualJson = commandCreator.CreateSubscribeBalanceCommandStreaming();

        // Assert
        var doc = JsonDocument.Parse(actualJson);
        var root = doc.RootElement;

        root.GetProperty("command").GetString().Should().Be("getBalance");
        root.GetProperty("streamSessionId").GetString().Should().Be("test-session");
    }

    [Fact]
    public void CreateStopBalanceCommandStreaming_ShouldReturnExpectedJsonString()
    {
        // Arrange
        var commandCreator = new CommandCreatorXtb();

        // Act
        var actualJson = commandCreator.CreateStopBalanceCommandStreaming();

        // Assert
        var doc = JsonDocument.Parse(actualJson);
        var root = doc.RootElement;

        root.GetProperty("command").GetString().Should().Be("stopBalance");
    }

    [Fact]
    public void CreateSubscribeCandleCommandStreaming_ShouldReturnExpectedJsonString()
    {
        // Arrange
        var commandCreator = new CommandCreatorXtb();
        var symbol = "EURUSD";
        commandCreator.StreamingSessionId = "test-session";

        // Act
        var actualJson = commandCreator.CreateSubscribeCandleCommandStreaming(symbol);

        // Assert
        var doc = JsonDocument.Parse(actualJson);
        var root = doc.RootElement;

        root.GetProperty("command").GetString().Should().Be("getCandles");
        root.GetProperty("streamSessionId").GetString().Should().Be("test-session");
        root.GetProperty("symbol").GetString().Should().Be(symbol);
    }

    [Fact]
    public void CreateStopCandleCommandStreaming_ShouldReturnExpectedJsonString()
    {
        // Arrange
        var commandCreator = new CommandCreatorXtb();
        var symbol = "EURUSD";
        commandCreator.StreamingSessionId = "test-session";

        // Act
        var actualJson = commandCreator.CreateStopCandleCommandStreaming(symbol);

        // Assert
        var doc = JsonDocument.Parse(actualJson);
        var root = doc.RootElement;

        root.GetProperty("command").GetString().Should().Be("getCandles");
        root.GetProperty("streamSessionId").GetString().Should().Be("test-session");
        root.GetProperty("symbol").GetString().Should().Be(symbol);
    }

    [Fact]
    public void CreateSubscribeKeepAliveCommandStreaming_ShouldReturnExpectedJsonString()
    {
        // Arrange
        var commandCreator = new CommandCreatorXtb();
        commandCreator.StreamingSessionId = "test-session";

        // Act
        var actualJson = commandCreator.CreateSubscribeKeepAliveCommandStreaming();

        // Assert
        var doc = JsonDocument.Parse(actualJson);
        var root = doc.RootElement;

        root.GetProperty("command").GetString().Should().Be("getKeepAlive");
        root.GetProperty("streamSessionId").GetString().Should().Be("test-session");
    }

    [Fact]
    public void CreateStopKeepAliveCommandStreaming_ShouldReturnExpectedJsonString()
    {
        // Arrange
        var commandCreator = new CommandCreatorXtb();
        commandCreator.StreamingSessionId = "test-session";

        // Act
        var actualJson = commandCreator.CreateStopKeepAliveCommandStreaming();

        // Assert
        var doc = JsonDocument.Parse(actualJson);
        var root = doc.RootElement;

        root.GetProperty("command").GetString().Should().Be("stopKeepAlive");
        root.GetProperty("streamSessionId").GetString().Should().Be("test-session");
    }

    [Fact]
    public void CreateSubscribeNewsCommandStreaming_ShouldReturnExpectedJsonString()
    {
        // Arrange
        var commandCreator = new CommandCreatorXtb();
        commandCreator.StreamingSessionId = "test-session";

        // Act
        var actualJson = commandCreator.CreateSubscribeNewsCommandStreaming();

        // Assert
        var doc = JsonDocument.Parse(actualJson);
        var root = doc.RootElement;

        root.GetProperty("command").GetString().Should().Be("getNews");
        root.GetProperty("streamSessionId").GetString().Should().Be("test-session");
    }

    [Fact]
    public void CreateStopNewsCommandStreaming_ShouldReturnExpectedJsonString()
    {
        // Arrange
        var commandCreator = new CommandCreatorXtb();

        // Act
        var actualJson = commandCreator.CreateStopNewsCommandStreaming();

        // Assert
        var doc = JsonDocument.Parse(actualJson);
        var root = doc.RootElement;

        root.GetProperty("command").GetString().Should().Be("stopNews");
    }

    [Fact]
    public void CreateSubscribeProfitsCommandStreaming_ShouldReturnExpectedJsonString()
    {
        // Arrange
        var commandCreator = new CommandCreatorXtb();
        commandCreator.StreamingSessionId = "test-session";

        // Act
        var actualJson = commandCreator.CreateSubscribeProfitsCommandStreaming();

        // Assert
        var doc = JsonDocument.Parse(actualJson);
        var root = doc.RootElement;

        root.GetProperty("command").GetString().Should().Be("getProfits");
        root.GetProperty("streamSessionId").GetString().Should().Be("test-session");
    }

    [Fact]
    public void CreateStopProfitsCommandStreaming_ShouldReturnExpectedJsonString()
    {
        // Arrange
        var commandCreator = new CommandCreatorXtb();
        commandCreator.StreamingSessionId = "test-session";

        // Act
        var actualJson = commandCreator.CreateStopProfitsCommandStreaming();

        // Assert
        var doc = JsonDocument.Parse(actualJson);
        var root = doc.RootElement;

        root.GetProperty("command").GetString().Should().Be("stopProfits");
        root.GetProperty("streamSessionId").GetString().Should().Be("test-session");
    }

    [Fact]
    public void CreateTickPricesCommandStreaming_ShouldReturnExpectedJsonString()
    {
        // Arrange
        var commandCreator = new CommandCreatorXtb();
        var symbol = "EURUSD";
        commandCreator.StreamingSessionId = "test-session";

        // Act
        var actualJson = commandCreator.CreateTickPricesCommandStreaming(symbol);

        // Assert
        var doc = JsonDocument.Parse(actualJson);
        var root = doc.RootElement;

        root.GetProperty("command").GetString().Should().Be("getTickPrices");
        root.GetProperty("streamSessionId").GetString().Should().Be("test-session");
        root.GetProperty("symbol").GetString().Should().Be(symbol);
        root.GetProperty("minArrivalTime").GetInt32().Should().Be(0);
        root.GetProperty("maxLevel").GetInt32().Should().Be(0);
    }

    [Fact]
    public void CreateStopTickPriceCommandStreaming_ShouldReturnExpectedJsonString()
    {
        // Arrange
        var commandCreator = new CommandCreatorXtb();
        var symbol = "EURUSD";
        commandCreator.StreamingSessionId = "test-session";

        // Act
        var actualJson = commandCreator.CreateStopTickPriceCommandStreaming(symbol);

        // Assert
        var doc = JsonDocument.Parse(actualJson);
        var root = doc.RootElement;

        root.GetProperty("command").GetString().Should().Be("stopTickPrices");
        root.GetProperty("symbol").GetString().Should().Be(symbol);
        root.GetProperty("streamSessionId").GetString().Should().Be("test-session");
    }

    [Fact]
    public void CreateTradesCommandStreaming_ShouldReturnExpectedJsonString()
    {
        // Arrange
        var commandCreator = new CommandCreatorXtb();
        commandCreator.StreamingSessionId = "test-session";

        // Act
        var actualJson = commandCreator.CreateTradesCommandStreaming();

        // Assert
        var doc = JsonDocument.Parse(actualJson);
        var root = doc.RootElement;

        root.GetProperty("command").GetString().Should().Be("getTrades");
        root.GetProperty("streamSessionId").GetString().Should().Be("test-session");
    }

    [Fact]
    public void CreateStopTradesCommandStreaming_ShouldReturnExpectedJsonString()
    {
        // Arrange
        var commandCreator = new CommandCreatorXtb();

        // Act
        var actualJson = commandCreator.CreateStopTradesCommandStreaming();

        // Assert
        var doc = JsonDocument.Parse(actualJson);
        var root = doc.RootElement;

        root.GetProperty("command").GetString().Should().Be("stopTrades");
    }

    [Fact]
    public void CreateTradeStatusCommandStreaming_ShouldReturnExpectedJsonString()
    {
        // Arrange
        var commandCreator = new CommandCreatorXtb();
        commandCreator.StreamingSessionId = "test-session";

        // Act
        var actualJson = commandCreator.CreateTradeStatusCommandStreaming();

        // Assert
        var doc = JsonDocument.Parse(actualJson);
        var root = doc.RootElement;

        root.GetProperty("command").GetString().Should().Be("getTradeStatus");
        root.GetProperty("streamSessionId").GetString().Should().Be("test-session");
    }

    [Fact]
    public void CreateStopTradeStatusCommandStreaming_ShouldReturnExpectedJsonString()
    {
        // Arrange
        var commandCreator = new CommandCreatorXtb();

        // Act
        var actualJson = commandCreator.CreateStopTradeStatusCommandStreaming();

        // Assert
        var doc = JsonDocument.Parse(actualJson);
        var root = doc.RootElement;

        root.GetProperty("command").GetString().Should().Be("stopTradeStatus");
    }

    [Fact]
    public void CreatePingCommandStreaming_ShouldReturnExpectedJsonString()
    {
        // Arrange
        var commandCreator = new CommandCreatorXtb();
        commandCreator.StreamingSessionId = "test-session";

        // Act
        var actualJson = commandCreator.CreatePingCommandStreaming();

        // Assert
        var doc = JsonDocument.Parse(actualJson);
        var root = doc.RootElement;

        root.GetProperty("command").GetString().Should().Be("ping");
        root.GetProperty("streamSessionId").GetString().Should().Be("test-session");
    }


    private JsonDocument ValidateCommandJson(string json, string expectedCommandName)
    {
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        root.GetProperty("command").GetString().Should().Be(expectedCommandName);
        root.GetProperty("prettyPrint").GetBoolean().Should().BeTrue();
        root.GetProperty("customTag").GetString().Should().MatchRegex("^[0-9]+$");

        return doc;
    }
}