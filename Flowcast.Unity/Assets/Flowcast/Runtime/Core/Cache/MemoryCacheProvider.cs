// Runtime/Core/Cache/MemoryCacheProvider.cs
using System;
using System.Collections.Generic;

namespace Flowcast.Core.Cache
{
    /// KISS: process-local dictionary with soft size limit and simple TTL.
    public sealed class MemoryCacheProvider : ICacheProvider
    {
        private readonly Dictionary<string, CacheEntry> _map = new(StringComparer.Ordinal);
        private readonly object _lock = new();
        private readonly int _maxEntries;

        public MemoryCacheProvider(int maxEntries = 256) { _maxEntries = Math.Max(16, maxEntries); }

        public bool TryGet(string key, out CacheEntry entry)
        {
            lock (_lock)
            {
                if (_map.TryGetValue(key, out entry))
                {
                    if (entry.ExpiresUtc.HasValue && entry.ExpiresUtc.Value <= DateTimeOffset.UtcNow)
                    {
                        _map.Remove(key);
                        entry = null;
                        return false;
                    }
                    return true;
                }
                return false;
            }
        }

        public void Set(string key, CacheEntry entry)
        {
            lock (_lock)
            {
                // naive eviction: if over limit, clear oldest by StoredAtUtc
                if (_map.Count >= _maxEntries)
                {
                    string oldestKey = null;
                    DateTimeOffset oldest = DateTimeOffset.MaxValue;
                    foreach (var kv in _map)
                    {
                        if (kv.Value.StoredAtUtc < oldest)
                        {
                            oldest = kv.Value.StoredAtUtc;
                            oldestKey = kv.Key;
                        }
                    }
                    if (oldestKey != null) _map.Remove(oldestKey);
                }
                _map[key] = entry;
            }
        }

        public void Remove(string key)
        {
            lock (_lock)
            {
                _map.Remove(key);
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _map.Clear();
            }
        }
    }
}
