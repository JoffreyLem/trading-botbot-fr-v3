using FluentAssertions;
using Moq;
using RobotAppLibrary.Api.Providers.Base;
using RobotAppLibrary.Modeles;
using RobotAppLibrary.TradingManager;
using Serilog;

namespace RobotAppLibrary.Tests.Results;

public class StrategyResultTests
{
    private readonly Mock<IApiProviderBase> _apiProviderBaseMock;
    private readonly string _positionReference = "TestReference";
    private readonly StrategyResult _strategyResult;
    private readonly Mock<ILogger> _loggerMock = new Mock<ILogger>();

    public StrategyResultTests()
    {
        _apiProviderBaseMock = new Mock<IApiProviderBase>();
        _loggerMock.Setup(x => x.ForContext<StrategyResult>()).Returns(_loggerMock.Object);
        _apiProviderBaseMock.Setup(api => api.GetBalanceAsync()).ReturnsAsync(new AccountBalance { Balance = 1000 });
        _apiProviderBaseMock.Setup(api => api.GetAllPositionsByCommentAsync(It.IsAny<string>())).ReturnsAsync(
            new List<Position>
            {
                new()
                {
                    Profit = 10,
                    DateClose = DateTime.Now.AddMonths(-1)
                }
            });

        _strategyResult = new StrategyResult(_apiProviderBaseMock.Object, _positionReference, _loggerMock.Object);
    }

    [Fact]
    public void Constructor_ShouldInitializeGlobalResults()
    {
        _strategyResult.GlobalResults.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_ShouldSetAccountBalance()
    {
        _apiProviderBaseMock.Verify(api => api.GetBalanceAsync(), Times.Once);
    }

    [Fact]
    public void Init_ShouldSubscribeToEvents()
    {
        _apiProviderBaseMock.VerifyAdd(api => api.NewBalanceEvent += It.IsAny<EventHandler<AccountBalance>>(),
            Times.Once);
        _apiProviderBaseMock.VerifyAdd(api => api.PositionClosedEvent += It.IsAny<EventHandler<Position>>(),
            Times.Once);
    }

    [Fact]
    public void ApiProviderBaseOnPositionClosedEvent_ShouldUpdateResult_WhenPositionClosed()
    {
        var position = new Position { StrategyId = _positionReference, Profit = 100, DateClose = DateTime.Now };

        _strategyResult.GlobalResults.Positions.Count.Should().Be(1);

        _apiProviderBaseMock.Raise(api => api.PositionClosedEvent += null, this, position);

        _strategyResult.GlobalResults.Positions.Should().ContainSingle(p => p == position);
    }


    [Fact]
    public void ApiProviderBaseOnPositionClosedEvent_ShouldUpdateMonthlyResult_WhenPositionClosed()
    {
        var position = new Position
            { StrategyId = _positionReference, Profit = 100, DateClose = DateTime.Now.AddMonths(-1) };

        _strategyResult.GlobalResults.MonthlyResults.First().Positions.Count.Should().Be(1);

        _apiProviderBaseMock.Raise(api => api.PositionClosedEvent += null, this, position);

        _strategyResult.GlobalResults.MonthlyResults.First().Positions.Should().ContainSingle(p => p == position);
    }

    [Fact]
    public void PositionClosedEvent_ShouldInvokeDrawdownEvent_WhenDrawdownExceedsToleratedDrawdown()
    {
        var eventInvoked = false;
        _strategyResult.ResultTresholdEvent += (sender, args) =>
        {
            if (args == EventTreshold.Drowdown)
                eventInvoked = true;
        };
        _strategyResult.ToleratedDrawnDown = 5;
        _strategyResult.GlobalResults.Result = new Result { Drawndown = 60 };
        _strategyResult.SecureControlPosition = true;

        var position = new Position { StrategyId = _positionReference, Profit = -100, DateClose = DateTime.Now };

        _apiProviderBaseMock.Raise(api => api.PositionClosedEvent += null, this, position);

        eventInvoked.Should().BeTrue();
        //TODO : Voir pour corriger ce cas. 
        //_strategyResult.Treshold.Should().Be(EventTreshold.Drowdown);
    }

    [Fact]
    public void PositionClosedEvent_ShouldInvokeLooseStreakEvent_WhenLooseStreakConditionMet()
    {
        var eventInvoked = false;
        _strategyResult.ResultTresholdEvent += (sender, args) =>
        {
            if (args == EventTreshold.LooseStreak)
                eventInvoked = true;
        };
        _strategyResult.LooseStreak = 3;
        _strategyResult.GlobalResults.Positions.AddRange(new List<Position>
        {
            new() { Profit = 100000 },
            new() { Profit = -5 },
            new() { Profit = -5 },
            new() { Profit = -5 }
        });
        _strategyResult.SecureControlPosition = true;

        var position = new Position { StrategyId = _positionReference, Profit = -40, DateClose = DateTime.Now };

        _apiProviderBaseMock.Raise(api => api.PositionClosedEvent += null, this, position);

        eventInvoked.Should().BeTrue();
        _strategyResult.Treshold.Should().Be(EventTreshold.LooseStreak);
    }

    [Fact]
    public void PositionClosedEvent_ShouldInvokeProfitFactorEvent_WhenProfitFactorIsLessThanOrEqualToOne()
    {
        var eventInvoked = false;
        _strategyResult.ResultTresholdEvent += (sender, args) =>
        {
            if (args == EventTreshold.Profitfactor)
                eventInvoked = true;
        };
        _strategyResult.GlobalResults.Result = new Result { ProfitFactor = 0.5M };
        _strategyResult.SecureControlPosition = true;

        var position = new Position { StrategyId = _positionReference, Profit = -100, DateClose = DateTime.Now };

        _apiProviderBaseMock.Raise(api => api.PositionClosedEvent += null, this, position);

        eventInvoked.Should().BeTrue();
        _strategyResult.Treshold.Should().Be(EventTreshold.Profitfactor);
    }
}