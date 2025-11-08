// Runtime/Rest/Transport/MockTransport.cs
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Flowcast.Core.Common;
using Flowcast.Rest.Client;

namespace Flowcast.Rest.Transport
{
    /// A simple in-memory HTTP simulator keyed by (method, absoluteUrl[, bodyHash]).
    /// - Deterministic: returns canned ApiResponse(s) you register.
    /// - Latency: per-route artificial delay (ms).
    /// - Sequence: register multiple responses; they'll be dequeued per call.
    /// - Wildcards: optional prefix match for URLs (see RouteMatchMode.Prefix).
    ///
    /// KISS: No cookies, redirects, or streaming. Good enough for tests & Editor.
    public sealed class MockTransport : ITransport
    {
        private readonly ConcurrentDictionary<MockKey, Queue<MockResponse>> _routes =
            new(new MockKeyComparer());

        private readonly List<MockPrefixRoute> _prefixRoutes = new(); // rare; keep simple
        private readonly object _prefixLock = new();

        private readonly Func<ApiRequest, string> _bodyHasher;

        public MockTransport(Func<ApiRequest, string> bodyHasher = null)
        {
            _bodyHasher = bodyHasher ?? DefaultBodyHash;
        }

        // ==== Public API ====

        /// Register a fixed response for an exact (method, absoluteUrl) match.
        public MockTransport When(string method, string absoluteUrl, MockResponse response, string bodyHash = null)
        {
            var key = new MockKey(NormMethod(method), absoluteUrl, bodyHash ?? "*");
            var q = _routes.GetOrAdd(key, _ => new Queue<MockResponse>());
            q.Enqueue(response ?? throw new ArgumentNullException(nameof(response)));
            return this;
        }

        /// Register a sequence of responses for the same match; each SendAsync dequeues one.
        public MockTransport When(string method, string absoluteUrl, IEnumerable<MockResponse> responses, string bodyHash = null)
        {
            var key = new MockKey(NormMethod(method), absoluteUrl, bodyHash ?? "*");
            var q = _routes.GetOrAdd(key, _ => new Queue<MockResponse>());
            foreach (var r in responses) q.Enqueue(r ?? throw new ArgumentNullException(nameof(responses)));
            return this;
        }

        /// Register a URL-PREFIX match (e.g., any /v1/profile?*). Evaluated if exact match not found.
        public MockTransport WhenPrefix(string method, string urlPrefix, MockResponse response)
        {
            lock (_prefixLock)
            {
                _prefixRoutes.Add(new MockPrefixRoute(NormMethod(method), urlPrefix, response));
            }
            return this;
        }

        /// Clear all routes.
        public void Reset()
        {
            _routes.Clear();
            lock (_prefixLock) _prefixRoutes.Clear();
        }

        // ==== ITransport ====

        public async Task<ApiResponse> SendAsync(ApiRequest req, CancellationToken ct)
        {
            var method = NormMethod(req.Method);
            var url = req.Url?.AbsoluteUri ?? "";
            var bodyHash = ResolveBodyHashKey(req);

            // 1) Exact with bodyHash
            if (TryDequeue(new MockKey(method, url, bodyHash), out var resp) ||
                // 2) Exact wildcard body (*)
                TryDequeue(new MockKey(method, url, "*"), out resp) ||
                // 3) Prefix routes
                TryMatchPrefix(method, url, out resp))
            {
                if (resp.LatencyMs > 0)
                {
                    try { await Task.Delay(resp.LatencyMs, ct).ConfigureAwait(false); }
                    catch (OperationCanceledException) { /* fall through and return */ }
                }

                // Build ApiResponse
                var api = new ApiResponse
                {
                    Status = resp.Status,
                    MediaType = resp.MediaType ?? "",
                    BodyBytes = resp.BodyBytes,
                };
                if (resp.Headers != null)
                {
                    foreach (var kv in resp.Headers.Pairs)
                        api.Headers.Set(kv.Key, kv.Value);
                }
                return api;
            }

            // Default "no route" behavior: 599 mock error
            var fail = new ApiResponse
            {
                Status = 599, // non-standard; indicates missing mock
                MediaType = "text/plain",
                BodyBytes = Encoding.UTF8.GetBytes($"No mock for {method} {url} (hash={bodyHash})."),
            };
            return fail;
        }

        // ==== Internals ====

        private bool TryDequeue(MockKey key, out MockResponse response)
        {
            if (_routes.TryGetValue(key, out var q) && q.Count > 0)
            {
                response = q.Dequeue();
                return true;
            }
            response = null;
            return false;
        }

        private bool TryMatchPrefix(string method, string url, out MockResponse response)
        {
            lock (_prefixLock)
            {
                for (int i = 0; i < _prefixRoutes.Count; i++)
                {
                    var pr = _prefixRoutes[i];
                    if (pr.Method == method && url.StartsWith(pr.UrlPrefix, StringComparison.Ordinal))
                    {
                        response = pr.Response;
                        return true;
                    }
                }
            }
            response = null;
            return false;
        }

        private string ResolveBodyHashKey(ApiRequest req)
        {
            // Only hash if there's a body
            if (req.BodyBytes == null || req.BodyBytes.Length == 0) return "*";
            return _bodyHasher?.Invoke(req) ?? "*";
        }

        private static string NormMethod(string m) => string.IsNullOrWhiteSpace(m) ? "GET" : m.ToUpperInvariant();

        private static string DefaultBodyHash(ApiRequest req)
        {
            // Simple & stable: base64 of first 64 bytes + length; cheap and adequate for tests.
            var len = req.BodyBytes?.Length ?? 0;
            var head = len > 0 ? Convert.ToBase64String(req.BodyBytes, 0, Math.Min(len, 64)) : "";
            return $"{len}:{head}";
        }

        // ==== Types ====

        private readonly struct MockKey
        {
            public readonly string Method;
            public readonly string Url;
            public readonly string BodyHash; // "*" wildcard allowed

            public MockKey(string method, string url, string bodyHash)
            { Method = method; Url = url; BodyHash = bodyHash ?? "*"; }
        }

        private sealed class MockKeyComparer : IEqualityComparer<MockKey>
        {
            public bool Equals(MockKey x, MockKey y)
                => x.Method == y.Method && x.Url == y.Url && x.BodyHash == y.BodyHash;
            public int GetHashCode(MockKey k)
            {
                unchecked
                {
                    int h = 17;
                    h = h * 31 + k.Method.GetHashCode();
                    h = h * 31 + (k.Url?.GetHashCode() ?? 0);
                    h = h * 31 + (k.BodyHash?.GetHashCode() ?? 0);
                    return h;
                }
            }
        }

        private readonly struct MockPrefixRoute
        {
            public readonly string Method;
            public readonly string UrlPrefix;
            public readonly MockResponse Response;
            public MockPrefixRoute(string method, string prefix, MockResponse resp)
            { Method = method; UrlPrefix = prefix ?? ""; Response = resp; }
        }
    }

    /// A canned response with optional headers and latency.
    public sealed class MockResponse
    {
        public int Status { get; set; } = 200;
        public string MediaType { get; set; } = "application/json";
        public byte[] BodyBytes { get; set; }
        public Headers Headers { get; set; } = new Headers();
        public int LatencyMs { get; set; } = 0;

        public static MockResponse Json(string json, int status = 200, int latencyMs = 0)
            => new MockResponse
            {
                Status = status,
                MediaType = "application/json",
                BodyBytes = Encoding.UTF8.GetBytes(json ?? "null"),
                LatencyMs = latencyMs
            };

        public static MockResponse Text(string text, int status = 200, int latencyMs = 0)
            => new MockResponse
            {
                Status = status,
                MediaType = "text/plain",
                BodyBytes = Encoding.UTF8.GetBytes(text ?? string.Empty),
                LatencyMs = latencyMs
            };

        public static MockResponse Bytes(byte[] bytes, string mediaType = "application/octet-stream", int status = 200, int latencyMs = 0)
            => new MockResponse
            {
                Status = status,
                MediaType = mediaType,
                BodyBytes = bytes ?? Array.Empty<byte>(),
                LatencyMs = latencyMs
            };
    }
}
