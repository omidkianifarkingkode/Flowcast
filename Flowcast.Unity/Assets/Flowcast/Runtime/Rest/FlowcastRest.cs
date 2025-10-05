using Cysharp.Threading.Tasks;
using Flowcast.Rest.Attributes;
using Flowcast.Rest.Configuration;
using Flowcast.Rest.Core;
using Flowcast.Rest.Pipeline;
using Flowcast.Rest.Transport;
using System;
using System.Linq;
using System.Reflection;

namespace Flowcast.Rest
{
    public class FlowcastRest
    {
        private readonly RequestPipeline _pipeline;
        private readonly ISerializer _serializer;
        private readonly string _baseUrl;
        private readonly RequestConfigRegistry _configRegistry = new();

        public FlowcastRest(string baseUrl, IHttpTransport transport, ISerializer serializer)
        {
            _baseUrl = baseUrl;
            _serializer = serializer;
            _pipeline = new RequestPipeline(transport);
        }

        public FlowcastRest Use(IRequestMiddleware middleware)
        {
            _pipeline.Use(middleware);
            return this;
        }

        public FlowcastRest Configure<TRequest>(Action<RequestConfig> config)
        {
            _configRegistry.Configure<TRequest>(config);
            return this;
        }

        public async UniTask<Result<TResponse>> Send<TResponse>(IRequest<TResponse> request)
        {
            var reqType = request.GetType();

            // 1. Find route + method (attributes or config)
            var (method, route) = ResolveRoute(reqType, request);

            // 2. Build HttpRequest
            var httpReq = new HttpRequest
            {
                Url = $"{_baseUrl}{route}",
                Method = method,
                Payload = request,
                Body = _serializer.Serialize(request)
            };

            // TODO: add cache/auth metadata from attributes/config

            // 3. Execute pipeline
            var httpResult = await _pipeline.ExecuteAsync(httpReq);
            if (!httpResult.IsSuccess)
                return Result<TResponse>.Fail(httpResult.Error);

            // 4. Deserialize
            try
            {
                var data = _serializer.Deserialize<TResponse>(httpResult.Value.Body);
                return Result<TResponse>.Success(data);
            }
            catch (Exception ex)
            {
                return Result<TResponse>.Fail(new Error
                {
                    Code = "SerializationError",
                    Message = ex.Message,
                    Details = ex.ToString()
                });
            }
        }

        private (string Method, string Route) ResolveRoute(Type reqType, object request)
        {
            // Attribute check
            var getAttr = reqType.GetCustomAttribute<GetAttribute>();
            if (getAttr != null) return ("GET", ReplaceRouteParams(getAttr.Route, request));

            var postAttr = reqType.GetCustomAttribute<PostAttribute>();
            if (postAttr != null) return ("POST", ReplaceRouteParams(postAttr.Route, request));

            // Config check
            var cfg = _configRegistry.GetConfig(reqType);
            if (cfg != null) return (cfg.Method, ReplaceRouteParams(cfg.Route, request));

            throw new InvalidOperationException($"No route configuration found for {reqType.Name}");
        }

        private string ReplaceRouteParams(string route, object request)
        {
            var props = request.GetType().GetProperties();
            foreach (var prop in props)
            {
                route = route.Replace($"{{{prop.Name}}}", prop.GetValue(request)?.ToString());
            }
            return route;
        }
    }
}
