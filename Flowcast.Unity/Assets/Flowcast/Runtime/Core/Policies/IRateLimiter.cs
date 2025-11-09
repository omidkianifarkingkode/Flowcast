// Runtime/Core/Policies/IRateLimiter.cs
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Flowcast.Core.Policies
{
    public interface IRateLimiter
    {
        Task WaitAsync(string key, CancellationToken ct); // key can be host or route
    }

    /// Simple token-bucket per key:
    /// - capacity: max tokens
    /// - refillRate: tokens per second
    public sealed class TokenBucketRateLimiter : IRateLimiter
    {
        private readonly int _capacity;
        private readonly double _refillPerSecond;
        private readonly System.Collections.Concurrent.ConcurrentDictionary<string, Bucket> _buckets = new();

        public TokenBucketRateLimiter(int capacity = 10, double refillPerSecond = 5)
        {
            _capacity = Math.Max(1, capacity);
            _refillPerSecond = System.Math.Max(0.1, refillPerSecond);
        }

        public async Task WaitAsync(string key, CancellationToken ct)
        {
            var b = _buckets.GetOrAdd(key ?? "", _ => new Bucket(_capacity));
            while (true)
            {
                ct.ThrowIfCancellationRequested();

                var now = System.DateTimeOffset.UtcNow;
                b.Refill(now, _refillPerSecond, _capacity);

                lock (b.Lock)
                {
                    if (b.Tokens >= 1)
                    {
                        b.Tokens -= 1;
                        return;
                    }
                }
                // Sleep until next token (approx)
                var waitMs = System.Math.Max(10, (int)(1000 / _refillPerSecond));
                try { await Task.Delay(waitMs, ct).ConfigureAwait(false); } catch { }
            }
        }

        private sealed class Bucket
        {
            public readonly object Lock = new();
            public double Tokens;
            public DateTimeOffset LastRefill;

            public Bucket(int capacity)
            {
                Tokens = capacity;
                LastRefill = DateTimeOffset.UtcNow;
            }

            public void Refill(DateTimeOffset now, double ratePerSec, int capacity)
            {
                var delta = (now - LastRefill).TotalSeconds;
                if (delta <= 0) return;
                lock (Lock)
                {
                    Tokens = System.Math.Min(capacity, Tokens + delta * ratePerSec);
                    LastRefill = now;
                }
            }
        }
    }
}
