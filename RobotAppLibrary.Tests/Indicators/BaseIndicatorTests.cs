using FluentAssertions;
using RobotAppLibrary.Indicators.Base;
using RobotAppLibrary.Modeles;
using Skender.Stock.Indicators;

namespace RobotAppLibrary.Tests.Indicators
{
    public class BaseIndicatorTests
    {
        private class TestResult : ResultBase
        {
            public double Value { get; set; }
        }

        private class TestIndicator : BaseIndicator<TestResult>
        {
            protected override IEnumerable<TestResult> Update(List<Candle> data)
            {
                return data.Select(c => new TestResult { Value = (double) c.Close });
            }
        }

        [Fact]
        public void Indexer_Should_Return_Correct_Item()
        {
            var indicator = new TestIndicator();
            var candles = GenerateCandles(5).ToList();
            indicator.UpdateIndicator(candles);

            for (int i = 0; i < candles.Count(); i++)
            {
                indicator[i].Value.Should().Be((double)candles.ElementAt(i).Close);
            }
        }

        [Fact]
        public void Indexer_Should_Throw_IndexOutOfRangeException()
        {
            var indicator = new TestIndicator();
            var candles = GenerateCandles(5).ToList();
            indicator.UpdateIndicator(candles);

            Action act = () => { var value = indicator[5]; };
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
            for (int i = 0; i < newCandles.Count(); i++)
            {
                indicator[i].Value.Should().Be((double)newCandles.ElementAt(i).Close);
            }
        }


        private IEnumerable<Candle> GenerateCandles(int count)
        {
            var candles = new List<Candle>();
            for (int i = 1; i <= count; i++)
            {
                candles.Add(new Candle { Close = i });
            }
            return candles;
        }
    }
}
