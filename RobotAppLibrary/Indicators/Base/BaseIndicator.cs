using System.Buffers;
using System.Collections;
using RobotAppLibrary.Modeles;
using Skender.Stock.Indicators;

namespace RobotAppLibrary.Indicators.Base;


public interface IIndicator : IList
{
    public string Name { get; set; }
    public Tick LastTick { get; set; }
    public void UpdateIndicator(List<Candle> candles);
}

public abstract class BaseIndicator<T>(int initialCapacity = 2100)
    : List<T>(initialCapacity), IIndicator, IDisposable
    where T : ResultBase
{
    public virtual int LoopBackPeriod { get; set; } = 1;

    private bool _disposed = false;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
     
                Clear();
                TrimExcess();
            }
     
            _disposed = true;
        }
    }

    public string Name { get; set; }
    public Tick LastTick { get; set; } = new();

    public void UpdateIndicator(List<Candle> candles)
    {
     
        var dataEnumerable = Update(candles);
        var data = dataEnumerable as List<T> ?? dataEnumerable.ToList();
        
        int newCount = data.Count;
        
        if (newCount <= Capacity)
        {
      
            for (int i = 0; i < newCount; i++)
            {
                if (i < Count)
                {
                    this[i] = data[i];
                }
                else
                {
                    Add(data[i]);
                }
            }
     
            if (Count > newCount)
            {
                RemoveRange(newCount, Count - newCount);
            }
        }
        else
        {
            Clear();
            AddRange(data);
        }
    }

    public T Last()
    {
        if (Count == 0)
            throw new InvalidOperationException("The list is empty.");
        return this[Count - 1];
    }

    public T? LastOrDefault()
    {
        return Count == 0 ? default(T) : this[Count - 1];
    }

    protected abstract IEnumerable<T> Update(List<Candle> data);

    ~BaseIndicator()
    {
        Dispose(false);
    }
}

