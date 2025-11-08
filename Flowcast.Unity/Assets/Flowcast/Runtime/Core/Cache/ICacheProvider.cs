// Runtime/Core/Cache/ICacheProvider.cs
using System;

namespace Flowcast.Core.Cache
{
    public interface ICacheProvider
    {
        bool TryGet(string key, out CacheEntry entry);
        void Set(string key, CacheEntry entry);
        void Remove(string key);
        void Clear();
    }

    public sealed class CacheEntry
    {
        public byte[] BodyBytes;
        public string MediaType;
        public Core.Common.Headers Headers;   // response headers snapshot
        public int Status;
        public DateTimeOffset StoredAtUtc;
        public DateTimeOffset? ExpiresUtc;    // optional TTL from Cache-Control max-age
        public string ETag;                   // for conditional requests
    }
}
