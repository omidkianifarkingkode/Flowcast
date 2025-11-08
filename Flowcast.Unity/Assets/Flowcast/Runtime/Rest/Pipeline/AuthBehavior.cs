// Runtime/Rest/Pipeline/AuthBehavior.cs
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;
using Flowcast.Core.Auth;
using Flowcast.Rest.Client;

namespace Flowcast.Rest.Pipeline
{
    /// Adds Authorization before send.
    /// If response is 401 and a refreshing provider is available:
    ///  - single-flight refresh (one refresh for concurrent requests)
    ///  - replay original request ONCE if safe:
    ///      * always for GET/HEAD
    ///      * for non-GET only when "Idempotency-Key" header exists
    public sealed class AuthBehavior : IPipelineBehavior
    {
        private readonly IAuthProvider _auth;
        private readonly IRefreshingAuthProvider _refreshing;
        private readonly SemaphoreSlim _refreshLock = new(1, 1);

        public AuthBehavior(IAuthProvider authProvider)
        {
            _auth = authProvider;
            _refreshing = authProvider as IRefreshingAuthProvider;
        }

        public async Task<ApiResponse> HandleAsync(ApiRequest req, CancellationToken ct, PipelineNext next)
        {
            // 1) Attach current token (if any)
            if (_auth != null)
            {
                var token = await _auth.GetAccessTokenAsync(ct).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(token))
                    req.SetHeader("Authorization", "Bearer " + token);
            }

            var resp = await next(req, ct).ConfigureAwait(false);

            // 2) On 401, try refresh+replay (once)
            if (resp.Status == 401 && _refreshing != null && !HasAuthRetriedMarker(req))
            {
                // Avoid multiple refresh calls under concurrency (single-flight)
                await _refreshLock.WaitAsync(ct).ConfigureAwait(false);
                try
                {
                    // Another request may have refreshed while we waited; recheck token usefulness by trying refresh anyway.
                    var ok = await _refreshing.RefreshAsync(ct).ConfigureAwait(false);
                    if (!ok) return resp; // still 401 -> give up as Auth error

                    // Mark request to avoid infinite loops
                    MarkAuthRetried(req);

                    // Re-attach new token and replay if safe
                    var method = (req.Method ?? "GET").ToUpperInvariant();
                    var canReplay =
                        method == "GET" || method == "HEAD" ||
                        req.Headers.TryGet("Idempotency-Key", out _);

                    if (!canReplay)
                        return resp; // don't risk duplicating writes

                    // Replace Authorization header
                    var token = await _auth.GetAccessTokenAsync(ct).ConfigureAwait(false);
                    if (!string.IsNullOrEmpty(token))
                        req.SetHeader("Authorization", "Bearer " + token);

                    // Replay once
                    var replay = await next(req, ct).ConfigureAwait(false);
                    return replay;
                }
                finally
                {
                    _refreshLock.Release();
                }
            }

            return resp;
        }

        // We use a tiny internal marker header to prevent infinite loops.
        private static bool HasAuthRetriedMarker(ApiRequest req)
            => req.Headers.TryGet("X-Flowcast-Auth-Retried", out _);

        private static void MarkAuthRetried(ApiRequest req)
            => req.SetHeader("X-Flowcast-Auth-Retried", "1");
    }
}
