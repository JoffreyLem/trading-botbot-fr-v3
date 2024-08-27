using RobotAppLibrary.Api.Modeles;
using RobotAppLibrary.Api.Providers.Base;
using RobotAppLibrary.Modeles;
using RobotAppLibrary.Utils;
using Serilog;
using Skender.Stock.Indicators;

namespace RobotAppLibrary.Chart;

public interface IChartBase : IList<Candle>;

public interface IChartAggregate : IChartBase
{
    public IEnumerable<Candle> AggregateChart(Timeframe timeframeData)
    {
        return this.Aggregate(timeframeData.ToPeriodSize()).Select(x => new Candle
        {
            Open = x.Open,
            High = x.High,
            Low = x.Low,
            Close = x.Close,
            Date = x.Date
        });
    }
}

public interface IChart : IChartAggregate
{
    Tick LastPrice { get; }

    public Candle LastCandle => this[^2];
    public Candle CurrentCandle => this.Last();
    event Func<Tick, Task>? OnTickEvent;
    event Func<Candle, Task>? OnCandleEvent;
}

public class Chart : List<Candle>, IChart
{
    private readonly IApiProviderBase _apiHandler;
    private readonly ILogger _logger;
    private readonly string _symbol;
    private readonly Timeframe _timeframe;
    private TradeHourRecord _tradeHourRecord = new();


    public Chart(IApiProviderBase apiHandler, ILogger logger, Timeframe timeframe, string symbol) : base(2100)
    {
        _apiHandler = apiHandler;
        _timeframe = timeframe;
        _symbol = symbol;
        _logger = logger.ForContext<Chart>();
        Init().ConfigureAwait(false).GetAwaiter().GetResult();
    }

    public bool IsReadOnly => false;

    public Tick LastPrice { get; private set; }

    public event Func<Tick, Task>? OnTickEvent;
    public event Func<Candle, Task>? OnCandleEvent;

    public Candle LastCandle => this[Count - 2];
    public Candle CurrentCandle => this[Count - 1];


    private async Task Init()
    {
        try
        {
            _apiHandler.TickEvent += ApiHandlerOnTickEvent;
            var data = await _apiHandler.GetChartAsync(new ChartRequest()
            {
                Symbol = this._symbol,
                Timeframe = this._timeframe
            });
            LastPrice = await _apiHandler.GetTickPriceAsync(_symbol);
            if (data is { Count: > 0 })
            {
                foreach (var candle in data.TakeLast(2000)) Add(candle);
                this.Validate();
            }

            _tradeHourRecord = await _apiHandler.GetTradingHoursAsync(_symbol);
        }
        catch (Exception e)
        {
            throw new ChartException("Can't initialize candle list", e);
        }
    }

    private async void ApiHandlerOnTickEvent(object? sender, Tick tick)
    {
        if (tick.Symbol == _symbol)
        {
            LastPrice = tick;
            var candleStartTimeTick = GetReferenceTime(tick.Date);

            if (Count == 0 || this[Count - 1].Date != candleStartTimeTick)
                AddNewCandle(candleStartTimeTick, tick);
            else
                UpdateLast(tick);
        }
    }

    private void OnOnTickEvent(Tick obj)
    {
        OnTickEvent?.Invoke(obj);
    }

    private void OnOnCandleEvent(Candle obj)
    {
        OnCandleEvent?.Invoke(obj);
    }

    private void AddNewCandle(DateTime dateTime, Tick tick)
    {
        var price = tick.Bid.GetValueOrDefault();
        var candle = new Candle
        {
            Date = dateTime,
            Open = price,
            High = price,
            Low = price,
            Close = price,
            Volume = tick.AskVolume.GetValueOrDefault() + tick.BidVolume.GetValueOrDefault(),
            AskVolume = tick.AskVolume.GetValueOrDefault(),
            BidVolume = tick.BidVolume.GetValueOrDefault()
        };
       candle.Ticks.Add(tick);
        Add(candle);
        OnOnCandleEvent(this.Last());
        if (Count >= 2000) RemoveAt(0);
    }

    private void UpdateLast(Tick tick)
    {
        var last = CurrentCandle;
        last.Ticks.Add(tick);
        last.Close = tick.Bid.GetValueOrDefault();
        last.AskVolume += tick.AskVolume.GetValueOrDefault();
        last.BidVolume += tick.BidVolume.GetValueOrDefault();
        last.Volume += tick.AskVolume.GetValueOrDefault() + tick.BidVolume.GetValueOrDefault();
        if (last.Open == 0) last.Open = tick.Bid.GetValueOrDefault();
        if (last.High == 0) last.High = tick.Bid.GetValueOrDefault();
        if (last.Low == 0) last.Low = tick.Bid.GetValueOrDefault();
        if (last.Close >= last.High)
            last.High = last.Close;
        else if (last.Close <= last.Low)
            last.Low = last.Close;

        OnOnTickEvent(tick);
    }

    private DateTime GetReferenceTime(DateTime utcTime)
    {
        var timeframeValue = _timeframe.GetMinuteFromTimeframe();
        
        if (_timeframe < Timeframe.Daily) return GetMinuteReference(utcTime, timeframeValue);
        return _timeframe switch
        {
            Timeframe.Daily => GetDailyReference(utcTime),
            Timeframe.Weekly => GetWeeklyReference(utcTime),
            Timeframe.Monthly => GetMonthlyReference(utcTime),
            _ => throw new ArgumentException("Unsupported timeframe.")
        };
    }

    private DateTime GetMinuteReference(DateTime utcTime, int timeframeMinutes)
    {
        var totalMinutes = utcTime.Hour * 60 + utcTime.Minute;
        var referenceMinute = totalMinutes / timeframeMinutes * timeframeMinutes;
        return new DateTime(utcTime.Year, utcTime.Month, utcTime.Day, referenceMinute / 60, referenceMinute % 60, 0);
    }

    private DateTime GetDailyReference(DateTime utcTime)
    {
        return new DateTime(utcTime.Year, utcTime.Month, utcTime.Day, 0, 0, 0);
    }

    private DateTime GetWeeklyReference(DateTime utcTime)
    {
        var daysSinceMonday = ((int)utcTime.DayOfWeek + 6) % 7;
        var monday = utcTime.Date.AddDays(-daysSinceMonday);
        return new DateTime(monday.Year, monday.Month, monday.Day, 0, 0, 0);
    }

    private DateTime GetMonthlyReference(DateTime utcTime)
    {
        return new DateTime(utcTime.Year, utcTime.Month, 1, 0, 0, 0);
    }
}