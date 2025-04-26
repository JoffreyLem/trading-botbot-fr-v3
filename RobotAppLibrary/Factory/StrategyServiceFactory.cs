using RobotAppLibrary.Api.Providers.Base;
using RobotAppLibrary.Chart;
using RobotAppLibrary.LLM;
using RobotAppLibrary.Modeles;
using RobotAppLibrary.TradingManager;
using RobotAppLibrary.TradingManager.Interfaces;
using Serilog;

namespace RobotAppLibrary.Factory;

public interface IStrategyServiceFactory
{
    IStrategyResult GetStrategyResultService(IApiProviderBase apiHandler, string positionRefenrece, ILogger logger);
    ILotValueCalculator GetLotValueCalculator(IApiProviderBase apiHandler, ILogger logger, string symbol);

    IPositionHandler GetPositionHandler(ILogger logger, IApiProviderBase handler, string symbol,
        string positionReferene);

    IChart GetChart(ILogger logger, IApiProviderBase apiHandler, string symbol, Timeframe timeframe);

    ILLMManager GetLLMManager();
}

public class StrategyServiceFactory(IServiceProvider serviceProvider) : IStrategyServiceFactory
{
    public IStrategyResult GetStrategyResultService(IApiProviderBase apiHandler, string positionRefenrece, ILogger logger)
    {
        return new StrategyResult(apiHandler, positionRefenrece, logger);
    }

    public ILotValueCalculator GetLotValueCalculator(IApiProviderBase apiHandler, ILogger logger, string symbol)
    {
        return new LotValueCalculator(apiHandler, logger, symbol);
    }

    public IPositionHandler GetPositionHandler(ILogger logger, IApiProviderBase handler, string symbol,
        string positionReferene)
    {
        return new PositionHandler(logger, handler, symbol, positionReferene,
            new LotValueCalculator(handler, logger, symbol));
    }

    public IChart GetChart(ILogger logger, IApiProviderBase apiHandler, string symbol, Timeframe timeframe)
    {
        return new Chart.Chart(apiHandler, logger, timeframe, symbol);
    }

    public ILLMManager GetLLMManager()
    {
        return new LLMManager(serviceProvider);
    }
}