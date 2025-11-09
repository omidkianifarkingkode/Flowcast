// Runtime/Rest/Pipeline/IdempotencyBehavior.cs
using Flowcast.Rest.Client;
using System.Threading;
using System.Threading.Tasks;

namespace Flowcast.Rest.Pipeline
{
    /// Auto-adds Idempotency-Key for unsafe methods if not present.
    public sealed class IdempotencyBehavior : IPipelineBehavior
    {
        public async Task<ApiResponse> HandleAsync(ApiRequest req, CancellationToken ct, PipelineNext next)
        {
            if (req.Policy == null || !req.Policy.Features.Has(RequestFeatures.Idempotency))
                return await next(req, ct).ConfigureAwait(false);

            // If no header set, apply policy key (already done by builder typically)
            if (!req.Headers.TryGet("Idempotency-Key", out _))
            {
                var key = req.Policy.IdempotencyKey ?? Flowcast.Core.Common.IdempotencyKey.New();
                req.SetHeader("Idempotency-Key", key);
            }

            return await next(req, ct).ConfigureAwait(false);

            //var m = (req.Method ?? "GET").ToUpperInvariant();
            //if (m is "POST" or "PUT" or "PATCH" or "DELETE")
            //{
            //    if (!req.Headers.TryGet("Idempotency-Key", out _))
            //        req.SetHeader("Idempotency-Key", Flowcast.Core.Common.IdempotencyKey.New());
            //}
            //return await next(req, ct);
        }
    }
}
