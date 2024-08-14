using FluentAssertions;
using RobotAppLibrary.Modeles;
using RobotAppLibrary.TradingManager;
using RobotAppLibrary.TradingManager.Exceptions;

namespace RobotAppLibrary.Tests.Results
{
    public class ResultCalculatorTests
    {
        [Fact]
        public void CalculateResults_ShouldReturnCorrectResult_WhenPositionsHavePositiveAndNegativeProfits()
        {
            var positions = new List<Position>
            {
                new Position { Profit = 100, DateClose = DateTime.Now.AddDays(-3) },
                new Position { Profit = -50, DateClose = DateTime.Now.AddDays(-2) },
                new Position { Profit = 200, DateClose = DateTime.Now.AddDays(-1) },
                new Position { Profit = -100, DateClose = DateTime.Now }
            };

            var calculator = new ResultCalculator();

            var result = calculator.CalculateResults(positions);

            result.GainMax.Should().Be(200);
            result.ProfitPositif.Should().Be(300);
            result.TotalPositionPositive.Should().Be(2);
            result.MoyennePositive.Should().Be(150);

            result.PerteMax.Should().Be(-100);
            result.ProfitNegatif.Should().Be(-150);
            result.TotalPositionNegative.Should().Be(2);
            result.MoyenneNegative.Should().Be(-75);

            result.Profit.Should().Be(150);
            result.TotalPositions.Should().Be(4);
            result.MoyenneProfit.Should().BeApproximately(37.5m, 0.01m);

            result.RatioMoyennePositifNegatif.Should().BeApproximately(-2, 0.01m);
            result.ProfitFactor.Should().BeApproximately(2, 0.01m);
            result.TauxReussite.Should().BeApproximately(50, 0.01);

            result.DrawndownMax.Should().Be(300); // Peak was 200, then went down to -100 (200 - (-100))
            result.Drawndown.Should().Be(300);
        }

        [Fact]
        public void CalculateResults_ShouldHandleEmptyPositionsList()
        {
            var positions = new List<Position>();
            var calculator = new ResultCalculator();

            var result = calculator.CalculateResults(positions);

            result.GainMax.Should().Be(0);
            result.ProfitPositif.Should().Be(0);
            result.TotalPositionPositive.Should().Be(0);
            result.MoyennePositive.Should().Be(0);

            result.PerteMax.Should().Be(0);
            result.ProfitNegatif.Should().Be(0);
            result.TotalPositionNegative.Should().Be(0);
            result.MoyenneNegative.Should().Be(0);

            result.Profit.Should().Be(0);
            result.TotalPositions.Should().Be(0);
            result.MoyenneProfit.Should().Be(0);

            result.RatioMoyennePositifNegatif.Should().Be(0);
            result.ProfitFactor.Should().Be(0);
            result.TauxReussite.Should().Be(0);

            result.DrawndownMax.Should().Be(0);
            result.Drawndown.Should().Be(0);
        }

        [Fact]
        public void CalculateResults_ShouldThrowResultException_WhenExceptionOccurs()
        {
            var positions = new List<Position> { null };
            var calculator = new ResultCalculator();

            Action act = () => calculator.CalculateResults(positions);

            act.Should().Throw<ResultException>()
                .WithMessage("Error on calculating result");
        }

        [Fact]
        public void CalculateResults_ShouldCalculateDrawdownsCorrectly()
        {
            var positions = new List<Position>
            {
                new Position { Profit = 100, DateClose = DateTime.Now.AddDays(-3) },
                new Position { Profit = 200, DateClose = DateTime.Now.AddDays(-2) },
                new Position { Profit = 150, DateClose = DateTime.Now.AddDays(-1) },
                new Position { Profit = 50, DateClose = DateTime.Now }
            };

            var calculator = new ResultCalculator();

            var result = calculator.CalculateResults(positions);

            result.DrawndownMax.Should().Be(0); 
            result.Drawndown.Should().Be(0);
        }

        [Fact]
        public void CalculateResults_ShouldReturnCorrectResult_WhenAllProfitsArePositive()
        {
            var positions = new List<Position>
            {
                new Position { Profit = 100, DateClose = DateTime.Now.AddDays(-3) },
                new Position { Profit = 50, DateClose = DateTime.Now.AddDays(-2) },
                new Position { Profit = 200, DateClose = DateTime.Now.AddDays(-1) }
            };

            var calculator = new ResultCalculator();

            var result = calculator.CalculateResults(positions);

            result.GainMax.Should().Be(200);
            result.ProfitPositif.Should().Be(350);
            result.TotalPositionPositive.Should().Be(3);
            result.MoyennePositive.Should().BeApproximately(116.67m, 0.01m);

            result.PerteMax.Should().Be(0);
            result.ProfitNegatif.Should().Be(0);
            result.TotalPositionNegative.Should().Be(0);
            result.MoyenneNegative.Should().Be(0);

            result.Profit.Should().Be(350);
            result.TotalPositions.Should().Be(3);
            result.MoyenneProfit.Should().BeApproximately(116.67m, 0.01m);

            result.RatioMoyennePositifNegatif.Should().Be(0);
            result.ProfitFactor.Should().Be(0);
            result.TauxReussite.Should().Be(100);

            result.DrawndownMax.Should().Be(0); // Aucun drawdown car tous les profits sont positifs
            result.Drawndown.Should().Be(0);
        }

        [Fact]
        public void CalculateResults_ShouldCalculateNoDrawdown_WhenAllProfitsIncreasing()
        {
            var positions = new List<Position>
            {
                new Position { Profit = 100, DateClose = DateTime.Now.AddDays(-3) },
                new Position { Profit = 150, DateClose = DateTime.Now.AddDays(-2) },
                new Position { Profit = 200, DateClose = DateTime.Now.AddDays(-1) },
                new Position { Profit = 250, DateClose = DateTime.Now }
            };

            var calculator = new ResultCalculator();

            var result = calculator.CalculateResults(positions);

            result.DrawndownMax.Should().Be(0); // Aucun drawdown car profits toujours croissants
            result.Drawndown.Should().Be(0);
        }

        [Fact]
        public void CalculateResults_ShouldHandleUnorderedPositionsList()
        {
            var positions = new List<Position>
            {
                new Position { Profit = 100, DateClose = DateTime.Now.AddDays(-1) },
                new Position { Profit = 200, DateClose = DateTime.Now.AddDays(-3) },
                new Position { Profit = -50, DateClose = DateTime.Now.AddDays(-2) }
            };

            var calculator = new ResultCalculator();

            var result = calculator.CalculateResults(positions);

            result.GainMax.Should().Be(200);
            result.ProfitPositif.Should().Be(300);
            result.TotalPositionPositive.Should().Be(2);
            result.MoyennePositive.Should().Be(150);

            result.PerteMax.Should().Be(-50);
            result.ProfitNegatif.Should().Be(-50);
            result.TotalPositionNegative.Should().Be(1);
            result.MoyenneNegative.Should().Be(-50);

            result.Profit.Should().Be(250);
            result.TotalPositions.Should().Be(3);
            result.MoyenneProfit.Should().BeApproximately(83.33m, 0.01m);

            result.RatioMoyennePositifNegatif.Should().BeApproximately(-3, 0.01m);
            result.ProfitFactor.Should().BeApproximately(6, 0.01m);
            result.TauxReussite.Should().BeApproximately(66.67, 0.01);

            result.DrawndownMax.Should().Be(250); // Peak was 200, then went down to -50 (200 - (-50))
            result.Drawndown.Should().Be(100);
        }
    }
}
