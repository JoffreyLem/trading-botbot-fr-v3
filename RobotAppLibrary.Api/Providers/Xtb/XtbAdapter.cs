using System.Text.Json;
using RobotAppLibrary.Api.Interfaces;
using RobotAppLibrary.Api.Modeles;
using RobotAppLibrary.Api.Providers.Exceptions;
using RobotAppLibrary.Api.Providers.Xtb.Assembler;
using RobotAppLibrary.Api.Providers.Xtb.Code;
using RobotAppLibrary.Api.Providers.Xtb.Modeles;
using RobotAppLibrary.Modeles;
using RobotAppLibrary.Utils;

namespace RobotAppLibrary.Api.Providers.Xtb;

public class XtbAdapter : IReponseAdapter
{
    public List<SymbolInfo> AdaptAllSymbolsResponse(string jsonResponse)
    {
        using var doc = JsonDocument.Parse(jsonResponse);

        CheckApiStatus(doc);
        var returnData = ReturnData(doc);

        var symbolRecords = new List<SymbolInfo>();
        
        var symbolElements = returnData.EnumerateArray();

        symbolRecords.AddRange(symbolElements.Select(symbolElement => new SymbolInfo
        {
            Category = FromXtbToRobotAssembler.GetCategory(symbolElement.GetProperty("categoryName").GetString()),
            ContractSize = symbolElement.GetProperty("contractSize").GetInt64(),
            Currency = symbolElement.GetProperty("currency").GetString(),
            CurrencyProfit = symbolElement.GetProperty("currencyProfit").GetString(),
            LotMin = symbolElement.GetProperty("lotMin").GetDouble(),
            Precision = symbolElement.GetProperty("precision").GetInt32(),
            Symbol = symbolElement.GetProperty("symbol").GetString(),
            TickSize = symbolElement.GetProperty("tickSize").GetDouble(),
            Leverage = symbolElement.GetProperty("leverage").GetDouble()
        }));

        return symbolRecords;
    }



    public List<CalendarEvent> AdaptCalendarResponse(string jsonResponse)
    {
        using var doc = JsonDocument.Parse(jsonResponse);

        CheckApiStatus(doc);
        var returnData = ReturnData(doc);

        var calendarList = new List<CalendarEvent>();

        if (returnData is { ValueKind: JsonValueKind.Array })
            calendarList.AddRange(returnData.EnumerateArray()
                .Select(calendarElement => new CalendarEvent()
                {
                    Time = DateTimeOffset.FromUnixTimeMilliseconds(calendarElement.GetProperty("time").GetInt64())
                        .DateTime,
                    Country = calendarElement.GetProperty("country").GetString(),
                    Title = calendarElement.GetProperty("title").GetString(),
                    Current = calendarElement.GetProperty("current").GetString(),
                    Previous = calendarElement.GetProperty("previous").GetString(),
                    Forecast = calendarElement.GetProperty("forecast").GetString(),
                    Impact = calendarElement.GetProperty("impact").GetString(),
                    Period = calendarElement.GetProperty("period").GetString()
                }));

        return calendarList;
    }

    public List<Candle> AdaptFullChartResponse(string jsonResponse)
    {
        using var doc = JsonDocument.Parse(jsonResponse);

        CheckApiStatus(doc);
        var returnData = ReturnData(doc);

        var dataRecordsList = new List<Candle>();
        
        var digits = returnData.GetProperty("digits").GetInt32();
        dataRecordsList.AddRange(returnData.GetProperty("rateInfos").EnumerateArray().Select(recordElement => MapCandle(recordElement, digits)));
        
        dataRecordsList.Sort((c1, c2) => c1.Date.CompareTo(c2.Date));

        return dataRecordsList;
    }

    public List<Candle> AdaptRangeChartResponse(string jsonResponse)
    {
        return AdaptFullChartResponse(jsonResponse);
    }


    // TODO : voir pour peut être changer ? 
    public string AdaptLogOutResponse(string jsonResponse)
    {
        return "";
    }

    public AccountBalance AdaptBalanceAccountResponse(string jsonResponse)
    {
        using var doc = JsonDocument.Parse(jsonResponse);

        CheckApiStatus(doc);
        var returnData = ReturnData(doc);

        return MapAccountBalance(returnData);
    }


    public List<News> AdaptNewsResponse(string jsonResponse)
    {
        using var doc = JsonDocument.Parse(jsonResponse);

        CheckApiStatus(doc);
        var returnData = ReturnData(doc, JsonValueKind.Array);
        var data = new List<News>();

        data.AddRange(returnData.EnumerateArray().Select(MapNews));
        
        return data;
    }


    public string AdaptCurrentUserDataResponse(string jsonResponse)
    {
        throw new NotImplementedException();
    }

    public bool AdaptPingResponse(string jsonResponse)
    {
        using var doc = JsonDocument.Parse(jsonResponse);

        CheckApiStatus(doc);
        return true;
    }

    public SymbolInfo AdaptSymbolResponse(string jsonResponse)
    {
        using var doc = JsonDocument.Parse(jsonResponse);

        CheckApiStatus(doc);
        var returnData = ReturnData(doc);

        var symbolRecord = new SymbolInfo
        {
            Symbol = returnData.GetProperty("symbol").GetString(),
            Category = FromXtbToRobotAssembler.GetCategory(returnData.GetProperty("categoryName").GetString()),
            ContractSize = returnData.GetProperty("contractSize").GetInt64(),
            Currency = returnData.GetProperty("currency").GetString(),
            CurrencyProfit = returnData.GetProperty("currencyProfit").GetString(),
            LotMin = returnData.GetProperty("lotMin").GetDouble(),
            Precision = returnData.GetProperty("precision").GetInt32(),
            TickSize = returnData.GetProperty("tickSize").GetDouble(),
            Leverage = returnData.GetProperty("leverage").GetDouble()
        };

        return symbolRecord;
    }

    public Tick AdaptTickResponse(string jsonResponse)
    {
        using var doc = JsonDocument.Parse(jsonResponse);

        CheckApiStatus(doc);
        var returnData = ReturnData(doc);

        var quotation = returnData.GetProperty("quotations").EnumerateArray().First();
        return MapTick(quotation);
     
    }

    public List<Position> AdaptTradesHistoryResponse(string jsonResponse, string positionReference)
    {
        using var doc = JsonDocument.Parse(jsonResponse);

        CheckApiStatus(doc);
        var returnData = ReturnData(doc);

        var listPosition = new List<Position>();


        listPosition.AddRange(from recordElement in returnData.EnumerateArray() where recordElement.GetProperty("customComment").GetString().Contains(positionReference) select MapPosition(recordElement));

        return listPosition;
    }

    public Position? AdaptTradesOpenedTradesResponse(string jsonResponse, string positionId)
    {
        using var doc = JsonDocument.Parse(jsonResponse);

        CheckApiStatus(doc);
        var returnData = ReturnData(doc);

        var listPosition = (from recordElement in returnData.EnumerateArray() where recordElement.GetProperty("customComment").GetString().Contains(positionId) select MapPosition(recordElement)).ToList();

        return listPosition.FirstOrDefault();
    }

    public TradeHourRecord AdaptTradingHoursResponse(string jsonResponse)
    {
        using var doc = JsonDocument.Parse(jsonResponse);

        CheckApiStatus(doc);
        var tradeHourRecord = new TradeHourRecord();
        var returnData = ReturnData(doc);
        
        var firstData = returnData.EnumerateArray().First();
        
        foreach (var tradingElement in firstData.GetProperty("trading").EnumerateArray())
            tradeHourRecord.HoursRecords.Add(new TradeHourRecord.HoursRecordData
            {
                Day = (DayOfWeek)tradingElement.GetProperty("day").GetInt32(),
                From = ParseFromDatTradeHour(tradingElement
                    .GetProperty("fromT").GetInt64()),
                To = ParseToDatTradeHour(tradingElement.GetProperty("toT")
                    .GetInt64())
            });


        return tradeHourRecord;
    }


    public Position AdaptOpenTradeResponse(string jsonResponse)
    {
        using var doc = JsonDocument.Parse(jsonResponse);
        CheckApiStatus(doc);
        
        var returnData = ReturnData(doc);
        
        return MapPositionTrasaction(returnData);
    }

    public Position AdaptUpdateTradeResponse(string jsonResponse)
    {
        using var doc = JsonDocument.Parse(jsonResponse);

        CheckApiStatus(doc);
        var returnData = ReturnData(doc);

        return MapPositionTrasaction(returnData);
    }

    public Position AdaptCloseTradeResponse(string jsonResponse)
    {
        using var doc = JsonDocument.Parse(jsonResponse);

        CheckApiStatus(doc);
        var returnData = ReturnData(doc);

        return MapPositionTrasaction(returnData);
    }

    public Tick AdaptTickRecordStreaming(string input)
    {
        using var doc = JsonDocument.Parse(input);

        var data = ReturnDataStreaming(doc);

        return MapTick(data);
    }

    public Position? AdaptTradeRecordStreaming(string input)
    {
        using var doc = JsonDocument.Parse(input);


        var data = ReturnDataStreaming(doc);

        var recordElement = data;
        var customComment = recordElement.GetProperty("customComment").GetString();

        if (!string.IsNullOrEmpty(customComment))
        {
            var position = new Position();

            var order = recordElement.GetProperty("order").GetInt64();
            var order2 = recordElement.GetProperty("order2").GetInt64();
            var positionId = recordElement.GetProperty("position").GetInt64();


            position.Order = $"{order}|{order2}|{positionId}";

            position.Symbol = recordElement.GetProperty("symbol").GetString();
            position.TypePosition =
                FromXtbToRobotAssembler.GetTypeOperation(recordElement.GetProperty("cmd").GetInt64());

            var type = recordElement.GetProperty("type").GetInt32();

            if (recordElement.GetProperty("closed").GetBoolean())
            {
                position.StatusPosition = StatusPosition.Close;
            }
            else
            {
                if (type == 1)
                    position.StatusPosition = StatusPosition.Pending;
                else if (type == 0) position.StatusPosition = StatusPosition.Open;
            }

            position.Profit = recordElement.TryGetProperty("profit", out var profit) &&
                              profit.ValueKind != JsonValueKind.Null
                ? profit.GetDecimal()
                : 0;
            position.OpenPrice = recordElement.GetProperty("open_price").GetDecimal();
            position.DateOpen =
                TimeZoneConverter.ConvertMillisecondsToUtc(recordElement.GetProperty("open_time").GetInt64());
            position.ClosePrice = recordElement.GetProperty("close_price").GetDecimal();
            position.DateClose = recordElement.TryGetProperty("close_time", out var closeDate) &&
                                 closeDate.ValueKind != JsonValueKind.Null
                ? TimeZoneConverter.ConvertMillisecondsToUtc(closeDate.GetInt64())
                : new DateTime();

            position.ReasonClosed =
                FromXtbToRobotAssembler.ComputeCommentReasonClosed(recordElement.GetProperty("comment").GetString());
            position.StopLoss = recordElement.GetProperty("sl").GetDecimal();
            position.TakeProfit = recordElement.GetProperty("tp").GetDecimal();
            position.Volume = recordElement.GetProperty("volume").GetDouble();


            var dataSplit = customComment.Split("|");

            position.StrategyId = dataSplit[0];
            position.Id = dataSplit[1];
          
            return position;
        }

        return null;
    }

    public AccountBalance AdaptBalanceRecordStreaming(string input)
    {
        using var doc = JsonDocument.Parse(input);

        var data = ReturnDataStreaming(doc);
        return MapAccountBalanceStreaming(data);
    }

    public Position AdaptTradeStatusRecordStreaming(string input)
    {
        using var doc = JsonDocument.Parse(input);

        var data = ReturnDataStreaming(doc);
        var order = data.GetProperty("order").GetInt64();

        return new Position
        {
            Order = $"{order}|{order}|{order}",
            StatusPosition =
                FromXtbToRobotAssembler.ToTradeStatusFromTradeStatusStreaming(data
                    .GetProperty("requestStatus").GetInt64())
        };
    }

    public Position AdaptProfitRecordStreaming(string input)
    {
        using var doc = JsonDocument.Parse(input);

        var data = ReturnDataStreaming(doc);
        var order = data.GetProperty("order").GetInt64();
        var order2 = data.GetProperty("order2").GetInt64();
        var positionId = data.GetProperty("position").GetInt64();
        return new Position
        {
            Order = $"{order}|{order2}|{positionId}",
            Profit = data.GetProperty("profit").GetDecimal()
        };
    }

    public News AdaptNewsRecordStreaming(string input)
    {
        using var doc = JsonDocument.Parse(input);

        var data = ReturnDataStreaming(doc);
        return MapNews(data);
    }

    public Candle AdaptCandleRecordStreaming(string input)
    {
        using var doc = JsonDocument.Parse(input);
        throw new NotImplementedException();
    }

    public LoginResponse AdaptLoginResponse(string jsonResponse)
    {
        using var doc = JsonDocument.Parse(jsonResponse);

        CheckApiStatus(doc);

        var root = doc.RootElement;

        var streamSessionId = root.GetProperty("streamSessionId").GetString();

        return new LoginResponseXtb
        {
            StreamingSessionId = streamSessionId ?? throw new ApiProvidersException("Can't get the stream session id")
        };
    }

    private TimeSpan ParseFromDatTradeHour(long time)
    {
        if (time is 0) return TimeSpan.FromMilliseconds(0);

        return TimeZoneConverter.ConvertMidnightCetCestMillisecondsToUtcOffset(time);
    }

    private TimeSpan ParseToDatTradeHour(long time)
    {
        var dateRefLimitDay = DateTime.Now.Date.AddHours(23).AddMinutes(59).AddSeconds(59).TimeOfDay.TotalMilliseconds;
        if (time is 86400000) return TimeSpan.FromMilliseconds(dateRefLimitDay);

        return TimeZoneConverter.ConvertMidnightCetCestMillisecondsToUtcOffset(time);
    }

    private Position MapPosition(JsonElement recordElement)
    {
        var position = new Position();
        var order = recordElement.GetProperty("order").GetInt64();
        var order2 = recordElement.GetProperty("order2").GetInt64();
        var positionId = recordElement.GetProperty("position").GetInt64();

        position.Order = $"{order}|{order2}|{positionId}";

        string?[] positionComment = recordElement.GetProperty("customComment").GetString()!.Split('|');

        position.StrategyId = positionComment[0];
        position.Id = positionComment[1];

        position.Symbol = recordElement.GetProperty("symbol").GetString();
        position.TypePosition = FromXtbToRobotAssembler.GetTypeOperation(recordElement.GetProperty("cmd").GetInt64());
        position.StatusPosition =
            recordElement.TryGetProperty("type", out var type) && type.ValueKind != JsonValueKind.Null
                ? FromXtbToRobotAssembler.ToTradeStatusFromTradeStreaming(type.GetInt64())
                : StatusPosition.Close;
        position.Profit = recordElement.TryGetProperty("profit", out var profit) &&
                          profit.ValueKind != JsonValueKind.Null
            ? profit.GetDecimal()
            : 0;
        position.OpenPrice = recordElement.GetProperty("open_price").GetDecimal();
        position.DateOpen =
            TimeZoneConverter.ConvertMillisecondsToUtc(recordElement.GetProperty("open_time").GetInt64());
        position.ClosePrice = recordElement.GetProperty("close_price").GetDecimal();
        position.DateClose = recordElement.TryGetProperty("close_time", out var closeDate) &&
                             closeDate.ValueKind != JsonValueKind.Null
            ? TimeZoneConverter.ConvertMillisecondsToUtc(closeDate.GetInt64())
            : new DateTime();

        var comment = recordElement.GetProperty("comment").GetString();

        if (!string.IsNullOrEmpty(comment))
            position.ReasonClosed =
                FromXtbToRobotAssembler.ComputeCommentReasonClosed(comment);

        position.StopLoss = recordElement.GetProperty("sl").GetDecimal();
        position.TakeProfit = recordElement.GetProperty("tp").GetDecimal();
        position.Volume = recordElement.GetProperty("volume").GetDouble();

        return position;
    }

    private Position MapPositionTrasaction(JsonElement jsonElement)
    {
        var position = new Position();
        var order = jsonElement.GetProperty("order").GetInt64();
        var order2 = jsonElement.GetProperty("order").GetInt64();
        var positionId = jsonElement.GetProperty("order").GetInt64();

        position.Order = $"{order}|{order2}|{positionId}";
        return position;
    }

    private void CheckApiStatus(JsonDocument doc)
    {
        var root = doc.RootElement;

        if (root.TryGetProperty("status", out var statusProperty) && !statusProperty.GetBoolean())
        {
            var errorCode = "";
            var errorDescr = "";

            if (root.TryGetProperty("errorCode", out var errorCodeProperty)) errorCode = errorCodeProperty.GetString();

            if (root.TryGetProperty("errorDescr", out var errorDescrProperty))
                errorDescr = errorDescrProperty.GetString();

            if (errorDescr == null && !string.IsNullOrEmpty(errorCode))
                errorDescr = ERR_CODE.getErrorDescription(errorCode);

            throw new ApiProvidersException(errorDescr);
        }
    }

    private Tick MapTick(JsonElement jsonElement)
    {
        decimal? ask = jsonElement.TryGetProperty("ask", out var askProp) ? askProp.GetDecimal() : null;
        decimal? bid = jsonElement.TryGetProperty("bid", out var bidProp) ? bidProp.GetDecimal() : null;
        jsonElement.TryGetProperty("symbol", out var symbolProp) ;
        decimal? askVolume = jsonElement.TryGetProperty("askVolume", out var askVolumeProp)
            ? askVolumeProp.GetDecimal()
            : null;
        decimal? bidVolume = jsonElement.TryGetProperty("bidVolume", out var bidVolumeProp)
            ? bidVolumeProp.GetDecimal()
            : null;
        var date = jsonElement.TryGetProperty("timestamp", out var timestampProp)
            ? TimeZoneConverter.ConvertMillisecondsToUtc(timestampProp.GetInt64())
            : new DateTime();

        var tick = new Tick
        {
            Ask = ask,
            Bid = bid,
            Symbol = symbolProp.ToString(),
            AskVolume = askVolume,
            BidVolume = bidVolume,
            Date = date
        };

        return tick;
    }


    private static JsonElement ReturnData(JsonDocument doc, JsonValueKind? expectedValueKind = null)
    {
        
        var root = doc.RootElement;

        if (root.ValueKind == JsonValueKind.Undefined )
        {
            throw new ApiProvidersException("Json value kind unknow");
        }

        if (root.TryGetProperty("returnData", out var returnDataProperty))
        {
            if ((expectedValueKind is not null && returnDataProperty.ValueKind != expectedValueKind))
            {
                throw new ApiProvidersException("Return data value kind unknow");
            }
            return returnDataProperty;
        }

        throw new ApiProvidersException("The json does not contain return data");

    }


    private JsonElement ReturnDataStreaming(JsonDocument doc)
    {
        var root = doc.RootElement;

        if (root.TryGetProperty("data", out var returnDataProperty)) return returnDataProperty;

        throw new ApiProvidersException("The streaming json does not contain data");
    }


    private AccountBalance MapAccountBalance(JsonElement element)
    {
        var accountBalance = new AccountBalance
        {
            MarginLevel = element.GetProperty("margin_level").GetDouble(),
            MarginFree = element.GetProperty("margin_free").GetDouble(),
            Margin = element.GetProperty("margin").GetDouble(),
            Equity = element.GetProperty("equity").GetDouble(),
            Credit = element.GetProperty("credit").GetDouble(),
            Balance = element.GetProperty("balance").GetDouble()
        };


        return accountBalance;
    }

    private AccountBalance MapAccountBalanceStreaming(JsonElement element)
    {
        var accountBalance = new AccountBalance
        {
            MarginLevel = element.GetProperty("marginLevel").GetDouble(),
            MarginFree = element.GetProperty("marginFree").GetDouble(),
            Margin = element.GetProperty("margin").GetDouble(),
            Equity = element.GetProperty("equity").GetDouble(),
            Credit = element.GetProperty("credit").GetDouble(),
            Balance = element.GetProperty("balance").GetDouble()
        };


        return accountBalance;
    }

    private News MapNews(JsonElement jsonElement)
    {
        return new News
        {
            Body = jsonElement.GetProperty("body").ToString(),
            Time = TimeZoneConverter.ConvertMillisecondsToUtc(jsonElement.GetProperty("time").GetInt64()),
            Title = jsonElement.GetProperty("title").ToString()
        };
    }


    private Candle MapCandle(JsonElement element, int decimals)
    {
        var open = element.GetProperty("open").GetDecimal() / (decimal)Math.Pow(10, decimals);
        var close = element.GetProperty("close").GetDecimal();
        var high = element.GetProperty("high").GetDecimal();
        var low = element.GetProperty("low").GetDecimal();
        close = open + close / (decimal)Math.Pow(10, decimals);
        high = open + high / (decimal)Math.Pow(10, decimals);
        low = open + low / (decimal)Math.Pow(10, decimals);

        return new Candle
        {
            Close = close,
            Date = TimeZoneConverter.ConvertMillisecondsToUtc(element.GetProperty("ctm").GetInt64()),
            High = high,
            Low = low,
            Open = open,
            Volume = element.GetProperty("vol").GetDecimal()
        };
    }
}