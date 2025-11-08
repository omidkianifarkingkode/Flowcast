// Runtime/Core/Common/Result/ResponseMeta.cs
namespace Flowcast.Core.Common
{
    public readonly struct ResponseMeta
    {
        public string TraceId { get; }
        public long DurationMs { get; }
        public bool FromCache { get; }
        public ResponseMeta(string traceId, long durationMs, bool fromCache)
        { TraceId = traceId; DurationMs = durationMs; FromCache = fromCache; }
    }
}
