// Runtime/Rest/Pipeline/CacheBehavior.cs
using System;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Flowcast.Core.Cache;
using Flowcast.Rest.Client;

namespace Flowcast.Rest.Pipeline
{
    public sealed class CacheBehavior : IPipelineBehavior
    {
        private readonly ICacheProvider _cache;
        private readonly bool _respectNoStore;
        private readonly bool _respectNoCache;
        private readonly bool _enableSWR;              // serve stale immediately, revalidate in background
        private readonly TimeSpan _maxStale;           // guardrail for how stale we allow to serve

        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _revalGates = new();

        public CacheBehavior(
            ICacheProvider cache,
            bool respectNoStore = true,
            bool respectNoCache = true,
            bool enableStaleWhileRevalidate = true,
            int maxStaleSeconds = 30)
        {
            _cache = cache;
            _respectNoStore = respectNoStore;
            _respectNoCache = respectNoCache;
            _enableSWR = enableStaleWhileRevalidate;
            _maxStale = TimeSpan.FromSeconds(Math.Max(0, maxStaleSeconds));
        }

        public async Task<ApiResponse> HandleAsync(ApiRequest req, CancellationToken ct, PipelineNext next)
        {
            if (req.Policy == null || !req.Policy.Features.Has(RequestFeatures.Caching))
                return await next(req, ct).ConfigureAwait(false);

            if (!IsGet(req)) return await next(req, ct).ConfigureAwait(false);

            var key = req.Url.AbsoluteUri;

            // If we have cache: set If-None-Match
            if (_cache.TryGet(key, out var entry) && !string.IsNullOrEmpty(entry.ETag))
                req.SetHeader("If-None-Match", entry.ETag);

            // If we have a fresh entry and not forced to revalidate, just return it
            if (entry != null && IsFresh(entry))
                return CloneAs200(entry); // short-circuit

            // If entry is stale and SWR enabled, serve stale now and revalidate in background
            if (entry != null && _enableSWR && IsStaleButAllowed(entry))
            {
                _ = RevalidateInBackgroundAsync(key, req, next); // fire & forget
                return CloneAs200(entry);
            }

            // Otherwise, go to network
            var resp = await next(req, ct).ConfigureAwait(false);

            // If 304 Not Modified and we have cache, promote cached body
            if (resp.Status == 304 && entry != null)
            {
                var merged = new ApiResponse
                {
                    Status = 200,
                    MediaType = entry.MediaType,
                    BodyBytes = entry.BodyBytes,
                    Headers = resp.Headers
                };
                ApplyCachingHeaders(key, merged); // refresh TTL/ETag
                return merged;
            }

            // Cache 200 responses unless no-store
            if (resp.Status == 200 && !_respectNoStore || (resp.Status == 200 && !HasNoStore(resp)))
                Store(key, resp);

            return resp;
        }

        private static bool IsGet(ApiRequest req) => (req.Method ?? "GET").ToUpperInvariant() == "GET";

        private bool IsFresh(CacheEntry e)
            => !e.ExpiresUtc.HasValue || e.ExpiresUtc.Value > DateTimeOffset.UtcNow;

        private bool IsStaleButAllowed(CacheEntry e)
            => e.ExpiresUtc.HasValue && DateTimeOffset.UtcNow - e.ExpiresUtc.Value <= _maxStale;

        private async Task RevalidateInBackgroundAsync(string key, ApiRequest original, PipelineNext next)
        {
            var gate = _revalGates.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
            if (!gate.Wait(0)) return;  // someone else is revalidating
            try
            {
                // Clone a minimal conditional request
                var reval = new ApiRequest
                {
                    Method = "GET",
                    Url = original.Url,
                    TimeoutSeconds = original.TimeoutSeconds
                };
                foreach (var h in original.Headers.Pairs) reval.SetHeader(h.Key, h.Value);
                if (_cache.TryGet(key, out var entry) && !string.IsNullOrEmpty(entry.ETag))
                    reval.SetHeader("If-None-Match", entry.ETag);

                var resp = await next(reval, CancellationToken.None).ConfigureAwait(false);
                if (resp.Status == 200)
                    Store(key, resp);
                else if (resp.Status == 304 && entry != null)
                    ApplyCachingHeaders(key, resp); // renew TTL from headers
            }
            catch { /* swallow */ }
            finally { gate.Release(); }
        }

        private void Store(string key, ApiResponse resp)
        {
            if (_respectNoStore && HasNoStore(resp)) return;

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
            if (_respectNoCache && HasNoCache(resp))
            {
                // force revalidate each time; but still keep for SWR short windows
                toStore.ExpiresUtc = DateTimeOffset.UtcNow; // immediately stale
            }
            else
            {
                var (hasTtl, ttl) = TryParseMaxAge(TryGet(resp, "Cache-Control"));
                toStore.ExpiresUtc = hasTtl ? DateTimeOffset.UtcNow.Add(ttl) : (DateTimeOffset?)null;
            }

            _cache.Set(key, toStore);
        }

        private void ApplyCachingHeaders(string key, ApiResponse resp)
        {
            if (_cache.TryGet(key, out var existing))
            {
                existing.ETag = TryGet(resp, "ETag") ?? existing.ETag;
                var (ok, ttl) = TryParseMaxAge(TryGet(resp, "Cache-Control"));
                existing.ExpiresUtc = ok ? DateTimeOffset.UtcNow.Add(ttl) : existing.ExpiresUtc;
                _cache.Set(key, existing);
            }
        }

        private static ApiResponse CloneAs200(CacheEntry e)
            => new ApiResponse { Status = 200, MediaType = e.MediaType, BodyBytes = e.BodyBytes, Headers = e.Headers };

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

        private static bool HasNoStore(ApiResponse resp)
            => (TryGet(resp, "Cache-Control") ?? "").IndexOf("no-store", StringComparison.OrdinalIgnoreCase) >= 0;

        private static bool HasNoCache(ApiResponse resp)
            => (TryGet(resp, "Cache-Control") ?? "").IndexOf("no-cache", StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
