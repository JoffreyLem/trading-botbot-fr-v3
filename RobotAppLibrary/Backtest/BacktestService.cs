using System.Collections;
using System.Reflection;
using RobotAppLibrary.Api.Providers.Base;
using RobotAppLibrary.Chart;
using RobotAppLibrary.Indicators.Base;
using RobotAppLibrary.Modeles;
using RobotAppLibrary.Modeles.Attribute;
using RobotAppLibrary.Strategy;
using Serilog;
using Skender.Stock.Indicators;

namespace RobotAppLibrary.Backtest;

public class BacktestService(IApiProviderBase apiProvider, StrategyImplementationBase strategyImplementationBase)
{
    private Func<decimal, TypeOperation, decimal> CalculateStopLossFunc = null!;
    private Func<decimal, TypeOperation, decimal> CalculateTakeProfitFunc = null!;
    
    private StrategyImplementationBase _strategyImplementationBase { get; set; } = strategyImplementationBase;

    private readonly ILogger _logger;
    
    private List<IIndicator> _mainIndicatorList = new();

    private Dictionary<Timeframe, List<IIndicator>> _secondaryIndicatorList = new();
    
    private ChartForBacktest MainChart { get; set; }
    
    private Dictionary<Timeframe, ChartForBacktest> _secondaryChartList = new Dictionary<Timeframe, ChartForBacktest>();

    private PositionHandlerBacktest _positionHandler;

    private string Symbol;
    
    public void RunBacktest()
    {

        var tempChartMain = SaveAndClearList(MainChart);
        var tempChartList = SaveAndClearDictionary(_secondaryChartList);
        var tempMainListIndicator = SaveAndClearList(_mainIndicatorList);
        var tempSecondaryChartList = SaveAndClearDictionary(_secondaryIndicatorList);


        foreach (var candle in tempChartMain)
        {
            MainChart.Add(candle);
            HandleSecondaryChartList(tempChartList, candle);
            HandleMainIndicatorList(tempMainListIndicator);
            HandleSecondaryIndicatorList(tempSecondaryChartList);
            HandleBacktestProcess();
        }
    }

    private List<T> SaveAndClearList<T>(List<T> list)
    {
        var tempList = new List<T>(list);
        list.Clear();
        return tempList;
    }

    private Dictionary<TKey, TValue> SaveAndClearDictionary<TKey, TValue>(Dictionary<TKey, TValue> dictionary) where TValue : IList where TKey : notnull
    {
        var tempDict = new Dictionary<TKey, TValue>(dictionary);
        foreach (var value in dictionary.Values)
        {
            value.Clear();
        }
        return tempDict;
    }

    private void HandleBacktestProcess()
    {
       
    }

    private void HandleSecondaryIndicatorList(Dictionary<Timeframe, List<IIndicator>> indicatorsSecondaryTemp)
    {
        foreach (var (key, value) in _secondaryIndicatorList)
        {
            var selectedKeyTemp = indicatorsSecondaryTemp[key];
            for (var i = 0; i < value.Count; i++)
            {
                var currentIndicator = value[i];
                var selectedTempIndicator = selectedKeyTemp.First(x => x.Name == currentIndicator.Name);
                currentIndicator.Add(selectedTempIndicator[0]);
                selectedTempIndicator.RemoveAt(0);
            }
        }
    }

    private void HandleMainIndicatorList(List<IIndicator> indicatorsTemp)
    {
        for (var index = 0; index < _mainIndicatorList.Count; index++)
        {
            var indicator = _mainIndicatorList[index];
            var selectedTemp = indicatorsTemp.First(x => x.Name == indicator.Name);
            indicator.Add(selectedTemp[0]);
            selectedTemp.RemoveAt(0);
        }
    }

    private void HandleSecondaryChartList(Dictionary<Timeframe, ChartForBacktest> tempChartList, Candle candle)
    {
        foreach (var (key, value) in tempChartList)
        {
            while (value.Count > 0)
            {
                var biggerCandle = value[0];
                if (candle.Date >= biggerCandle.Date)
                {
                    _secondaryChartList[key].Add(biggerCandle);
                    value.RemoveAt(0);
                }
                else
                {
                    break;
                }
            }
        }
    }

 
    private void InitStrategyImplementation()
    {
        _strategyImplementationBase.Logger = _logger;
        _strategyImplementationBase.CalculateStopLossFunc = CalculateStopLossFunc;
        _strategyImplementationBase.CalculateTakeProfitFunc = CalculateTakeProfitFunc;
        _strategyImplementationBase.OpenPositionAction = OpenPosition;
        _positionHandler.DefaultSl = _strategyImplementationBase.DefaultSl;
        _positionHandler.DefaultTp = _strategyImplementationBase.DefaultTp;
    }
    
    private Task OpenPosition(TypeOperation typePosition, decimal sl = 0,
        decimal tp = 0, double volume = 0, double risk = 0, long expiration = 0)
    {
        return _positionHandler
            .OpenPositionAsync(Symbol, typePosition, volume, sl, tp, risk, expiration);
    }

    private async Task Init()
    {
        InitStrategyImplementation();
       await  InitChart();
       InitIndicator();
        
    }

    private async Task InitChart()
    {
        var chartFields = _strategyImplementationBase.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
            .Where(f => typeof(IChart).IsAssignableFrom(f.FieldType)).ToList();
        
        foreach (var fieldInfo in chartFields)
        {
            var timeframeAttribute = fieldInfo.GetCustomAttribute<TimeframeAttribute>();

            var chart =(ChartForBacktest)(await apiProvider.GetChartAsync(Symbol, timeframeAttribute!.Timeframe)) ;
        
            if (fieldInfo.GetCustomAttribute<MainChartAttribute>() != null)
            {
                MainChart = chart;
            } 
            else
            {
                _secondaryChartList.Add(timeframeAttribute.Timeframe, chart);
            }
        }
    }

    
    private void InitIndicator()
    {
        var indicators = _strategyImplementationBase.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance)
            .Where(f => typeof(IIndicator).IsAssignableFrom(f.FieldType)).ToList();

        foreach (var fieldInfo in indicators)
        {
            if (fieldInfo.GetValue(_strategyImplementationBase) is not IIndicator indicator)
            {
                indicator = Activator.CreateInstance(fieldInfo.FieldType) as IIndicator ?? throw new InvalidOperationException();
                fieldInfo.SetValue(_strategyImplementationBase, indicator);
            }
            
            if (fieldInfo.GetCustomAttribute<TimeframeAttribute>() is { Timeframe: var timeframe })
            {
                if (!_secondaryIndicatorList.TryGetValue(timeframe, out var value))
                {
                    value = new List<IIndicator>();
                    _secondaryIndicatorList.Add(timeframe, value);
                }

                value.Add(indicator);
            }
            else
            {
                _mainIndicatorList.Add(indicator);
            }
        }

        UpdateIndicator();
    }
    
    private void UpdateIndicator()
    {
        try
        {
            void UpdateIndicators(List<IIndicator> indicators, List<Candle> candles)
            {
                Parallel.ForEach(indicators, indicator =>
                {
                    indicator.UpdateIndicator(candles);
                });
            }

            var mainCandles = GetLastCandlesAsList(MainChart, 1000);
            UpdateIndicators(_mainIndicatorList, mainCandles);

            foreach (var keyValuePair in _secondaryIndicatorList)
            {
                if (_secondaryChartList.TryGetValue(keyValuePair.Key, out var secondaryChart))
                {
                    var secondaryCandles = GetLastCandlesAsList(secondaryChart, 1000);
                    UpdateIndicators(keyValuePair.Value, secondaryCandles);
                }
                else
                {
                    var aggregatedCandles =((IChartAggregate) MainChart).AggregateChart(keyValuePair.Key).ToList();
                    UpdateIndicators(keyValuePair.Value, aggregatedCandles);
                }
            }
        }
        catch (Exception e)
        {
            throw new StrategyException("Impossible de mettre à jour les indicateurs", e);
        }
    }
    
    private List<Candle> GetLastCandlesAsList(ChartForBacktest candles, int count)
    {
        int candleCount = candles.Count;
        if (candleCount <= count)
        {
            return [..candles];
        }
        
        var result = new List<Candle>(count);
        for (int i = candleCount - count; i < candleCount; i++)
        {
            result.Add(candles[i]);
        }

        return result;
    }


    public void StartBacktest()
    {
        
    }
    
    
}