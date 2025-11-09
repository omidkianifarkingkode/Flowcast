// Runtime/Rest/Pipeline/RateLimiterBehavior.cs
using System.Threading;
using System.Threading.Tasks;
using Flowcast.Core.Policies;
using Flowcast.Rest.Client;

namespace Flowcast.Rest.Pipeline
{
    /// Waits for a token before sending (per host by default).
    public sealed class RateLimiterBehavior : IPipelineBehavior
    {
        private readonly IRateLimiter _limiter;
        private readonly System.Func<ApiRequest, string> _keySelector;

        public RateLimiterBehavior(IRateLimiter limiter, System.Func<ApiRequest, string> keySelector = null)
        {
            _limiter = limiter;
            _keySelector = keySelector ?? (r => r.Url?.Host ?? "");
        }

        public async Task<ApiResponse> HandleAsync(ApiRequest req, CancellationToken ct, PipelineNext next)
        {
            if (_limiter == null || req.Policy == null || !req.Policy.Features.Has(RequestFeatures.RateLimit))
                return await next(req, ct).ConfigureAwait(false);

            var key = req.Policy.RateLimitKey ?? _keySelector(req);
            await _limiter.WaitAsync(key, ct).ConfigureAwait(false);

            return await next(req, ct).ConfigureAwait(false);
        }
    }
}
