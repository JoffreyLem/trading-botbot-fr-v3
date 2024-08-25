using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RobotAppLibrary.Modeles;
using Skender.Stock.Indicators;

namespace RobotAppLibrary.Indicators.Base
{
    public interface IIndicator
    {
        string Name { get; set; }
        Tick LastTick { get; set; }
        void UpdateIndicator(List<Candle> candles);
    }

    public abstract class BaseIndicator<T>(int initialCapacity = 2100) : IIndicator, IList<T>, IDisposable
        where T : ResultBase
    {
        private bool _disposed;
        private T[] _items = ArrayPool.Rent(initialCapacity); 
        private int _count = 0; 
        private static readonly ArrayPool<T> ArrayPool = ArrayPool<T>.Shared;

        public virtual int LoopBackPeriod { get; set; } = 1;
        public string Name { get; set; }
        public Tick LastTick { get; set; } = new();

        public void UpdateIndicator(List<Candle> candles)
        {
            var dataEnumerable = Update(candles);
            var data = dataEnumerable as T[] ?? dataEnumerable.ToArray();

            Clear(); // Nettoie le tableau existant
            EnsureCapacity(data.Length); // Assure que la capacité du tableau est suffisante
            Array.Copy(data, _items, data.Length); // Copie les nouveaux éléments dans le tableau
            _count = data.Length; // Met à jour le nombre d'éléments
        }

        protected abstract IEnumerable<T> Update(List<Candle> data);

        public T Last()
        {
            if (_count == 0)
                throw new InvalidOperationException("The list is empty.");
            return _items[_count - 1];
        }

        public T? LastOrDefault()
        {
            return _count == 0 ? default : _items[_count - 1];
        }

        public void Clear()
        {
            Array.Clear(_items, 0, _count);
            _count = 0;
        }

        private void EnsureCapacity(int min)
        {
            if (_items.Length < min)
            {
                T[] newItems = ArrayPool.Rent(min);
                Array.Copy(_items, newItems, _count);
                ArrayPool.Return(_items);
                _items = newItems;
            }
        }

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
                    ArrayPool.Return(_items);
                }
                _disposed = true;
            }
        }

        ~BaseIndicator()
        {
            Dispose(false);
        }

        #region IList<T> Members Implementation

        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= _count)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return _items[index];
            }
            set
            {
                if (index < 0 || index >= _count)
                    throw new ArgumentOutOfRangeException(nameof(index));
                _items[index] = value;
            }
        }

        public int Count => _count;
        public bool IsReadOnly => false;

        public void Add(T item)
        {
            EnsureCapacity(_count + 1);
            _items[_count++] = item;
        }

        public bool Contains(T item)
        {
            return Array.IndexOf(_items, item, 0, _count) != -1;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Array.Copy(_items, 0, array, arrayIndex, _count);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _items.Take(_count).GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return Array.IndexOf(_items, item, 0, _count);
        }

        public void Insert(int index, T item)
        {
            if (index < 0 || index > _count)
                throw new ArgumentOutOfRangeException(nameof(index));

            EnsureCapacity(_count + 1);

            if (index < _count)
            {
                Array.Copy(_items, index, _items, index + 1, _count - index);
            }

            _items[index] = item;
            _count++;
        }

        public bool Remove(T item)
        {
            int index = IndexOf(item);
            if (index < 0) return false;
            RemoveAt(index);
            return true;
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= _count)
                throw new ArgumentOutOfRangeException(nameof(index));

            _count--;
            if (index < _count)
            {
                Array.Copy(_items, index + 1, _items, index, _count - index);
            }
            _items[_count] = default!;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
