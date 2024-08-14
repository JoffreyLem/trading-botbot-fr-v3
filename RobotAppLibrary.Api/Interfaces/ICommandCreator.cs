using RobotAppLibrary.Api.Modeles;
using RobotAppLibrary.Modeles;

namespace RobotAppLibrary.Api.Interfaces;

public interface ICommandCreator
{
    string CreateLoginCommand(Credentials credentials);
    string CreateAllSymbolsCommand();
    string CreateCalendarCommand();
    string CreateFullChartCommand(Timeframe timeframe, DateTime start, string symbol);
    string CreateRangeChartCommand(Timeframe timeframe, DateTime start, DateTime end, string symbol);
    string CreateLogOutCommand();
    string CreateBalanceAccountCommand();
    string CreateNewsCommand(DateTime? start, DateTime? end);
    string CreateCurrentUserDataCommand();
    string CreatePingCommand();
    string CreateSymbolCommand(string symbol);
    string CreateTickCommand(string symbol);
    string CreateTradesHistoryCommand();
    string CreateTradesOpenedTradesCommand();
    string CreateTradingHoursCommand(string symbol);
    string CreateOpenTradeCommande(Position position, decimal price);
    string CreateUpdateTradeCommande(Position position, decimal price);
    string CreateCloseTradeCommande(Position position, decimal price);
    string CreateSubscribeBalanceCommandStreaming();
    string CreateStopBalanceCommandStreaming();
    string CreateSubscribeCandleCommandStreaming(string symbol);
    string CreateStopCandleCommandStreaming(string symbol);
    string CreateSubscribeKeepAliveCommandStreaming();
    string CreateStopKeepAliveCommandStreaming();
    string CreateSubscribeNewsCommandStreaming();
    string CreateStopNewsCommandStreaming();
    string CreateSubscribeProfitsCommandStreaming();
    string CreateStopProfitsCommandStreaming();
    string CreateTickPricesCommandStreaming(string symbol);
    string CreateStopTickPriceCommandStreaming(string symbol);
    string CreateTradesCommandStreaming();
    string CreateStopTradesCommandStreaming();
    string CreateTradeStatusCommandStreaming();
    string CreateStopTradeStatusCommandStreaming();
    string CreatePingCommandStreaming();
    string CreateStopPingCommandStreaming();
}