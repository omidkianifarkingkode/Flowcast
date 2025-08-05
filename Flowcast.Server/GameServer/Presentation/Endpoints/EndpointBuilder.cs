using Presentation.Infrastructure;
using SharedKernel;

namespace Presentation.Endpoints;

public class EndpointBuilder<TRequest, TResponse>
{
    private readonly WebApplication _app;
    private readonly string _method;
    private readonly string _route;

    public EndpointBuilder(WebApplication app, string method, string route)
    {
        _app = app;
        _method = method.ToUpperInvariant();
        _route = route;
    }

    public void Handle(Func<TRequest, CancellationToken, Task<Result<TResponse>>> handler)
    {
        // We register it directly so DI can still inject other parameters automatically
        switch (_method)
        {
            case "POST":
                _app.MapPost(_route, handler);
                break;
            case "GET":
                _app.MapGet(_route, handler);
                break;
            case "PUT":
                _app.MapPut(_route, handler);
                break;
            case "DELETE":
                _app.MapDelete(_route, handler);
                break;
            default:
                throw new NotSupportedException($"HTTP method '{_method}' is not supported.");
        }
    }
}


public class RequestStage
{
    private readonly WebApplication _app;
    private readonly string _method;
    private readonly string _route;

    public RequestStage(WebApplication app, string method, string route)
    {
        _app = app;
        _method = method;
        _route = route;
    }

    public ResponseStage<TRequest> WithRequest<TRequest>()
        => new(_app, _method, _route);
}

public class ResponseStage<TRequest>
{
    private readonly WebApplication _app;
    private readonly string _method;
    private readonly string _route;

    public ResponseStage(WebApplication app, string method, string route)
    {
        _app = app;
        _method = method;
        _route = route;
    }

    public EndpointBuilder<TRequest, TResponse> WithResponse<TResponse>()
        => new(_app, _method, _route);
}



public static class WebApplicationExtensions
{
    public static RequestStage Map(this WebApplication app, string method, string route)
        => new(app, method, route);
}



