using FluentAssertions;
using Moq;
using RobotAppLibrary.Api.Interfaces;
using RobotAppLibrary.Api.Modeles;
using RobotAppLibrary.Api.Providers;
using RobotAppLibrary.Api.Providers.Base;
using RobotAppLibrary.Api.Providers.Exceptions;
using RobotAppLibrary.Modeles;
using Serilog;
using Range = Moq.Range;

namespace RobotAppLibrary.Tests.Api.Providers;

internal class TestApiHandler(ICommandExecutor commandExecutor, ILogger logger, TimeSpan pingInterval)
    : ApiProviderBase(commandExecutor, logger, pingInterval)
{
    public override ApiProviderEnum ApiProviderName { get; }
}

public class ApiProviderBaseTest
{
    private readonly TestApiHandler _apiHandler;
    private readonly Mock<ICommandExecutor> _mockCommandExecutor;
    private readonly Mock<ILogger> _mockLogger;
    private readonly TimeSpan _pingInterval;

    public ApiProviderBaseTest()
    {
        _mockCommandExecutor = new Mock<ICommandExecutor>();
        _mockLogger = new Mock<ILogger>();
        _mockLogger.Setup(x => x.ForContext<ApiProviderBase>())
            .Returns(_mockLogger.Object);
        _pingInterval = TimeSpan.FromSeconds(30);
        _apiHandler = new TestApiHandler(_mockCommandExecutor.Object, _mockLogger.Object, _pingInterval);
    }

    #region Balance event

    [Fact]
    public void Test_BalanceEvent()
    {
        var caller = false;

        var accountBalance = new AccountBalance
        {
            Balance = 10,
            Credit = 10,
            Equity = 10,
            Margin = 10,
            MarginFree = 10,
            MarginLevel = 10
        };

        _apiHandler.NewBalanceEvent += (sender, balance) =>
        {
            caller = true;
            balance.Balance.Should().Be(10);
            balance.Credit.Should().Be(10);
            balance.Equity.Should().Be(10);
            balance.Margin.Should().Be(10);
            balance.MarginFree.Should().Be(10);
            balance.MarginLevel.Should().Be(10);
        };

        _mockCommandExecutor.Raise(x => x.BalanceRecordReceived += null, accountBalance);

        caller.Should().BeTrue();
    }

    #endregion

    #region OnDisconnected

    [Fact]
    public void Test_OnDisconnectedEvent()
    {
        var caller = false;
        _apiHandler.Disconnected += (sender, args) => caller = true;

        _mockCommandExecutor.Raise(x => x.Disconnected += null, this, EventArgs.Empty);

        caller.Should().BeTrue();
    }

    #endregion


    #region Constructor Test

    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Arrange
        var handler = _apiHandler;

        // Act
        var name = handler.Name;

        // Assert
        name.Should().Be("TestApiHandler");

        handler.AllSymbols.Should().BeEmpty();
        handler.AccountBalance.Should().NotBeNull();
        handler.PingInterval.Should().Be(_pingInterval);
    }

    [Fact]
    public void Constructor_ShouldSetUpEventHandlers()
    {
        // Arrange
        var handler = _apiHandler;

        // Act & Assert
        _mockCommandExecutor.VerifyAdd(m => m.BalanceRecordReceived += It.IsAny<Action<AccountBalance>>(), Times.Once);
        _mockCommandExecutor.VerifyAdd(m => m.NewsRecordReceived += It.IsAny<Action<News>>(), Times.Once);
        _mockCommandExecutor.VerifyAdd(m => m.TickRecordReceived += It.IsAny<Action<Tick>>(), Times.Once);
        _mockCommandExecutor.VerifyAdd(m => m.TradeRecordReceived += It.IsAny<Action<Position>>(), Times.Once);
        _mockCommandExecutor.VerifyAdd(m => m.Disconnected += It.IsAny<EventHandler>(), Times.Once);
    }

    #endregion

    #region connect

    [Fact]
    public async void Test_Connect_Async()
    {
        // Arrange
        _mockCommandExecutor.Setup(x => x.ExecuteIsConnected()).Returns(true);

        // Act
        await _apiHandler.ConnectAsync(new Credentials());

        // Assert

        _mockCommandExecutor.Verify(x => x.ExecuteLoginCommand(It.IsAny<Credentials>()), Times.Once);
        _mockCommandExecutor.Verify(x => x.ExecuteSubscribeBalanceCommandStreaming(), Times.Once);
        _mockCommandExecutor.Verify(x => x.ExecuteTradesCommandStreaming(), Times.Once);
        _mockCommandExecutor.Verify(x => x.ExecuteSubscribeNewsCommandStreaming(), Times.Once);
        _mockCommandExecutor.Verify(x => x.ExecutePingCommand(), Times.Between(0, 1, Range.Inclusive));
        _mockCommandExecutor.Verify(x => x.ExecutePingCommandStreaming(), Times.Between(0, 1, Range.Inclusive));
    }


    [Fact]
    public async void Test_Connect_Async_Exception()
    {
        // Arrange
        _mockCommandExecutor.Setup(x => x.ExecuteLoginCommand(It.IsAny<Credentials>())).ThrowsAsync(new Exception());

        // Act
        var act = async () => await _apiHandler.ConnectAsync(new Credentials());

        // Assert

        await act.Should().ThrowAsync<ApiProvidersException>();
    }

    #endregion

    #region Disconnect

    [Fact]
    public async void Test_Disconnect_Success()
    {
        // Arrange and Act
        await _apiHandler.DisconnectAsync();

        // Assert
        _mockCommandExecutor.Verify(x => x.ExecuteLogoutCommand(), Times.Once);
    }

    [Fact]
    public async void Test_Disconnect_Throw_Exception()
    {
        // Arrange
        _mockCommandExecutor.Setup(x => x.ExecuteLogoutCommand()).ThrowsAsync(new Exception());

        // Act
        var act = () => _apiHandler.DisconnectAsync();

        // Assert
        await act.Should().ThrowAsync<ApiProvidersException>();
    }

    #endregion

    #region IsConnected

    [Fact]
    public async void Test_IsConnected_Success()
    {
        // Arrange


        //  Act
        var result = _apiHandler.IsConnected();

        // assert
        _mockCommandExecutor.Verify(x => x.ExecuteIsConnected(), Times.AtLeastOnce);
    }

    [Fact]
    public async void Test_IsConnected_Throw_Exception()
    {
        // Arrange
        _mockCommandExecutor.Setup(x => x.ExecuteIsConnected())
            .Throws(new Exception());

        //  Act
        var result = () => _apiHandler.IsConnected();

        // assert
        result.Should().Throw<ApiProvidersException>();
    }

    #endregion

    #region Ping async

    [Fact]
    public async void Test_Ping_IsConnected()
    {
        // Arrange
        _mockCommandExecutor.Setup(x => x.ExecuteIsConnected()).Returns(true);

        // Act
        await _apiHandler.PingAsync();

        // Assert
        _mockCommandExecutor.Verify(x => x.ExecutePingCommand(), Times.AtLeastOnce);
        _apiHandler.LastPing.Should().NotBeSameDateAs(default);
    }

    [Fact]
    public async void Test_Ping_IsConnected_true()
    {
        // Arrange
        _mockCommandExecutor.Setup(x => x.ExecuteIsConnected()).Returns(false);

        // Act
        await _apiHandler.PingAsync();

        // Assert
        _mockCommandExecutor.Verify(x => x.ExecutePingCommand(), Times.Once);
    }

    [Fact]
    public async void Test_Ping_IsConnected_Exception()
    {
        // Arrange
        _mockCommandExecutor.Setup(x => x.ExecuteIsConnected()).Returns(true);

        _mockCommandExecutor.Setup(x => x.ExecutePingCommand()).ThrowsAsync(new Exception());

        // Act
        await _apiHandler.PingAsync();

        // Assert
        _mockLogger.Verify(x => x.Error(It.IsAny<Exception>(), It.IsAny<string>()), Times.Exactly(1));
    }

    #endregion

    #region Get balance

    [Fact]
    public async void Test_GetBalance_Success()
    {
        // Arrange and act
        await _apiHandler.GetBalanceAsync();

        // Assert
        _mockCommandExecutor.Verify(x => x.ExecuteBalanceAccountCommand(), Times.Once);
    }


    [Fact]
    public async void Test_GetBalance_Throw_Exception()
    {
        // Arrange
        _mockCommandExecutor.Setup(x => x.ExecuteBalanceAccountCommand())
            .ThrowsAsync(new Exception());

        // Arrange and act
        var act = () => _apiHandler.GetBalanceAsync();

        // Assert
        await act.Should().ThrowAsync<ApiProvidersException>();
    }

    #endregion

    #region GetCalendar

    [Fact]
    public async Task Test_GetCalendarAsync_Success()
    {
        // Arrange
        var expectedCalendarData = new List<CalendarEvent>
        {
            new()
            {
                /* ... properties ... */
            },
            new()
            {
                /* ... properties ... */
            }
        };
        _mockCommandExecutor.Setup(x => x.ExecuteCalendarCommand())
            .ReturnsAsync(expectedCalendarData);

        // Act
        var calendarData = await _apiHandler.GetCalendarAsync();

        // Assert
        calendarData.Should().BeEquivalentTo(expectedCalendarData);
        _mockCommandExecutor.Verify(x => x.ExecuteCalendarCommand(), Times.Once);
    }

    [Fact]
    public async Task Test_GetCalendarAsync_Throw_Exception()
    {
        // Arrange
        _mockCommandExecutor.Setup(x => x.ExecuteCalendarCommand())
            .ThrowsAsync(new Exception());

        // Act & Assert
        Func<Task> act = async () => await _apiHandler.GetCalendarAsync();
        await act.Should().ThrowAsync<ApiProvidersException>();

        _mockCommandExecutor.Verify(x => x.ExecuteCalendarCommand(), Times.Once);
    }

    #endregion

    #region GetAllSymbols

    [Fact]
    public async Task Test_GetAllSymbolsAsync_Success_WithEmptyCache()
    {
        // Arrange
        var expectedSymbols = new List<SymbolInfo>
        {
            new()
            {
                Symbol = "test",
                TickSize = 1
            },
            new()
            {
                Symbol = "test",
                TickSize = 1
            }
        };
        // Assuming AllSymbols is a property or field that can be manipulated for the test
        _apiHandler.AllSymbols = new List<SymbolInfo>(); // Start with empty cache
        _mockCommandExecutor.Setup(x => x.ExecuteAllSymbolsCommand())
            .ReturnsAsync(expectedSymbols);

        // Act
        var symbols = await _apiHandler.GetAllSymbolsAsync();

        // Assert
        symbols.Should().BeEquivalentTo(expectedSymbols);
        _mockCommandExecutor.Verify(x => x.ExecuteAllSymbolsCommand(), Times.Once);
    }

    [Fact]
    public async Task Test_GetAllSymbolsAsync_Success_WithPopulatedCache()
    {
        // Arrange
        var cachedSymbols = new List<SymbolInfo>
        {
            new()
            {
                Symbol = "test",
                TickSize = 1
            },
            new()
            {
                Symbol = "test",
                TickSize = 1
            }
        };
        // Set the cache with already retrieved symbols
        _apiHandler.AllSymbols = cachedSymbols;

        // Act
        var symbols = await _apiHandler.GetAllSymbolsAsync();

        // Assert
        symbols.Should().BeEquivalentTo(cachedSymbols);
        _mockCommandExecutor.Verify(x => x.ExecuteAllSymbolsCommand(),
            Times.Never); // Command should not be called if cache is populated
    }

    [Fact]
    public async Task Test_GetAllSymbolsAsync_Throw_Exception()
    {
        // Arrange
        _apiHandler.AllSymbols = new List<SymbolInfo>(); // Start with empty cache
        _mockCommandExecutor.Setup(x => x.ExecuteAllSymbolsCommand())
            .ThrowsAsync(new Exception());

        // Act & Assert
        Func<Task> act = async () => await _apiHandler.GetAllSymbolsAsync();
        await act.Should().ThrowAsync<ApiProvidersException>();

        _mockCommandExecutor.Verify(x => x.ExecuteAllSymbolsCommand(), Times.Once);
    }

    #endregion

    #region GetCurrentTradesAsync

    [Fact]
    public async Task Test_GetCurrentTradesAsync_Success()
    {
        // Arrange
        var comment = "testComment";
        var expectedTrades = new Position();
        _mockCommandExecutor.Setup(x => x.ExecuteTradesOpenedTradesCommand(comment))
            .ReturnsAsync(expectedTrades);

        // Act
        var trades = await _apiHandler.GetOpenedTradesAsync(comment);

        // Assert
        trades.Should().BeEquivalentTo(expectedTrades);
        _mockCommandExecutor.Verify(x => x.ExecuteTradesOpenedTradesCommand(comment), Times.Once);
    }

    [Fact]
    public async Task Test_GetCurrentTradesAsync_Throw_Exception()
    {
        // Arrange
        var comment = "testComment";
        _mockCommandExecutor.Setup(x => x.ExecuteTradesOpenedTradesCommand(comment))
            .ThrowsAsync(new Exception());

        // Act & Assert
        Func<Task> act = async () => await _apiHandler.GetOpenedTradesAsync(comment);
        await act.Should().ThrowAsync<ApiProvidersException>();

        _mockCommandExecutor.Verify(x => x.ExecuteTradesOpenedTradesCommand(comment), Times.Once);
    }

    #endregion

    #region GetAllPositionsByCommentAsync

    [Fact]
    public async Task Test_GetAllPositionsByCommentAsync_Success()
    {
        // Arrange
        var comment = "testComment";
        var expectedPositions = new List<Position>
        {
            // ...initialize mock positions...
        };
        _mockCommandExecutor.Setup(x => x.ExecuteTradesHistoryCommand(comment))
            .ReturnsAsync(expectedPositions);

        // Act
        var positions = await _apiHandler.GetAllPositionsByCommentAsync(comment);

        // Assert
        positions.Should().BeEquivalentTo(expectedPositions);
        _mockCommandExecutor.Verify(x => x.ExecuteTradesHistoryCommand(comment), Times.Once);
    }

    [Fact]
    public async Task Test_GetAllPositionsByCommentAsync_Throw_Exception()
    {
        // Arrange
        var comment = "testComment";
        _mockCommandExecutor.Setup(x => x.ExecuteTradesHistoryCommand(comment))
            .ThrowsAsync(new Exception());

        // Act & Assert
        Func<Task> act = async () => await _apiHandler.GetAllPositionsByCommentAsync(comment);
        await act.Should().ThrowAsync<ApiProvidersException>();

        _mockCommandExecutor.Verify(x => x.ExecuteTradesHistoryCommand(comment), Times.Once);
    }

    #endregion

    #region GetSymbolInformationAsync

    [Fact]
    public async Task Test_GetSymbolInformationAsync_Success_WithCache()
    {
        // Arrange
        var symbol = "testSymbol";
        var expectedSymbolInfo = new SymbolInfo { Symbol = symbol, TickSize = 1 };
        _apiHandler.AllSymbols = new List<SymbolInfo> { expectedSymbolInfo };

        // Act
        var symbolInfo = await _apiHandler.GetSymbolInformationAsync(symbol);

        // Assert
        symbolInfo.Should().BeEquivalentTo(expectedSymbolInfo);
        _mockCommandExecutor.Verify(x => x.ExecuteSymbolCommand(It.IsAny<string>()),
            Times.Never); // Verify that the command executor was not called
    }

    [Fact]
    public async Task Test_GetSymbolInformationAsync_Success_WithoutCache()
    {
        // Arrange
        var symbol = "testSymbol";
        var expectedSymbolInfo = new SymbolInfo { Symbol = symbol, TickSize = 1 };
        _apiHandler.AllSymbols = new List<SymbolInfo>(); // Cache is empty
        _mockCommandExecutor.Setup(x => x.ExecuteSymbolCommand(symbol))
            .ReturnsAsync(expectedSymbolInfo);

        // Act
        var symbolInfo = await _apiHandler.GetSymbolInformationAsync(symbol);

        // Assert
        symbolInfo.Should().BeEquivalentTo(expectedSymbolInfo);
        _mockCommandExecutor.Verify(x => x.ExecuteSymbolCommand(symbol), Times.Once);
    }

    [Fact]
    public async Task Test_GetSymbolInformationAsync_Throw_Exception()
    {
        // Arrange
        var symbol = "testSymbol";
        _mockCommandExecutor.Setup(x => x.ExecuteSymbolCommand(symbol))
            .ThrowsAsync(new Exception());
        _apiHandler.AllSymbols = new List<SymbolInfo>(); // Cache is empty

        // Act & Assert
        Func<Task> act = async () => await _apiHandler.GetSymbolInformationAsync(symbol);
        await act.Should().ThrowAsync<ApiProvidersException>();

        _mockCommandExecutor.Verify(x => x.ExecuteSymbolCommand(symbol), Times.Once);
    }

    #endregion

    #region GetTradingHoursAsync

    [Fact]
    public async Task Test_GetTradingHoursAsync_Success()
    {
        // Arrange
        var symbol = "testSymbol";
        var expectedTradeHours = new TradeHourRecord
        {
            // ...initialize mock trade hours...
        };
        _mockCommandExecutor.Setup(x => x.ExecuteTradingHoursCommand(symbol))
            .ReturnsAsync(expectedTradeHours);

        // Act
        var tradeHours = await _apiHandler.GetTradingHoursAsync(symbol);

        // Assert
        tradeHours.Should().BeEquivalentTo(expectedTradeHours);
        _mockCommandExecutor.Verify(x => x.ExecuteTradingHoursCommand(symbol), Times.Once);
    }

    [Fact]
    public async Task Test_GetTradingHoursAsync_Throw_Exception()
    {
        // Arrange
        var symbol = "testSymbol";
        _mockCommandExecutor.Setup(x => x.ExecuteTradingHoursCommand(symbol))
            .ThrowsAsync(new Exception());

        // Act & Assert
        Func<Task> act = async () => await _apiHandler.GetTradingHoursAsync(symbol);
        await act.Should().ThrowAsync<ApiProvidersException>();

        _mockCommandExecutor.Verify(x => x.ExecuteTradingHoursCommand(symbol), Times.Once);
    }

    #endregion

    #region GetChartAsync

    [Fact]
    public async Task Test_GetChartAsync_Success()
    {
        // Arrange
        var symbol = "testSymbol";
        var timeframe = Timeframe.OneHour;
        var start = DateTime.Now;
        var chartRequest = new ChartRequest()
        {
            Symbol = symbol,
            Timeframe = timeframe,
            Start = start // Utilisez une date valide
        };
        var expectedCandles = new List<Candle>
        {
            // Add expected Candle objects here
        };

        _mockCommandExecutor
            .Setup(x => x.ExecuteFullChartCommand(It.IsAny<ChartRequest>()))
            .ReturnsAsync(expectedCandles);

        // Act
        var candles = await _apiHandler.GetChartAsync(chartRequest);

        // Assert
        candles.Should().BeEquivalentTo(expectedCandles);
        _mockCommandExecutor.Verify(x => x.ExecuteFullChartCommand(It.Is<ChartRequest>(c =>
                c.Symbol == symbol && 
                c.Timeframe == timeframe &&
                c.Start == start && c.End == null)),
            Times.Once);
    }


    [Fact]
    public async Task Test_GetChartAsync_Throw_Exception()
    {
        // Arrange
        var symbol = "testSymbol";
        var timeframe = Timeframe.OneHour;
        _mockCommandExecutor.Setup(x => x.ExecuteFullChartCommand(It.IsAny<ChartRequest>()))
            .ThrowsAsync(new Exception());

        // Act & Assert
        Func<Task> act = async () => await _apiHandler.GetChartAsync(new ChartRequest());
        await act.Should().ThrowAsync<ApiProvidersException>();

        _mockCommandExecutor.Verify(x => x.ExecuteFullChartCommand(It.IsAny<ChartRequest>()),
            Times.Once);
    }

    #endregion

    #region GetChartByDateAsync

    [Fact]
    public async Task Test_GetChartByDateAsync_Success()
    {
        // Arrange
        var symbol = "testSymbol";
        var timeframe = Timeframe.OneHour;
        var start = new DateTime(2023, 1, 1);
        var end = new DateTime(2023, 1, 31);
        var chartRequest = new ChartRequest
        {
            Symbol = symbol,
            Timeframe = timeframe,
            Start = start,
            End = end // Ajoutez End si votre ChartRequest supporte ce champ
        };
        var expectedCandles = new List<Candle>
        {
            // ...initialize mock candles...
        };

        _mockCommandExecutor.Setup(x => x.ExecuteRangeChartCommand(
                It.Is<ChartRequest>(c => 
                    c.Symbol == chartRequest.Symbol &&
                    c.Timeframe == chartRequest.Timeframe &&
                    c.Start == chartRequest.Start &&
                    c.End == chartRequest.End))) // Utilisation de It.Is<ChartRequest> pour matcher l'objet
            .ReturnsAsync(expectedCandles);

        // Act
        var candles = await _apiHandler.GetChartByDateAsync(chartRequest);

        // Assert
        candles.Should().BeEquivalentTo(expectedCandles);
        _mockCommandExecutor.Verify(x => x.ExecuteRangeChartCommand(
            It.Is<ChartRequest>(c => 
                c.Symbol == chartRequest.Symbol &&
                c.Timeframe == chartRequest.Timeframe &&
                c.Start == chartRequest.Start &&
                c.End == chartRequest.End)), Times.Once); // Vérification de l'appel avec l'objet correct
    }


    [Fact]
    public async Task Test_GetChartByDateAsync_Throw_Exception()
    {
        // Arrange
        var symbol = "testSymbol";
        var timeframe = Timeframe.OneHour;
        var start = new DateTime(2023, 1, 1);
        var end = new DateTime(2023, 1, 31);
        _mockCommandExecutor.Setup(x => x.ExecuteRangeChartCommand(It.IsAny<ChartRequest>()))
            .ThrowsAsync(new Exception());

        // Act & Assert
        Func<Task> act = async () => await _apiHandler.GetChartByDateAsync(new ChartRequest());
        await act.Should().ThrowAsync<ApiProvidersException>();

        _mockCommandExecutor.Verify(x => x.ExecuteRangeChartCommand(It.IsAny<ChartRequest>()), Times.Once);
    }

    #endregion

    #region GetTickPriceAsync

    [Fact]
    public async Task Test_GetTickPriceAsync_Success()
    {
        // Arrange
        var symbol = "testSymbol";
        var expectedTick = new Tick
        {
            // ...initialize mock tick data...
        };
        _mockCommandExecutor.Setup(x => x.ExecuteTickCommand(symbol))
            .ReturnsAsync(expectedTick);

        // Act
        var tick = await _apiHandler.GetTickPriceAsync(symbol);

        // Assert
        Assert.Equal(expectedTick, tick);
        _mockCommandExecutor.Verify(x => x.ExecuteTickCommand(symbol), Times.Once);
    }

    [Fact]
    public async Task Test_GetTickPriceAsync_Throw_Exception()
    {
        // Arrange
        var symbol = "testSymbol";
        _mockCommandExecutor.Setup(x => x.ExecuteTickCommand(symbol))
            .ThrowsAsync(new Exception());

        // Act & Assert
        await Assert.ThrowsAsync<ApiProvidersException>(async () =>
            await _apiHandler.GetTickPriceAsync(symbol));
        _mockCommandExecutor.Verify(x => x.ExecuteTickCommand(symbol), Times.Once);
    }

    #endregion

    #region OpenPositionAsync

    [Fact]
    public async Task Test_OpenPositionAsync_Success()
    {
        // Arrange
        var position = new Position
        {
            // ...initialize mock position data...
        };
        var price = 100.00m;
        _mockCommandExecutor.Setup(x => x.ExecuteOpenTradeCommand(position))
            .ReturnsAsync(position);

        // Act
        var result = await _apiHandler.OpenPositionAsync(position);

        // Assert
        Assert.Equal(position, result);
        _mockCommandExecutor.Verify(x => x.ExecuteOpenTradeCommand(position), Times.Once);
        // You may also want to verify that position is added to CachePosition, if that's observable or can be checked.
    }

    [Fact]
    public async Task Test_OpenPositionAsync_Throw_Exception()
    {
        // Arrange
        var position = new Position
        {
            // ...initialize mock position data...
        };
        var price = 100.00m;
        _mockCommandExecutor.Setup(x => x.ExecuteOpenTradeCommand(position))
            .ThrowsAsync(new Exception());

        // Act & Assert
        await Assert.ThrowsAsync<ApiProvidersException>(async () =>
            await _apiHandler.OpenPositionAsync(position));
        _mockCommandExecutor.Verify(x => x.ExecuteOpenTradeCommand(position), Times.Once);
        // Additional check can be done to ensure CachePosition doesn't contain the position after the exception.
    }

    #endregion

    #region UpdatePositionAsync

    [Fact]
    public async Task Test_UpdatePositionAsync_Success()
    {
        // Arrange
        var position = new Position
        {
            // ...initialize mock position data...
        };
        var price = 100.00m;
        _mockCommandExecutor.Setup(x => x.ExecuteUpdateTradeCommand(position))
            .ReturnsAsync(new Position());

        // Act
        await _apiHandler.UpdatePositionAsync(position);

        // Assert
        _mockCommandExecutor.Verify(x => x.ExecuteUpdateTradeCommand(position), Times.Once);
    }

    [Fact]
    public async Task Test_UpdatePositionAsync_Throw_Exception()
    {
        // Arrange
        var position = new Position
        {
            // ...initialize mock position data...
        };
        var price = 100.00m;
        _mockCommandExecutor.Setup(x => x.ExecuteUpdateTradeCommand(position))
            .ThrowsAsync(new Exception());

        // Act & Assert
        await Assert.ThrowsAsync<ApiProvidersException>(async () =>
            await _apiHandler.UpdatePositionAsync(position));
        _mockCommandExecutor.Verify(x => x.ExecuteUpdateTradeCommand(position), Times.Once);
    }

    #endregion

    #region ClosePositionAsync

    [Fact]
    public async Task Test_ClosePositionAsync_Success()
    {
        // Arrange
        var position = new Position
        {
            // ...initialize mock position data...
        };
        var price = 100.00m;
        _mockCommandExecutor.Setup(x => x.ExecuteCloseTradeCommand(position))
            .ReturnsAsync(new Position());

        // Act
        await _apiHandler.ClosePositionAsync(position);

        // Assert
        _mockCommandExecutor.Verify(x => x.ExecuteCloseTradeCommand(position), Times.Once);
    }

    [Fact]
    public async Task Test_ClosePositionAsync_Throw_Exception()
    {
        // Arrange
        var position = new Position
        {
            // ...initialize mock position data...
        };
        var price = 100.00m;
        _mockCommandExecutor.Setup(x => x.ExecuteCloseTradeCommand(position))
            .ThrowsAsync(new Exception());

        // Act & Assert
        var act = () => _apiHandler.ClosePositionAsync(position);
        await act.Should().ThrowAsync<ApiProvidersException>();
    }

    #endregion

    #region SubscribePrice

    [Fact]
    public void Test_SubscribePrice_Success()
    {
        // Arrange
        var symbol = "AAPL";
        _mockCommandExecutor.Setup(x => x.ExecuteTickPricesCommandStreaming(symbol));

        // Act
        _apiHandler.SubscribePrice(symbol);

        // Assert
        _mockCommandExecutor.Verify(x => x.ExecuteTickPricesCommandStreaming(symbol), Times.Once);
    }

    [Fact]
    public void Test_SubscribePrice_Throw_Exception()
    {
        // Arrange
        var symbol = "AAPL";
        _mockCommandExecutor.Setup(x => x.ExecuteTickPricesCommandStreaming(symbol))
            .Throws(new Exception());

        // Act & Assert
        var ex = Assert.Throws<ApiProvidersException>(() => _apiHandler.SubscribePrice(symbol));
        Assert.Contains($"Error on  {nameof(_apiHandler.SubscribePrice)}", ex.Message);
        _mockCommandExecutor.Verify(x => x.ExecuteTickPricesCommandStreaming(symbol), Times.Once);
    }

    #endregion

    #region UnsubscribePrice

    [Fact]
    public void Test_UnsubscribePrice_Success()
    {
        // Arrange
        var symbol = "AAPL";
        _mockCommandExecutor.Setup(x => x.ExecuteStopTickPriceCommandStreaming(symbol));

        // Act
        _apiHandler.UnsubscribePrice(symbol);

        // Assert
        _mockCommandExecutor.Verify(x => x.ExecuteStopTickPriceCommandStreaming(symbol), Times.Once);
    }

    [Fact]
    public void Test_UnsubscribePrice_Throw_Exception()
    {
        // Arrange
        var symbol = "AAPL";
        _mockCommandExecutor.Setup(x => x.ExecuteStopTickPriceCommandStreaming(symbol))
            .Throws(new Exception());

        // Act & Assert
        var ex = Assert.Throws<ApiProvidersException>(() => _apiHandler.UnsubscribePrice(symbol));
        Assert.Contains($"Error on  {nameof(_apiHandler.UnsubscribePrice)}", ex.Message);
        _mockCommandExecutor.Verify(x => x.ExecuteStopTickPriceCommandStreaming(symbol), Times.Once);
    }

    #endregion

    #region Position state event

    // [Fact]
    // public void Test_NoCall_PositionState_No_Custom_Comment()
    // {
    //     // Arrange
    //     var caller = false;
    //     var position = new Position
    //     {
    //         StatusPosition = StatusPosition.Open
    //     };
    //
    //     _apiHandler.Object.PositionRejectedEvent += (sender, position1) => caller = true;
    //     _apiHandler.Object.PositionOpenedEvent += (sender, position1) => caller = true;
    //     _apiHandler.Object.PositionUpdatedEvent += (sender, position1) => caller = true;
    //     _apiHandler.Object.PositionClosedEvent += (sender, position1) => caller = true;
    //
    //     // Act
    //     _mockCommandExecutor.Raise(x => x.TcpStreamingConnector.TradeRecordReceived += null, position);
    //
    //     // Assert
    //     caller.Should().BeFalse();
    // }

    [Fact]
    public void Test_PositionState_Pending()
    {
        // Arrange
        var caller = false;
        var position = new Position
        {
            StatusPosition = StatusPosition.Pending
        };

        _apiHandler.PositionRejectedEvent += (sender, position1) => caller = true;
        _apiHandler.PositionOpenedEvent += (sender, position1) => caller = true;
        _apiHandler.PositionUpdatedEvent += (sender, position1) => caller = true;
        _apiHandler.PositionClosedEvent += (sender, position1) => caller = true;

        // Act
        _mockCommandExecutor.Raise(x => x.TradeRecordReceived += null, position);

        // Assert
        caller.Should().BeFalse();
    }

    [Fact]
    public async Task Test_PositionState_Open()
    {
        // Arrange
        var caller = false;
        var position = new Position
        {
            StrategyId = "1",
            Id = "1",
            DateOpen = new DateTime(2022,01,01),
            StopLoss = 1,
            TakeProfit = 1,
            OpenPrice = 100,
        };

        _mockCommandExecutor.Setup(x => x.ExecuteOpenTradeCommand(It.IsAny<Position>()))
            .ReturnsAsync(new Position
            {
                StrategyId = "1",
                Id = "1",
                Order = "orderTest",
            });

        await _apiHandler.OpenPositionAsync(new Position()
        {
            StrategyId = "1",
            Id = "1"
        });


        _apiHandler.PositionOpenedEvent += (sender, position1) =>
        {
            caller = true;
            position1.StatusPosition.Should().Be(StatusPosition.Open);
            position1.Opened.Should().BeTrue();
            position1.DateOpen.Should().Be(new DateTime(2022, 01, 01));
            position1.StopLoss.Should().Be(1);
            position1.TakeProfit.Should().Be(1);
            position1.OpenPrice.Should().Be(100);
        };


        // Act
         _mockCommandExecutor.Raise(x => x.TradeRecordReceived += null, position);

        // Assert
        caller.Should().BeTrue();
    }

    [Fact]
    public async void Test_PositionState_Update()
    {
        // Arrange
        await Test_PositionState_Open();

        var caller = false;
        var position = new Position
        {
            StrategyId = "1",
            Id = "1",
            Profit = 10,
            StopLoss = 11,
            TakeProfit = 12,
            StatusPosition = StatusPosition.Updated,
        };

        _apiHandler.PositionUpdatedEvent += (sender, position1) =>
        {
            position1.Profit.Should().Be(10);
            position1.StopLoss.Should().Be(11);
            position1.TakeProfit.Should().Be(12);
            caller = true;
        };

        // Act
        _mockCommandExecutor.Raise(x => x.TradeRecordReceived += null, position);

        // Assert
        caller.Should().BeTrue();
    }
    
    [Fact]
    public async void Test_PositionState_Update_no_sltp_update()
    {
        // Arrange
        await Test_PositionState_Open();

        var caller = false;
        var position = new Position
        {
            StrategyId = "1",
            Id = "1",
            Profit = 10,
            StopLoss = 0,
            TakeProfit = 0,
            StatusPosition = StatusPosition.Updated
        };

        _apiHandler.PositionUpdatedEvent += (sender, position1) =>
        {
            position1.Profit.Should().Be(10);
            position1.StopLoss.Should().Be(1);
            position1.TakeProfit.Should().Be(1);
            position1.StatusPosition.Should().Be(StatusPosition.Updated);
            caller = true;
        };

        // Act
        _mockCommandExecutor.Raise(x => x.TradeRecordReceived += null, position);

        // Assert
        caller.Should().BeTrue();
    }


    [Fact]
    public async Task Test_PositionState_Close()
    {
        // Arrange
        await Test_PositionState_Open();
        var caller = false;
        var position = new Position
        {
            StrategyId = "1",
            Id = "1",
            DateClose = new DateTime(2022,01,01),
            Profit = 10,
            StatusPosition = StatusPosition.Close,
            
        };

        _apiHandler.PositionClosedEvent += (sender, position1) =>
        {
            caller = true;
            position1.StatusPosition.Should().Be(StatusPosition.Close);
            position1.DateClose = new DateTime(2022, 01, 01);
            position1.Profit = 10;
        };


        // Act
        _mockCommandExecutor.Raise(x => x.TradeRecordReceived += null, position);

        // Assert
        caller.Should().BeTrue();
    }

    [Fact]
    public async Task Test_PositionState_Rejected()
    {
        // Arrange
        await Test_PositionState_Open();
        var caller = false;
        var position = new Position
        {
            StatusPosition = StatusPosition.Rejected,
            StrategyId = "1",
            Id = "1"
        };

        _apiHandler.PositionRejectedEvent += (sender, position1) =>
        {
            position1.Opened.Should().BeFalse();
            position1.StatusPosition.Should().Be(StatusPosition.Rejected);
            caller = true;
        };

        // Act
        _mockCommandExecutor.Raise(x => x.TradeRecordReceived += null, position);

        // Assert
        caller.Should().BeTrue();
    }

    #endregion
}