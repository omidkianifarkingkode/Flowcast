// Runtime/Rest/Client/RequestFeatures.cs
using System;

namespace Flowcast.Rest.Client
{
    [Flags]
    public enum RequestFeatures : uint
    {
        None = 0,
        Logging = 1 << 0,
        Caching = 1 << 1,
        Auth = 1 << 2,
        Retry = 1 << 3,
        CircuitBreaker = 1 << 4,
        RateLimit = 1 << 5,
        Idempotency = 1 << 6,
        CompressRequest = 1 << 7,
        DecompressResp = 1 << 8,
        Record = 1 << 9,
    }

    public static class FeatureBits
    {
        public static bool Has(this RequestFeatures flags, RequestFeatures f) => (flags & f) == f;
        public static RequestFeatures With(this RequestFeatures flags, RequestFeatures f) => flags | f;
        public static RequestFeatures Without(this RequestFeatures flags, RequestFeatures f) => flags & ~f;
    }
}
