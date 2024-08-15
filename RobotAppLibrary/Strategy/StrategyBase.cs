using System.Reflection;
using System.Runtime.CompilerServices;
using RobotAppLibrary.Api.Providers.Base;
using RobotAppLibrary.Chart;
using RobotAppLibrary.Factory;
using RobotAppLibrary.Indicators.Base;
using RobotAppLibrary.Modeles;
using RobotAppLibrary.Modeles.Attribute;
using RobotAppLibrary.TradingManager;
using RobotAppLibrary.TradingManager.Interfaces;
using Serilog;

[assembly: InternalsVisibleTo("RobotAppLibrary.Tests")]

namespace RobotAppLibrary.Strategy;

public interface IStrategyBase
{
    GlobalResults Results { get; }
    Position? PositionOpened { get; }
    bool CanRun { get; set; }
    IList<Candle> Chart { get; }
    Task DisableStrategy(StrategyReasonDisabled strategyReasonDisabled, Exception? ex = null);
}

public class StrategyBase : IStrategyBase
{
    private readonly IApiProviderBase _apiProvider;
    private readonly object _lockRunHandler = new();
    private readonly ILogger _logger;
    private readonly IPositionHandler _positionHandler;
    private readonly StrategyImplementationBase _strategyImplementationBase;
    private readonly IStrategyServiceFactory _strategyServiceFactory;

    internal readonly List<IIndicator> MainIndicatorList = new();

    internal readonly Dictionary<Timeframe, IChart> SecondaryChartList = new();

    internal readonly Dictionary<Timeframe, List<IIndicator>> SecondaryIndicatorList = new();
    public readonly IStrategyResult StrategyResult;

    public StrategyBase(string symbol, StrategyImplementationBase strategyImplementationBase,
        IApiProviderBase apiProviderBase, ILogger logger, IStrategyServiceFactory strategyServiceFactory)
    {
        try
        {
            Id = Guid.NewGuid().ToString();
            _strategyImplementationBase = strategyImplementationBase;
            Symbol = symbol;
            _strategyServiceFactory = strategyServiceFactory;

            _logger = logger.ForContext<StrategyBase>()
                .ForContext("StrategyName", StrategyName)
                .ForContext("StrategyId", Id);

            _apiProvider = apiProviderBase;
            _positionHandler =
                strategyServiceFactory.GetPositionHandler(logger, apiProviderBase, Symbol, StrategyId);
            StrategyResult = strategyServiceFactory.GetStrategyResultService(apiProviderBase, StrategyId);

            Init();
        }
        catch (Exception e)
        {
            logger.Error(e, "Can't initialize strategy");
            throw new StrategyException("Can't create strategy", e);
        }
    }

    public string Symbol { get; set; }
    public string Id { get; set; }
    public string StrategyName => _strategyImplementationBase.Name;
    public string Version => _strategyImplementationBase.Version ?? "NotDefined";
    public string StrategyId => $"{StrategyName}-{Version}-{Symbol}";
    public bool StrategyDisabled { get; set; }
    public IChart MainChart { get; set; } = null!;
    public bool CanRun { get; set; }
    public GlobalResults Results => StrategyResult.GlobalResults;
    public Position? PositionOpened => _positionHandler.PositionOpened;
    public IList<Candle> Chart => MainChart;


    public async Task DisableStrategy(StrategyReasonDisabled strategyReasonDisabled, Exception? ex = null)
    {
        _logger.Fatal(ex, "On disabling strategy for reason {Reason}", strategyReasonDisabled);
        CanRun = false;
        StrategyDisabled = true;

        try
        {
            if (strategyReasonDisabled is StrategyReasonDisabled.User)
            {
                _apiProvider.UnsubscribePrice(Symbol);
                var trades = _apiProvider.GetOpenedTradesAsync(StrategyId).Result;
                if (trades is not null) await _positionHandler.ClosePositionAsync(trades);
            }

            _logger.Fatal("Strategy disabled");
        }
        catch (Exception e)
        {
            _logger.Fatal(e, "Can't completly disable strategy, some action don't work");
            throw new StrategyException();
        }
        finally
        {
            var disableMessage =
                $"The strategy {StrategyId} have been disabled, cause of {strategyReasonDisabled}";
            StrategyDisabledEvent?.Invoke(this, new RobotEvent<string>(disableMessage, Id));
        }
    }

    public event EventHandler<RobotEvent<string>>? StrategyDisabledEvent;
    public event EventHandler<RobotEvent<Tick>>? TickEvent;
    public event EventHandler<RobotEvent<Candle>>? CandleEvent;
    public event EventHandler<RobotEvent<Position>>? PositionOpenedEvent;
    public event EventHandler<RobotEvent<Position>>? PositionUpdatedEvent;
    public event EventHandler<RobotEvent<Position>>? PositionRejectedEvent;
    public event EventHandler<RobotEvent<Position>>? PositionClosedEvent;

    private Task OpenPosition(TypeOperation typePosition, decimal sl = 0,
        decimal tp = 0, double volume = 0, double risk = 0, long expiration = 0)
    {
        return _positionHandler
            .OpenPositionAsync(Symbol, typePosition, volume, sl, tp, risk, expiration);
    }

    private void Init()
    {
        _apiProvider.Disconnected += (_, _) => DisableStrategy(StrategyReasonDisabled.Api).GetAwaiter().GetResult();

        StrategyResult.ResultTresholdEvent +=
            (_, _) => DisableStrategy(StrategyReasonDisabled.Treshold).GetAwaiter().GetResult();
        _positionHandler.PositionOpenedEvent += (_, position) =>
            PositionOpenedEvent?.Invoke(this, new RobotEvent<Position>(position, Id));
        _positionHandler.PositionUpdatedEvent += (_, position) =>
            PositionUpdatedEvent?.Invoke(this, new RobotEvent<Position>(position, Id));
        _positionHandler.PositionClosedEvent += (_, position) =>
            PositionClosedEvent?.Invoke(this, new RobotEvent<Position>(position, Id));
        _positionHandler.PositionRejectedEvent += (_, position) =>
            PositionRejectedEvent?.Invoke(this, new RobotEvent<Position>(position, Id));

        // Care about order call
        InitStrategyImplementation();
        InitChart();
        InitIndicator();

        _apiProvider.SubscribePrice(Symbol);
    }

    private void InitStrategyImplementation()
    {
        _strategyImplementationBase.Logger = _logger;
        _strategyImplementationBase.CalculateStopLossFunc = _positionHandler.CalculateStopLoss;
        _strategyImplementationBase.CalculateTakeProfitFunc = _positionHandler.CalculateTakeProfit;
        _strategyImplementationBase.OpenPositionAction = OpenPosition;
        _positionHandler.DefaultSl = _strategyImplementationBase.DefaultSl;
        _positionHandler.DefaultTp = _strategyImplementationBase.DefaultTp;
    }


    private void InitChart()
    {
        var chartFields = _strategyImplementationBase.GetType()
            .GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
            .Where(f => typeof(IChart).IsAssignableFrom(f.FieldType)).ToList();

        if (chartFields.All(f => f.GetCustomAttribute<MainChartAttribute>() == null))
            throw new StrategyException("No main chart defined.");

        foreach (var fieldInfo in chartFields)
        {
            var timeframeAttribute = fieldInfo.GetCustomAttribute<TimeframeAttribute>();
            if (timeframeAttribute == null)
                throw new StrategyException("One of the charts does not have a timeframe attribute.");

            var chart = _strategyServiceFactory.GetChart(_logger, _apiProvider, Symbol, timeframeAttribute.Timeframe);

            if (fieldInfo.GetCustomAttribute<MainChartAttribute>() != null)
            {
                chart.OnTickEvent += ChartOnOnTickEvent;
                chart.OnCandleEvent += ChartOnOnCandleEvent;
                MainChart = chart;
            }
            else
            {
                SecondaryChartList.Add(timeframeAttribute.Timeframe, chart);
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
                indicator = Activator.CreateInstance(fieldInfo.FieldType) as IIndicator ??
                            throw new InvalidOperationException();
                fieldInfo.SetValue(_strategyImplementationBase, indicator);
                indicator.Name = fieldInfo.Name;
            }

            if (fieldInfo.GetCustomAttribute<TimeframeAttribute>() is { Timeframe: var timeframe })
            {
                if (!SecondaryIndicatorList.TryGetValue(timeframe, out var value))
                {
                    value = new List<IIndicator>();
                    SecondaryIndicatorList.Add(timeframe, value);
                }

                value.Add(indicator);
            }
            else
            {
                MainIndicatorList.Add(indicator);
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
                Parallel.ForEach(indicators, indicator => { indicator.UpdateIndicator(candles); });
            }

            var mainCandles = GetLastCandlesAsList(MainChart, 1000);
            UpdateIndicators(MainIndicatorList, mainCandles);

            foreach (var keyValuePair in SecondaryIndicatorList)
                if (SecondaryChartList.TryGetValue(keyValuePair.Key, out var secondaryChart))
                {
                    var secondaryCandles = GetLastCandlesAsList(secondaryChart, 1000);
                    UpdateIndicators(keyValuePair.Value, secondaryCandles);
                }
                else
                {
                    var aggregatedCandles = MainChart.AggregateChart(keyValuePair.Key).ToList();
                    UpdateIndicators(keyValuePair.Value, aggregatedCandles);
                }
        }
        catch (Exception e)
        {
            CanRun = false;
            _logger.Error(e, "Impossible de mettre à jour les indicateurs");
            throw new StrategyException("Impossible de mettre à jour les indicateurs", e);
        }
    }

    private List<Candle> GetLastCandlesAsList(IChart candles, int count)
    {
        var candleCount = candles.Count;
        if (candleCount <= count) return [..candles];

        var result = new List<Candle>(count);
        for (var i = candleCount - count; i < candleCount; i++) result.Add(candles[i]);

        return result;
    }


    private async Task ChartOnOnCandleEvent(Candle arg)
    {
        try
        {
            UpdateIndicator();
            if (CanRun && !_strategyImplementationBase.RunOnTick && !_positionHandler.PositionInProgress)
            {
                lock (_lockRunHandler)
                {
                    RunHandler();
                }
            }
            else
            {
                if (_positionHandler.PositionOpened is not null)
                {
                    var currentPosition = _positionHandler.PositionOpened;

                    if (!_strategyImplementationBase.UpdateOnTick) await UpdateHandler(currentPosition);

                    if (!_strategyImplementationBase.CloseOnTick) await CloseHandler(currentPosition);
                }
            }

            CandleEvent?.Invoke(this, new RobotEvent<Candle>(arg, Id));
        }
        catch (Exception e)
        {
            CanRun = false;
            _logger.Error(e, "Erreur de traitement candle");
            await DisableStrategy(StrategyReasonDisabled.Error, e);
        }
    }

    private async Task ChartOnOnTickEvent(Tick tick)
    {
        UpdateIndicator();
        try
        {
            if (CanRun && _strategyImplementationBase.RunOnTick && !_positionHandler.PositionInProgress)
            {
                lock (_lockRunHandler)
                {
                    RunHandler();
                }
            }
            else
            {
                if (_positionHandler.PositionOpened is not null)
                {
                    var currentPosition = _positionHandler.PositionOpened;

                    if (_strategyImplementationBase.UpdateOnTick) await UpdateHandler(currentPosition);

                    if (_strategyImplementationBase.CloseOnTick) await CloseHandler(currentPosition);
                }
            }

            TickEvent?.Invoke(this, new RobotEvent<Tick>(tick, Id));
        }
        catch (Exception e)
        {
            CanRun = false;
            _logger.Error(e, "Erreur de traitement tick");
            await DisableStrategy(StrategyReasonDisabled.Error, e);
        }
    }

    private void RunHandler()
    {
        try
        {
            _logger.Verbose("Try run strategy");
            _strategyImplementationBase.Run();
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error on run");
        }
    }


    private async Task UpdateHandler(Position? position)
    {
        try
        {
            if (position != null)
            {
                var positionClone = position.Clone();
                _logger.Verbose("Try strategy update position {Id}", positionClone.Id);
                if (_strategyImplementationBase.ShouldUpdatePosition(positionClone))
                {
                    if (positionClone.StopLoss is 0) positionClone.StopLoss = position.StopLoss;

                    if (positionClone.TakeProfit is 0) positionClone.TakeProfit = position.TakeProfit;

                    _logger.Verbose("Position {Id} can be updated | New Sl {Sl} New Tp {Tp}", positionClone.Id,
                        positionClone.StopLoss, positionClone.TakeProfit);
                    await _positionHandler.UpdatePositionAsync(positionClone);
                }
                else
                {
                    _logger.Verbose("Position {Id} can't be updated", position.Id);
                }
            }
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error on update");
        }
    }

    private async Task CloseHandler(Position? position)
    {
        try
        {
            if (position != null)
            {
                _logger.Verbose("Try strategy close position {Id} ", position.Id);
                if (_strategyImplementationBase.ShouldClosePosition(position))
                {
                    _logger.Verbose("Position {Id} can be closed", position.Id);
                    await _positionHandler.ClosePositionAsync(position);
                }
                else
                {
                    _logger.Verbose("Position {Id} can't be closed", position.Id);
                }
            }
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error on close");
        }
    }
}