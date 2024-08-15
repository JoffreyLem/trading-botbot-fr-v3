using FluentAssertions;
using Moq;
using RobotAppLibrary.Api.Providers.Base;
using RobotAppLibrary.Modeles;
using RobotAppLibrary.TradingManager;
using Serilog;

namespace RobotAppLibrary.Tests.MoneyManagement;

public class LotValueCalculatorTest
{
    private readonly Mock<IApiProviderBase> apiHandlerMock = new();
    private readonly Mock<ILogger> loggerMock = new();


    [Fact]
    public void Test_Init()
    {
        // Arrange 
        apiHandlerMock.Setup(x => x.GetSymbolInformationAsync(It.IsAny<string>()))
            .ReturnsAsync(new SymbolInfo
            {
                Symbol = "EURUSD",
                Currency = "EUR",
                CurrencyProfit = "USD",
                TickSize = 1
            });

        apiHandlerMock.Setup(x => x.GetTickPriceAsync(It.IsAny<string>()))
            .ReturnsAsync(new Tick
            {
                Bid = 1.0730m
            });

        // Act
        var lotCalculator = new LotValueCalculator(apiHandlerMock.Object, loggerMock.Object, "EURUSD");

        // Assert
        apiHandlerMock.Verify(x => x.GetTickPriceAsync(It.IsAny<string>()), Times.Once);
        apiHandlerMock.Verify(x => x.GetSymbolInformationAsync(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void Test_Init_No_BaseSymbolAccount()
    {
        // Arrange 
        apiHandlerMock.Setup(x => x.GetSymbolInformationAsync(It.IsAny<string>()))
            .ReturnsAsync(new SymbolInfo
            {
                Symbol = "AUDUSD",
                Currency = "AUD",
                CurrencyProfit = "USD",
                TickSize = 1
            });

        apiHandlerMock.Setup(x => x.GetTickPriceAsync(It.IsAny<string>()))
            .ReturnsAsync(new Tick
            {
                Bid = 1.0730m
            });

        apiHandlerMock.Setup(x => x.GetAllSymbolsAsync())
            .ReturnsAsync(new List<SymbolInfo>
            {
                new()
                {
                    Symbol = "EURUSD",
                    Currency = "EUR",
                    CurrencyProfit = "USD",
                    TickSize = 1
                }
            });

        // Act
        _ = new LotValueCalculator(apiHandlerMock.Object, loggerMock.Object, "AUDUSD");

        // Assert
        apiHandlerMock.Verify(x => x.GetTickPriceAsync("AUDUSD"), Times.Once);
        apiHandlerMock.Verify(x => x.GetTickPriceAsync("EURUSD"), Times.Once);
        apiHandlerMock.Verify(x => x.GetSymbolInformationAsync("AUDUSD"), Times.Once);
        apiHandlerMock.Verify(x => x.SubscribePrice("EURUSD"));
    }

    [Fact]
    public void Test_LotCalculator_Forex_EUR_USD()
    {
        // Arrange 
        apiHandlerMock.Setup(x => x.GetSymbolInformationAsync(It.IsAny<string>()))
            .ReturnsAsync(new SymbolInfo
            {
                Symbol = "EURUSD",
                Currency = "EUR",
                CurrencyProfit = "USD",
                Leverage = 5,
                TickSize = 0.0001
            });

        apiHandlerMock.Setup(x => x.GetTickPriceAsync(It.IsAny<string>()))
            .ReturnsAsync(new Tick
            {
                Bid = 1.0730m
            });

        // Act
        var lotCalculator = new LotValueCalculator(apiHandlerMock.Object, loggerMock.Object, "EURUSD");

        // Assert
        lotCalculator.PipValueStandard.Should().BeApproximately(9.31, 0.02);
        lotCalculator.MarginPerLot.Should().BeApproximately(5000, 0.01);
    }


    [Fact]
    public void Test_LotCalculator_Forex_EUR_USD_NO_LEVERAGE()
    {
        // Arrange 
        apiHandlerMock.Setup(x => x.GetSymbolInformationAsync(It.IsAny<string>()))
            .ReturnsAsync(new SymbolInfo
            {
                Symbol = "EURUSD",
                Currency = "EUR",
                CurrencyProfit = "USD",
                Leverage = 0,
                TickSize = 0.0001
            });

        apiHandlerMock.Setup(x => x.GetTickPriceAsync(It.IsAny<string>()))
            .ReturnsAsync(new Tick
            {
                Bid = 1.0730m
            });

        // Act
        var lotCalculator = new LotValueCalculator(apiHandlerMock.Object, loggerMock.Object, "EURUSD");

        // Assert
        lotCalculator.MarginPerLot.Should().BeApproximately(931966.44, 0.1);
    }


    [Fact]
    public void Test_LotCalculator_Forex_EUR_USD_tick_event()
    {
        // Arrange 
        apiHandlerMock.Setup(x => x.GetSymbolInformationAsync(It.IsAny<string>()))
            .ReturnsAsync(new SymbolInfo
            {
                Symbol = "EURUSD",
                Currency = "EUR",
                CurrencyProfit = "USD",
                TickSize = 0.0001
            });

        apiHandlerMock.Setup(x => x.GetTickPriceAsync(It.IsAny<string>()))
            .ReturnsAsync(new Tick
            {
                Bid = 1.0730m
            });

        // Act
        var lotCalculator = new LotValueCalculator(apiHandlerMock.Object, loggerMock.Object, "EURUSD");

        var newTick = new Tick
        {
            Symbol = "EURUSD",
            Bid = 1.0830m
        };

        apiHandlerMock.Raise(x => x.TickEvent += null, this, newTick);

        // Assert
        lotCalculator.PipValueStandard.Should().BeApproximately(9.23, 0.02);
    }

    [Fact]
    public void Test_LotCalculator_Indices_DE30()
    {
        // Arrange 
        apiHandlerMock.Setup(x => x.GetSymbolInformationAsync(It.IsAny<string>()))
            .ReturnsAsync(new SymbolInfo
            {
                Symbol = "DE30",
                Currency = "EUR",
                CurrencyProfit = "EUR",
                ContractSize = 25,
                Category = Category.Indices,
                Leverage = 5,
                TickSize = 1
            });

        apiHandlerMock.Setup(x => x.GetTickPriceAsync(It.IsAny<string>()))
            .ReturnsAsync(new Tick
            {
                Bid = 15262.2m
            });

        // Act
        var lotCalculator = new LotValueCalculator(apiHandlerMock.Object, loggerMock.Object, "DE30");

        // Assert
        lotCalculator.PipValueStandard.Should().BeApproximately(25, 0.02);
        lotCalculator.MarginPerLot.Should().BeApproximately(19077.75, 0.1);
    }

    [Fact]
    public void Test_LotCalculator_Indices_Not_Eur_Base()
    {
        // Arrange 
        apiHandlerMock.Setup(x => x.GetSymbolInformationAsync(It.IsAny<string>()))
            .ReturnsAsync(new SymbolInfo
            {
                Symbol = "US500",
                Currency = "USD",
                CurrencyProfit = "USD",
                ContractSize = 50,
                Category = Category.Indices,
                TickSize = 1
            });

        apiHandlerMock.Setup(x => x.GetAllSymbolsAsync())
            .ReturnsAsync(new List<SymbolInfo>
            {
                new()
                {
                    Symbol = "EURUSD",
                    Currency = "EUR",
                    CurrencyProfit = "USD",
                    TickSize = 1
                }
            });

        apiHandlerMock.SetupSequence(x => x.GetTickPriceAsync(It.IsAny<string>()))
            .ReturnsAsync(new Tick
            {
                Bid = 4378.1m
            })
            .ReturnsAsync(new Tick
            {
                Bid = 1.0730m
            });

        // Act
        var lotCalculator = new LotValueCalculator(apiHandlerMock.Object, loggerMock.Object, "US500");

        // Assert
        lotCalculator.PipValueStandard.Should().BeApproximately(46.59, 0.02);
    }

    [Fact]
    public void Test_LotCalculator_Indices_Not_Eur_Base_New_Tick()
    {
        // Arrange 
        apiHandlerMock.Setup(x => x.GetSymbolInformationAsync(It.IsAny<string>()))
            .ReturnsAsync(new SymbolInfo
            {
                Symbol = "US500",
                Currency = "USD",
                CurrencyProfit = "USD",
                ContractSize = 50,
                Category = Category.Indices,
                TickSize = 1
            });

        apiHandlerMock.Setup(x => x.GetAllSymbolsAsync())
            .ReturnsAsync(new List<SymbolInfo>
            {
                new()
                {
                    Symbol = "EURUSD",
                    Currency = "EUR",
                    CurrencyProfit = "USD",
                    TickSize = 1
                }
            });

        apiHandlerMock.SetupSequence(x => x.GetTickPriceAsync(It.IsAny<string>()))
            .ReturnsAsync(new Tick
            {
                Bid = 4378.1m
            })
            .ReturnsAsync(new Tick
            {
                Bid = 1.0730m
            });

        // Act
        var lotCalculator = new LotValueCalculator(apiHandlerMock.Object, loggerMock.Object, "US500");
        var newTick = new Tick
        {
            Symbol = "EURUSD",
            Bid = 1.0530m
        };
        apiHandlerMock.Raise(x => x.TickEvent += null, this, newTick);


        // Assert
        lotCalculator.PipValueStandard.Should().BeApproximately(47.48, 0.02);
    }
}