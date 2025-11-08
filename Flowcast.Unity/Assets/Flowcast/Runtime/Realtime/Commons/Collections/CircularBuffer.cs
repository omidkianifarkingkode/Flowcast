using System;
using System.Collections;
using System.Collections.Generic;

namespace Flowcast.Collections
{
    public class CircularBuffer<T> : IEnumerable<T>
    {
        private readonly T[] _buffer;
        private int _head;
        private int _count;

        public int Capacity { get; }
        public int Count => _count;

        public CircularBuffer(int capacity)
        {
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be positive");

            Capacity = capacity;
            _buffer = new T[capacity];
        }

        public void Add(T item)
        {
            _buffer[_head] = item;
            _head = (_head + 1) % Capacity;
            if (_count < Capacity)
                _count++;
        }

        public T GetAt(int indexFromNewest)
        {
            if (indexFromNewest < 0 || indexFromNewest >= _count)
                throw new ArgumentOutOfRangeException(nameof(indexFromNewest));

            int index = (_head - 1 - indexFromNewest + Capacity) % Capacity;
            return _buffer[index];
        }

        public bool TryGet(Func<T, bool> predicate, out T result)
        {
            for (int i = 0; i < _count; i++)
            {
                var item = GetAt(i);
                if (predicate(item))
                {
                    result = item;
                    return true;
                }
            }

            result = default!;
            return false;
        }

        public ref T RefAt(int indexFromNewest)
        {
            if (indexFromNewest < 0 || indexFromNewest >= _count)
                throw new ArgumentOutOfRangeException(nameof(indexFromNewest));

            int index = (_head - 1 - indexFromNewest + Capacity) % Capacity;
            return ref _buffer[index];
        }

        public void RemoveAfter(Predicate<T> keepIf)
        {
            int kept = 0;

            for (int i = 0; i < _count; i++)
            {
                int index = (_head - 1 - i + Capacity) % Capacity;
                if (keepIf(_buffer[index]))
                {
                    kept++;
                }
                else
                {
                    // Clear unused slot (important for GC if T is reference type)
                    _buffer[index] = default!;
                }
            }

            _count = kept;
            _head = (_head - (_count - kept) + Capacity) % Capacity;
        }

        public void TrimToLatest(int newCount)
        {
            if (newCount < 0 || newCount > _count)
                throw new ArgumentOutOfRangeException(nameof(newCount));

            int toClear = _count - newCount;

            for (int i = 0; i < toClear; i++)
            {
                int index = (_head - 1 - i + Capacity) % Capacity;
                _buffer[index] = default!;
            }

            _count = newCount;
            _head = (_head - toClear + Capacity) % Capacity;
        }


        public void Clear()
        {
            _head = 0;
            _count = 0;
            Array.Clear(_buffer, 0, _buffer.Length);
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < _count; i++)
                yield return GetAt(i);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
