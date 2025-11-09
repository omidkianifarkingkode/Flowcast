// Runtime/Rest/Bootstrap/FlowcastRestBootstrapper.cs
using System;
using UnityEngine;
using Flowcast.Core.Cache;
using Flowcast.Core.Environments;
using Flowcast.Core.Policies;
using Flowcast.Core.Serialization;
using Flowcast.Core.Auth;
using Flowcast.Rest.Client;
using Flowcast.Rest.Pipeline;
using Flowcast.Rest.Transport;
using Environment = Flowcast.Core.Environments.Environment;

namespace Flowcast.Rest.Bootstrap
{
    /// <summary> Drop on a GameObject to configure Flowcast.Rest from FlowcastRestSettings. </summary>
    public sealed class FlowcastRestBootstrapper : MonoBehaviour
    {
        [SerializeField] private FlowcastRestSettings settings;

        private void Awake()
        {
            if (settings == null)
            {
                Debug.LogError("[Flowcast] FlowcastRestBootstrapper requires FlowcastRestSettings.");
                return;
            }

            // 1) Ensure environment provider is configured
            var envProvider = EnvironmentProvider.Instance;
            envProvider.Configure(settings.CreateConfiguration());

            var env = envProvider.Current;
            if (env == null)
            {
                Debug.LogWarning("[Flowcast] No active Environment; Flowcast.Rest will still initialize with defaults.");
            }

            // 2) Build services
            var serializers = CreateSerializers(settings);
            var cache = new MemoryCacheProvider(256);
            var auth = CreateAuth(settings);

            var options = CreateRestClientOptionsFromEnv(env);

            var transport = CreateTransport(env);

            // 3) Create RestClient
            var client = new RestClient(
                envProvider: envProvider,
                transport: transport,
                serializers: serializers,
                auth: auth,
                addDefaultLogging: false,
                options: options);

            // 4) Pipeline (gated behaviors)
            var breaker = new SimpleCircuitBreaker(env?.Rest.BreakerFailureThreshold ?? 5, env?.Rest.BreakerOpenMs ?? 5000);
            var limiter = new TokenBucketRateLimiter(env?.Rest.RateLimitCapacity ?? 8, env?.Rest.RateLimitRefillPerSecond ?? 4);

            client.AddBehavior(new CacheBehavior(cache,
                respectNoStore: true,
                respectNoCache: true,
                enableStaleWhileRevalidate: env?.Rest.CacheSWR ?? true,
                maxStaleSeconds: Math.Max(0, (env?.Rest.CacheDefaultTtlSeconds ?? 60) / 2)));

            client.AddBehavior(new CircuitBreakerBehavior(breaker));
            client.AddBehavior(new RateLimiterBehavior(limiter));
            client.AddBehavior(new IdempotencyBehavior());
            client.AddBehavior(new AuthBehavior(auth));
            client.AddBehavior(new RetryBehavior());
            client.AddBehavior(new LoggingBehavior(envProvider));

            // 5) Wrap in service and expose singleton
            var service = new FlowcastRestService(client);
            FlowcastRest.SetInstance(service);

            if (env != null && env.EnableLogging)
                Debug.Log($"[Flowcast] Rest bootstrap ready for env '{env.DisplayName}' ({env.Id}).");
        }

        private ISerializerRegistry CreateSerializers(FlowcastRestSettings restSettings)
        {
            ISerializer json = new UnityJsonSerializer();
#if FLOWCAST_NEWTONSOFT_JSON
            if (restSettings.PreferNewtonsoftIfDefined) json = new NewtonsoftJsonSerializer();
#endif
            var reg = new SerializerRegistry(json);
            // register XML or others if you like
            return reg;
        }

        private IAuthProvider CreateAuth(FlowcastRestSettings restSettings)
        {
            if (!restSettings.UseOAuth2) return null;
            if (string.IsNullOrWhiteSpace(restSettings.TokenEndpoint) || string.IsNullOrWhiteSpace(restSettings.ClientId))
                return null;

            return new OAuth2RefreshingAuthProvider(
                tokenEndpoint: restSettings.TokenEndpoint,
                clientId: restSettings.ClientId,
                clientSecret: restSettings.ClientSecret,
                initialAccessToken: restSettings.InitialAccessToken,
                initialRefreshToken: restSettings.InitialRefreshToken);
        }

        private ITransport CreateTransport(Environment env)
        {
            var mode = env?.Rest.TransportMode ?? RestTransportModeCore.Real;
            switch (mode)
            {
                case RestTransportModeCore.Mock: return new MockTransport();
                default: return new UnityWebRequestTransport();
            }
        }

        private RestClientOptions CreateRestClientOptionsFromEnv(Environment env)
        {
            var opts = new RestClientOptions();

            // Map feature defaults (Core → Rest.RequestFeatures)
            var features = RequestFeatures.None;
            if (env?.Rest.DefaultLogging ?? true) features |= RequestFeatures.Logging;
            if (env?.Rest.DefaultRetry ?? true) features |= RequestFeatures.Retry;
            if (env?.Rest.DefaultRateLimit ?? true) features |= RequestFeatures.RateLimit;
            if (env?.Rest.DefaultCircuitBreaker ?? true) features |= RequestFeatures.CircuitBreaker;
            if (env?.Rest.DefaultCaching ?? false) features |= RequestFeatures.Caching;
            if (env?.Rest.DefaultAuth ?? false) features |= RequestFeatures.Auth;

            opts.DefaultPolicy = new RequestPolicy { Features = features };

            // Prefix→Policy selector
            opts.PolicySelector = (req, policy) =>
            {
                var p = new RequestPolicy { Features = policy.Features };
                if (env == null) return p;

                var path = req.Url.AbsolutePath ?? "";
                foreach (var rule in env.Rest.PrefixPolicies)
                {
                    if (string.IsNullOrEmpty(rule.PathPrefix)) continue;
                    if (!path.StartsWith(rule.PathPrefix, StringComparison.OrdinalIgnoreCase)) continue;

                    if (rule.RequireAuth) p.Features |= RequestFeatures.Auth;
                    if (rule.EnableCache)
                    {
                        p.Features |= RequestFeatures.Caching;
                        p.CacheTtlSeconds = rule.CacheTtlSeconds > 0 ? rule.CacheTtlSeconds : env.Rest.CacheDefaultTtlSeconds;
                        p.CacheSWR = env.Rest.CacheSWR;
                    }
                    if (rule.DisableRetry) p.Features &= ~RequestFeatures.Retry;
                    if (!string.IsNullOrEmpty(rule.RateLimitKey)) p.RateLimitKey = rule.RateLimitKey;
                }
                return p;
            };

            return opts;
        }
    }
}
