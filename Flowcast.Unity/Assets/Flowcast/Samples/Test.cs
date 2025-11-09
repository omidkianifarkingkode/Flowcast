using Flowcast.Core.Auth;
using Flowcast.Core.Cache;
using Flowcast.Core.Environments;
using Flowcast.Core.Policies;
using Flowcast.Core.Serialization;
using Flowcast.Rest.Bootstrap;
using Flowcast.Rest.Client;
using Flowcast.Rest.Pipeline;
using Flowcast.Rest.Transport;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Test : MonoBehaviour
{
    RestClientOptions options = new RestClientOptions
    {
        DefaultPolicy = new RequestPolicy
        {
            Features = RequestFeatures.Logging | RequestFeatures.Retry | RequestFeatures.RateLimit | RequestFeatures.CircuitBreaker
        },
        PolicySelector = (req, policy) =>
        {
            var p = new RequestPolicy
            {
                Features = policy.Features,
                CacheTtlSeconds = policy.CacheTtlSeconds,
                CacheSWR = policy.CacheSWR,
                RateLimitKey = policy.RateLimitKey,
                RetryMaxAttempts = policy.RetryMaxAttempts,
                RetryBaseDelayMs = policy.RetryBaseDelayMs,
                IdempotencyKey = policy.IdempotencyKey,
                CompressAlways = policy.CompressAlways
            };

            var path = req.Url.AbsolutePath.ToLowerInvariant();

            // Public catalog is cacheable for 60s with SWR
            if (req.Method == "GET" && path.StartsWith("/v1/catalog"))
            {
                p.Features = p.Features.With(RequestFeatures.Caching);
                p.CacheTtlSeconds = 60;
                p.CacheSWR = true;
            }

            // Secure area requires auth
            if (path.StartsWith("/v1/secure"))
                p.Features = p.Features.With(RequestFeatures.Auth);

            // Payments: no retry
            if (path.StartsWith("/v1/payments"))
                p.Features = p.Features.Without(RequestFeatures.Retry);

            return p;
        }
    };

    // Start is called before the first frame update
    async void Start()
    {
        // 1) Core services
        var serializers = new SerializerRegistry(new UnityJsonSerializer());
        var cache = new MemoryCacheProvider();
        var breaker = new SimpleCircuitBreaker(failureThreshold: 5, openMs: 5000);
        var limiter = new TokenBucketRateLimiter(capacity: 8, refillPerSecond: 4);

        var auth = new SimpleBearerAuthProvider();
        auth.SetToken("<access_token>");

        var refreshingAuth = new SimpleRefreshingAuthProvider(
            refreshFunc: async ct =>
            {
                // TODO: call your refresh endpoint here and return the NEW access token string
                // If refresh fails, return null or empty string.
                await Task.Yield();
                return "<new_access_token>";
            },
            initialToken: "<optional_initial_access_token>"
        );

        // 2) Rest client
        var client = new RestClient(
            envProvider: EnvironmentProvider.Instance,
            serializers: serializers,
            auth: auth,
            options: options,
            addDefaultLogging: false);

        // 3) Behaviors (order matters)
        client.AddBehavior(new CacheBehavior(cache));
        client.AddBehavior(new CircuitBreakerBehavior(breaker));
        client.AddBehavior(new RateLimiterBehavior(limiter));  // per-host tokens
        client.AddBehavior(new IdempotencyBehavior());         // auto key on writes
        client.AddBehavior(new AuthBehavior(auth));
        client.AddBehavior(new RequestCompressionBehavior());
        client.AddBehavior(new RetryBehavior());               // retries 429/5xx/408
        client.AddBehavior(new LoggingBehavior(EnvironmentProvider.Instance));

        // Public read, cached
        await client.Send("GET", "/v1/catalog/items")
                   .EnableCache(ttlSeconds: 120, swr: true)
                   .AsResultAsync<object>();

        // Secure write, idempotent, custom rate limit bucket, no retry
        await client.Send("POST", "/v1/orders")
                   .RequireAuth()
                   .WithIdempotencyKey()
                   .WithRateLimitKey("writes")
                   .NoRetry()
                   .AsResultAsync<object>();
    }

    public async void MockTest() 
    {
        var env = Flowcast.Core.Environments.EnvironmentProvider.Instance;

        var serializers = new SerializerRegistry(new UnityJsonSerializer());
        var cache = new MemoryCacheProvider();

        var mock = new MockTransport()
            .WhenText("GET", "https://api.dev.example.com/ping", "pong", 200, latencyMs: 50);

        var client = new RestClient(
            transport: mock,
            serializers: new SerializerRegistry(new UnityJsonSerializer()),
            options: options
        );

        client.AddBehavior(new CacheBehavior(cache));
        // (AuthBehavior can be added too; it will just set headers—mock ignores them)
        client.AddBehavior(new RetryBehavior());      // retries still run; mock just returns quickly
        client.AddBehavior(new LoggingBehavior(env));

        var res = await client.Send("GET", "https://api.dev.example.com/ping").AsResultAsync<string>();
        Debug.Assert(res.IsSuccess && res.Value == "pong");
    }

    public async void T1() 
    {
        // easy static:
        var r1 = await FlowcastRest.GetAsync<object>("/v1/profile");

        // or full control (per-request policy gating still applies):
        var r2 = await FlowcastRest
                   .Send("GET", "/v1/catalog")
                   .EnableCache(ttlSeconds: 120, swr: true)
                   .AsResultAsync<object>();
    }
}
