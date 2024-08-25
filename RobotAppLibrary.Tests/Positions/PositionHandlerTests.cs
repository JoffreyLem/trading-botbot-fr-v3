using FluentAssertions;
using Moq;
using RobotAppLibrary.Api.Providers.Base;
using RobotAppLibrary.Modeles;
using RobotAppLibrary.TradingManager;
using Serilog;

namespace RobotAppLibrary.Tests.Positions;

public class PositionsCommandTest
{
    private readonly Mock<IApiProviderBase> _apiHandlerMock = new();
    private readonly Mock<ILogger> _logger = new();

    private readonly Mock<ILotValueCalculator> _mockLotValueCalculator = new();
    private readonly PositionHandler _positionHandler;

    private readonly Position? positionTest = new();

    private readonly string strategyId = "idTest";
    private readonly Tick tickRef = new() { Bid = (decimal?)1.11247, Ask = (decimal?)1.112450 };

    public PositionsCommandTest()
    {
        _logger.Setup(x => x.ForContext<PositionHandler>())
            .Returns(_logger.Object);

        _apiHandlerMock.Setup(x => x.GetSymbolInformationAsync(It.IsAny<string>()))
            .ReturnsAsync(new SymbolInfo
            {
                Leverage = 10,
                TickSize = 0.00001,
                Currency = "EUR",
                CurrencyProfit = "USD",
                Category = Category.Forex,
                Symbol = "EURUSD",
                Precision = 5,
                LotMin = 0.01
            });

        _apiHandlerMock.Setup(api => api.GetTickPriceAsync(It.IsAny<string>()))
            .ReturnsAsync(tickRef);

        _apiHandlerMock
            .Setup(api => api.GetBalanceAsync())
            .ReturnsAsync(new AccountBalance { Balance = 10000, Equity = 10000 });

        _mockLotValueCalculator
            .Setup(lot => lot.PipValueStandard)
            .Returns(10);

        _mockLotValueCalculator
            .Setup(lot => lot.MarginPerLot)
            .Returns(1000);

        _positionHandler =
            new PositionHandler(_logger.Object, _apiHandlerMock.Object, "EURUSD", strategyId,
                _mockLotValueCalculator.Object);
    }


    private void OpenPositionByTest()
    {
        Test_OpenPosition_Buy();
        Test_OpenPosition_CallBack();
    }

    #region Init test

    [Fact]
    public async Task Init_ShouldInitializeSymbolInfoAndLastPrice()
    {
        // Act
        await Task.Run(() => _positionHandler); // To ensure Init method is called

        // Assert
        _positionHandler.LastPrice.Should().Be(tickRef);
        _apiHandlerMock.Verify(api => api.GetSymbolInformationAsync("EURUSD"), Times.Once);
        _apiHandlerMock.Verify(api => api.GetTickPriceAsync("EURUSD"), Times.Once);
    }


    [Fact]
    public async Task Init_ShouldSubscribeToApiEvents()
    {
        // Act
        await Task.Run(() => _positionHandler); // To ensure Init method is called

        // Assert
        _apiHandlerMock.VerifyAdd(api => api.TickEvent += It.IsAny<EventHandler<Tick>>(), Times.Once);
        _apiHandlerMock.VerifyAdd(api => api.PositionOpenedEvent += It.IsAny<EventHandler<Position>>(), Times.Once);
        _apiHandlerMock.VerifyAdd(api => api.PositionUpdatedEvent += It.IsAny<EventHandler<Position>>(), Times.Once);
        _apiHandlerMock.VerifyAdd(api => api.PositionRejectedEvent += It.IsAny<EventHandler<Position>>(), Times.Once);
        _apiHandlerMock.VerifyAdd(api => api.PositionClosedEvent += It.IsAny<EventHandler<Position>>(), Times.Once);
    }

    [Fact]
    public async Task Init_ShouldRestoreSessionIfCurrentPositionIsNotNull()
    {
        // Arrange
        _apiHandlerMock.Setup(api => api.GetOpenedTradesAsync(strategyId)).ReturnsAsync(positionTest);

        // Act
        var handler = new PositionHandler(_logger.Object, _apiHandlerMock.Object, "EURUSD", strategyId,
            _mockLotValueCalculator.Object);

        // Assert
        _apiHandlerMock.Verify(api => api.RestoreSession(positionTest), Times.Once);
        handler.PositionOpened.Should().Be(positionTest);
    }

    [Fact]
    public async Task Init_ShouldNotRestoreSessionIfCurrentPositionIsNull()
    {
        // Arrange
        _apiHandlerMock.Setup(api => api.GetOpenedTradesAsync(strategyId)).ReturnsAsync((Position?)null);

        // Act
        var handler = new PositionHandler(_logger.Object, _apiHandlerMock.Object, "EURUSD", strategyId,
            _mockLotValueCalculator.Object);

        // Assert
        _apiHandlerMock.Verify(api => api.RestoreSession(It.IsAny<Position>()), Times.Never);
        handler.PositionOpened.Should().BeNull();
    }

    #endregion


    #region Position in progress test

    [Fact]
    public void Test_PositionInProgressPending()
    {
        Test_OpenPosition_Buy();

        _positionHandler.PositionInProgress.Should().BeTrue();
    }

    [Fact]
    public void Test_PositionInProgressOpen()
    {
        OpenPositionByTest();

        _positionHandler.PositionInProgress.Should().BeTrue();
    }

    #endregion

    #region OpenPosition

    [Fact]
    public async void Test_OpenPosition_Default_sl_tp_volume_risk()
    {
        // Arrange and Act

        await _positionHandler.OpenPositionAsync("EURUSD", TypeOperation.Buy, 0.5);

        // Assert

        _positionHandler.PositionPending.Should().NotBeNull();
        _positionHandler.PositionPending.Id.Should().NotBeNull();
        _positionHandler.PositionPending.Symbol.Should().Be("EURUSD");
        _positionHandler.PositionPending.TypePosition.Should().Be(TypeOperation.Buy);
        _positionHandler.PositionPending.OpenPrice.Should().Be(1.112450m);
        _positionHandler.PositionPending.StopLoss.Should().Be(1.11147M);
        _positionHandler.PositionPending.TakeProfit.Should().Be(1.11345M);
        _positionHandler.PositionPending.Volume.Should().Be(0.5);
        _positionHandler.PositionPending.StrategyId.Should().Be("idTest");
    }


    [Fact]
    public async void Test_OpenPosition_Buy()
    {
        // Arrange and Act

        await _positionHandler.OpenPositionAsync("EURUSD", TypeOperation.Buy, 0.5, 1.11237m, 1.11257m);

        // Assert

        _positionHandler.PositionPending.Should().NotBeNull();
        _positionHandler.PositionPending.Id.Should().NotBeNull();
        _positionHandler.PositionPending.Symbol.Should().Be("EURUSD");
        _positionHandler.PositionPending.TypePosition.Should().Be(TypeOperation.Buy);
        _positionHandler.PositionPending.OpenPrice.Should().Be(1.112450m);
        _positionHandler.PositionPending.StopLoss.Should().Be(1.11237m);
        _positionHandler.PositionPending.TakeProfit.Should().Be(1.11257m);
        _positionHandler.PositionPending.Volume.Should().Be(0.5);
        _positionHandler.PositionPending.PositionStrategyReferenceId.Should()
            .Be($"{_positionHandler.PositionPending.StrategyId}|{_positionHandler.PositionPending.Id}");
        var PositionReference = $"{_positionHandler.PositionPending.StrategyId}|{_positionHandler.PositionPending.Id}";
        _positionHandler.PositionPending.StrategyId.Should().Be("idTest");
        _positionHandler.PositionPending.PositionStrategyReferenceId.Should().Be(PositionReference);
    }

    [Fact]
    public async void Test_OpenPosition_Sell()
    {
        // Arrange and Act

        await _positionHandler.OpenPositionAsync("EURUSD", TypeOperation.Sell, 0.5, 1.11237m, 1.11257m);

        // Assert

        _positionHandler.PositionPending.Should().NotBeNull();
        _positionHandler.PositionPending.Id.Should().NotBeNull();
        _positionHandler.PositionPending.Symbol.Should().Be("EURUSD");
        _positionHandler.PositionPending.TypePosition.Should().Be(TypeOperation.Sell);
        _positionHandler.PositionPending.OpenPrice.Should().Be(1.11247M);
        _positionHandler.PositionPending.StopLoss.Should().Be(1.11237m);
        _positionHandler.PositionPending.TakeProfit.Should().Be(1.11257m);
        _positionHandler.PositionPending.Volume.Should().Be(0.5);
        _positionHandler.PositionPending.PositionStrategyReferenceId.Should()
            .Be($"{_positionHandler.PositionPending.StrategyId}|{_positionHandler.PositionPending.Id}");
        var PositionReference = $"{_positionHandler.PositionPending.StrategyId}|{_positionHandler.PositionPending.Id}";
        _positionHandler.PositionPending.StrategyId.Should().Be("idTest");
        _positionHandler.PositionPending.PositionStrategyReferenceId.Should().Be(PositionReference);
    }

    [Fact]
    public async void Test_OpenPosition_For_Other_Type_Symbol()
    {
        // Arrange and Act
        _apiHandlerMock
            .Setup(api => api.GetSymbolInformationAsync(It.IsAny<string>()))
            .ReturnsAsync(new SymbolInfo
                { Symbol = "test", Category = Category.Commodities, LotMin = 0.01, TickSize = 0.01, Precision = 5 });


        var positionHandler =
            new PositionHandler(_logger.Object, _apiHandlerMock.Object, "EURUSD", strategyId,
                _mockLotValueCalculator.Object);

        await positionHandler.OpenPositionAsync("test", TypeOperation.Buy, 0.5, 1.11237m, 1.11257m);

        // Assert

        positionHandler.PositionPending.Should().NotBeNull();
        positionHandler.PositionPending.Id.Should().NotBeNull();
        positionHandler.PositionPending.Symbol.Should().Be("test");
        positionHandler.PositionPending.TypePosition.Should().Be(TypeOperation.Buy);
        positionHandler.PositionPending.OpenPrice.Should().Be(1.112450m);
        positionHandler.PositionPending.StopLoss.Should().Be(1.11237m);
        positionHandler.PositionPending.TakeProfit.Should().Be(1.11257m);
        positionHandler.PositionPending.Volume.Should().Be(0.5);
        positionHandler.PositionPending.PositionStrategyReferenceId.Should()
            .Be($"{positionHandler.PositionPending.StrategyId}|{positionHandler.PositionPending.Id}");
        var PositionReference = $"{positionHandler.PositionPending.StrategyId}|{positionHandler.PositionPending.Id}";
        positionHandler.PositionPending.StrategyId.Should().Be("idTest");
        positionHandler.PositionPending.PositionStrategyReferenceId.Should().Be(PositionReference);
    }


    [Fact]
    public async void Test_OpenPosition_DefaultSl_DefaultTp()
    {
        // Arrange and Act

        await _positionHandler.OpenPositionAsync("EURUSD", TypeOperation.Buy, 0.5);

        // Assert

        _positionHandler.PositionPending.Should().NotBeNull();
        _positionHandler.PositionPending.Id.Should().NotBeNull();
        _positionHandler.PositionPending.Symbol.Should().Be("EURUSD");
        _positionHandler.PositionPending.TypePosition.Should().Be(TypeOperation.Buy);
        _positionHandler.PositionPending.OpenPrice.Should().Be(1.112450m);
        _positionHandler.PositionPending.StopLoss.Should().Be(1.11147M);
        _positionHandler.PositionPending.TakeProfit.Should().Be(1.11345M);
        _positionHandler.PositionPending.Volume.Should().Be(0.5);
        _positionHandler.PositionPending.PositionStrategyReferenceId.Should()
            .Be($"{_positionHandler.PositionPending.StrategyId}|{_positionHandler.PositionPending.Id}");
        var PositionReference = $"{_positionHandler.PositionPending.StrategyId}|{_positionHandler.PositionPending.Id}";
        _positionHandler.PositionPending.StrategyId.Should().Be("idTest");
        _positionHandler.PositionPending.PositionStrategyReferenceId.Should().Be(PositionReference);
    }


    [Fact]
    public async void Test_OpenPosition_No_Volume_NoRisk()
    {
        // Arrange and Act
        await _positionHandler.OpenPositionAsync("EURUSD", TypeOperation.Buy, 0, 1.11237m, 1.11257m);

        // Assert
        _positionHandler.PositionPending.Should().NotBeNull();
        _positionHandler.PositionPending.Id.Should().NotBeNull();
        _positionHandler.PositionPending.Symbol.Should().Be("EURUSD");
        _positionHandler.PositionPending.TypePosition.Should().Be(TypeOperation.Buy);
        _positionHandler.PositionPending.OpenPrice.Should().Be(1.112450m);
        _positionHandler.PositionPending.StopLoss.Should().Be(1.11237m);
        _positionHandler.PositionPending.TakeProfit.Should().Be(1.11257m);
        _positionHandler.PositionPending.Volume.Should().Be(2.49);
        _positionHandler.PositionPending.PositionStrategyReferenceId.Should()
            .Be($"{_positionHandler.PositionPending.StrategyId}|{_positionHandler.PositionPending.Id}");
        var PositionReference = $"{_positionHandler.PositionPending.StrategyId}|{_positionHandler.PositionPending.Id}";
        _positionHandler.PositionPending.StrategyId.Should().Be("idTest");
        _positionHandler.PositionPending.PositionStrategyReferenceId.Should().Be(PositionReference);
    }

    [Fact]
    public async void Test_OpenPosition_No_Volume_Risk()
    {
        // Arrange and Act
        await _positionHandler.OpenPositionAsync("EURUSD", TypeOperation.Buy, 0, 1.11237m, 1.11257m, 4);

        // Assert
        _positionHandler.PositionPending.Should().NotBeNull();
        _positionHandler.PositionPending.Id.Should().NotBeNull();
        _positionHandler.PositionPending.Symbol.Should().Be("EURUSD");
        _positionHandler.PositionPending.TypePosition.Should().Be(TypeOperation.Buy);
        _positionHandler.PositionPending.OpenPrice.Should().Be(1.112450m);
        _positionHandler.PositionPending.StopLoss.Should().Be(1.11237m);
        _positionHandler.PositionPending.TakeProfit.Should().Be(1.11257m);
        _positionHandler.PositionPending.Volume.Should().Be(4.99);
        _positionHandler.PositionPending.PositionStrategyReferenceId.Should()
            .Be($"{_positionHandler.PositionPending.StrategyId}|{_positionHandler.PositionPending.Id}");
        var PositionReference = $"{_positionHandler.PositionPending.StrategyId}|{_positionHandler.PositionPending.Id}";
        _positionHandler.PositionPending.StrategyId.Should().Be("idTest");
        _positionHandler.PositionPending.PositionStrategyReferenceId.Should().Be(PositionReference);
    }

    [Fact]
    public async void Test_OpenPosition_No_Volume_NoRisk_NoSl()
    {
        // Arrange and Act
        await _positionHandler.OpenPositionAsync("EURUSD", TypeOperation.Buy, 0, 0, 1.11257m);

        // Assert
        _positionHandler.PositionPending.Should().NotBeNull();
        _positionHandler.PositionPending.Id.Should().NotBeNull();
        _positionHandler.PositionPending.Symbol.Should().Be("EURUSD");
        _positionHandler.PositionPending.TypePosition.Should().Be(TypeOperation.Buy);
        _positionHandler.PositionPending.OpenPrice.Should().Be(1.112450m);
        _positionHandler.PositionPending.StopLoss.Should().Be(1.11147M);
        _positionHandler.PositionPending.TakeProfit.Should().Be(1.11257m);
        _positionHandler.PositionPending.Volume.Should().Be(0.19);
        _positionHandler.PositionPending.PositionStrategyReferenceId.Should()
            .Be($"{_positionHandler.PositionPending.StrategyId}|{_positionHandler.PositionPending.Id}");
        var PositionReference = $"{_positionHandler.PositionPending.StrategyId}|{_positionHandler.PositionPending.Id}";
        _positionHandler.PositionPending.StrategyId.Should().Be("idTest");
        _positionHandler.PositionPending.PositionStrategyReferenceId.Should().Be(PositionReference);
    }

    [Fact]
    public async void Test_OpenPosition_No_Volume_Risk_NoSl()
    {
        // Arrange and Act
        await _positionHandler.OpenPositionAsync("EURUSD", TypeOperation.Buy, 0, 0, 1.11257m, 4);

        // Assert
        _positionHandler.PositionPending.Should().NotBeNull();
        _positionHandler.PositionPending.Id.Should().NotBeNull();
        _positionHandler.PositionPending.Symbol.Should().Be("EURUSD");
        _positionHandler.PositionPending.TypePosition.Should().Be(TypeOperation.Buy);
        _positionHandler.PositionPending.OpenPrice.Should().Be(1.112450m);
        _positionHandler.PositionPending.StopLoss.Should().Be(1.11147M);
        _positionHandler.PositionPending.TakeProfit.Should().Be(1.11257m);
        _positionHandler.PositionPending.Volume.Should().Be(0.4);
        _positionHandler.PositionPending.PositionStrategyReferenceId.Should()
            .Be($"{_positionHandler.PositionPending.StrategyId}|{_positionHandler.PositionPending.Id}");
        var PositionReference = $"{_positionHandler.PositionPending.StrategyId}|{_positionHandler.PositionPending.Id}";
        _positionHandler.PositionPending.StrategyId.Should().Be("idTest");
        _positionHandler.PositionPending.PositionStrategyReferenceId.Should().Be(PositionReference);
    }

    [Fact]
    public async void Test_PositionSize_Return_MinLot()
    {
        // Arrange and Act
        await _positionHandler.OpenPositionAsync("EURUSD", TypeOperation.Buy, 0, 0, 1.11257m, 0.001);

        _positionHandler.PositionPending.Volume.Should().Be(0.01);
    }

    [Fact]
    public async void Test_PositionSize_Return_MaxLot()
    {
        // Arrange and Act
        await _positionHandler.OpenPositionAsync("EURUSD", TypeOperation.Buy, 0, 0, 1.11257m, 100);

        _positionHandler.PositionPending.Volume.Should().Be(9.99);
    }

    [Fact]
    public async void Test_PositionSize_Return_MaxLot_AfterUpdate()
    {
        // Arrange and Act
        _mockLotValueCalculator.SetupGet(x => x.MarginPerLot).Returns(1000);
        _apiHandlerMock.Raise(api => api.NewBalanceEvent += null, this,
            new AccountBalance { Balance = 200000, Equity = 200000 });


        await _positionHandler.OpenPositionAsync("EURUSD", TypeOperation.Buy, 0, 0, 1.11257m, 100);
        _positionHandler.PositionPending.Volume.Should().Be(199.99);
    }


    [Fact]
    public async void Test_OpenPosition_throw_Exception()
    {
        // Arrange
        _apiHandlerMock.Setup(x => x.OpenPositionAsync(It.IsAny<Position>()))
            .ThrowsAsync(new Exception());

        // Act
        await _positionHandler.OpenPositionAsync("EURUSD", TypeOperation.Buy, 0.5, 1.11237m, 1.11257m);

        // Assert
        _positionHandler.PositionPending.Should().BeNull();
    }

    [Fact]
    public async void Test_OpenPosition_CallBack()
    {
        // Arrange
        Test_OpenPosition_Buy();
        var position = new Position
        {
            Symbol = "EURUSD",
            Id = _positionHandler.PositionPending.Id,
            StrategyId = "idTest"
        };

        var caller = false;

        _positionHandler.PositionOpenedEvent += (sender, position1) => caller = true;

        // Act 

        _apiHandlerMock.Raise(x => x.PositionOpenedEvent += null, this, position);

        // Assert

        _positionHandler.PositionOpened.Should().NotBeNull();
        _positionHandler.PositionOpened.StatusPosition.Should().Be(StatusPosition.Open);
        _positionHandler.PositionPending.Should().BeNull();
        caller.Should().BeTrue();
    }


    [Fact]
    public async void Test_OpenPosition_CallBack_nomatch_id()
    {
        // Arrange
        Test_OpenPosition_Buy();
        var position = new Position
        {
            Symbol = "EURUSD"
        };

        var caller = false;

        _positionHandler.PositionOpenedEvent += (sender, position1) => caller = true;

        // Act 

        _apiHandlerMock.Raise(x => x.PositionOpenedEvent += null, this, position);

        // Assert
        _positionHandler.PositionOpened.Should().BeNull();
        _positionHandler.PositionPending.Should().NotBeNull();
        caller.Should().BeFalse();
    }

    [Fact]
    public async void Test_OpenPosition_CallBack_rejected()
    {
        // Arrange
        Test_OpenPosition_Buy();
        var position = new Position
        {
            Symbol = "EURUSD",
            Id = _positionHandler.PositionPending.Id,
            StrategyId = "idTest"
        };
        ;
        var caller = false;

        _positionHandler.PositionRejectedEvent += (sender, position1) => caller = true;

        // Act 

        _apiHandlerMock.Raise(x => x.PositionRejectedEvent += null, this, position);

        // Assert

        _positionHandler.PositionOpened.Should().BeNull();
        _positionHandler.PositionPending.Should().BeNull();
        caller.Should().BeTrue();
    }


    [Fact]
    public async void Test_OpenPosition_CallBack_no_match_id()
    {
        // Arrange
        Test_OpenPosition_Buy();
        var position = new Position
        {
            Symbol = "EURUSD",
            Id = "truc",
            StrategyId = "testing"
        };
        ;
        var caller = false;

        _positionHandler.PositionRejectedEvent += (sender, position1) => caller = true;

        // Act 

        _apiHandlerMock.Raise(x => x.PositionRejectedEvent += null, this, position);

        // Assert

        _positionHandler.PositionOpened.Should().BeNull();
        _positionHandler.PositionPending.Should().NotBeNull();
        caller.Should().BeFalse();
    }

    #endregion


    #region UpdatePosition

    [Fact]
    public async void Test_UpdatePosition_noUpdate_same_sltp()
    {
        // Arrange
        OpenPositionByTest();
        var position = _positionHandler.PositionOpened;

        // Act
        await _positionHandler.UpdatePositionAsync(position);

        // Assert
        _apiHandlerMock.Verify(x => x.UpdatePositionAsync(It.IsAny<Position>()), Times.Never);
    }


    [Fact]
    public async void Test_UpdatePosition_different_sl()
    {
        // Arrange
        OpenPositionByTest();
        var position = new Position
        {
            StopLoss = 1.11247m
        };


        // Act
        await _positionHandler.UpdatePositionAsync(position);

        // Assert
        _apiHandlerMock.Verify(x => x.UpdatePositionAsync(It.IsAny<Position>()), Times.Once);
    }

    [Fact]
    public async void Test_UpdatePosition__waitClosel()
    {
        // Arrange
        OpenPositionByTest();
        var position = new Position
        {
            StatusPosition = StatusPosition.Close,
            StopLoss = 1.11247m
        };

        // Act
        await _positionHandler.UpdatePositionAsync(position);

        // Assert
        _apiHandlerMock.Verify(x => x.UpdatePositionAsync(It.IsAny<Position>()), Times.Never);
    }

    [Fact]
    public async void Test_UpdatePosition_different_tp()
    {
        // Arrange
        OpenPositionByTest();
        var position = new Position
        {
            TakeProfit = 1.11267m
        };


        // Act
        await _positionHandler.UpdatePositionAsync(position);

        // Assert
        _apiHandlerMock.Verify(x => x.UpdatePositionAsync(It.IsAny<Position>()), Times.Once);
    }

    [Fact]
    public async void Test_UpdatePosition_throw_Exception()
    {
        // Arrange
        OpenPositionByTest();
        var position = new Position
        {
            TakeProfit = 1.11267m
        };

        _apiHandlerMock.Setup(x => x.UpdatePositionAsync(It.IsAny<Position>()))
            .ThrowsAsync(new Exception());

        // Act
        await _positionHandler.UpdatePositionAsync(position);

        // Assert
        _apiHandlerMock.Verify(x => x.UpdatePositionAsync(It.IsAny<Position>()), Times.Once);
        _logger.Verify(x => x.Error(It.IsAny<Exception?>(), It.IsAny<string>(), It.IsAny<string>()));
    }

    [Fact]
    public void Test_UpdatePosition_Callback()
    {
        // Arrange
        OpenPositionByTest();
        var position = new Position
        {
            Id = _positionHandler.PositionOpened.Id,
            StrategyId = "idTest",
            TakeProfit = 1.11267m,
            Profit = 20
        };

        var caller = false;

        _positionHandler.PositionUpdatedEvent += (sender, position1) => { caller = true; };

        // Act
        _apiHandlerMock.Raise(x => x.PositionUpdatedEvent += null, this, position);

        // Assert

        _positionHandler.PositionOpened.Profit.Should().Be(position.Profit);
        _positionHandler.PositionOpened.TakeProfit.Should().Be(position.TakeProfit);
        _positionHandler.PositionOpened.StopLoss.Should().Be(position.StopLoss);
        caller.Should().BeTrue();
    }

    [Fact]
    public void Test_UpdatePosition_Callback_noMatch_id()
    {
        // Arrange
        OpenPositionByTest();
        var position = new Position
        {
            Id = "truc",
            StrategyId = "testing",
            TakeProfit = 1.11267m,
            StopLoss = 1.11247m,
            Profit = 20
        };

        var caller = false;

        _positionHandler.PositionUpdatedEvent += (sender, position1) => caller = true;

        // Act
        _apiHandlerMock.Raise(x => x.PositionUpdatedEvent += null, this, position);

        // Assert

        _positionHandler.PositionOpened.Profit.Should().NotBe(position.Profit);
        _positionHandler.PositionOpened.TakeProfit.Should().NotBe(position.TakeProfit);
        _positionHandler.PositionOpened.StopLoss.Should().NotBe(position.StopLoss);
        caller.Should().BeFalse();
    }

    #endregion

    #region Close position

    [Fact]
    public async void Test_ClosePosition()
    {
        // Arrange
        OpenPositionByTest();
        var position = new Position
        {
            Id = _positionHandler.PositionOpened.Id
        };

        // Act

        await _positionHandler.ClosePositionAsync(position);

        // Assert

        _apiHandlerMock.Verify(x => x.ClosePositionAsync(It.IsAny<Position>()), Times.Once);
    }


    [Fact]
    public async void Test_ClosePosition_state_waitclose()
    {
        // Arrange
        OpenPositionByTest();
        var position = new Position
        {
            Id = _positionHandler.PositionOpened.Id,
            StatusPosition = StatusPosition.Close,
     
        };

        // Act

        await _positionHandler.ClosePositionAsync(position);

        // Assert

        _apiHandlerMock.Verify(x => x.ClosePositionAsync(It.IsAny<Position>()), Times.Never);
    }


    [Fact]
    public async void Test_ClosePosition_throw_error()
    {
        // Arrange
        OpenPositionByTest();
        var position = new Position
        {
            Id = _positionHandler.PositionOpened.Id
        };

        _apiHandlerMock.Setup(x => x.ClosePositionAsync(It.IsAny<Position>()))
            .ThrowsAsync(new Exception());

        // Act
        await _positionHandler.ClosePositionAsync(position);

        // Assert
        _apiHandlerMock.Verify(x => x.ClosePositionAsync(It.IsAny<Position>()), Times.Once);
        _logger.Verify(x => x.Error(It.IsAny<Exception?>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async void Test_ClosePosition_callBack()
    {
        // Arrange
        OpenPositionByTest();
        var position = new Position
        {
            Id = _positionHandler.PositionOpened.Id,
            StrategyId = "idTest",
            StatusPosition = StatusPosition.Close,
            Profit = 100,
            StopLoss = 10,
            TakeProfit = 11,
            DateClose = new DateTime(2022,01,01),
            ClosePrice = 0.10m,
            ReasonClosed = ReasonClosed.Closed,
        };

        var caller = false;
        _positionHandler.PositionClosedEvent += (sender, position1) =>
        {
            position1.StatusPosition.Should().Be(StatusPosition.Close);
            position1.Profit.Should().Be(100);
            position1.StopLoss.Should().Be(10);
            position1.TakeProfit.Should().Be(11);
            position1.DateClose.Should().Be(new DateTime(2022, 01, 01));
            position1.ClosePrice.Should().Be(0.10m);
            position1.Opened.Should().Be(false);
            caller = true;
        };

        // Act
        _apiHandlerMock.Raise(x => x.PositionClosedEvent += null, this, position);


        // Assert
        caller.Should().BeTrue();
        _positionHandler.PositionOpened.Should().BeNull();
    }


    [Fact]
    public async void Test_ClosePosition_callBack_noMatch_id()
    {
        // Arrange
        OpenPositionByTest();
        var position = new Position
        {
            Id = "trcds",
            StrategyId = "testing"
        };

        var caller = false;
        _positionHandler.PositionClosedEvent += (sender, position1) => caller = true;

        // Act
        _apiHandlerMock.Raise(x => x.PositionClosedEvent += null, this, position);


        // Assert
        caller.Should().BeFalse();
        _positionHandler.PositionOpened.Should().NotBeNull();
    }

    #endregion

    #region Calculate StopLoss

    [Fact]
    public void Test_Calculate_StopLoss_buy()
    {
        // arrange and act

        var sl = _positionHandler.CalculateStopLoss(50, TypeOperation.Buy);

        // Assert 

        sl.Should().Be(1.11197M);
    }

    [Fact]
    public void Test_Calculate_StopLoss_sell()
    {
        // arrange and act

        var sl = _positionHandler.CalculateStopLoss(50, TypeOperation.Sell);

        // Assert 

        sl.Should().Be(1.11295M);
    }


    [Fact]
    public void Test_Calculate_StopLoss_buy_other_quotation()
    {
        // Arrange
        var symbolInfo = new SymbolInfo
        {
            Leverage = 10,
            TickSize = 0.1,
            Currency = "EUR",
            CurrencyProfit = "EUR",
            Category = Category.Indices,
            Symbol = "DE30",
            Precision = 1
        };

        var apiHandlerMock = new Mock<IApiProviderBase>();

        apiHandlerMock.Setup(x => x.GetSymbolInformationAsync(It.IsAny<string>()))
            .ReturnsAsync(symbolInfo);

        apiHandlerMock.Setup(api => api.GetTickPriceAsync(It.IsAny<string>()))
            .ReturnsAsync(tickRef);


        apiHandlerMock.Setup(api => api.GetTickPriceAsync(It.IsAny<string>()))
            .ReturnsAsync(new Tick { Ask = 15924.1m, Bid = 15921.9m });

        apiHandlerMock
            .Setup(api => api.GetBalanceAsync())
            .ReturnsAsync(new AccountBalance { Balance = 10000, Equity = 10000 });

        var positionHandler = new PositionHandler(_logger.Object, apiHandlerMock.Object, "DE30", "",
            _mockLotValueCalculator.Object);


        // act

        var sl = positionHandler.CalculateStopLoss(50, TypeOperation.Buy);

        // Assert 

        sl.Should().Be(15871.9M);
    }

    [Fact]
    public void Test_Calculate_StopLoss_sell_other_quotation()
    {
        // Arrange
        var symbolInfo = new SymbolInfo
        {
            Leverage = 10,
            TickSize = 0.1,
            Currency = "EUR",
            CurrencyProfit = "EUR",
            Category = Category.Indices,
            Symbol = "DE30",
            Precision = 1
        };

        var apiHandlerMock = new Mock<IApiProviderBase>();

        apiHandlerMock.Setup(x => x.GetSymbolInformationAsync(It.IsAny<string>()))
            .ReturnsAsync(symbolInfo);

        apiHandlerMock.Setup(api => api.GetTickPriceAsync(It.IsAny<string>()))
            .ReturnsAsync(tickRef);

        apiHandlerMock
            .Setup(api => api.GetBalanceAsync())
            .ReturnsAsync(new AccountBalance { Balance = 10000, Equity = 10000 });

        apiHandlerMock.Setup(api => api.GetTickPriceAsync(It.IsAny<string>()))
            .ReturnsAsync(new Tick { Ask = 15924.1m, Bid = 15921.9m });


        var positionHandler = new PositionHandler(_logger.Object, apiHandlerMock.Object, "DE30", "",
            _mockLotValueCalculator.Object);


        // act

        var sl = positionHandler.CalculateStopLoss(50, TypeOperation.Sell);

        // Assert 

        sl.Should().Be(15974.1M);
    }

    #endregion

    #region Calculate Take Profit

    [Fact]
    public void Test_Calculate_takeProfit_buy()
    {
        // arrange and act

        var sl = _positionHandler.CalculateTakeProfit(50, TypeOperation.Buy);

        // Assert 

        sl.Should().Be(1.11295M);
    }

    [Fact]
    public void Test_Calculate_TakeProfit_sell()
    {
        // arrange and act

        var sl = _positionHandler.CalculateTakeProfit(50, TypeOperation.Sell);

        // Assert 

        sl.Should().Be(1.11197M);
    }


    [Fact]
    public void Test_Calculate_TakePRofit_buy_other_quotation()
    {
        // Arrange
        var symbolInfo = new SymbolInfo
        {
            Leverage = 10,
            TickSize = 0.1,
            Currency = "EUR",
            CurrencyProfit = "EUR",
            Category = Category.Indices,
            Symbol = "DE30",
            Precision = 1
        };

        var apiHandlerMock = new Mock<IApiProviderBase>();

        apiHandlerMock.Setup(x => x.GetSymbolInformationAsync(It.IsAny<string>()))
            .ReturnsAsync(symbolInfo);

        apiHandlerMock.Setup(api => api.GetTickPriceAsync(It.IsAny<string>()))
            .ReturnsAsync(tickRef);


        apiHandlerMock.Setup(api => api.GetTickPriceAsync(It.IsAny<string>()))
            .ReturnsAsync(new Tick { Ask = 15924.1m, Bid = 15921.9m });

        apiHandlerMock
            .Setup(api => api.GetBalanceAsync())
            .ReturnsAsync(new AccountBalance { Balance = 10000, Equity = 10000 });

        var positionHandler = new PositionHandler(_logger.Object, apiHandlerMock.Object, "DE30", "",
            _mockLotValueCalculator.Object);


        // act

        var sl = positionHandler.CalculateTakeProfit(50, TypeOperation.Buy);

        // Assert 

        sl.Should().Be(15974.1M);
    }

    [Fact]
    public void Test_Calculate_TakeProfit_sell_other_quotation()
    {
        // Arrange
        var symbolInfo = new SymbolInfo
        {
            Leverage = 10,
            TickSize = 0.1,
            Currency = "EUR",
            CurrencyProfit = "EUR",
            Category = Category.Indices,
            Symbol = "DE30",
            Precision = 1
        };

        var apiHandlerMock = new Mock<IApiProviderBase>();

        apiHandlerMock.Setup(x => x.GetSymbolInformationAsync(It.IsAny<string>()))
            .ReturnsAsync(symbolInfo);

        apiHandlerMock.Setup(api => api.GetTickPriceAsync(It.IsAny<string>()))
            .ReturnsAsync(tickRef);


        apiHandlerMock.Setup(api => api.GetTickPriceAsync(It.IsAny<string>()))
            .ReturnsAsync(new Tick { Ask = 15924.1m, Bid = 15921.9m });

        apiHandlerMock
            .Setup(api => api.GetBalanceAsync())
            .ReturnsAsync(new AccountBalance { Balance = 10000, Equity = 10000 });

        var positionHandler = new PositionHandler(_logger.Object, apiHandlerMock.Object, "DE30", "",
            _mockLotValueCalculator.Object);


        // act

        var sl = positionHandler.CalculateTakeProfit(50, TypeOperation.Sell);

        // Assert 

        sl.Should().Be(15871.9M);
    }

    #endregion
}