using System.Globalization;
using System.Reflection;
using System.Text.Json;
using FluentAssertions;
using RobotAppLibrary.Api.Providers.Exceptions;
using RobotAppLibrary.Api.Providers.Xtb;
using RobotAppLibrary.Api.Providers.Xtb.Modeles;
using RobotAppLibrary.Modeles;

namespace RobotAppLibrary.Tests.Api.Providers.Xtb;

public class XtbAdapterTests
{
    private readonly XtbAdapter _xtbAdapter = new();

    [Fact]
    public void AdaptAllSymbolsResponse_ShouldReturnCorrectList_WhenReturnDataIsValid()
    {
        // Arrange
        var jsonResponse = @"{
            ""returnData"": [
                {
                    ""categoryName"": ""FX"",
                    ""contractSize"": 100000,
                    ""currencyPair"": true,
                    ""currency"": ""USD"",
                    ""currencyProfit"": ""USD"",
                    ""lotMax"": 100.0,
                    ""lotMin"": 0.01,
                    ""precision"": 5,
                    ""symbol"": ""EURUSD"",
                    ""tickSize"": 0.00001,
                    ""leverage"": 100.0
                }
            ]
        }";

        // Act
        var result = _xtbAdapter.AdaptAllSymbolsResponse(jsonResponse);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);

        var symbolInfo = result[0];
        symbolInfo.Category.Should().Be(Category.Forex);
        symbolInfo.ContractSize.Should().Be(100000);
        symbolInfo.Currency.Should().Be("USD");
        symbolInfo.CurrencyProfit.Should().Be("USD");
        symbolInfo.LotMin.Should().Be(0.01);
        symbolInfo.Precision.Should().Be(5);
        symbolInfo.Symbol.Should().Be("EURUSD");
        symbolInfo.TickSize.Should().Be(0.00001);
        symbolInfo.Leverage.Should().Be(100.0);
    }

    [Fact]
    public void AdaptCalendarResponse_ShouldReturnExpectedCalendarEvents()
    {
        // Arrange
        var jsonResponse = @"
        {
            ""returnData"": [
                {
                    ""time"": 1625097600000,
                    ""country"": ""USA"",
                    ""title"": ""Nonfarm Payrolls"",
                    ""current"": ""850K"",
                    ""previous"": ""583K"",
                    ""forecast"": ""700K"",
                    ""impact"": ""High"",
                    ""period"": ""June""
                },
                {
                    ""time"": 1625184000000,
                    ""country"": ""Germany"",
                    ""title"": ""Industrial Production"",
                    ""current"": ""2.5%"",
                    ""previous"": ""-0.3%"",
                    ""forecast"": ""2.0%"",
                    ""impact"": ""Medium"",
                    ""period"": ""May""
                }
            ]
        }";

        // Act
        var result = _xtbAdapter.AdaptCalendarResponse(jsonResponse);

        // Assert
        result.Should().HaveCount(2);
        result[0].Should().BeEquivalentTo(new CalendarEvent
        {
            Time = DateTimeOffset.FromUnixTimeMilliseconds(1625097600000).DateTime,
            Country = "USA",
            Title = "Nonfarm Payrolls",
            Current = "850K",
            Previous = "583K",
            Forecast = "700K",
            Impact = "High",
            Period = "June"
        });
        result[1].Should().BeEquivalentTo(new CalendarEvent
        {
            Time = DateTimeOffset.FromUnixTimeMilliseconds(1625184000000).DateTime,
            Country = "Germany",
            Title = "Industrial Production",
            Current = "2.5%",
            Previous = "-0.3%",
            Forecast = "2.0%",
            Impact = "Medium",
            Period = "May"
        });
    }


    [Fact]
    public void AdaptFullChartResponse_ShouldReturnExpectedCandles()
    {
        // Arrange
        var jsonResponse = @"
        {
            ""returnData"": {
                ""digits"": 5,
                ""rateInfos"": [
                    {
                        ""open"": 110000,
                        ""close"": 5000,
                        ""high"": 6000,
                        ""low"": 4000,
                        ""ctm"": 1625097600000,
                        ""vol"": 100
                    },
                    {
                        ""open"": 120000,
                        ""close"": 3000,
                        ""high"": 7000,
                        ""low"": 2000,
                        ""ctm"": 1625184000000,
                        ""vol"": 150
                    }
                ]
            }
        }";


        // Act
        var result = _xtbAdapter.AdaptFullChartResponse(jsonResponse);

        // Assert
        result.Should().HaveCount(2);
        result[0].Should().BeEquivalentTo(new Candle
        {
            Open = 1.10000m,
            Close = 1.15000m,
            High = 1.16000m,
            Low = 1.14000m,
            Date = DateTimeOffset.FromUnixTimeMilliseconds(1625097600000).UtcDateTime,
            Volume = 100
        });
        result[1].Should().BeEquivalentTo(new Candle
        {
            Open = 1.20000m,
            Close = 1.23000m,
            High = 1.27000m,
            Low = 1.22000m,
            Date = DateTimeOffset.FromUnixTimeMilliseconds(1625184000000).UtcDateTime,
            Volume = 150
        });
    }


    [Fact]
    public void AdaptRangeChartResponse_ShouldReturnExpectedCandles()
    {
        // Arrange
        var jsonResponse = @"
        {
            ""returnData"": {
                ""digits"": 5,
                ""rateInfos"": [
                    {
                        ""open"": 110000,
                        ""close"": 5000,
                        ""high"": 6000,
                        ""low"": 4000,
                        ""ctm"": 1625097600000,
                        ""vol"": 100
                    },
                    {
                        ""open"": 120000,
                        ""close"": 3000,
                        ""high"": 7000,
                        ""low"": 2000,
                        ""ctm"": 1625184000000,
                        ""vol"": 150
                    }
                ]
            }
        }";


        // Act
        var result = _xtbAdapter.AdaptRangeChartResponse(jsonResponse);

        // Assert
        result.Should().HaveCount(2);
        result[0].Should().BeEquivalentTo(new Candle
        {
            Open = 1.10000m,
            Close = 1.15000m,
            High = 1.16000m,
            Low = 1.14000m,
            Date = DateTimeOffset.FromUnixTimeMilliseconds(1625097600000).UtcDateTime,
            Volume = 100
        });
        result[1].Should().BeEquivalentTo(new Candle
        {
            Open = 1.20000m,
            Close = 1.23000m,
            High = 1.27000m,
            Low = 1.22000m,
            Date = DateTimeOffset.FromUnixTimeMilliseconds(1625184000000).UtcDateTime,
            Volume = 150
        });
    }

    [Fact]
    public void AdaptLogOutResponse_ShouldReturnEmptyString()
    {
        // Arrange
        var jsonResponse = "{ \"status\": \"success\" }"; // Example JSON response

        // Act
        var result = _xtbAdapter.AdaptLogOutResponse(jsonResponse);

        // Assert
        result.Should().BeEmpty();
    }


    [Fact]
    public void AdaptBalanceAccountResponse_ShouldReturnExpectedAccountBalance()
    {
        // Arrange
        var jsonResponse = @"
        {
            ""returnData"": {
                ""margin_level"": 25.5,
                ""margin_free"": 10000.0,
                ""margin"": 2000.0,
                ""equity"": 12000.0,
                ""credit"": 500.0,
                ""balance"": 11500.0
            }
        }";

        // Act
        var result = _xtbAdapter.AdaptBalanceAccountResponse(jsonResponse);

        // Assert
        result.Should().BeEquivalentTo(new AccountBalance
        {
            MarginLevel = 25.5,
            MarginFree = 10000.0,
            Margin = 2000.0,
            Equity = 12000.0,
            Credit = 500.0,
            Balance = 11500.0
        });
    }

    [Fact]
    public void AdaptNewsResponse_ShouldReturnExpectedNews()
    {
        // Arrange
        var jsonResponse = """
                           
                                   {
                                       "returnData": [
                                           {
                                               "body": "First news body",
                                               "time": 1625097600000,
                                               "title": "First news title"
                                           },
                                           {
                                               "body": "Second news body",
                                               "time": 1625184000000,
                                               "title": "Second news title"
                                           }
                                       ]
                                   }
                           """;

        // Act
        var result = _xtbAdapter.AdaptNewsResponse(jsonResponse);

        // Assert
        result.Should().HaveCount(2);
        result[0].Should().BeEquivalentTo(new News
        {
            Body = "First news body",
            Time = DateTimeOffset.FromUnixTimeMilliseconds(1625097600000).UtcDateTime,
            Title = "First news title"
        });
        result[1].Should().BeEquivalentTo(new News
        {
            Body = "Second news body",
            Time = DateTimeOffset.FromUnixTimeMilliseconds(1625184000000).UtcDateTime,
            Title = "Second news title"
        });
    }

    [Fact]
    public void AdaptCurrentUserDataResponse_ThrowsNotImplementedException()
    {
        // Arrange
        var jsonResponse = "{ /* your JSON here */ }";

        // Act & Assert
        Assert.Throws<NotImplementedException>(() => _xtbAdapter.AdaptCurrentUserDataResponse(jsonResponse));
    }

    [Fact]
    public void AdaptCurrentUserDataResponse_AlwaysThrowsNotImplementedException()
    {
        // Arrange
        var jsonResponse = "some JSON response";

        // Act & Assert
        FluentActions.Invoking(() => _xtbAdapter.AdaptCurrentUserDataResponse(jsonResponse))
            .Should().Throw<NotImplementedException>();
    }

    [Fact]
    public void AdaptPingResponse_ValidJsonResponse_ReturnsTrue()
    {
        // Arrange
        var validJsonResponse = "{ \"status\": true }";

        // Act
        var result = _xtbAdapter.AdaptPingResponse(validJsonResponse);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void AdaptSymbolResponse_ValidJsonResponse_AdaptsCorrectly()
    {
        // Suppose this is a correct JSON response, with 'returnData' property
        var validJsonResponse = @"{
        ""status"": true,
        ""returnData"": {
            ""categoryName"": ""FX"",
            ""contractSize"": 100000,
            ""currencyPair"": true,
            ""currency"": ""USD"",
            ""currencyProfit"": ""USD"",
            ""lotMax"": 100.0,
            ""lotMin"": 0.01,
            ""precision"": 5,
            ""symbol"": ""EURUSD"",
            ""tickSize"": 0.00001,
            ""leverage"": 100.0
        }
    }";

        // Act
        var result = _xtbAdapter.AdaptSymbolResponse(validJsonResponse);

        // Assert
        result.Should().NotBeNull();
        result.Category.Should().Be(Category.Forex);
        result.ContractSize.Should().Be(100000);
        result.Currency.Should().Be("USD");
        result.CurrencyProfit.Should().Be("USD");
        result.LotMin.Should().Be(0.01);
        result.Precision.Should().Be(5);
        result.Symbol.Should().Be("EURUSD");
        result.TickSize.Should().Be(0.00001);
        result.Leverage.Should().Be(100.0);
    }


    [Fact]
    public void AdaptTickResponse_ValidJsonResponse_AdaptsCorrectly()
    {
        // Suppose this is a correct JSON response, with 'returnData' property
        var validJsonResponse = @"{
        ""status"": true,
        ""returnData"": {
      
            ""quotations"": [
                {
                    ""symbol"": ""EURUSD"",
                    ""timestamp"": 1627997285590,
                    ""ask"": 1.11743,
                    ""bid"": 1.11723
                }
            ]
        }
    }";

        // Act
        var result = _xtbAdapter.AdaptTickResponse(validJsonResponse);

        // Assert
        result.Should().NotBeNull();
        result.Symbol.Should().Be("EURUSD");
        result.Ask.Should().Be((decimal?)1.11743);
        result.Bid.Should().Be((decimal?)1.11723);
        // Assuming TimeStamp is converted correctly
        result.Date.Should().Be(DateTime.Parse("2021-08-03T13:28:05.590", CultureInfo.InvariantCulture,
            DateTimeStyles.AdjustToUniversal));
    }

    [Fact]
    public void AdaptTradesHistoryResponse_ShouldReturnExpectedPositions()
    {
        // Arrange
        var xtbAdapter = new XtbAdapter();
        var positionReference = "someReference";
        var apiResponse = @"
    {
        ""status"": true,
        ""returnData"": [
            {
                ""order"": 1,
                ""order2"": 2,
                ""position"": 1,
                ""customComment"": ""someReference|id"",
                ""symbol"": ""symbol_name"",
                ""cmd"": 0,
                ""type"": 2,
                ""profit"": 1000.5,
                ""open_price"": 1.234,
                ""open_time"": 1627588800000,
                ""close_price"": 1.456,
                ""close_time"": 1627592400000,
                ""comment"": ""system"",
                ""sl"": 1.23,
                ""tp"": 1.45,
                ""volume"": 1.0
            }
        ]
    }";

        // Act
        var result = xtbAdapter.AdaptTradesHistoryResponse(apiResponse, positionReference);

        // Assert
        var expectedPosition = new Position
        {
            Order = "1|2|1",
            StrategyId = "someReference",
            Id = "id",
            Symbol = "symbol_name",
            TypePosition = TypeOperation.Buy,
            StatusPosition = StatusPosition.Close,
            Profit = 1000.5m,
            OpenPrice = 1.234m,
            DateOpen = new DateTime(2021, 7, 29, 20, 0, 0, DateTimeKind.Utc),
            ClosePrice = 1.456m,
            DateClose = new DateTime(2021, 7, 29, 21, 0, 0,
                DateTimeKind.Utc), // The date for 1627592400000 in milliseconds
            ReasonClosed = ReasonClosed.Closed,
            StopLoss = 1.23m,
            TakeProfit = 1.45m,
            Volume = 1.0
        };

        result.First().Should().BeEquivalentTo(expectedPosition);
    }


    [Fact]
    public void AdaptTradesOpenedTradesResponse_ShouldReturnExpectedPositions()
    {
        // Arrange
        var xtbAdapter = new XtbAdapter();
        var positionReference = "someReference";
        var apiResponse = @"
    {
        ""status"": true,
        ""returnData"": [
            {
                ""order"": 1,
                ""order2"": 2,
                ""position"": 1,
                ""customComment"": ""someReference|id"",
                ""symbol"": ""symbol_name"",
                ""cmd"": 0,
                ""type"": 0,
                ""profit"": 1000.5,
                ""open_price"": 1.234,
                ""open_time"": 1627588800000,
                ""close_price"": 1.456,
                ""close_time"": 1627592400000,
                ""comment"": ""system"",
                ""sl"": 1.23,
                ""tp"": 1.45,
                ""volume"": 1.0
            }
        ]
    }";

        // Act
        var result = xtbAdapter.AdaptTradesOpenedTradesResponse(apiResponse, positionReference);

        // Assert
        var expectedPosition = new Position
        {
            Order = "1|2|1",
            StrategyId = "someReference",
            Id = "id",
            Symbol = "symbol_name",
            TypePosition = TypeOperation.Buy,
            StatusPosition = StatusPosition.Open,
            Profit = 1000.5m,
            OpenPrice = 1.234m,
            DateOpen = new DateTime(2021, 7, 29, 20, 0, 0, DateTimeKind.Utc),
            ClosePrice = 1.456m,
            DateClose = new DateTime(2021, 7, 29, 21, 0, 0,
                DateTimeKind.Utc), // The date for 1627592400000 in milliseconds
            ReasonClosed = ReasonClosed.Closed,
            StopLoss = 1.23m,
            TakeProfit = 1.45m,
            Volume = 1.0
        };

        result.Should().BeEquivalentTo(expectedPosition);
    }

    [Fact]
    public void AdaptTradingHoursResponse_ParsesValidJson_ShouldReturnTradeHourRecord()
    {
        // Arrange
        var adapter = new XtbAdapter();
        var jsonResponse = "{ \"returnData\": [ { \"trading\": [ { \"day\": 1, \"fromT\": 0, \"toT\": 1 } ] } ] }";

        // Act
        var result = adapter.AdaptTradingHoursResponse(jsonResponse);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<TradeHourRecord>(result);
        Assert.Single(result.HoursRecords);
        var record = result.HoursRecords.First();

        Assert.Equal(DayOfWeek.Monday, record.Day);
        Assert.Equal(TimeSpan.Zero, record.From);
        Assert.Equal(TimeSpan.Parse("22:00:00.0010000"), record.To);
    }


    [Fact]
    public void AdaptTradingHoursResponse_ShouldReturnTradeHourRecord_GivenValidJson()
    {
        // Arrange
        var adapter = new XtbAdapter();
        var jsonResponse =
            "{ \"returnData\": [ { \"trading\": [ { \"day\": 1, \"fromT\": 63000000, \"toT\": 63000000 } ] } ] }";

        // Act
        var result = adapter.AdaptTradingHoursResponse(jsonResponse);

        // Assert
        result.Should().NotBeNull().And.BeOfType<TradeHourRecord>();
        result.HoursRecords.Should().HaveCount(1);
        var record = result.HoursRecords.First();

        record.Day.Should().Be(DayOfWeek.Monday);
        record.From.Should().Be(new TimeSpan(15, 30, 0));
        record.To.Should().Be(new TimeSpan(15, 30, 0));
    }


    [Theory]
    [InlineData("{\"returnData\": {\"order\": 123456}}", "123456|123456|123456")]
    public void AdaptOpenTradeResponse_ShouldReturnExpectedPosition(string jsonResponse, string expectedOrder)
    {
        // Arrange


        // Act
        var result = _xtbAdapter.AdaptOpenTradeResponse(jsonResponse);

        // Assert
        result.Order.Should().Be(expectedOrder);
    }

    [Theory]
    [InlineData("{\"returnData\": {\"order\": 123456}}", "123456|123456|123456")]
    public void AdaptUpdateTradeResponse_ShouldReturnExpectedPosition(string jsonResponse, string expectedOrder)
    {
        // Arrange

        // Act
        var result = _xtbAdapter.AdaptUpdateTradeResponse(jsonResponse);

        // Assert
        result.Order.Should().Be(expectedOrder);
    }

    [Theory]
    [InlineData("{\"returnData\": {\"order\": 123456}}", "123456|123456|123456")]
    public void AdaptCloseTradeResponse_ShouldReturnExpectedPosition(string jsonResponse, string expectedOrder)
    {
        // Arrange


        // Act
        var result = _xtbAdapter.AdaptCloseTradeResponse(jsonResponse);

        // Assert
        result.Order.Should().Be(expectedOrder);
    }


    [Fact]
    public void AdaptTradeRecordStreaming_ShouldReturnExpectedPosition_Open()
    {
        // Arrange
        var jsonResponse = @"
        {
            ""data"": {
                ""order"": 123456,
                ""order2"": 654321,
                ""position"": 789012,
                ""symbol"": ""EURUSD"",
                ""cmd"": 0,
                ""type"": 0,
                ""closed"": false,
                ""profit"": 150.25,
                ""open_price"": 1.23456,
                ""open_time"": 1625097600000,
                ""close_price"": 1.23470,
                ""close_time"": 1625101200000,
                ""comment"": ""[S/L]"",
                ""sl"": 1.23000,
                ""tp"": 1.24000,
                ""volume"": 1.0,
                ""customComment"": ""strategy1|trade123""
            }
        }";

        // Act
        var result = _xtbAdapter.AdaptTradeRecordStreaming(jsonResponse);

        // Assert
        result.Should().BeEquivalentTo(new Position
        {
            Order = "123456|654321|789012",
            Symbol = "EURUSD",
            TypePosition = TypeOperation.Buy,
            StatusPosition = StatusPosition.Open,
            Profit = 150.25m,
            OpenPrice = 1.23456m,
            DateOpen = DateTimeOffset.FromUnixTimeMilliseconds(1625097600000).UtcDateTime,
            ClosePrice = 1.23470m,
            DateClose = DateTimeOffset.FromUnixTimeMilliseconds(1625101200000).UtcDateTime,
            ReasonClosed = ReasonClosed.Sl,
            StopLoss = 1.23000m,
            TakeProfit = 1.24000m,
            Volume = 1.0,
            StrategyId = "strategy1",
            Id = "trade123"
        });
    }

    [Fact]
    public void AdaptTradeRecordStreaming_ShouldReturnExpectedPosition_Pending()
    {
        // Arrange
        var jsonResponse = @"
        {
            ""data"": {
                ""order"": 123456,
                ""order2"": 654321,
                ""position"": 789012,
                ""symbol"": ""EURUSD"",
                ""cmd"": 0,
                ""type"": 1,
                ""closed"": false,
                ""profit"": 150.25,
                ""open_price"": 1.23456,
                ""open_time"": 1625097600000,
                ""close_price"": 1.23470,
                ""close_time"": 1625101200000,
                ""comment"": ""[S/L]"",
                ""sl"": 1.23000,
                ""tp"": 1.24000,
                ""volume"": 1.0,
                ""customComment"": ""strategy1|trade123""
            }
        }";

        // Act
        var result = _xtbAdapter.AdaptTradeRecordStreaming(jsonResponse);

        // Assert
        result.Should().BeEquivalentTo(new Position
        {
            Order = "123456|654321|789012",
            Symbol = "EURUSD",
            TypePosition = TypeOperation.Buy,
            StatusPosition = StatusPosition.Pending,
            Profit = 150.25m,
            OpenPrice = 1.23456m,
            DateOpen = DateTimeOffset.FromUnixTimeMilliseconds(1625097600000).UtcDateTime,
            ClosePrice = 1.23470m,
            DateClose = DateTimeOffset.FromUnixTimeMilliseconds(1625101200000).UtcDateTime,
            ReasonClosed = ReasonClosed.Sl,
            StopLoss = 1.23000m,
            TakeProfit = 1.24000m,
            Volume = 1.0,
            StrategyId = "strategy1",
            Id = "trade123"
        });
    }

    [Fact]
    public void AdaptTradeRecordStreaming_ShouldReturnExpectedPosition_Closed()
    {
        // Arrange
        var jsonResponse = @"
        {
            ""data"": {
                ""order"": 123456,
                ""order2"": 654321,
                ""position"": 789012,
                ""symbol"": ""EURUSD"",
                ""cmd"": 0,
                ""type"": 0,
                ""closed"": true,
                ""profit"": 150.25,
                ""open_price"": 1.23456,
                ""open_time"": 1625097600000,
                ""close_price"": 1.23470,
                ""close_time"": 1625101200000,
                ""comment"": ""[S/L]"",
                ""sl"": 1.23000,
                ""tp"": 1.24000,
                ""volume"": 1.0,
                ""customComment"": ""strategy1|trade123""
            }
        }";

        // Act
        var result = _xtbAdapter.AdaptTradeRecordStreaming(jsonResponse);

        // Assert
        result.Should().BeEquivalentTo(new Position
        {
            Order = "123456|654321|789012",
            Symbol = "EURUSD",
            TypePosition = TypeOperation.Buy,
            StatusPosition = StatusPosition.Close,
            Profit = 150.25m,
            OpenPrice = 1.23456m,
            DateOpen = DateTimeOffset.FromUnixTimeMilliseconds(1625097600000).UtcDateTime,
            ClosePrice = 1.23470m,
            DateClose = DateTimeOffset.FromUnixTimeMilliseconds(1625101200000).UtcDateTime,
            ReasonClosed = ReasonClosed.Sl,
            StopLoss = 1.23000m,
            TakeProfit = 1.24000m,
            Volume = 1.0,
            StrategyId = "strategy1",
            Id = "trade123"
        });
    }


    [Fact]
    public void AdaptTickRecordStreaming_ShouldReturnExpectedTick()
    {
        // Arrange
        var jsonResponse = @"
        {
            ""data"": {
                ""ask"": 1.23456,
                ""bid"": 1.23450,
                ""symbol"": ""EURUSD"",
                ""askVolume"": 1000,
                ""bidVolume"": 1000,
                ""timestamp"": 1625097600000
            }
        }";

        // Act
        var result = _xtbAdapter.AdaptTickRecordStreaming(jsonResponse);

        // Assert
        result.Should().BeEquivalentTo(new Tick
        {
            Ask = 1.23456m,
            Bid = 1.23450m,
            Symbol = "EURUSD",
            AskVolume = 1000m,
            BidVolume = 1000m,
            Date = DateTimeOffset.FromUnixTimeMilliseconds(1625097600000).UtcDateTime
        });
    }


    [Fact]
    public void AdaptBalanceRecordStreaming_ShouldReturnExpectedAccountBalance()
    {
        // Arrange
        var jsonResponse = @"
        {
            ""data"": {
                ""marginLevel"": 25.5,
                ""marginFree"": 10000.0,
                ""margin"": 2000.0,
                ""equity"": 12000.0,
                ""credit"": 500.0,
                ""balance"": 11500.0
            }
        }";

        // Act
        var result = _xtbAdapter.AdaptBalanceRecordStreaming(jsonResponse);

        // Assert
        result.Should().BeEquivalentTo(new AccountBalance
        {
            MarginLevel = 25.5,
            MarginFree = 10000.0,
            Margin = 2000.0,
            Equity = 12000.0,
            Credit = 500.0,
            Balance = 11500.0
        });
    }

    [Theory]
    [InlineData("{\"data\": {\"order\": 123456, \"requestStatus\": 0}}", "123456|123456|123456", StatusPosition.Close)]
    [InlineData("{\"data\": {\"order\": 123456, \"requestStatus\": 1}}", "123456|123456|123456",
        StatusPosition.Pending)]
    [InlineData("{\"data\": {\"order\": 123456, \"requestStatus\": 3}}", "123456|123456|123456",
        StatusPosition.Accepted)]
    [InlineData("{\"data\": {\"order\": 123456, \"requestStatus\": 4}}", "123456|123456|123456",
        StatusPosition.Rejected)]
    public void AdaptTradeStatusRecordStreaming_ShouldReturnExpectedPosition(string jsonResponse, string expectedOrder,
        StatusPosition expectedStatusPosition)
    {
        // Arrange


        // Act
        var result = _xtbAdapter.AdaptTradeStatusRecordStreaming(jsonResponse);

        // Assert
        result.Order.Should().Be(expectedOrder);
        result.StatusPosition.Should().Be(expectedStatusPosition);
    }

    [Fact]
    public void AdaptProfitRecordStreaming_ShouldReturnExpectedPosition()
    {
        // Arrange
        var jsonResponse = @"
        {
            ""data"": {
                ""order"": 123456,
                ""order2"": 654321,
                ""position"": 789012,
                ""profit"": 150.25
            }
        }";

        // Act
        var result = _xtbAdapter.AdaptProfitRecordStreaming(jsonResponse);

        // Assert
        result.Should().BeEquivalentTo(new Position
        {
            Order = "123456|654321|789012",
            Profit = 150.25m,
            StatusPosition = StatusPosition.Updated,
        });
    }


    [Fact]
    public void AdaptNewsRecordStreaming_ShouldReturnExpectedNews()
    {
        // Arrange
        var jsonResponse = @"
        {
            ""data"": {
                ""body"": ""Breaking news body"",
                ""time"": 1625097600000,
                ""title"": ""Breaking news title""
            }
        }";

        // Act
        var result = _xtbAdapter.AdaptNewsRecordStreaming(jsonResponse);

        // Assert
        result.Should().BeEquivalentTo(new News
        {
            Body = "Breaking news body",
            Time = DateTimeOffset.FromUnixTimeMilliseconds(1625097600000).UtcDateTime,
            Title = "Breaking news title"
        });
    }

    [Fact]
    public void AdaptCandleRecordStreaming_ShouldThrowNotImplementedException()
    {
        // Arrange
        var jsonResponse = @"
        {
            ""data"": {
                ""open"": 1.23456,
                ""close"": 1.23470,
                ""high"": 1.23500,
                ""low"": 1.23400,
                ""timestamp"": 1625097600000,
                ""volume"": 1000
            }
        }";

        // Act
        Action act = () => _xtbAdapter.AdaptCandleRecordStreaming(jsonResponse);

        // Assert
        act.Should().Throw<NotImplementedException>();
    }

    [Fact]
    public void AdaptLoginResponse_ShouldReturnExpectedLoginResponse()
    {
        // Arrange
        var jsonResponse = @"
        {
            ""streamSessionId"": ""session123""
        }";


        // Act
        var result = _xtbAdapter.AdaptLoginResponse(jsonResponse);

        // Assert
        result.Should().BeEquivalentTo(new LoginResponseXtb
        {
            StreamingSessionId = "session123"
        });
    }

    [Fact]
    public void AdaptLoginResponse_ShouldThrowExceptionWhenStreamSessionIdIsMissing()
    {
        // Arrange
        var jsonResponse = @"
        {
            ""streamSessionId"": null
        }";

        // Act
        Action act = () => _xtbAdapter.AdaptLoginResponse(jsonResponse);

        // Assert
        act.Should().Throw<ApiProvidersException>().WithMessage("Can't get the stream session id");
    }

    [Fact]
    public void CheckApiStatus_ShouldBehaveAsExpected()
    {
        // Arrange
        var jsonResponse = @"
        {
            ""status"": true
        }";

        var methodInfo = typeof(XtbAdapter).GetMethod("CheckApiStatus", BindingFlags.NonPublic | BindingFlags.Instance);

        using var doc = JsonDocument.Parse(jsonResponse);

        // Act
        Action act = () => methodInfo.Invoke(_xtbAdapter, new object[] { doc });

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void CheckApiStatus_ShouldThrowExceptionWhenStatusIsNotOk()
    {
        // Arrange
        var jsonResponse = @"
        {
            ""status"": false
        }";

        var methodInfo = typeof(XtbAdapter).GetMethod("CheckApiStatus", BindingFlags.NonPublic | BindingFlags.Instance);

        using var doc = JsonDocument.Parse(jsonResponse);

        // Act
        Action act = () => methodInfo.Invoke(_xtbAdapter, new object[] { doc });

        // Assert
        act.Should().Throw<TargetInvocationException>().WithInnerException<ApiProvidersException>();
    }
}