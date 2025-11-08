// Runtime/Rest/Client/RestClient.cs
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Flowcast.Core.Auth;
using Flowcast.Core.Common;
using Flowcast.Core.Environments;
using Flowcast.Core.Serialization;
using Flowcast.Rest.Pipeline;
using Flowcast.Rest.Transport;

namespace Flowcast.Rest.Client
{
    public sealed class RestClient
    {
        private readonly IEnvironmentProvider _env;
        private readonly ITransport _transport;
        private readonly ISerializerRegistry _serializers;
        private readonly List<IPipelineBehavior> _pipeline = new();

        public RestClient(
            IEnvironmentProvider envProvider = null,
            ITransport transport = null,
            ISerializerRegistry serializers = null,
            IAuthProvider auth = null,
            bool addDefaultLogging = true)
        {
            _env = envProvider ?? EnvironmentProvider.Instance;
            _transport = transport ?? new UnityWebRequestTransport();

            // KISS: JSON-only by default, registry allows future plug-ins
            _serializers = serializers ?? new SerializerRegistry(new UnityJsonSerializer());

            // tiny default pipeline: Auth → Logging
            if (auth != null) _pipeline.Add(new AuthBehavior(auth));
            if (addDefaultLogging) _pipeline.Add(new LoggingBehavior(_env));
        }

        public void AddBehavior(IPipelineBehavior behavior) { if (behavior != null) _pipeline.Add(behavior); }

        public RequestBuilder Send(string method, string pathOrAbsoluteUrl)
            => new RequestBuilder(this, method, pathOrAbsoluteUrl);

        // ===== Fluent =====
        public sealed class RequestBuilder
        {
            private readonly RestClient _client;
            private readonly ApiRequest _req = new ApiRequest();

            internal RequestBuilder(RestClient client, string method, string pathOrAbsoluteUrl)
            {
                _client = client;
                _req.Method = string.IsNullOrWhiteSpace(method) ? "GET" : method.ToUpperInvariant();
                _req.Url = BuildUrl(client._env.Current, pathOrAbsoluteUrl);

                var env = client._env.Current;
                if (env != null)
                {
                    foreach (var kv in env.GetDefaultHeadersDictionary()) _req.SetHeader(kv.Key, kv.Value);
                    _req.TimeoutSeconds = env.TimeoutSeconds;
                }
            }

            private static Uri BuildUrl(Flowcast.Core.Environments.Environment env, string pathOrAbsolute)
            {
                if (Uri.TryCreate(pathOrAbsolute, UriKind.Absolute, out var abs)) return abs;
                var baseUrl = env?.BaseUrl ?? "";
                if (!baseUrl.EndsWith("/")) baseUrl += "/";
                if (pathOrAbsolute.StartsWith("/")) pathOrAbsolute = pathOrAbsolute.Substring(1);
                return new Uri(baseUrl + pathOrAbsolute);
            }

            public RequestBuilder WithHeader(string name, string value) { _req.SetHeader(name, value); return this; }
            public RequestBuilder TimeoutSeconds(int seconds) { _req.TimeoutSeconds = seconds; return this; }

            // Use registry to choose serializer by media type (defaults to JSON)
            public RequestBuilder WithBody<T>(T body, string mediaType = "application/json")
            {
                if (!_client._serializers.TryGet(mediaType, out var ser))
                    ser = _client._serializers.Default;
                _req.BodyBytes = ser.Serialize(body);
                _req.MediaType = mediaType;
                if (_req.Method == "GET") _req.Method = "POST";
                return this;
            }

            public RequestBuilder WithBytesBody(byte[] bytes, string mediaType = "application/octet-stream")
            {
                _req.BodyBytes = bytes;
                _req.MediaType = mediaType;
                if (_req.Method == "GET") _req.Method = "POST";
                return this;
            }

            public Task<Result<T>> AsResultAsync<T>(CancellationToken ct = default)
                => ExecuteAsync<T>(ct);

            public Task<Result<RawResponse>> AsRawAsync(CancellationToken ct = default)
                => ExecuteAsync<RawResponse>(ct);

#if FLOWCAST_UNITASK
            // Optional UniTask façade
            public Cysharp.Threading.Tasks.UniTask<Result<T>> AsResultUniTask<T>(System.Threading.CancellationToken ct = default)
                => new Cysharp.Threading.Tasks.UniTask<Result<T>>(ExecuteAsync<T>(ct));
            public Cysharp.Threading.Tasks.UniTask<Result<RawResponse>> AsRawUniTask(System.Threading.CancellationToken ct = default)
                => new Cysharp.Threading.Tasks.UniTask<Result<RawResponse>>(ExecuteAsync<RawResponse>(ct));
#endif

            // ---- core execution with tiny pipeline ----
            private async Task<Result<T>> ExecuteAsync<T>(CancellationToken ct)
            {
                // Build pipeline chain
                PipelineNext terminal = (r, token) => _client._transport.SendAsync(r, token);
                for (int i = _client._pipeline.Count - 1; i >= 0; i--)
                {
                    var behavior = _client._pipeline[i];
                    var nextCopy = terminal;
                    terminal = (r, token) => behavior.HandleAsync(r, token, nextCopy);
                }

                var resp = await terminal(_req, ct).ConfigureAwait(false);

                var meta = new ResponseMeta(
                    traceId: resp.Headers.TryGet("x-correlation-id", out var tid) ? tid : null,
                    durationMs: 0, fromCache: false);

                if (resp.Status >= 200 && resp.Status < 300)
                {
                    if (typeof(T) == typeof(RawResponse))
                    {
                        var raw = new RawResponse(resp.Status, resp.MediaType, resp.BodyBytes, resp.Headers);

                        return Result<T>.Success((T)(object)raw, meta);
                    }

                    var media = (resp.MediaType ?? "").ToLowerInvariant();
                    if (_client._serializers.TryGet(media, out var deser) || media.Contains("+json") || media.Contains("application/json"))
                        deser ??= _client._serializers.Default;

                    if (deser != null && (media.Contains("json") || deser == _client._serializers.Default))
                    {
                        try
                        {
                            var val = deser.Deserialize<T>(resp.BodyBytes);
                            return Result<T>.Success(val, meta);
                        }
                        catch (Exception ex)
                        {
                            return Result<T>.Failure(new Error(ErrorKind.Unknown, resp.Status, "deserialization_error", ex.Message), meta);
                        }
                    }

                    if (typeof(T) == typeof(string))
                    {
                        var txt = resp.BodyBytes != null ? Encoding.UTF8.GetString(resp.BodyBytes) : null;
                        return Result<T>.Success((T)(object)txt, meta);
                    }
                    if (typeof(T) == typeof(byte[]))
                        return Result<T>.Success((T)(object)resp.BodyBytes, meta);

                    return Result<T>.Failure(
                        new Error(ErrorKind.Unknown, resp.Status, "unsupported_media_type",
                                  $"Cannot deserialize '{resp.MediaType}' to {typeof(T).Name}."), meta);
                }

                var kind = MapErrorKind(resp.Status);
                var bodyText = resp.BodyBytes != null ? Encoding.UTF8.GetString(resp.BodyBytes) : null;
                return Result<T>.Failure(new Error(kind, resp.Status, null, bodyText ?? $"HTTP {resp.Status}"), meta);
            }

            private static ErrorKind MapErrorKind(int status)
            {
                if (status == 401) return ErrorKind.Auth;
                if (status == 408 || status == 429 || status >= 500) return ErrorKind.Transient;
                if (status >= 400 && status < 500) return ErrorKind.Client;
                if (status == 0) return ErrorKind.Unknown;
                return ErrorKind.Server;
            }
        }
    }
}
