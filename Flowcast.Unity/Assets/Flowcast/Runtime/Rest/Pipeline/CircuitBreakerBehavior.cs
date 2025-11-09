// Runtime/Rest/Pipeline/CircuitBreakerBehavior.cs
using System.Threading;
using System.Threading.Tasks;
using Flowcast.Core.Policies;
using Flowcast.Rest.Client;

namespace Flowcast.Rest.Pipeline
{
    public sealed class CircuitBreakerBehavior : IPipelineBehavior
    {
        private readonly ICircuitBreaker _breaker;

        public CircuitBreakerBehavior(ICircuitBreaker breaker) { _breaker = breaker; }

        public async Task<ApiResponse> HandleAsync(ApiRequest req, CancellationToken ct, PipelineNext next)
        {
            if (req.Policy == null || !req.Policy.Features.Has(RequestFeatures.CircuitBreaker))
                return await next(req, ct).ConfigureAwait(false);

            if (_breaker != null && !_breaker.AllowRequest())
                return new ApiResponse { Status = 503, MediaType = "text/plain", BodyBytes = System.Text.Encoding.UTF8.GetBytes("circuit_open") };

            var resp = await next(req, ct).ConfigureAwait(false);

            if (resp.Status >= 200 && resp.Status < 400) _breaker?.OnSuccess();
            else if (resp.Status == 0 || resp.Status == 408 || resp.Status == 429 || resp.Status >= 500) _breaker?.OnFailure();
            else _breaker?.OnSuccess(); // client errors don't trip the breaker

            return resp;
        }
    }
}
