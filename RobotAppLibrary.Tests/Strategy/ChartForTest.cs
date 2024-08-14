using System.Collections;
using RobotAppLibrary.Chart;
using RobotAppLibrary.Modeles;
using RobotAppLibrary.Utils;
using Skender.Stock.Indicators;

namespace RobotAppLibrary.Tests.Strategy;

public abstract class ChartForTest : IChart
{
    private readonly List<Candle> _candles = new List<Candle>();
    private Tick _lastPrice;

    public IEnumerator<Candle> GetEnumerator()
    {
        return _candles.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(Candle item)
    {
        _candles.Add(item);
    }

    public void Clear()
    {
        _candles.Clear();
    }

    public bool Contains(Candle item)
    {
        return _candles.Contains(item);
    }

    public void CopyTo(Candle[] array, int arrayIndex)
    {
        _candles.CopyTo(array, arrayIndex);
    }

    public bool Remove(Candle item)
    {
        return _candles.Remove(item);
    }

    public int Count => _candles.Count;
    public bool IsReadOnly => false;

    public int IndexOf(Candle item)
    {
        return _candles.IndexOf(item);
    }

    public void Insert(int index, Candle item)
    {
        _candles.Insert(index, item);
    }

    public void RemoveAt(int index)
    {
        _candles.RemoveAt(index);
    }

    public Candle this[int index]
    {
        get => _candles[index];
        set => _candles[index] = value;
    }

    public virtual Tick LastPrice
    {
        get => _lastPrice;
        set
        {
            _lastPrice = value;
            OnTickEvent?.Invoke(_lastPrice);
        }
    }

    public virtual event Func<Tick, Task>? OnTickEvent;
    public virtual event Func<Candle, Task>? OnCandleEvent;

    public Candle LastCandle => _candles[^2];
    public Candle CurrentCandle => _candles.Last();

    public virtual IEnumerable<Candle> AggregateChart(Timeframe timeframeData)
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

    protected internal virtual void OnOnCandleEvent(Candle candle)
    {
        OnCandleEvent?.Invoke(candle);
    }

    public void AddCandle(Candle candle)
    {
        Add(candle);
        OnOnCandleEvent(candle);
    }

    public void UpdateLastCandle(Tick tick)
    {
        var last = _candles.Last();
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

        OnTickEvent?.Invoke(tick);
    }
}