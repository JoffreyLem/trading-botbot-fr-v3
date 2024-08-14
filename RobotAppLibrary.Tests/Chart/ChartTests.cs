using FluentAssertions;
using Moq;
using RobotAppLibrary.Api.Providers.Base;
using RobotAppLibrary.Chart;
using RobotAppLibrary.Modeles;
using RobotAppLibrary.Utils;
using Serilog;

namespace RobotAppLibrary.Tests.Chart;

public class ChartTests
{
    private readonly Mock<IApiProviderBase> _mockApiProvider;
    private readonly Mock<ILogger> _mockLogger;

    public ChartTests()
    {
        _mockApiProvider = new Mock<IApiProviderBase>();
        _mockLogger = new Mock<ILogger>();
        _mockLogger.Setup(x => x.ForContext<RobotAppLibrary.Chart.Chart>())
            .Returns(_mockLogger.Object);

        _mockApiProvider.Setup(x => x.GetTradingHoursAsync(It.IsAny<string>())).ReturnsAsync(new TradeHourRecord());
    }

    #region Init

    [Fact]
    public void Init_Should_Initialize_With_Candles()
    {
        var candles = TestUtils.GenerateCandle(Timeframe.OneMinute, 5);
        _mockApiProvider.Setup(x => x.GetChartAsync(It.IsAny<string>(), It.IsAny<Timeframe>())).ReturnsAsync(candles);
        _mockApiProvider.Setup(x => x.GetTickPriceAsync(It.IsAny<string>())).ReturnsAsync(new Tick());

        var chart = new RobotAppLibrary.Chart.Chart(_mockApiProvider.Object, _mockLogger.Object, Timeframe.OneMinute, "TEST_SYMBOL");

        _mockApiProvider.Verify(x=>x.GetChartAsync(It.IsAny<string>(),It.IsAny<Timeframe>()),Times.Once);
        _mockApiProvider.Verify(x => x.GetTradingHoursAsync(It.IsAny<string>()),Times.Once);
        _mockApiProvider.Verify(x=>x.GetTickPriceAsync(It.IsAny<string>()),Times.Once);
        
        chart.Should().NotBeEmpty();
        chart.Count.Should().Be(5);
    }
    
    [Fact]
    public void Test_Init_throw_exception()
    {
        // Arrange

        _mockApiProvider.Setup(x => x.GetChartAsync(It.IsAny<string>(), It.IsAny<Timeframe>()))
            .ThrowsAsync(new Exception());

        // Act && assert
        var candleList = () => new RobotAppLibrary.Chart.Chart(_mockApiProvider.Object, _mockLogger.Object,
            Timeframe.FifteenMinutes,
            "EURUSD");

        // Assert
        candleList.Should().Throw<ChartException>();
    }

    #endregion

    #region Aggregate

    [Theory]
    [InlineData(Timeframe.FiveMinutes,401)]
    [InlineData(Timeframe.FifteenMinutes,135)]
    [InlineData(Timeframe.ThirtyMinutes,68 )]
    [InlineData(Timeframe.OneHour,35)]
    [InlineData(Timeframe.FourHour,10 )]
    [InlineData(Timeframe.Daily,3 )]
    [InlineData(Timeframe.Weekly,1 )]
    [InlineData(Timeframe.Monthly, 2)]
    public void Test_Aggregate(Timeframe timeframe, int expectedCount)
    {
        var candles = TestUtils.GenerateCandle(Timeframe.OneMinute, 10000);
        _mockApiProvider.Setup(x => x.GetChartAsync(It.IsAny<string>(), It.IsAny<Timeframe>())).ReturnsAsync(candles);
        _mockApiProvider.Setup(x => x.GetTickPriceAsync(It.IsAny<string>())).ReturnsAsync(new Tick());

        var chart = new RobotAppLibrary.Chart.Chart(_mockApiProvider.Object, _mockLogger.Object, Timeframe.OneMinute, "TEST_SYMBOL");

        _mockApiProvider.Verify(x=>x.GetChartAsync(It.IsAny<string>(),It.IsAny<Timeframe>()),Times.Once);
        _mockApiProvider.Verify(x => x.GetTradingHoursAsync(It.IsAny<string>()),Times.Once);
        _mockApiProvider.Verify(x=>x.GetTickPriceAsync(It.IsAny<string>()),Times.Once);

        var result = chart.As<IChart>().AggregateChart(timeframe);

        result.Count().Should().Be(expectedCount);
    }

    #endregion

    #region New tick

     [Theory]
        [InlineData(Timeframe.OneMinute)]
        [InlineData(Timeframe.FiveMinutes)]
        [InlineData(Timeframe.FifteenMinutes)]
        [InlineData(Timeframe.ThirtyMinutes)]
        [InlineData(Timeframe.OneHour)]
        [InlineData(Timeframe.FourHour)]
        [InlineData(Timeframe.Daily)]
        [InlineData(Timeframe.Weekly)]
        [InlineData(Timeframe.Monthly)]
        public void Test_NewTick_UpdateLastCandle(Timeframe timeframe)
        {
            // Arrange
            var timeframeMinute = timeframe.GetMinuteFromTimeframe();
            var callerCandle = false;
            var callerTick = false;

            var initialCandles = TestUtils.GenerateCandle(timeframe, 10);
            _mockApiProvider.Setup(x => x.GetChartAsync(It.IsAny<string>(), It.IsAny<Timeframe>()))
                .ReturnsAsync(initialCandles);

            var chart = new RobotAppLibrary.Chart.Chart(_mockApiProvider.Object, _mockLogger.Object, timeframe, "TEST_SYMBOL");

            var lastDate = chart.Last().Date.AddMinutes(timeframeMinute).AddSeconds(-10);
            var tick = new Tick { Symbol = "TEST_SYMBOL", Date = lastDate, Bid = 1, AskVolume = 1, BidVolume = 1};
       

            chart.OnCandleEvent += candle =>
            {
                callerCandle = true;
                return Task.CompletedTask;
            };
            chart.OnTickEvent += tick1 =>
            {
                callerTick = true;
                return Task.CompletedTask;
            };

            // Act
            _mockApiProvider.Raise(x => x.TickEvent += null, this, tick);

            // Assert
            callerCandle.Should().BeFalse();
            callerTick.Should().BeTrue();
            chart.Last().Ticks.Count.Should().Be(1);
            chart.Last().Close.Should().Be(1);
            chart.Last().AskVolume.Should().Be(1);
            chart.Last().BidVolume.Should().Be(1);
            chart.Last().Volume.Should().Be(2); 
        }
        
        
          [Theory]
        [InlineData(Timeframe.OneMinute)]
        [InlineData(Timeframe.FiveMinutes)]
        [InlineData(Timeframe.FifteenMinutes)]
        [InlineData(Timeframe.ThirtyMinutes)]
        [InlineData(Timeframe.OneHour)] 
        [InlineData(Timeframe.FourHour)]
        [InlineData(Timeframe.Daily)]
        [InlineData(Timeframe.Weekly)]
        [InlineData(Timeframe.Monthly)]
        public void Test_NewTick_UpdateLastCandle_If_last_is_0(Timeframe timeframe)
        {
            // Arrange
            var timeframeMinute = timeframe.GetMinuteFromTimeframe();
            var caller = false;
            var callerTick = false;

            var candleListData = TestUtils.GenerateCandle(timeframe, 1);
            var lastCandle = candleListData.Last();
            lastCandle.Open = 0;
            lastCandle.High = 0;
            lastCandle.Low = 0;
            lastCandle.Close = 0;
            
            _mockApiProvider.Setup(x => x.GetChartAsync(It.IsAny<string>(), It.IsAny<Timeframe>()))
                .ReturnsAsync(candleListData);

            var chart = new RobotAppLibrary.Chart.Chart(_mockApiProvider.Object, _mockLogger.Object, timeframe, "TEST_SYMBOL");

            var lastDate = chart.Last().Date.AddMinutes(timeframeMinute).AddSeconds(-10);
            var tick = new Tick { Symbol = "TEST_SYMBOL", Date = lastDate, Bid = 10, AskVolume = 1, BidVolume = 1};
            
            chart.OnCandleEvent += candle =>
            {
                caller = true;
                return Task.CompletedTask;
            };
            chart.OnTickEvent += tick1 =>
            {
                callerTick = true;
                return Task.CompletedTask;
            };

            // Act
            _mockApiProvider.Raise(x => x.TickEvent += null, this, tick);

            // Assert
            caller.Should().BeFalse();
            callerTick.Should().BeTrue();
            chart.Last().Ticks.Count.Should().Be(1);
            chart.Last().AskVolume.Should().Be(1);
            chart.Last().BidVolume.Should().Be(1);
            chart.Last().Volume.Should().Be(2);
            chart.Last().Open.Should().Be(10);
            chart.Last().High.Should().Be(10);
            chart.Last().Low.Should().Be(10);
            chart.Last().Close.Should().Be(10);
        }
        
        
         [Theory]
        [InlineData(Timeframe.OneMinute)]
        [InlineData(Timeframe.FiveMinutes)]
        [InlineData(Timeframe.FifteenMinutes)]
        [InlineData(Timeframe.ThirtyMinutes)]
        [InlineData(Timeframe.OneHour)]
        [InlineData(Timeframe.FourHour)]
        [InlineData(Timeframe.Daily)]
        [InlineData(Timeframe.Monthly)]
        [InlineData(Timeframe.Weekly)]
        public void Test_NewTick_AddNewCandle(Timeframe timeframe)
        {
            // Arrange
            var timeframeMinute = timeframe.GetMinuteFromTimeframe();
            var caller = false;
            var callerTick = false;

            var candleListData = TestUtils.GenerateCandle(timeframe, 100);
            _mockApiProvider.Setup(x => x.GetChartAsync(It.IsAny<string>(), It.IsAny<Timeframe>()))
                .ReturnsAsync(candleListData);

            var chart = new RobotAppLibrary.Chart.Chart(_mockApiProvider.Object, _mockLogger.Object, timeframe, "EURUSD");

            var lastDate = new DateTime();
            if (timeframe == Timeframe.Monthly)
                lastDate = chart.Last().Date.AddMonths(1);
            else
                lastDate = chart.Last().Date.AddMinutes(timeframeMinute);

            var tick = new Tick { Symbol = "EURUSD", Date = lastDate, Bid = 1, AskVolume = 1, BidVolume = 1 };

            chart.OnCandleEvent += candle =>
            {
                caller = true;
                return Task.CompletedTask;
            };
            chart.OnTickEvent += tick1 =>
            {
                callerTick = true;
                return Task.CompletedTask;
            };

            // Act
            _mockApiProvider.Raise(x => x.TickEvent += null, this, tick);

            // Assert
            caller.Should().BeTrue();
            callerTick.Should().BeFalse();
            chart.Count.Should().Be(101); 
            chart.Last().Ticks.Count.Should().Be(1);
            chart.Last().Close.Should().Be(1);
            chart.Last().AskVolume.Should().Be(1);
            chart.Last().BidVolume.Should().Be(1);
            chart.Last().Volume.Should().Be(2); 
        }
        
        
          [Theory]
        [InlineData(Timeframe.OneMinute)]
        [InlineData(Timeframe.FiveMinutes)]
        [InlineData(Timeframe.FifteenMinutes)]
        [InlineData(Timeframe.ThirtyMinutes)]
        [InlineData(Timeframe.OneHour)]
        [InlineData(Timeframe.FourHour)]
        [InlineData(Timeframe.Daily)]
        [InlineData(Timeframe.Monthly)]
        [InlineData(Timeframe.Weekly)]
        public void Test_NewTick_AddNewCandle_at2000(Timeframe timeframe)
        {
            // Arrange
            var timeframeMinute = timeframe.GetMinuteFromTimeframe();
            var caller = false;
            var callerTick = false;

            var candleListData = TestUtils.GenerateCandle(timeframe, 2000);
            _mockApiProvider.Setup(x => x.GetChartAsync(It.IsAny<string>(), It.IsAny<Timeframe>()))
                .ReturnsAsync(candleListData);

            var chart = new RobotAppLibrary.Chart.Chart(_mockApiProvider.Object, _mockLogger.Object, timeframe, "EURUSD");

            var lastDate = new DateTime();
            if (timeframe == Timeframe.Monthly)
                lastDate = chart.Last().Date.AddMonths(1);
            else
                lastDate = chart.Last().Date.AddMinutes(timeframeMinute);

            var tick = new Tick { Symbol = "EURUSD", Date = lastDate, Bid = 1, AskVolume = 1, BidVolume = 1};
        

            chart.OnCandleEvent += candle =>
            {
                caller = true;
                return Task.CompletedTask;
            };
            chart.OnTickEvent += tick1 =>
            {
                callerTick = true;
                return Task.CompletedTask;
            };

            // Act
            _mockApiProvider.Raise(x => x.TickEvent += null, this, tick);

            // Assert
            caller.Should().BeTrue();
            callerTick.Should().BeFalse();
            chart.Count.Should().Be(2000); // Ensure the count remains at 2000
            chart.Last().Ticks.Count.Should().Be(1);
            chart.Last().Close.Should().Be(1);
            chart.Last().AskVolume.Should().Be(1);
            chart.Last().BidVolume.Should().Be(1);
            chart.Last().Volume.Should().Be(2); // Volume is sum of AskVolume and BidVolume
        }
        
        [Theory]
        [InlineData(Timeframe.OneMinute)]
        [InlineData(Timeframe.FiveMinutes)]
        [InlineData(Timeframe.FifteenMinutes)]
        [InlineData(Timeframe.ThirtyMinutes)]
        [InlineData(Timeframe.OneHour)]
        [InlineData(Timeframe.FourHour)]
        [InlineData(Timeframe.Daily)]
        [InlineData(Timeframe.Monthly)]
        [InlineData(Timeframe.Weekly)]
        public void Test_NewTick_AddNewTick_count_is_0(Timeframe timeframe)
        {
            // Arrange
            var timeframeMinute = timeframe.GetMinuteFromTimeframe();
            var caller = false;
            var callerTick = false;

            _mockApiProvider.Setup(x => x.GetChartAsync(It.IsAny<string>(), It.IsAny<Timeframe>()))
                .ReturnsAsync(new List<Candle>());

            var chart = new RobotAppLibrary.Chart.Chart(_mockApiProvider.Object, _mockLogger.Object, timeframe, "EURUSD");

            var tick = new Tick { Symbol = "EURUSD", Date = DateTime.UtcNow, Bid = 1, AskVolume = 1, BidVolume = 1};
     

            chart.OnCandleEvent += candle =>
            {
                caller = true;
                return Task.CompletedTask;
            };
            chart.OnTickEvent += tick1 =>
            {
                callerTick = true;
                return Task.CompletedTask;
            };

            // Act
            _mockApiProvider.Raise(x => x.TickEvent += null, this, tick);

            // Assert
            caller.Should().BeTrue();
            callerTick.Should().BeFalse();
            chart.Count.Should().Be(1);
            chart.Last().Ticks.Count.Should().Be(1);
            chart.Last().Close.Should().Be(1);
            chart.Last().AskVolume.Should().Be(1);
            chart.Last().BidVolume.Should().Be(1);
            chart.Last().Volume.Should().Be(2); // Volume is sum of AskVolume and BidVolume
        }
        
        [Theory]
        [InlineData(Timeframe.OneMinute)]
        [InlineData(Timeframe.FiveMinutes)]
        [InlineData(Timeframe.FifteenMinutes)]
        [InlineData(Timeframe.ThirtyMinutes)]
        [InlineData(Timeframe.OneHour)]
        [InlineData(Timeframe.FourHour)]
        [InlineData(Timeframe.Daily)]
        [InlineData(Timeframe.Monthly)]
        [InlineData(Timeframe.Weekly)]
        public void Test_NewTick_Tick_on_new_candle(Timeframe timeframe)
        {
            // Arrange
            var timeframeMinute = timeframe.GetMinuteFromTimeframe();
            var caller = false;
            var callerTick = false;

            var candleListData = TestUtils.GenerateCandle(timeframe, 100);
            _mockApiProvider.Setup(x => x.GetChartAsync(It.IsAny<string>(), It.IsAny<Timeframe>()))
                .ReturnsAsync(candleListData);

            var chart = new RobotAppLibrary.Chart.Chart(_mockApiProvider.Object, _mockLogger.Object, timeframe, "EURUSD");

            var lastDate = new DateTime();
            if (timeframe == Timeframe.Monthly)
                lastDate = chart.Last().Date.AddMonths(1);
            else
                lastDate = chart.Last().Date.AddMinutes(timeframeMinute);

            var tick = new Tick { Symbol = "EURUSD", Date = lastDate, Bid = 1, AskVolume = 1, BidVolume = 1};
    

            chart.OnCandleEvent += candle =>
            {
                caller = true;
                return Task.CompletedTask;
            };
            chart.OnTickEvent += tick1 =>
            {
                callerTick = true;
                return Task.CompletedTask;
            };

            // Act
            _mockApiProvider.Raise(x => x.TickEvent += null, this, tick);

            // Assert
            caller.Should().BeTrue();
            callerTick.Should().BeFalse();
            chart.Last().Date.Should().Be(lastDate);
            chart.Last().Ticks[ ^1].Should().Be(tick);
        }
        
        

    #endregion

   
}