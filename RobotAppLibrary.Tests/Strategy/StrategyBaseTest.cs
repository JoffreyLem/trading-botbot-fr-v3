using FluentAssertions;
using Moq;
using RobotAppLibrary.Api.Providers.Base;
using RobotAppLibrary.Chart;
using RobotAppLibrary.Factory;
using RobotAppLibrary.LLM;
using RobotAppLibrary.Modeles;
using RobotAppLibrary.Strategy;
using RobotAppLibrary.Tests.Strategy.Context;
using RobotAppLibrary.TradingManager;
using RobotAppLibrary.TradingManager.Interfaces;
using Serilog;

namespace RobotAppLibrary.Tests.Strategy;

public class StrategyBaseTest
{
    private readonly Mock<IApiProviderBase> _apiHandlerMock = new();

    private readonly Mock<ChartForTest> _charmockMonthly = new()
    {
        CallBase = true
    };

    private readonly Mock<ChartForTest> _chartMockMain = new()
    {
        CallBase = true
    };

    private readonly Mock<ChartForTest> _chartMockWeekly = new()
    {
        CallBase = true
    };

    private readonly Mock<ILogger> _loggerMock = new();


    private readonly Mock<IPositionHandler> _positionHandlerMock = new();
    private readonly Mock<IStrategyResult> _strategyResultMock = new();
    private readonly Mock<IStrategyServiceFactory> _strategyServiceFactoryMock = new();
    private readonly Mock<ILLMManager> _llmManagerMock = new();


    public StrategyBaseTest()
    {
        _loggerMock.Setup(x => x.ForContext<StrategyBase>())
            .Returns(_loggerMock.Object);
        _loggerMock.Setup(x => x.ForContext(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<bool>()))
            .Returns(_loggerMock.Object);


        _strategyServiceFactoryMock.Setup(x =>
                x.GetChart(It.IsAny<ILogger>(), It.IsAny<IApiProviderBase>(), It.IsAny<string>(),
                    It.Is<Timeframe>(y => y == Timeframe.Daily)))
            .Returns(_chartMockMain.Object);

        _strategyServiceFactoryMock.Setup(x =>
                x.GetChart(It.IsAny<ILogger>(), It.IsAny<IApiProviderBase>(), It.IsAny<string>(),
                    It.Is<Timeframe>(y => y == Timeframe.Monthly)))
            .Returns(_charmockMonthly.Object);

        _strategyServiceFactoryMock.Setup(x =>
                x.GetChart(It.IsAny<ILogger>(), It.IsAny<IApiProviderBase>(), It.IsAny<string>(),
                    It.Is<Timeframe>(y => y == Timeframe.Weekly)))
            .Returns(_chartMockWeekly.Object);


        _strategyServiceFactoryMock
            .Setup(x => x.GetStrategyResultService(It.IsAny<IApiProviderBase>(), It.IsAny<string>(), It.IsAny<ILogger>()))
            .Returns(_strategyResultMock.Object);

        _strategyServiceFactoryMock.Setup(x =>
                x.GetPositionHandler(It.IsAny<ILogger>(), It.IsAny<IApiProviderBase>(), It.IsAny<string>(),
                    It.IsAny<string>()))
            .Returns(_positionHandlerMock.Object);
        
        _strategyServiceFactoryMock.Setup(x => x.GetLLMManager()).Returns(_llmManagerMock.Object);


        InitIndicatorForTest();
    }

    private void InitIndicatorForTest()
    {
        InitializeChartMock(_chartMockMain, TestUtils.GenerateCandle(Timeframe.Daily, 100));
        InitializeChartMock(_charmockMonthly, TestUtils.GenerateCandle(Timeframe.Monthly, 100));
        InitializeChartMock(_chartMockWeekly, TestUtils.GenerateCandle(Timeframe.Weekly, 100));
    }


    private void InitializeChartMock(Mock<ChartForTest> chartMock, List<Candle> fakeHistory)
    {
        var internalList = new List<Candle>(fakeHistory);


        foreach (var t in internalList) chartMock.Object.Add(t);

        chartMock.Setup(x => x.AggregateChart(It.IsAny<Timeframe>()));
    }

    #region Treshold

    [Theory]
    [InlineData(EventTreshold.Drowdown)]
    [InlineData(EventTreshold.Profitfactor)]
    [InlineData(EventTreshold.LooseStreak)]
    [InlineData(EventTreshold.ProfitTreshHold)]
    public void Test_MoneyManagement_Treshold(EventTreshold eventTreshold)
    {
        // Arrange
        var caller = false;

        var strategyImpl = new FakeStrategyContextTest();
        var strategyImplBase = new StrategyBase("EURUSD", strategyImpl, _apiHandlerMock.Object, _loggerMock.Object,
            _strategyServiceFactoryMock.Object);
        strategyImplBase.CanRun = true;

        strategyImplBase.StrategyDisabledEvent += (sender, treshold) =>
        {
            caller = true;
            treshold.EventField.Should().NotContain(StrategyReasonDisabled.User.ToString());
        };

        _strategyResultMock.Raise(x => x.ResultTresholdEvent += null, this, eventTreshold);

        caller.Should().BeTrue();
        strategyImplBase.CanRun.Should().BeFalse();
    }

    #endregion


    #region Init

    [Fact]
    public void Init_Test_StrategyImplementation()
    {
        var strategyImpl = new FakeStrategyContextTest();
        var strategyImplBase = new StrategyBase("EURUSD", strategyImpl, _apiHandlerMock.Object, _loggerMock.Object,
            _strategyServiceFactoryMock.Object);

        _apiHandlerMock.Verify(x => x.SubscribePrice("EURUSD"), Times.Exactly(1));

        _apiHandlerMock.VerifyAdd(x => x.Disconnected += It.IsAny<EventHandler>(), Times.Once);
        _strategyResultMock.VerifyAdd(x => x.ResultTresholdEvent += It.IsAny<EventHandler<EventTreshold>>(),
            Times.Once);
        _positionHandlerMock.VerifyAdd(x => x.PositionOpenedEvent += It.IsAny<EventHandler<Position>>(), Times.Once);
        _positionHandlerMock.VerifyAdd(x => x.PositionUpdatedEvent += It.IsAny<EventHandler<Position>>(), Times.Once);
        _positionHandlerMock.VerifyAdd(x => x.PositionClosedEvent += It.IsAny<EventHandler<Position>>(), Times.Once);
        _positionHandlerMock.VerifyAdd(x => x.PositionRejectedEvent += It.IsAny<EventHandler<Position>>(), Times.Once);


        // Test init strategy Implementation
        // OpenPositionAction is tested directly in the openPosition region
        strategyImpl.Logger.Should().BeSameAs(_loggerMock.Object);
        _positionHandlerMock.VerifySet(m => m.DefaultSl = 10, Times.Once());
        _positionHandlerMock.VerifySet(m => m.DefaultTp = 10, Times.Once());
        strategyImpl.CalculateStopLossFunc.Should().Be(_positionHandlerMock.Object.CalculateStopLoss);
        strategyImpl.CalculateTakeProfitFunc.Should().Be(_positionHandlerMock.Object.CalculateTakeProfit);
    }

    [Fact]
    public void Init_Test_Chart()
    {
        var strategyImpl = new FakeStrategyContextTest();

        var strategyImplBase = new StrategyBase("EURUSD", strategyImpl, _apiHandlerMock.Object, _loggerMock.Object,
            _strategyServiceFactoryMock.Object);

        strategyImplBase.MainChart.Should().NotBeNull();
        strategyImplBase.MainChart.Should().BeSameAs(_chartMockMain.Object);
        strategyImplBase.MainChart.Count.Should().Be(100);

        strategyImplBase.SecondaryChartList.Count.Should().Be(2);

        _strategyServiceFactoryMock.Verify(
            x => x.GetChart(It.IsAny<ILogger>(), It.IsAny<IApiProviderBase>(), It.IsAny<string>(),
                It.Is<Timeframe>(x => x == Timeframe.Daily)), Times.Once);
        _strategyServiceFactoryMock.Verify(
            x => x.GetChart(It.IsAny<ILogger>(), It.IsAny<IApiProviderBase>(), It.IsAny<string>(),
                It.Is<Timeframe>(x => x == Timeframe.Monthly)), Times.Once);
        _strategyServiceFactoryMock.Verify(
            x => x.GetChart(It.IsAny<ILogger>(), It.IsAny<IApiProviderBase>(), It.IsAny<string>(),
                It.Is<Timeframe>(x => x == Timeframe.Weekly)), Times.Once);

        _chartMockMain.VerifyAdd(x => x.OnTickEvent += It.IsAny<Func<Tick, Task>>(), Times.Once);
        _chartMockMain.VerifyAdd(x => x.OnCandleEvent += It.IsAny<Func<Candle, Task>>(), Times.Once);
    }

    [Fact]
    public void Init_Test_Chart_no_main_chart()
    {
        var strategyImpl = new FakeStrategyContextChartWithNoMainChart();


        Action act = () => new StrategyBase("EURUSD", strategyImpl, _apiHandlerMock.Object, _loggerMock.Object,
            _strategyServiceFactoryMock.Object);

        act.Should().Throw<StrategyException>();
    }

    [Fact]
    public void Init_Test_Chart_no_timeframe_attribute()
    {
        var strategyImpl = new FakeStrategyContextChartWithNoTimeframeAttribute();


        Action act = () => new StrategyBase("EURUSD", strategyImpl, _apiHandlerMock.Object, _loggerMock.Object,
            _strategyServiceFactoryMock.Object);

        act.Should().Throw<StrategyException>();
    }

    #endregion

    #region TickEvent

    [Theory]
    [InlineData(true, true, false, true, true, false)]
    [InlineData(false, true, false, false, true, false)]
    [InlineData(true, false, false, false, true, false)]
    [InlineData(true, true, true, false, true, false)]
    public void Test_StrategyBase_OnTickEvents(bool canRun, bool runOnTick, bool positionInProgress,
        bool expectedCallerRun, bool expectedCallerTick, bool expectedCallerCandle)
    {
        // Arrange
        var strategyImpl = new FakeStrategyContextTest();
        strategyImpl.RunOnTick = runOnTick;

        var callerRun = false;
        var callerTick = false;
        var callerCandle = false;


        var strategyImplBase = new StrategyBase("EURUSD", strategyImpl, _apiHandlerMock.Object, _loggerMock.Object,
            _strategyServiceFactoryMock.Object);
        strategyImplBase.CanRun = canRun;
        
        strategyImpl.RunEvent += (sender, args) => callerRun = true;
        strategyImplBase.TickEvent += (sender, @event) => callerTick = true;
        strategyImplBase.CandleEvent += (sender, @event) => callerCandle = true;

        _positionHandlerMock.SetupGet(x => x.PositionInProgress).Returns(positionInProgress);

        // Act
        _chartMockMain.Object.LastPrice = new Tick();


        // Assert
        callerRun.Should().Be(expectedCallerRun);
        callerTick.Should().Be(expectedCallerTick);
        callerCandle.Should().Be(expectedCallerCandle);
    }


    [Theory]
    [InlineData(true, true, true, true, 1, 1)]
    [InlineData(true, true, true, false, 1, 0)]
    [InlineData(true, true, false, true, 0, 0)]
    public void Test_OnNewTick_Run_PositionInProgress_Update(
        bool canRun, bool runOnTick, bool updateOnTick, bool shouldUpdatePositionReturn, int shouldUpdateTimes,
        int updatePositionTimes)
    {
        var strategyImpl = new FakeStrategyContextTest();
        strategyImpl.RunOnTick = runOnTick;
        strategyImpl.UpdateOnTick = updateOnTick;

        var strategyImplBase = new StrategyBase("EURUSD", strategyImpl, _apiHandlerMock.Object, _loggerMock.Object,
            _strategyServiceFactoryMock.Object);
        strategyImplBase.CanRun = canRun;


        _positionHandlerMock.SetupGet(x => x.PositionInProgress).Returns(true);
        _positionHandlerMock.SetupGet(x => x.PositionOpened).Returns(new Position());

        strategyImpl.ShouldUpdatePositionProperty = shouldUpdatePositionReturn;

        // Act 
        _chartMockMain.Object.LastPrice = new Tick();

        // Assert
        _positionHandlerMock.Verify(x => x.UpdatePositionAsync(It.IsAny<Position>()),
            Times.Exactly(updatePositionTimes));
    }


    [Theory]
    [InlineData(true, true, true, true, 1, 1)]
    [InlineData(true, true, true, false, 1, 0)]
    [InlineData(true, true, false, true, 0, 0)]
    public void Test_OnNewTick_Run_PositionInProgress_Close(
        bool canRun, bool runOnTick, bool closeOnTick, bool shouldClosePositionReturn, int shouldCloseTimes,
        int closePositionTimes)
    {
        var strategyImpl = new FakeStrategyContextTest();
        strategyImpl.RunOnTick = runOnTick;
        strategyImpl.CloseOnTick = closeOnTick;

        var strategyImplBase = new StrategyBase("EURUSD", strategyImpl, _apiHandlerMock.Object, _loggerMock.Object,
            _strategyServiceFactoryMock.Object);
        strategyImplBase.CanRun = canRun;


        _positionHandlerMock.SetupGet(x => x.PositionInProgress).Returns(true);
        _positionHandlerMock.SetupGet(x => x.PositionOpened).Returns(new Position());

        strategyImpl.ShouldClosePositionProperty = shouldClosePositionReturn;

        // Act 
        _chartMockMain.Object.LastPrice = new Tick();

        // Assert
        _positionHandlerMock.Verify(x => x.ClosePositionAsync(It.IsAny<Position>()), Times.Exactly(closePositionTimes));
    }

    #endregion

    #region CandleEvent

    [Theory]
    [InlineData(true, false, false, true, false, false)] // Run() appelé, Tick et Candle pas appelés
    [InlineData(false, true, false, false, false, false)] // Run(), Tick et Candle pas appelés
    [InlineData(false, false, false, false, false, false)] // Run(), Tick et Candle pas appelés
    [InlineData(true, true, true, false, false, false)] // Run(), Tick et Candle pas appelés, même si positionInProgress
    public void Test_CandleEvent(bool canRun, bool runOnTick, bool positionInProgress, bool expectedCallerRun,
        bool expectedCallerTick, bool expectedCallerCandle)
    {
        // Arrange
        var strategyImpl = new FakeStrategyContextTest();
        strategyImpl.RunOnTick = runOnTick;

        var callerRun = false;
        var callerTick = false;
        var callerCandle = false;

        
        var strategyImplBase = new StrategyBase("EURUSD", strategyImpl, _apiHandlerMock.Object, _loggerMock.Object,
            _strategyServiceFactoryMock.Object);
        strategyImplBase.CanRun = canRun;
        strategyImpl.RunEvent += (sender, args) => callerRun = true;
        strategyImplBase.TickEvent += (sender, @event) => callerTick = true;
        strategyImplBase.CandleEvent += (sender, @event) => callerCandle = true;

        _positionHandlerMock.SetupGet(x => x.PositionInProgress).Returns(positionInProgress);

        // Act
        _chartMockMain.Object.OnOnCandleEvent(new Candle());

        // Assert
        callerRun.Should().Be(expectedCallerRun);
        callerTick.Should().Be(expectedCallerTick);
        callerCandle.Should().Be(true);
    }


    [Theory]
    [InlineData(true, true, true, true, 1, 0)]
    [InlineData(true, true, true, false, 1, 0)]
    [InlineData(true, true, false, true, 0, 1)]
    public void Test_OnNewCandle_Update(
        bool canRun, bool runOnTick, bool updateOnTick, bool shouldUpdatePositionReturn, int shouldUpdateTimes,
        int updatePositionTimes)
    {
        var strategyImpl = new FakeStrategyContextTest();
        strategyImpl.RunOnTick = runOnTick;
        strategyImpl.UpdateOnTick = updateOnTick;

        var strategyImplBase = new StrategyBase("EURUSD", strategyImpl, _apiHandlerMock.Object, _loggerMock.Object,
            _strategyServiceFactoryMock.Object);
        strategyImplBase.CanRun = canRun;


        _positionHandlerMock.SetupGet(x => x.PositionInProgress).Returns(true);
        _positionHandlerMock.SetupGet(x => x.PositionOpened).Returns(new Position());

        strategyImpl.ShouldUpdatePositionProperty = shouldUpdatePositionReturn;

        // Act 
        _chartMockMain.Object.OnOnCandleEvent(new Candle());

        // Assert
        _positionHandlerMock.Verify(x => x.UpdatePositionAsync(It.IsAny<Position>()),
            Times.Exactly(updatePositionTimes));
    }


    [Theory]
    [InlineData(true, true, true, true, 1, 0)]
    [InlineData(true, true, true, false, 1, 0)]
    [InlineData(true, true, false, true, 0, 1)]
    public void Test_OnNewCandle_Close(
        bool canRun, bool runOnTick, bool closeOnTick, bool shouldClosePosition, int shouldUpdateTimes,
        int closePositionTimes)
    {
        var strategyImpl = new FakeStrategyContextTest();
        strategyImpl.RunOnTick = runOnTick;
        strategyImpl.CloseOnTick = closeOnTick;

        var strategyImplBase = new StrategyBase("EURUSD", strategyImpl, _apiHandlerMock.Object, _loggerMock.Object,
            _strategyServiceFactoryMock.Object);
        strategyImplBase.CanRun = canRun;


        _positionHandlerMock.SetupGet(x => x.PositionInProgress).Returns(true);
        _positionHandlerMock.SetupGet(x => x.PositionOpened).Returns(new Position());

        strategyImpl.ShouldClosePositionProperty = shouldClosePosition;

        // Act 
        _chartMockMain.Object.OnOnCandleEvent(new Candle());

        // Assert
        _positionHandlerMock.Verify(x => x.ClosePositionAsync(It.IsAny<Position>()), Times.Exactly(closePositionTimes));
    }

    #endregion

    #region Close Strategy

    [Theory]
    [InlineData(StrategyReasonDisabled.User)]
    [InlineData(StrategyReasonDisabled.Api)]
    [InlineData(StrategyReasonDisabled.Error)]
    [InlineData(StrategyReasonDisabled.Treshold)]
    public async void Test_EventClose(StrategyReasonDisabled strategyReasonClosed)
    {
        var caller = false;
        var strategyImpl = new FakeStrategyContextTest();
        var strategyImplBase = new StrategyBase("EURUSD", strategyImpl, _apiHandlerMock.Object, _loggerMock.Object,
            _strategyServiceFactoryMock.Object);
        strategyImplBase.CanRun = true;

        strategyImplBase.StrategyDisabledEvent += (sender, closed) =>
        {
            caller = true;
            closed.EventField.Should().Contain(strategyReasonClosed.ToString());
        };


        await strategyImplBase.DisableStrategy(strategyReasonClosed);

        caller.Should().BeTrue();
        strategyImplBase.CanRun.Should().BeFalse();
        strategyImplBase.StrategyDisabled.Should().BeTrue();
    }

    [Fact]
    public async void Test_CloseStrategy_User_Reason()
    {
        // Arrange
        _apiHandlerMock.Setup(x => x.GetOpenedTradesAsync(It.IsAny<string>()))
            .ReturnsAsync(new Position());
        var strategyImpl = new FakeStrategyContextTest();
        var strategyImplBase = new StrategyBase("EURUSD", strategyImpl, _apiHandlerMock.Object, _loggerMock.Object,
            _strategyServiceFactoryMock.Object);
        strategyImplBase.CanRun = true;

        // Act
        await strategyImplBase.DisableStrategy(StrategyReasonDisabled.User);

        // Assert
        _apiHandlerMock.Verify(x => x.GetOpenedTradesAsync(It.IsAny<string>()), Times.Once);
        _positionHandlerMock.Verify(x => x.ClosePositionAsync(It.IsAny<Position>()), Times.Exactly(1));
        _apiHandlerMock.Verify(x => x.UnsubscribePrice(It.IsAny<string>()), Times.Once);
        strategyImplBase.StrategyDisabled.Should().BeTrue();
    }

    [Fact]
    public async void Test_CloseStrategy_Api_Reason()
    {
        // Act and Arrange
        var strategyImpl = new FakeStrategyContextTest();
        var strategyImplBase = new StrategyBase("EURUSD", strategyImpl, _apiHandlerMock.Object, _loggerMock.Object,
            _strategyServiceFactoryMock.Object);

        await strategyImplBase.DisableStrategy(StrategyReasonDisabled.Api);

        // Assert
        _apiHandlerMock.Verify(x => x.GetOpenedTradesAsync(It.IsAny<string>()), Times.Never);
        _positionHandlerMock.Verify(x => x.ClosePositionAsync(It.IsAny<Position>()), Times.Never);
        _apiHandlerMock.Verify(x => x.UnsubscribePrice(It.IsAny<string>()), Times.Never);
        strategyImplBase.StrategyDisabled.Should().BeTrue();
    }

    #endregion

    #region OpenPosition test

    [Fact]
    public void Test_OpenPosition_define()
    {
        // Arrange
        var lastPrice = new Tick
        {
            Bid = 1,
            Ask = 2
        };

        _chartMockMain.SetupGet(x => x.LastPrice).Returns(lastPrice);

        var strategyImpl = new FakeStrategyContextTest();
        var strategyImplBase = new StrategyBase("EURUSD", strategyImpl, _apiHandlerMock.Object, _loggerMock.Object,
            _strategyServiceFactoryMock.Object);

        // act

        strategyImpl.OpenPositionForTest(TypeOperation.Buy, 1, 1, 1, 1);

        // Assert
        _positionHandlerMock.Verify(x => x.OpenPositionAsync(
            It.Is<string>(x => x == "EURUSD"),
            It.Is<TypeOperation>(x => x == TypeOperation.Buy),
            It.Is<double>(x => x == 1),
            It.Is<decimal>(x => x == 1),
            It.Is<decimal>(x => x == 1),
            It.Is<double>(x => x == 1),
            It.Is<long>(x => x == 0)
        ), Times.Once);
    }


    [Fact]
    public void Test_OpenPosition_NoSl_tp_StrategyVolume_define()
    {
        // Arrange
        var lastPrice = new Tick
        {
            Bid = 1,
            Ask = 2
        };

        _chartMockMain.SetupGet(x => x.LastPrice).Returns(lastPrice);


        // act

        var strategyImpl = new FakeStrategyContextTest();
        var strategyImplBase = new StrategyBase("EURUSD", strategyImpl, _apiHandlerMock.Object, _loggerMock.Object,
            _strategyServiceFactoryMock.Object);

        strategyImpl.OpenPositionForTest(TypeOperation.Buy, 0, 0, 1, 1);
        // Assert
        _positionHandlerMock.Verify(x => x.OpenPositionAsync(
            It.Is<string>(x => x == "EURUSD"),
            It.Is<TypeOperation>(x => x == TypeOperation.Buy),
            It.Is<double>(x => x == 1),
            It.Is<decimal>(x => x == 0),
            It.Is<decimal>(x => x == 0),
            It.Is<double>(x => x == 1),
            It.Is<long>(x => x == 0)
        ), Times.Once);
    }

    #endregion

    #region Indicators

    [Fact]
    public void Init_Test_Indicators()
    {
        var strategyImpl = new FakeStrategyContextTest();
        var strategyImplBase = new StrategyBase("EURUSD", strategyImpl, _apiHandlerMock.Object, _loggerMock.Object,
            _strategyServiceFactoryMock.Object);

        strategyImplBase.MainIndicatorList.Count.Should().Be(2);
        strategyImplBase.SecondaryIndicatorList.Count.Should().Be(4);
        strategyImpl.SarIndicator.Count.Should().Be(100);
        strategyImpl.BollingerBandtest.Count.Should().Be(100);

        strategyImplBase.SecondaryIndicatorList.Should().ContainKey(Timeframe.OneMinute);
        strategyImplBase.SecondaryIndicatorList.Should().ContainKey(Timeframe.OneHour);
        strategyImplBase.SecondaryIndicatorList.Should().ContainKey(Timeframe.Weekly);
        strategyImplBase.SecondaryIndicatorList.Should().ContainKey(Timeframe.Monthly);

        strategyImpl.SarIndicator3.Count.Should().Be(100);
        strategyImpl.BollingerBand.Count.Should().Be(100);

        foreach (var indicator in strategyImplBase.MainIndicatorList) indicator.Name.Should().NotBeNullOrEmpty();

        foreach (var (key, value) in strategyImplBase.SecondaryIndicatorList)
        foreach (var indicator in value)
            indicator.Name.Should().NotBeNullOrEmpty();

        _chartMockMain.Verify(x => x.AggregateChart(It.Is<Timeframe>(y => y == Timeframe.OneHour)), Times.Once);
    }

    [Fact]
    public void Test_UpdateIndicator_Candle()
    {
        var strategyImpl = new FakeStrategyContextTest();
        var strategyImplBase = new StrategyBase("EURUSD", strategyImpl, _apiHandlerMock.Object, _loggerMock.Object,
            _strategyServiceFactoryMock.Object);

        _chartMockMain.Invocations.Clear();

        var candleToAdd = new Candle
        {
            Open = 1,
            High = 1,
            Low = 1,
            Close = 1,
            Date = _chartMockMain.Object.LastCandle.Date.AddDays(1)
        };
        var lastSartIndicator = strategyImpl.SarIndicator.Last();
        var lastSarIndicator3 = strategyImpl.SarIndicator3.Last();
        var lastBollingerTest = strategyImpl.BollingerBandtest.Last();
        var lastBollinger = strategyImpl.BollingerBand.Last();
        _chartMockMain.Object.Add(candleToAdd);
        _chartMockMain.Object.OnOnCandleEvent(candleToAdd);

        strategyImpl.SarIndicator.Count.Should().Be(101);
        strategyImpl.BollingerBandtest.Count.Should().Be(101);


        strategyImpl.SarIndicator3.Count.Should().Be(100);
        strategyImpl.BollingerBand.Count.Should().Be(100);

        strategyImpl.SarIndicator.Last().Should().NotBe(lastSartIndicator);
        strategyImpl.BollingerBandtest.Last().Should().NotBe(lastBollingerTest);
        strategyImpl.SarIndicator3.Last().Should().NotBe(lastSarIndicator3);
        strategyImpl.BollingerBand.Last().Should().NotBe(lastBollinger);
        _chartMockMain.As<IChart>()
            .Verify(x => x.AggregateChart(It.Is<Timeframe>(y => y == Timeframe.OneHour)), Times.Once);
        _chartMockMain.As<IChart>()
            .Verify(x => x.AggregateChart(It.Is<Timeframe>(y => y == Timeframe.OneMinute)), Times.Once);
        _chartMockMain.As<IChart>()
            .Verify(x => x.AggregateChart(It.Is<Timeframe>(y => y == Timeframe.Monthly)), Times.Never);
        _chartMockMain.As<IChart>()
            .Verify(x => x.AggregateChart(It.Is<Timeframe>(y => y == Timeframe.Weekly)), Times.Never);
    }

    [Fact]
    public void Test_UpdateIndicator_Tick()
    {
        var strategyImpl = new FakeStrategyContextTest();
        var strategyImplBase = new StrategyBase("EURUSD", strategyImpl, _apiHandlerMock.Object, _loggerMock.Object,
            _strategyServiceFactoryMock.Object);

        _chartMockMain.Invocations.Clear();

        var tickToAdd = new Tick
        {
            Bid = 100,
            Ask = 100
        };
        var lastSartIndicator = strategyImpl.SarIndicator.Last();
        var lastSarIndicator3 = strategyImpl.SarIndicator3.Last();
        var lastBollingerTest = strategyImpl.BollingerBandtest.Last();
        var lastBollinger = strategyImpl.BollingerBand.Last();

        _chartMockMain.Object.Last().Close = 10;
        _chartMockMain.Object.LastPrice = tickToAdd;

        strategyImpl.SarIndicator.Count.Should().Be(100);
        strategyImpl.BollingerBandtest.Count.Should().Be(100);


        strategyImpl.SarIndicator3.Count.Should().Be(100);
        strategyImpl.BollingerBand.Count.Should().Be(100);

        strategyImpl.SarIndicator.Last().Should().NotBe(lastSartIndicator);
        strategyImpl.BollingerBandtest.Last().Should().NotBe(lastBollingerTest);
        strategyImpl.SarIndicator3.Last().Should().NotBe(lastSarIndicator3);
        strategyImpl.BollingerBand.Last().Should().NotBe(lastBollinger);
        _chartMockMain.As<IChart>()
            .Verify(x => x.AggregateChart(It.Is<Timeframe>(y => y == Timeframe.OneHour)), Times.Once);
        _chartMockMain.As<IChart>()
            .Verify(x => x.AggregateChart(It.Is<Timeframe>(y => y == Timeframe.OneMinute)), Times.Once);
        _chartMockMain.As<IChart>()
            .Verify(x => x.AggregateChart(It.Is<Timeframe>(y => y == Timeframe.Monthly)), Times.Never);
        _chartMockMain.As<IChart>()
            .Verify(x => x.AggregateChart(It.Is<Timeframe>(y => y == Timeframe.Weekly)), Times.Never);
    }

    #endregion
}