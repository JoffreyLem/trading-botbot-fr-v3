using FluentAssertions;
using Moq;
using RobotAppLibrary.Api.Connector.Tcp;
using RobotAppLibrary.Api.Executor;
using RobotAppLibrary.Api.Interfaces;
using RobotAppLibrary.Api.Modeles;
using RobotAppLibrary.Modeles;

namespace RobotAppLibrary.Tests.Api.Executor;

internal class TcpCommandExecutorTest(
    ITcpConnector tcpClient,
    ITcpStreamingConnector tcpStreamingClient,
    ICommandCreator commandCreator,
    IReponseAdapter responseAdapter)
    : TcpCommandExecutor(tcpClient, tcpStreamingClient, commandCreator, responseAdapter);

public class TcpCommandExecutorTests
{
    private const string CommandMock = "CommandMock";
    private readonly Mock<ICommandCreator> _commandCreatorMock;
    private readonly Mock<IReponseAdapter> _responseAdapterMock;
    private readonly TcpCommandExecutor _tcpCommandExecutor;

    private readonly Mock<ITcpConnector> _tcpConnectorMock;
    private readonly Mock<ITcpStreamingConnector> _tcpStreamingConnectorMock;

    public TcpCommandExecutorTests()
    {
        _tcpConnectorMock = new Mock<ITcpConnector>();
        _tcpStreamingConnectorMock = new Mock<ITcpStreamingConnector>();
        _commandCreatorMock = new Mock<ICommandCreator>();
        _responseAdapterMock = new Mock<IReponseAdapter>();

        _tcpCommandExecutor = new TcpCommandExecutorTest(
            _tcpConnectorMock.Object,
            _tcpStreamingConnectorMock.Object,
            _commandCreatorMock.Object,
            _responseAdapterMock.Object);
    }

    [Fact]
    public void TcpCommandExecutor_ShouldInvokeConnectedEvent_WhenTcpClientConnects()
    {
        // Arrange
        var eventInvoked = false;
        _tcpCommandExecutor.Connected += (sender, args) => eventInvoked = true;

        // Act
        _tcpConnectorMock.Raise(m => m.Connected += null, EventArgs.Empty);

        // Assert
        eventInvoked.Should().BeTrue();
    }

    [Fact]
    public void TcpCommandExecutor_ShouldInvokeDisconnectedEvent_WhenTcpClientDisconnects()
    {
        // Arrange
        var eventInvoked = false;
        _tcpCommandExecutor.Disconnected += (sender, args) => eventInvoked = true;

        // Act
        _tcpConnectorMock.Raise(m => m.Disconnected += null, EventArgs.Empty);

        // Assert
        eventInvoked.Should().BeTrue();
    }

    [Fact]
    public void TcpCommandExecutor_ShouldInvokeTickRecordReceivedEvent_WhenTcpStreamingClientReceivesTick()
    {
        // Arrange
        var eventInvoked = false;
        var tickRecord = new Tick();
        _tcpCommandExecutor.TickRecordReceived += tick => eventInvoked = true;

        // Act
        _tcpStreamingConnectorMock.Raise(m => m.TickRecordReceived += null, tickRecord);

        // Assert
        eventInvoked.Should().BeTrue();
    }

    [Fact]
    public void TcpCommandExecutor_ShouldInvokeTradeRecordReceivedEvent_WhenTcpStreamingClientReceivesTrade()
    {
        // Arrange
        var eventInvoked = false;
        var tradeRecord = new Position();
        _tcpCommandExecutor.TradeRecordReceived += trade => eventInvoked = true;

        // Act
        _tcpStreamingConnectorMock.Raise(m => m.TradeRecordReceived += null, tradeRecord);

        // Assert
        eventInvoked.Should().BeTrue();
    }

    [Fact]
    public void TcpCommandExecutor_ShouldInvokeBalanceRecordReceivedEvent_WhenTcpStreamingClientReceivesBalance()
    {
        // Arrange
        var eventInvoked = false;
        var balanceRecord = new AccountBalance();
        _tcpCommandExecutor.BalanceRecordReceived += balance => eventInvoked = true;

        // Act
        _tcpStreamingConnectorMock.Raise(m => m.BalanceRecordReceived += null, balanceRecord);

        // Assert
        eventInvoked.Should().BeTrue();
    }

    [Fact]
    public void TcpCommandExecutor_ShouldInvokeProfitRecordReceivedEvent_WhenTcpStreamingClientReceivesProfit()
    {
        // Arrange
        var eventInvoked = false;
        var profitRecord = new Position();
        _tcpCommandExecutor.ProfitRecordReceived += profit => eventInvoked = true;

        // Act
        _tcpStreamingConnectorMock.Raise(m => m.ProfitRecordReceived += null, profitRecord);

        // Assert
        eventInvoked.Should().BeTrue();
    }

    [Fact]
    public void TcpCommandExecutor_ShouldInvokeNewsRecordReceivedEvent_WhenTcpStreamingClientReceivesNews()
    {
        // Arrange
        var eventInvoked = false;
        var newsRecord = new News();
        _tcpCommandExecutor.NewsRecordReceived += news => eventInvoked = true;

        // Act
        _tcpStreamingConnectorMock.Raise(m => m.NewsRecordReceived += null, newsRecord);

        // Assert
        eventInvoked.Should().BeTrue();
    }

    [Fact]
    public void TcpCommandExecutor_ShouldInvokeKeepAliveRecordReceivedEvent_WhenTcpStreamingClientReceivesKeepAlive()
    {
        // Arrange
        var eventInvoked = false;
        _tcpCommandExecutor.KeepAliveRecordReceived += () => eventInvoked = true;

        // Act
        _tcpStreamingConnectorMock.Raise(m => m.KeepAliveRecordReceived += null);

        // Assert
        eventInvoked.Should().BeTrue();
    }

    [Fact]
    public void TcpCommandExecutor_ShouldInvokeCandleRecordReceivedEvent_WhenTcpStreamingClientReceivesCandle()
    {
        // Arrange
        var eventInvoked = false;
        var candleRecord = new Candle();
        _tcpCommandExecutor.CandleRecordReceived += candle => eventInvoked = true;

        // Act
        _tcpStreamingConnectorMock.Raise(m => m.CandleRecordReceived += null, candleRecord);

        // Assert
        eventInvoked.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteLoginCommand_ShouldConnectAndSendCommand()
    {
        // Arrange
        var credentials = new Credentials();
        _commandCreatorMock.Setup(m => m.CreateLoginCommand(credentials)).Returns(CommandMock);
        _tcpConnectorMock.Setup(m => m.ConnectAsync()).Returns(Task.CompletedTask);
        _tcpConnectorMock.Setup(m => m.SendAndReceiveAsync(CommandMock, It.IsAny<bool>())).Returns(Task.FromResult(""));
        _tcpStreamingConnectorMock.Setup(m => m.ConnectAsync()).Returns(Task.CompletedTask);

        // Act
        await _tcpCommandExecutor.ExecuteLoginCommand(credentials);

        // Assert
        _tcpConnectorMock.Verify(m => m.ConnectAsync(), Times.Once);
        _commandCreatorMock.Verify(m => m.CreateLoginCommand(credentials), Times.Once);
        _tcpConnectorMock.Verify(m => m.SendAndReceiveAsync(CommandMock, true), Times.Once);
        _tcpStreamingConnectorMock.Verify(m => m.ConnectAsync(), Times.Once);
    }

    [Fact]
    public async Task ExecuteLogoutCommand_ShouldSendLogoutCommand()
    {
        // Arrange
        var commandMock = "command";
        _commandCreatorMock.Setup(m => m.CreateLogOutCommand()).Returns(commandMock);
        _tcpConnectorMock.Setup(m => m.SendAndReceiveAsync(commandMock, It.IsAny<bool>())).Returns(Task.FromResult(""));

        // Act
        await _tcpCommandExecutor.ExecuteLogoutCommand();

        // Assert
        _commandCreatorMock.Verify(m => m.CreateLogOutCommand(), Times.Once);
        _tcpConnectorMock.Verify(m => m.SendAndReceiveAsync(commandMock, true), Times.Once);
    }


    [Fact]
    public async Task ExecuteAllSymbolsCommand_ShouldReturnListOfSymbols()
    {
        // Arrange
        var command = "command";
        var response = "response";
        var symbols = new List<SymbolInfo>();

        _commandCreatorMock.Setup(m => m.CreateAllSymbolsCommand()).Returns(command);
        _tcpConnectorMock.Setup(m => m.SendAndReceiveAsync(command, false)).ReturnsAsync(response);
        _responseAdapterMock.Setup(m => m.AdaptAllSymbolsResponse(response)).Returns(symbols);

        // Act
        var result = await _tcpCommandExecutor.ExecuteAllSymbolsCommand();

        // Assert
        _commandCreatorMock.Verify(m => m.CreateAllSymbolsCommand(), Times.Once);
        _tcpConnectorMock.Verify(m => m.SendAndReceiveAsync(command, false), Times.Once);
        _responseAdapterMock.Verify(m => m.AdaptAllSymbolsResponse(response), Times.Once);
        result.Should().BeEquivalentTo(symbols);
    }

    [Fact]
    public async Task ExecuteCalendarCommand_ShouldReturnCalendarEvents()
    {
        // Arrange
        var command = "command"; // Remplacez par votre classe réelle de commande
        var response = "response"; // Remplacez par votre type de réponse réelle
        var events = new List<CalendarEvent>(); // Remplacez par votre classe réelle de calendar event

        _commandCreatorMock.Setup(m => m.CreateCalendarCommand()).Returns(command);
        _tcpConnectorMock.Setup(m => m.SendAndReceiveAsync(command, It.IsAny<bool>())).ReturnsAsync(response);
        _responseAdapterMock.Setup(m => m.AdaptCalendarResponse(response)).Returns(events);

        // Act
        var result = await _tcpCommandExecutor.ExecuteCalendarCommand();

        // Assert
        _commandCreatorMock.Verify(m => m.CreateCalendarCommand(), Times.Once);
        _tcpConnectorMock.Verify(m => m.SendAndReceiveAsync(command, true), Times.Once);
        _responseAdapterMock.Verify(m => m.AdaptCalendarResponse(response), Times.Once);
        result.Should().BeEquivalentTo(events);
    }

    [Fact]
    public async Task ExecuteFullChartCommand_ShouldReturnCandles()
    {
        // Arrange
        var command = "Command"; // Remplacez par votre classe réelle de commande
        var response = "response"; // Remplacez par votre type de réponse réelle
        var candles = new List<Candle>(); // Remplacez par votre classe réelle de candle

        var timeframe = new Timeframe(); // Remplacez par votre classe réelle de timeframe
        var start = DateTime.UtcNow;
        var symbol = "symbol";

        _commandCreatorMock.Setup(m => m.CreateFullChartCommand(timeframe, start, symbol)).Returns(command);
        _tcpConnectorMock.Setup(m => m.SendAndReceiveAsync(command, false)).ReturnsAsync(response);
        _responseAdapterMock.Setup(m => m.AdaptFullChartResponse(response)).Returns(candles);

        // Act
        var result = await _tcpCommandExecutor.ExecuteFullChartCommand(timeframe, start, symbol);

        // Assert
        _commandCreatorMock.Verify(m => m.CreateFullChartCommand(timeframe, start, symbol), Times.Once);
        _tcpConnectorMock.Verify(m => m.SendAndReceiveAsync(command, false), Times.Once);
        _responseAdapterMock.Verify(m => m.AdaptFullChartResponse(response), Times.Once);
        result.Should().BeEquivalentTo(candles);
    }

    [Fact]
    public async Task ExecuteRangeChartCommand_ShouldReturnCandles()
    {
        // Arrange
        var command = "Command"; // Remplacez par votre classe réelle de commande
        var response = "response"; // Remplacez par votre type de réponse réelle
        var candles = new List<Candle>(); // Remplacez par votre classe réelle de candle

        var timeframe = new Timeframe(); // Remplacez par votre classe réelle de timeframe
        var start = DateTime.UtcNow;
        var end = DateTime.UtcNow.AddHours(1);
        var symbol = "symbol";

        _commandCreatorMock.Setup(m => m.CreateRangeChartCommand(timeframe, start, end, symbol)).Returns(command);
        _tcpConnectorMock.Setup(m => m.SendAndReceiveAsync(command, false)).ReturnsAsync(response);
        _responseAdapterMock.Setup(m => m.AdaptRangeChartResponse(response)).Returns(candles);

        // Act
        var result = await _tcpCommandExecutor.ExecuteRangeChartCommand(timeframe, start, end, symbol);

        // Assert
        _commandCreatorMock.Verify(m => m.CreateRangeChartCommand(timeframe, start, end, symbol), Times.Once);
        _tcpConnectorMock.Verify(m => m.SendAndReceiveAsync(command, false), Times.Once);
        _responseAdapterMock.Verify(m => m.AdaptRangeChartResponse(response), Times.Once);
        result.Should().BeEquivalentTo(candles);
    }

    [Fact]
    public async Task ExecuteBalanceAccountCommand_ShouldReturnAccountBalance()
    {
        // Arrange
        var command = "Command"; // Remplacez par votre classe réelle de commande
        var response = "response"; // Remplacez par votre type de réponse réelle
        var balance = new AccountBalance(); // Remplacez par votre classe réelle de account balance

        _commandCreatorMock.Setup(m => m.CreateBalanceAccountCommand()).Returns(command);
        _tcpConnectorMock.Setup(m => m.SendAndReceiveAsync(command, It.IsAny<bool>())).ReturnsAsync(response);
        _responseAdapterMock.Setup(m => m.AdaptBalanceAccountResponse(response)).Returns(balance);

        // Act
        var result = await _tcpCommandExecutor.ExecuteBalanceAccountCommand();

        // Assert
        _commandCreatorMock.Verify(m => m.CreateBalanceAccountCommand(), Times.Once);
        _tcpConnectorMock.Verify(m => m.SendAndReceiveAsync(command, true), Times.Once);
        _responseAdapterMock.Verify(m => m.AdaptBalanceAccountResponse(response), Times.Once);
        result.Should().Be(balance);
    }

    [Fact]
    public async Task ExecuteNewsCommand_ShouldReturnNews()
    {
        // Arrange
        var command = "Command"; // Remplacez par votre classe réelle de commande
        var response = "response"; // Remplacez par votre type de réponse réelle
        var news = new List<News>(); // Remplacez par votre classe réelle de news

        DateTime? start = DateTime.UtcNow.AddDays(-1);
        DateTime? end = DateTime.UtcNow;

        _commandCreatorMock.Setup(m => m.CreateNewsCommand(start, end)).Returns(command);
        _tcpConnectorMock.Setup(m => m.SendAndReceiveAsync(command, It.IsAny<bool>())).ReturnsAsync(response);
        _responseAdapterMock.Setup(m => m.AdaptNewsResponse(response)).Returns(news);

        // Act
        var result = await _tcpCommandExecutor.ExecuteNewsCommand(start, end);

        // Assert
        _commandCreatorMock.Verify(m => m.CreateNewsCommand(start, end), Times.Once);
        _tcpConnectorMock.Verify(m => m.SendAndReceiveAsync(command, true), Times.Once);
        _responseAdapterMock.Verify(m => m.AdaptNewsResponse(response), Times.Once);
        result.Should().BeEquivalentTo(news);
    }

    [Fact]
    public async Task ExecuteCurrentUserDataCommand_ShouldReturnUserData()
    {
        // Arrange
        var command = "Command"; // Remplacez par votre classe réelle de commande
        var response = "response"; // Remplacez par votre type de réponse réelle
        var userData = "user data"; // Remplacez par votre type de user data

        _commandCreatorMock.Setup(m => m.CreateCurrentUserDataCommand()).Returns(command);
        _tcpConnectorMock.Setup(m => m.SendAndReceiveAsync(command, It.IsAny<bool>())).ReturnsAsync(response);
        _responseAdapterMock.Setup(m => m.AdaptCurrentUserDataResponse(response)).Returns(userData);

        // Act
        var result = await _tcpCommandExecutor.ExecuteCurrentUserDataCommand();

        // Assert
        _commandCreatorMock.Verify(m => m.CreateCurrentUserDataCommand(), Times.Once);
        _tcpConnectorMock.Verify(m => m.SendAndReceiveAsync(command, true), Times.Once);
        _responseAdapterMock.Verify(m => m.AdaptCurrentUserDataResponse(response), Times.Once);
        result.Should().Be(userData);
    }

    [Fact]
    public async Task ExecutePingCommand_ShouldReturnPingResponse()
    {
        // Arrange
        var command = "Command"; // Remplacez par votre classe réelle de commande
        var response = "response"; // Remplacez par votre type de réponse réelle
        var pingResponse = true; // Remplacez par votre type de ping response

        _commandCreatorMock.Setup(m => m.CreatePingCommand()).Returns(command);
        _tcpConnectorMock.Setup(m => m.SendAndReceiveAsync(command, It.IsAny<bool>())).ReturnsAsync(response);
        _responseAdapterMock.Setup(m => m.AdaptPingResponse(response)).Returns(pingResponse);

        // Act
        var result = await _tcpCommandExecutor.ExecutePingCommand();

        // Assert
        _commandCreatorMock.Verify(m => m.CreatePingCommand(), Times.Once);
        _tcpConnectorMock.Verify(m => m.SendAndReceiveAsync(command, true), Times.Once);
        _responseAdapterMock.Verify(m => m.AdaptPingResponse(response), Times.Once);
        result.Should().Be(pingResponse);
    }

    [Fact]
    public async Task ExecuteSymbolCommand_ShouldReturnSymbolInfo()
    {
        // Arrange
        var command = "Command"; // Remplacez par votre classe réelle de commande
        var response = "response"; // Remplacez par votre type de réponse réelle
        var symbolInfo = new SymbolInfo
        {
            Symbol = "symbol",
            TickSize = 1
        }; // Remplacez par votre classe réelle de symbol info
        var symbol = "symbol";

        _commandCreatorMock.Setup(m => m.CreateSymbolCommand(symbol)).Returns(command);
        _tcpConnectorMock.Setup(m => m.SendAndReceiveAsync(command, It.IsAny<bool>())).ReturnsAsync(response);
        _responseAdapterMock.Setup(m => m.AdaptSymbolResponse(response)).Returns(symbolInfo);

        // Act
        var result = await _tcpCommandExecutor.ExecuteSymbolCommand(symbol);

        // Assert
        _commandCreatorMock.Verify(m => m.CreateSymbolCommand(symbol), Times.Once);
        _tcpConnectorMock.Verify(m => m.SendAndReceiveAsync(command, true), Times.Once);
        _responseAdapterMock.Verify(m => m.AdaptSymbolResponse(response), Times.Once);
        result.Should().Be(symbolInfo);
    }

    [Fact]
    public async Task ExecuteTickCommand_ShouldReturnTick()
    {
        // Arrange
        var command = "Command"; // Remplacez par votre classe réelle de commande
        var response = "response"; // Remplacez par votre type de réponse réelle
        var tick = new Tick(); // Remplacez par votre classe réelle de tick
        var symbol = "symbol";

        _commandCreatorMock.Setup(m => m.CreateTickCommand(symbol)).Returns(command);
        _tcpConnectorMock.Setup(m => m.SendAndReceiveAsync(command, It.IsAny<bool>())).ReturnsAsync(response);
        _responseAdapterMock.Setup(m => m.AdaptTickResponse(response)).Returns(tick);

        // Act
        var result = await _tcpCommandExecutor.ExecuteTickCommand(symbol);

        // Assert
        _commandCreatorMock.Verify(m => m.CreateTickCommand(symbol), Times.Once);
        _tcpConnectorMock.Verify(m => m.SendAndReceiveAsync(command, true), Times.Once);
        _responseAdapterMock.Verify(m => m.AdaptTickResponse(response), Times.Once);
        result.Should().Be(tick);
    }

    [Fact]
    public async Task ExecuteTradesHistoryCommand_ShouldReturnTradesHistory()
    {
        // Arrange
        var command = "Command"; // Remplacez par votre classe réelle de commande
        var response = "response"; // Remplacez par votre type de réponse réelle
        var trades = new List<Position>(); // Remplacez par votre classe réelle de position
        var tradeCom = "tradeCom";

        _commandCreatorMock.Setup(m => m.CreateTradesHistoryCommand()).Returns(command);
        _tcpConnectorMock.Setup(m => m.SendAndReceiveAsync(command, false)).ReturnsAsync(response);
        _responseAdapterMock.Setup(m => m.AdaptTradesHistoryResponse(response, tradeCom)).Returns(trades);

        // Act
        var result = await _tcpCommandExecutor.ExecuteTradesHistoryCommand(tradeCom);

        // Assert
        _commandCreatorMock.Verify(m => m.CreateTradesHistoryCommand(), Times.Once);
        _tcpConnectorMock.Verify(m => m.SendAndReceiveAsync(command, false), Times.Once);
        _responseAdapterMock.Verify(m => m.AdaptTradesHistoryResponse(response, tradeCom), Times.Once);
        result.Should().BeEquivalentTo(trades);
    }

    [Fact]
    public async Task ExecuteTradesOpenedTradesCommand_ShouldReturnOpenedTrades()
    {
        // Arrange
        var command = "Command"; // Remplacez par votre classe réelle de commande
        var response = "response"; // Remplacez par votre type de réponse réelle
        var trade = new Position(); // Remplacez par votre classe réelle de position
        var tradeCom = "tradeCom";

        _commandCreatorMock.Setup(m => m.CreateTradesOpenedTradesCommand()).Returns(command);
        _tcpConnectorMock.Setup(m => m.SendAndReceiveAsync(command, false)).ReturnsAsync(response);
        _responseAdapterMock.Setup(m => m.AdaptTradesOpenedTradesResponse(response, tradeCom)).Returns(trade);

        // Act
        var result = await _tcpCommandExecutor.ExecuteTradesOpenedTradesCommand(tradeCom);

        // Assert
        _commandCreatorMock.Verify(m => m.CreateTradesOpenedTradesCommand(), Times.Once);
        _tcpConnectorMock.Verify(m => m.SendAndReceiveAsync(command, false), Times.Once);
        _responseAdapterMock.Verify(m => m.AdaptTradesOpenedTradesResponse(response, tradeCom), Times.Once);
        result.Should().Be(trade);
    }

    [Fact]
    public async Task ExecuteTradingHoursCommand_ShouldReturnTradingHours()
    {
        // Arrange
        var command = "Command"; // Remplacez par votre classe réelle de commande
        var response = "response"; // Remplacez par votre type de réponse réelle
        var tradingHours = new TradeHourRecord(); // Remplacez par votre classe réelle de trading hours
        var symbol = "symbol";

        _commandCreatorMock.Setup(m => m.CreateTradingHoursCommand(symbol)).Returns(command);
        _tcpConnectorMock.Setup(m => m.SendAndReceiveAsync(command, It.IsAny<bool>())).ReturnsAsync(response);
        _responseAdapterMock.Setup(m => m.AdaptTradingHoursResponse(response)).Returns(tradingHours);

        // Act
        var result = await _tcpCommandExecutor.ExecuteTradingHoursCommand(symbol);

        // Assert
        _commandCreatorMock.Verify(m => m.CreateTradingHoursCommand(symbol), Times.Once);
        _tcpConnectorMock.Verify(m => m.SendAndReceiveAsync(command, true), Times.Once);
        _responseAdapterMock.Verify(m => m.AdaptTradingHoursResponse(response), Times.Once);
        result.Should().Be(tradingHours);
    }

    [Fact]
    public async Task ExecuteOpenTradeCommand_ShouldReturnOpenTrade()
    {
        // Arrange
        var command = "Command"; // Remplacez par votre classe réelle de commande
        var response = "response"; // Remplacez par votre type de réponse réelle
        var position = new Position(); // Remplacez par votre classe réelle de position
        var price = 123.45m;

        _commandCreatorMock.Setup(m => m.CreateOpenTradeCommande(position, price)).Returns(command);
        _tcpConnectorMock.Setup(m => m.SendAndReceiveAsync(command, It.IsAny<bool>())).ReturnsAsync(response);
        _responseAdapterMock.Setup(m => m.AdaptOpenTradeResponse(response)).Returns(position);

        // Act
        var result = await _tcpCommandExecutor.ExecuteOpenTradeCommand(position, price);

        // Assert
        _commandCreatorMock.Verify(m => m.CreateOpenTradeCommande(position, price), Times.Once);
        _tcpConnectorMock.Verify(m => m.SendAndReceiveAsync(command, true), Times.Once);
        _responseAdapterMock.Verify(m => m.AdaptOpenTradeResponse(response), Times.Once);
        result.Should().Be(position);
    }

    [Fact]
    public async Task ExecuteUpdateTradeCommand_ShouldReturnUpdatedTrade()
    {
        // Arrange
        var command = "Command"; // Remplacez par votre classe réelle de commande
        var response = "response"; // Remplacez par votre type de réponse réelle
        var position = new Position(); // Remplacez par votre classe réelle de position
        var price = 123.45m;

        _commandCreatorMock.Setup(m => m.CreateUpdateTradeCommande(position, price)).Returns(command);
        _tcpConnectorMock.Setup(m => m.SendAndReceiveAsync(command, It.IsAny<bool>())).ReturnsAsync(response);
        _responseAdapterMock.Setup(m => m.AdaptUpdateTradeResponse(response)).Returns(position);

        // Act
        var result = await _tcpCommandExecutor.ExecuteUpdateTradeCommand(position, price);

        // Assert
        _commandCreatorMock.Verify(m => m.CreateUpdateTradeCommande(position, price), Times.Once);
        _tcpConnectorMock.Verify(m => m.SendAndReceiveAsync(command, true), Times.Once);
        _responseAdapterMock.Verify(m => m.AdaptUpdateTradeResponse(response), Times.Once);
        result.Should().Be(position);
    }

    [Fact]
    public async Task ExecuteCloseTradeCommand_ShouldReturnClosedTrade()
    {
        // Arrange
        var command = "Command"; // Remplacez par votre classe réelle de commande
        var response = "response"; // Remplacez par votre type de réponse réelle
        var position = new Position(); // Remplacez par votre classe réelle de position
        var price = 123.45m;

        _commandCreatorMock.Setup(m => m.CreateCloseTradeCommande(position, price)).Returns(command);
        _tcpConnectorMock.Setup(m => m.SendAndReceiveAsync(command, It.IsAny<bool>())).ReturnsAsync(response);
        _responseAdapterMock.Setup(m => m.AdaptCloseTradeResponse(response)).Returns(position);

        // Act
        var result = await _tcpCommandExecutor.ExecuteCloseTradeCommand(position, price);

        // Assert
        _commandCreatorMock.Verify(m => m.CreateCloseTradeCommande(position, price), Times.Once);
        _tcpConnectorMock.Verify(m => m.SendAndReceiveAsync(command, true), Times.Once);
        _responseAdapterMock.Verify(m => m.AdaptCloseTradeResponse(response), Times.Once);
        result.Should().Be(position);
    }

    [Fact]
    public void ExecuteIsConnected_ShouldReturnTrueIfBothClientsAreConnected()
    {
        // Arrange
        _tcpConnectorMock.Setup(m => m.IsConnected).Returns(true);
        _tcpStreamingConnectorMock.Setup(m => m.IsConnected).Returns(true);

        // Act
        var result = _tcpCommandExecutor.ExecuteIsConnected();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteSubscribeBalanceCommandStreaming_ShouldSendSubscribeCommand()
    {
        // Arrange
        var command = "Command"; // Remplacez par votre classe réelle de commande

        _commandCreatorMock.Setup(m => m.CreateSubscribeBalanceCommandStreaming()).Returns(command);
        _tcpStreamingConnectorMock.Setup(m => m.SendAsync(command)).Returns(Task.CompletedTask);

        // Act
        _tcpCommandExecutor.ExecuteSubscribeBalanceCommandStreaming();

        // Assert
        _commandCreatorMock.Verify(m => m.CreateSubscribeBalanceCommandStreaming(), Times.Once);
        _tcpStreamingConnectorMock.Verify(m => m.SendAsync(command), Times.Once);
    }

    [Fact]
    public async Task ExecuteStopBalanceCommandStreaming_ShouldSendStopCommand()
    {
        // Arrange
        var command = "Command"; // Remplacez par votre classe réelle de commande

        _commandCreatorMock.Setup(m => m.CreateStopBalanceCommandStreaming()).Returns(command);
        _tcpStreamingConnectorMock.Setup(m => m.SendAsync(command)).Returns(Task.CompletedTask);

        // Act
        _tcpCommandExecutor.ExecuteStopBalanceCommandStreaming();

        // Assert
        _commandCreatorMock.Verify(m => m.CreateStopBalanceCommandStreaming(), Times.Once);
        _tcpStreamingConnectorMock.Verify(m => m.SendAsync(command), Times.Once);
    }

    [Fact]
    public async Task ExecuteSubscribeCandleCommandStreaming_ShouldSendSubscribeCommand()
    {
        // Arrange
        var command = "Command"; // Remplacez par votre classe réelle de commande
        var symbol = "symbol";

        _commandCreatorMock.Setup(m => m.CreateSubscribeCandleCommandStreaming(symbol)).Returns(command);
        _tcpStreamingConnectorMock.Setup(m => m.SendAsync(command)).Returns(Task.CompletedTask);

        // Act
        _tcpCommandExecutor.ExecuteSubscribeCandleCommandStreaming(symbol);

        // Assert
        _commandCreatorMock.Verify(m => m.CreateSubscribeCandleCommandStreaming(symbol), Times.Once);
        _tcpStreamingConnectorMock.Verify(m => m.SendAsync(command), Times.Once);
    }

    [Fact]
    public async Task ExecuteStopCandleCommandStreaming_ShouldSendStopCommand()
    {
        // Arrange
        var command = "Command"; // Remplacez par votre classe réelle de commande
        var symbol = "symbol";

        _commandCreatorMock.Setup(m => m.CreateStopCandleCommandStreaming(symbol)).Returns(command);
        _tcpStreamingConnectorMock.Setup(m => m.SendAsync(command)).Returns(Task.CompletedTask);

        // Act
        _tcpCommandExecutor.ExecuteStopCandleCommandStreaming(symbol);

        // Assert
        _commandCreatorMock.Verify(m => m.CreateStopCandleCommandStreaming(symbol), Times.Once);
        _tcpStreamingConnectorMock.Verify(m => m.SendAsync(command), Times.Once);
    }

    [Fact]
    public async Task ExecuteSubscribeKeepAliveCommandStreaming_ShouldSendSubscribeCommand()
    {
        // Arrange
        var command = "Command"; // Remplacez par votre classe réelle de commande

        _commandCreatorMock.Setup(m => m.CreateSubscribeKeepAliveCommandStreaming()).Returns(command);
        _tcpStreamingConnectorMock.Setup(m => m.SendAsync(command)).Returns(Task.CompletedTask);

        // Act
        _tcpCommandExecutor.ExecuteSubscribeKeepAliveCommandStreaming();

        // Assert
        _commandCreatorMock.Verify(m => m.CreateSubscribeKeepAliveCommandStreaming(), Times.Once);
        _tcpStreamingConnectorMock.Verify(m => m.SendAsync(command), Times.Once);
    }

    [Fact]
    public async Task ExecuteStopKeepAliveCommandStreaming_ShouldSendStopCommand()
    {
        // Arrange
        var command = "Command"; // Remplacez par votre classe réelle de commande

        _commandCreatorMock.Setup(m => m.CreateStopKeepAliveCommandStreaming()).Returns(command);
        _tcpStreamingConnectorMock.Setup(m => m.SendAsync(command)).Returns(Task.CompletedTask);

        // Act
        _tcpCommandExecutor.ExecuteStopKeepAliveCommandStreaming();

        // Assert
        _commandCreatorMock.Verify(m => m.CreateStopKeepAliveCommandStreaming(), Times.Once);
        _tcpStreamingConnectorMock.Verify(m => m.SendAsync(command), Times.Once);
    }

    [Fact]
    public async Task ExecuteSubscribeNewsCommandStreaming_ShouldSendSubscribeCommand()
    {
        // Arrange
        var command = "Command"; // Remplacez par votre classe réelle de commande

        _commandCreatorMock.Setup(m => m.CreateSubscribeNewsCommandStreaming()).Returns(command);
        _tcpStreamingConnectorMock.Setup(m => m.SendAsync(command)).Returns(Task.CompletedTask);

        // Act
        _tcpCommandExecutor.ExecuteSubscribeNewsCommandStreaming();

        // Assert
        _commandCreatorMock.Verify(m => m.CreateSubscribeNewsCommandStreaming(), Times.Once);
        _tcpStreamingConnectorMock.Verify(m => m.SendAsync(command), Times.Once);
    }

    [Fact]
    public async Task ExecuteStopNewsCommandStreaming_ShouldSendStopCommand()
    {
        // Arrange
        var command = "Command"; // Remplacez par votre classe réelle de commande

        _commandCreatorMock.Setup(m => m.CreateStopNewsCommandStreaming()).Returns(command);
        _tcpStreamingConnectorMock.Setup(m => m.SendAsync(command)).Returns(Task.CompletedTask);

        // Act
        _tcpCommandExecutor.ExecuteStopNewsCommandStreaming();

        // Assert
        _commandCreatorMock.Verify(m => m.CreateStopNewsCommandStreaming(), Times.Once);
        _tcpStreamingConnectorMock.Verify(m => m.SendAsync(command), Times.Once);
    }

    [Fact]
    public async Task ExecuteSubscribeProfitsCommandStreaming_ShouldSendSubscribeCommand()
    {
        // Arrange
        var command = "Command"; // Remplacez par votre classe réelle de commande

        _commandCreatorMock.Setup(m => m.CreateSubscribeProfitsCommandStreaming()).Returns(command);
        _tcpStreamingConnectorMock.Setup(m => m.SendAsync(command)).Returns(Task.CompletedTask);

        // Act
        _tcpCommandExecutor.ExecuteSubscribeProfitsCommandStreaming();

        // Assert
        _commandCreatorMock.Verify(m => m.CreateSubscribeProfitsCommandStreaming(), Times.Once);
        _tcpStreamingConnectorMock.Verify(m => m.SendAsync(command), Times.Once);
    }

    [Fact]
    public async Task ExecuteStopProfitsCommandStreaming_ShouldSendStopCommand()
    {
        // Arrange
        var command = "Command"; // Remplacez par votre classe réelle de commande

        _commandCreatorMock.Setup(m => m.CreateStopProfitsCommandStreaming()).Returns(command);
        _tcpStreamingConnectorMock.Setup(m => m.SendAsync(command)).Returns(Task.CompletedTask);

        // Act
        _tcpCommandExecutor.ExecuteStopProfitsCommandStreaming();

        // Assert
        _commandCreatorMock.Verify(m => m.CreateStopProfitsCommandStreaming(), Times.Once);
        _tcpStreamingConnectorMock.Verify(m => m.SendAsync(command), Times.Once);
    }

    [Fact]
    public async Task ExecuteTickPricesCommandStreaming_ShouldSendTickPricesCommand()
    {
        // Arrange
        var command = "Command"; // Remplacez par votre classe réelle de commande
        var symbol = "symbol";

        _commandCreatorMock.Setup(m => m.CreateTickPricesCommandStreaming(symbol)).Returns(command);
        _tcpStreamingConnectorMock.Setup(m => m.SendAsync(command)).Returns(Task.CompletedTask);

        // Act
        _tcpCommandExecutor.ExecuteTickPricesCommandStreaming(symbol);

        // Assert
        _commandCreatorMock.Verify(m => m.CreateTickPricesCommandStreaming(symbol), Times.Once);
        _tcpStreamingConnectorMock.Verify(m => m.SendAsync(command), Times.Once);
    }

    [Fact]
    public async Task ExecuteStopTickPriceCommandStreaming_ShouldSendStopTickPricesCommand()
    {
        // Arrange
        var command = "Command"; // Remplacez par votre classe réelle de commande
        var symbol = "symbol";

        _commandCreatorMock.Setup(m => m.CreateStopTickPriceCommandStreaming(symbol)).Returns(command);
        _tcpStreamingConnectorMock.Setup(m => m.SendAsync(command)).Returns(Task.CompletedTask);

        // Act
        _tcpCommandExecutor.ExecuteStopTickPriceCommandStreaming(symbol);

        // Assert
        _commandCreatorMock.Verify(m => m.CreateStopTickPriceCommandStreaming(symbol), Times.Once);
        _tcpStreamingConnectorMock.Verify(m => m.SendAsync(command), Times.Once);
    }

    [Fact]
    public async Task ExecuteTradesCommandStreaming_ShouldSendTradesCommand()
    {
        // Arrange
        var command = "Command"; // Remplacez par votre classe réelle de commande

        _commandCreatorMock.Setup(m => m.CreateTradesCommandStreaming()).Returns(command);
        _tcpStreamingConnectorMock.Setup(m => m.SendAsync(command)).Returns(Task.CompletedTask);

        // Act
        _tcpCommandExecutor.ExecuteTradesCommandStreaming();

        // Assert
        _commandCreatorMock.Verify(m => m.CreateTradesCommandStreaming(), Times.Once);
        _tcpStreamingConnectorMock.Verify(m => m.SendAsync(command), Times.Once);
    }

    [Fact]
    public async Task ExecuteStopTradesCommandStreaming_ShouldSendStopTradesCommand()
    {
        // Arrange
        var command = "Command"; // Remplacez par votre classe réelle de commande

        _commandCreatorMock.Setup(m => m.CreateStopTradesCommandStreaming()).Returns(command);
        _tcpStreamingConnectorMock.Setup(m => m.SendAsync(command)).Returns(Task.CompletedTask);

        // Act
        _tcpCommandExecutor.ExecuteStopTradesCommandStreaming();

        // Assert
        _commandCreatorMock.Verify(m => m.CreateStopTradesCommandStreaming(), Times.Once);
        _tcpStreamingConnectorMock.Verify(m => m.SendAsync(command), Times.Once);
    }

    [Fact]
    public async Task ExecuteTradeStatusCommandStreaming_ShouldSendTradeStatusCommand()
    {
        // Arrange
        var command = "Command"; // Remplacez par votre classe réelle de commande

        _commandCreatorMock.Setup(m => m.CreateTradeStatusCommandStreaming()).Returns(command);
        _tcpStreamingConnectorMock.Setup(m => m.SendAsync(command)).Returns(Task.CompletedTask);

        // Act
        _tcpCommandExecutor.ExecuteTradeStatusCommandStreaming();

        // Assert
        _commandCreatorMock.Verify(m => m.CreateTradeStatusCommandStreaming(), Times.Once);
        _tcpStreamingConnectorMock.Verify(m => m.SendAsync(command), Times.Once);
    }

    [Fact]
    public async Task ExecuteStopTradeStatusCommandStreaming_ShouldSendStopTradeStatusCommand()
    {
        // Arrange
        var command = "Command"; // Remplacez par votre classe réelle de commande

        _commandCreatorMock.Setup(m => m.CreateStopTradeStatusCommandStreaming()).Returns(command);
        _tcpStreamingConnectorMock.Setup(m => m.SendAsync(command)).Returns(Task.CompletedTask);

        // Act
        _tcpCommandExecutor.ExecuteStopTradeStatusCommandStreaming();

        // Assert
        _commandCreatorMock.Verify(m => m.CreateStopTradeStatusCommandStreaming(), Times.Once);
        _tcpStreamingConnectorMock.Verify(m => m.SendAsync(command), Times.Once);
    }

    [Fact]
    public async Task ExecutePingCommandStreaming_ShouldSendPingCommand()
    {
        // Arrange
        var command = "Command"; // Remplacez par votre classe réelle de commande

        _commandCreatorMock.Setup(m => m.CreatePingCommandStreaming()).Returns(command);
        _tcpStreamingConnectorMock.Setup(m => m.SendAsync(command)).Returns(Task.CompletedTask);

        // Act
        _tcpCommandExecutor.ExecutePingCommandStreaming();

        // Assert
        _commandCreatorMock.Verify(m => m.CreatePingCommandStreaming(), Times.Once);
        _tcpStreamingConnectorMock.Verify(m => m.SendAsync(command), Times.Once);
    }

    [Fact]
    public async Task ExecuteStopPingCommandStreaming_ShouldSendStopPingCommand()
    {
        // Arrange
        var command = "Command"; // Remplacez par votre classe réelle de commande

        _commandCreatorMock.Setup(m => m.CreateStopPingCommandStreaming()).Returns(command);
        _tcpStreamingConnectorMock.Setup(m => m.SendAsync(command)).Returns(Task.CompletedTask);

        // Act
        _tcpCommandExecutor.ExecuteStopPingCommandStreaming();

        // Assert
        _commandCreatorMock.Verify(m => m.CreateStopPingCommandStreaming(), Times.Once);
        _tcpStreamingConnectorMock.Verify(m => m.SendAsync(command), Times.Once);
    }
}