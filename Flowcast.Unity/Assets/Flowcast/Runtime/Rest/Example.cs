using UnityEngine;
using Flowcast.Rest;
using Flowcast.Rest.Core;
using Flowcast.Rest.Transport;
using Flowcast.Rest.Pipeline;
using Flowcast.Rest.Attributes;

public class Example : MonoBehaviour
{
    private FlowcastRest rest;

    async void Start()
    {
        // Setup SDK
        rest = new FlowcastRest(
            baseUrl: "https://api.flowcast.io",
            transport: new UnityWebRequestTransport(),
            serializer: new JsonSerializer()
        )
        .Use(new LoggingMiddleware())
        .Use(new RetryMiddleware(2));

        // Attribute-driven request
        var result = await rest.Send(new GetUserRequest { Id = 42 });

        if (result.IsSuccess)
            Debug.Log($"User: {result.Value.Name}");
        else
            Debug.LogError($"Error: {result.Error.Code}");
    }
}

[Get("/users/{Id}")]
[Cache(20)]
[RequireAuth]
public class GetUserRequest : IRequest<User>
{
    public int Id { get; set; }
}

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
}
