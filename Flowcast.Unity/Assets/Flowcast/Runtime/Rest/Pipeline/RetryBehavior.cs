// Runtime/Rest/Pipeline/RetryBehavior.cs
using Flowcast.Rest.Client;
using System;
using System.Buffers.Text;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace Flowcast.Rest.Pipeline
{
    /// KISS retry:
    /// - Retries on 408, 429, 5xx, or status==0 (connection)
    /// - Safe for GET/HEAD by default; for others only if "Idempotency-Key" header is present
    /// - Uses exponential backoff with jitter; honors Retry-After (seconds or HTTP date)
    public sealed class RetryBehavior : IPipelineBehavior
    {
        private readonly int _maxAttempts;
        private readonly int _baseDelayMs;
        private readonly int _maxDelayMs;

        public RetryBehavior(int maxAttempts = 3, int baseDelayMs = 250, int maxDelayMs = 3000)
        {
            _maxAttempts = Math.Max(1, maxAttempts);
            _baseDelayMs = Math.Max(0, baseDelayMs);
            _maxDelayMs = Math.Max(_baseDelayMs, maxDelayMs);
        }

        public async Task<ApiResponse> HandleAsync(ApiRequest req, CancellationToken ct, PipelineNext next)
        {
            if (req.Policy == null || !req.Policy.Features.Has(RequestFeatures.Retry))
                return await next(req, ct).ConfigureAwait(false);

            var attempts = req.Policy.RetryMaxAttempts ?? _maxAttempts;
            var baseDelayMs = req.Policy.RetryBaseDelayMs ?? _baseDelayMs;

            var attempt = 1;
            ApiResponse last = null;

            while (true)
            {
                last = await next(req, ct).ConfigureAwait(false);

                if (attempt >= attempts || !ShouldRetry(req, last))
                    return last;

                var delay = GetDelay(last, attempt, baseDelayMs);
                try
                {
                    await Task.Delay(delay, ct).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    return last; // surface last response on cancel
                }

                attempt++;
            }
        }

        private bool ShouldRetry(ApiRequest req, ApiResponse resp)
        {
            var status = resp.Status;
            var transient = status == 0 || status == 408 || status == 429 || status >= 500;
            if (!transient) return false;

            var method = (req.Method ?? "GET").ToUpperInvariant();
            if (method == "GET" || method == "HEAD") return true;

            // For non-GET, require idempotency key header
            return req.Headers.TryGet("Idempotency-Key", out _);
        }

        private TimeSpan GetDelay(ApiResponse resp, int attempt, int delay)
        {
            // Retry-After
            if (resp.Headers.TryGet("Retry-After", out var ra))
            {
                if (int.TryParse(ra, out var seconds))
                    return TimeSpan.FromSeconds(Math.Clamp(seconds, 0, 30));

                if (DateTimeOffset.TryParse(ra, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var when))
                {
                    var delta = when - DateTimeOffset.UtcNow;
                    if (delta > TimeSpan.Zero && delta < TimeSpan.FromSeconds(60))
                        return delta;
                }
            }

            // expo backoff with jitter
            var exp = Math.Min(_maxDelayMs, delay * (1 << Math.Min(6, attempt - 1)));
            var jitter = new Random().Next(delay / 2, exp);
            return TimeSpan.FromMilliseconds(jitter);
        }
    }
}
