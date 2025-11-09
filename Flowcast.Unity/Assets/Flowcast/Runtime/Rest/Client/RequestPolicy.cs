// Runtime/Rest/Client/RequestPolicy.cs
namespace Flowcast.Rest.Client
{
    public sealed class RequestPolicy
    {
        public RequestFeatures Features;

        // Common knobs (keep KISS; extend later as needed)
        public int? CacheTtlSeconds;     // null = behavior decides; 0 = no cache
        public bool? CacheSWR;           // stale-while-revalidate
        public string RateLimitKey;      // null => per-host
        public int? RetryMaxAttempts;
        public int? RetryBaseDelayMs;
        public string IdempotencyKey;    // null => behavior may auto-generate
        public bool? CompressAlways;     // otherwise threshold-based

        public static RequestPolicy Default => new() 
            { Features = RequestFeatures.Logging | RequestFeatures.Retry | RequestFeatures.RateLimit | RequestFeatures.CircuitBreaker };
    }
}
