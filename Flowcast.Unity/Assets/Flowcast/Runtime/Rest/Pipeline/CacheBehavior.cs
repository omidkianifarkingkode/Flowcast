// Runtime/Rest/Pipeline/CacheBehavior.cs
using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Flowcast.Core.Cache;
using Flowcast.Rest.Client;

namespace Flowcast.Rest.Pipeline
{
    /// GET-only cache with ETag revalidation.
    /// - Key: absolute URL (no Vary handling in MVP)
    /// - If cached with ETag, sends If-None-Match and maps 304 -> cached body
    /// - Honors Cache-Control: max-age for simple TTL; ignores no-store/no-cache for MVP simplicity
    public sealed class CacheBehavior : IPipelineBehavior
    {
        private readonly ICacheProvider _cache;

        public CacheBehavior(ICacheProvider cache) { _cache = cache; }

        public async Task<ApiResponse> HandleAsync(ApiRequest req, CancellationToken ct, PipelineNext next)
        {
            if (!IsGet(req)) return await next(req, ct).ConfigureAwait(false);

            var key = req.Url.AbsoluteUri;

            // attach If-None-Match
            if (_cache.TryGet(key, out var entry) && !string.IsNullOrEmpty(entry.ETag))
                req.SetHeader("If-None-Match", entry.ETag);

            var resp = await next(req, ct).ConfigureAwait(false);

            if (resp.Status == 304 && entry != null)
            {
                // Serve from cache (update headers; keep cached body)
                var merged = new ApiResponse
                {
                    Status = 200,
                    BodyBytes = entry.BodyBytes,
                    MediaType = entry.MediaType,
                    Headers = resp.Headers // keep new headers (fresh date, etc.)
                };
                // refresh TTL if max-age renewed
                ApplyCachingHeaders(key, merged);
                return merged;
            }

            // Cache fresh 200 responses
            if (resp.Status == 200)
            {
                var toStore = new CacheEntry
                {
                    BodyBytes = resp.BodyBytes,
                    MediaType = resp.MediaType,
                    Headers = resp.Headers,
                    Status = resp.Status,
                    StoredAtUtc = DateTimeOffset.UtcNow,
                    ETag = TryGet(resp, "ETag")
                };
                // TTL
                var (hasTtl, ttl) = TryParseMaxAge(TryGet(resp, "Cache-Control"));
                if (hasTtl) toStore.ExpiresUtc = DateTimeOffset.UtcNow.Add(ttl);

                _cache.Set(key, toStore);
            }

            return resp;
        }

        private static bool IsGet(ApiRequest req) => (req.Method ?? "GET").ToUpperInvariant() == "GET";

        private static string TryGet(ApiResponse resp, string header)
            => resp.Headers.TryGet(header, out var v) ? v : null;

        private static (bool ok, TimeSpan ttl) TryParseMaxAge(string cacheControl)
        {
            if (string.IsNullOrEmpty(cacheControl)) return (false, default);
            var m = Regex.Match(cacheControl, @"max-age\s*=\s*(\d+)", RegexOptions.IgnoreCase);
            if (m.Success && int.TryParse(m.Groups[1].Value, out var seconds))
                return (true, TimeSpan.FromSeconds(Math.Clamp(seconds, 1, 86400)));
            return (false, default);
        }

        private void ApplyCachingHeaders(string key, ApiResponse resp)
        {
            // If server gives new Cache-Control, update TTL on the stored entry
            if (_cache.TryGet(key, out var existing))
            {
                var (ok, ttl) = TryParseMaxAge(TryGet(resp, "Cache-Control"));
                if (ok) { existing.ExpiresUtc = DateTimeOffset.UtcNow.Add(ttl); _cache.Set(key, existing); }
            }
        }
    }
}
