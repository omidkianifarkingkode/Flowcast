// Runtime/Rest/Pipeline/AuthBehavior.cs  (superset)
using System.Threading;
using System.Threading.Tasks;
using Flowcast.Core.Auth;
using Flowcast.Rest.Client;

namespace Flowcast.Rest.Pipeline
{
    public sealed class AuthBehavior : IPipelineBehavior
    {
        private readonly IAuthProvider _auth;
        private readonly IRefreshingAuthProvider _refreshing;
        private readonly System.Threading.SemaphoreSlim _refreshLock = new(1, 1);

        public AuthBehavior(IAuthProvider authProvider)
        {
            _auth = authProvider;
            _refreshing = authProvider as IRefreshingAuthProvider;
        }

        public async Task<ApiResponse> HandleAsync(ApiRequest req, CancellationToken ct, PipelineNext next)
        {
            if (req.Policy == null || !req.Policy.Features.Has(RequestFeatures.Auth))
                return await next(req, ct).ConfigureAwait(false);

            AttachAuth(req, _auth, token: await _auth?.GetAccessTokenAsync(ct));
            var resp = await next(req, ct).ConfigureAwait(false);

            if (resp.Status == 401 && _refreshing != null && !HasAuthRetriedMarker(req))
            {
                await _refreshLock.WaitAsync(ct).ConfigureAwait(false);
                try
                {
                    var ok = await _refreshing.RefreshAsync(ct).ConfigureAwait(false);
                    if (!ok) return resp;

                    MarkAuthRetried(req);
                    AttachAuth(req, _auth, token: await _auth.GetAccessTokenAsync(ct));
                    resp = await next(req, ct).ConfigureAwait(false);
                }
                finally { _refreshLock.Release(); }
            }
            return resp;
        }

        private static void AttachAuth(ApiRequest req, IAuthProvider auth, string token)
        {
            switch (auth)
            {
                case null:
                    return;

                case BasicAuthProvider basic:
                    req.SetHeader("Authorization", basic.AuthorizationHeader);
                    return;

                case ApiKeyAuthProvider apiKey:
                    if (!string.IsNullOrEmpty(apiKey.HeaderName))
                        req.SetHeader(apiKey.HeaderName, apiKey.HeaderValue);
                    return;

                default:
                    if (!string.IsNullOrEmpty(token))
                        req.SetHeader("Authorization", "Bearer " + token);
                    return;
            }
        }

        private static bool HasAuthRetriedMarker(ApiRequest req)
            => req.Headers.TryGet("X-Flowcast-Auth-Retried", out _);

        private static void MarkAuthRetried(ApiRequest req)
            => req.SetHeader("X-Flowcast-Auth-Retried", "1");
    }
}
