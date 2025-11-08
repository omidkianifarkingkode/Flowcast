using Flowcast.Core.Cache;
using Flowcast.Core.Serialization;
using Flowcast.Rest.Client;
using Flowcast.Rest.Pipeline;
using Flowcast.Rest.Transport;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    async void Start()
    {
        // 1) Core services
        var env = Flowcast.Core.Environments.EnvironmentProvider.Instance;
        var auth = new Flowcast.Core.Auth.SimpleBearerAuthProvider();
        auth.SetToken("<access_token>");

        var refreshingAuth = new Flowcast.Core.Auth.SimpleRefreshingAuthProvider(
            refreshFunc: async ct =>
            {
                // TODO: call your refresh endpoint here and return the NEW access token string
                // If refresh fails, return null or empty string.
                await Task.Yield();
                return "<new_access_token>";
            },
            initialToken: "<optional_initial_access_token>"
        );

        var serializers = new SerializerRegistry(new UnityJsonSerializer());
        var cache = new MemoryCacheProvider(maxEntries: 256);

        // 2) Rest client
        var client = new RestClient(envProvider: env, serializers: serializers, auth: auth, addDefaultLogging: true);

        // 3) Behaviors (order matters): Cache → Retry → (Transport)
        client.AddBehavior(new CacheBehavior(cache));
        client.AddBehavior(new AuthBehavior(refreshingAuth));
        client.AddBehavior(new RetryBehavior());
        client.AddBehavior(new LoggingBehavior(env));

        // Typed
        var profile = await client.Send("GET", "/v1/profile")
                                  .WithHeader("Accept", "application/json")
                                  .AsResultAsync<object>();

        // Raw
        var raw = await client.Send("GET", "https://httpbin.org/bytes/16")
                              .AsRawAsync();


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
            serializers: new SerializerRegistry(new UnityJsonSerializer())
        );

        client.AddBehavior(new CacheBehavior(cache));
        // (AuthBehavior can be added too; it will just set headers—mock ignores them)
        client.AddBehavior(new RetryBehavior());      // retries still run; mock just returns quickly
        client.AddBehavior(new LoggingBehavior(env));

        var res = await client.Send("GET", "https://api.dev.example.com/ping").AsResultAsync<string>();
        Debug.Assert(res.IsSuccess && res.Value == "pong");


        
    }
}
