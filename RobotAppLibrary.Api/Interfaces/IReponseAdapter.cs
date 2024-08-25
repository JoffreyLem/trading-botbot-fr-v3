using System.Text.Json;
using RobotAppLibrary.Api.Modeles;
using RobotAppLibrary.Modeles;

namespace RobotAppLibrary.Api.Interfaces;

public interface IReponseAdapter
{
    List<SymbolInfo> AdaptAllSymbolsResponse(JsonDocument? jsonResponse);
    List<CalendarEvent> AdaptCalendarResponse(JsonDocument? jsonResponse);
    List<Candle> AdaptFullChartResponse(JsonDocument? jsonResponse);
    List<Candle> AdaptRangeChartResponse(JsonDocument? jsonResponse);
    string AdaptLogOutResponse(JsonDocument jsonResponse);
    public LoginResponse AdaptLoginResponse(JsonDocument? jsonResponse);
    AccountBalance AdaptBalanceAccountResponse(JsonDocument? jsonResponse);
    List<News> AdaptNewsResponse(JsonDocument? jsonResponse);
    string AdaptCurrentUserDataResponse(JsonDocument? jsonResponse);
    bool AdaptPingResponse(JsonDocument? jsonResponse);
    SymbolInfo AdaptSymbolResponse(JsonDocument? jsonResponse);
    Tick AdaptTickResponse(JsonDocument? jsonResponse);
    List<Position>? AdaptTradesHistoryResponse(JsonDocument? jsonResponse, string tradeCom);
    Position? AdaptTradesOpenedTradesResponse(JsonDocument? jsonResponse, string tradeCom);
    TradeHourRecord AdaptTradingHoursResponse(JsonDocument? jsonResponse);
    Position? AdaptOpenTradeResponse(JsonDocument? jsonResponse);
    Position? AdaptUpdateTradeResponse(JsonDocument? jsonResponse);
    Position? AdaptCloseTradeResponse(JsonDocument? jsonResponse);

    Tick AdaptTickRecordStreaming(JsonDocument input);

    Position AdaptTradeRecordStreaming(JsonDocument input);

    AccountBalance AdaptBalanceRecordStreaming(JsonDocument input);

    Position AdaptTradeStatusRecordStreaming(JsonDocument input);

    Position AdaptProfitRecordStreaming(JsonDocument input);

    News AdaptNewsRecordStreaming(JsonDocument input);

    Candle AdaptCandleRecordStreaming(JsonDocument input);
}