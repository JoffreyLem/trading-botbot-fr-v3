using FluentAssertions;
using RobotAppLibrary.Indicators.Base;
using RobotAppLibrary.Modeles;
using Skender.Stock.Indicators;

namespace RobotAppLibrary.Tests.Indicators;

public class BaseIndicatorTests
{
    [Fact]
    public void Indexer_Should_Return_Correct_Item()
    {
        var indicator = new TestIndicator();
        var candles = GenerateCandles(5).ToList();
        indicator.UpdateIndicator(candles);

        for (var i = 0; i < candles.Count(); i++) indicator[i].Value.Should().Be((double)candles.ElementAt(i).Close);
    }

    [Fact]
    public void Indexer_Should_Throw_IndexOutOfRangeException()
    {
        var indicator = new TestIndicator();
        var candles = GenerateCandles(5).ToList();
        indicator.UpdateIndicator(candles);

        var act = () =>
        {
            var value = indicator[5];
        };
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Last_Should_Return_Last_Element()
    {
        var indicator = new TestIndicator();
        var candles = GenerateCandles(5).ToList();
        indicator.UpdateIndicator(candles);

        var last = indicator.Last();
        last.Value.Should().Be((double)candles.Last().Close);
    }

    [Fact]
    public void Last_Should_Throw_InvalidOperationException_If_Empty()
    {
        var indicator = new TestIndicator();

        Action act = () => indicator.Last();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("The list is empty.");
    }

    [Fact]
    public void LastOrDefault_Should_Return_Last_Element()
    {
        var indicator = new TestIndicator();
        var candles = GenerateCandles(5).ToList();
        indicator.UpdateIndicator(candles);

        var lastOrDefault = indicator.LastOrDefault();
        lastOrDefault.Value.Should().Be((double)candles.Last().Close);
    }

    [Fact]
    public void LastOrDefault_Should_Return_Default_If_Empty()
    {
        var indicator = new TestIndicator();

        var lastOrDefault = indicator.LastOrDefault();
        lastOrDefault.Should().BeNull();
    }

    [Fact]
    public void UpdateIndicator_Should_Clear_Previous_Data()
    {
        var indicator = new TestIndicator();
        var initialCandles = GenerateCandles(3).ToList();
        indicator.UpdateIndicator(initialCandles);
        indicator.Count.Should().Be(3);

        var newCandles = GenerateCandles(5).ToList();
        indicator.UpdateIndicator(newCandles);
        indicator.Count.Should().Be(5);
        for (var i = 0; i < newCandles.Count(); i++)
            indicator[i].Value.Should().Be((double)newCandles.ElementAt(i).Close);
    }

      [Fact]
    public void HasCrossed_ShouldDetectUpwardCross()
    {
        // Arrange
        var candles = new List<Candle>
        {
            new Candle { Close = 100 },
            new Candle { Close = 95 },
            new Candle { Close = 105 } // Croisement haussier
        };

        var indicator = new TestIndicator();
        indicator.UpdateIndicator(candles);

        // Act
        var result = indicator.HasCrossed(
            candles,
            indicatorSelector: r =>(decimal) r.Value,
            priceSelector: c => c.Close,
            checkUpward: true
        );

        // Assert
        result.Should().BeTrue("the indicator crossed upward over the candles' close price.");
    }

    [Fact]
    public void HasCrossed_ShouldDetectDownwardCross()
    {
        // Arrange
        var candles = new List<Candle>
        {
            new Candle { Close = 100 },
            new Candle { Close = 105 },
            new Candle { Close = 95 } // Croisement baissier
        };

        var indicator = new TestIndicator();
        indicator.UpdateIndicator(candles);

        // Act
        var result = indicator.HasCrossed(
            candles,
            indicatorSelector: r =>(decimal) r.Value,
            priceSelector: c => c.Close,
            checkUpward: false
        );

        // Assert
        result.Should().BeTrue("the indicator crossed downward below the candles' close price.");
    }

    [Fact]
    public void HasCrossed_ShouldReturnFalse_WhenNoCrossOccurs()
    {
        // Arrange
        var candles = new List<Candle>
        {
            new Candle { Close = 100 },
            new Candle { Close = 100 },
            new Candle { Close = 100 } // Pas de croisement
        };

        var indicator = new TestIndicator();
        indicator.UpdateIndicator(candles);

        // Act
        var result = indicator.HasCrossed(
            candles,
            indicatorSelector: r =>(decimal) r.Value,
            priceSelector: c => c.Close,
            checkUpward: true
        );

        // Assert
        result.Should().BeFalse("no upward cross occurred in the given data.");
    }

    [Fact]
    public void HasCrossed_ShouldDetectCrossWithLookBackSteps()
    {
        // Arrange
        var candles = new List<Candle>
        {
            new Candle { Close = 100 },
            new Candle { Close = 95 },
            new Candle { Close = 105 }, // Croisement haussier
            new Candle { Close = 110 }
        };

        var indicator = new TestIndicator();
        indicator.UpdateIndicator(candles);

        // Act
        var result = indicator.HasCrossed(
            candles,
            indicatorSelector: r =>(decimal) r.Value,
            priceSelector: c => c.Close,
            checkUpward: true,
            lookBackSteps: 1
        );

        // Assert
        result.Should().BeTrue("an upward cross occurred within the look-back period.");
    }

    [Fact]
    public void HasCrossed_ShouldHandleEmptyData()
    {
        // Arrange
        var candles = new List<Candle>();
        var indicator = new TestIndicator();

        // Act
        Action act = () => indicator.HasCrossed(
            candles,
            indicatorSelector: r =>(decimal) r.Value,
            priceSelector: c => c.Close
        );

        // Assert
        act.Should().Throw<InvalidOperationException>("the data is insufficient to determine a crossover.");
    }

    private IEnumerable<Candle> GenerateCandles(int count)
    {
        var candles = new List<Candle>();
        for (var i = 1; i <= count; i++) candles.Add(new Candle { Close = i });
        return candles;
    }

    private class TestResult : ResultBase
    {
        public double Value { get; set; }
    }

    private class TestIndicator : BaseIndicator<TestResult>
    {
        protected override IEnumerable<TestResult> Update(List<Candle> data)
        {
            return data.Select(c => new TestResult { Value = (double)c.Close });
        }
    }
}