using RobotAppLibrary.Api.Modeles;
using RobotAppLibrary.Modeles;

namespace RobotAppLibrary.Api.Interfaces;

public interface IReponseAdapter
{
    List<SymbolInfo> AdaptAllSymbolsResponse(string jsonResponse);
    List<CalendarEvent> AdaptCalendarResponse(string jsonResponse);
    List<Candle> AdaptFullChartResponse(string jsonResponse);
    List<Candle> AdaptRangeChartResponse(string jsonResponse);
    string AdaptLogOutResponse(string jsonResponse);
    public LoginResponse AdaptLoginResponse(string jsonResponse);
    AccountBalance AdaptBalanceAccountResponse(string jsonResponse);
    List<News> AdaptNewsResponse(string jsonResponse);
    string AdaptCurrentUserDataResponse(string jsonResponse);
    bool AdaptPingResponse(string jsonResponse);
    SymbolInfo AdaptSymbolResponse(string jsonResponse);
    Tick AdaptTickResponse(string jsonResponse);
    List<Position>? AdaptTradesHistoryResponse(string jsonResponse, string tradeCom);
    Position? AdaptTradesOpenedTradesResponse(string jsonResponse, string tradeCom);
    TradeHourRecord AdaptTradingHoursResponse(string jsonResponse);
    Position AdaptOpenTradeResponse(string jsonResponse);
    Position AdaptUpdateTradeResponse(string jsonResponse);
    Position AdaptCloseTradeResponse(string jsonResponse);

    Tick AdaptTickRecordStreaming(string input);

    Position AdaptTradeRecordStreaming(string input);

    AccountBalance AdaptBalanceRecordStreaming(string input);

    Position AdaptTradeStatusRecordStreaming(string input);

    Position AdaptProfitRecordStreaming(string input);

    News AdaptNewsRecordStreaming(string input);

    Candle AdaptCandleRecordStreaming(string input);
}