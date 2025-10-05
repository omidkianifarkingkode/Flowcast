using Cysharp.Threading.Tasks;
using Flowcast.Rest.Core;
using Flowcast.Rest.Transport;
using System.Collections.Generic;

namespace Flowcast.Rest.Pipeline
{
    public delegate UniTask<Result<HttpResponse>> NextMiddleware(HttpRequest request);

    public interface IRequestMiddleware
    {
        UniTask<Result<HttpResponse>> InvokeAsync(HttpRequest request, NextMiddleware next);
    }

    public class RequestPipeline
    {
        private readonly List<IRequestMiddleware> _middlewares = new();
        private readonly IHttpTransport _transport;

        public RequestPipeline(IHttpTransport transport)
        {
            _transport = transport;
        }

        public RequestPipeline Use(IRequestMiddleware middleware)
        {
            _middlewares.Add(middleware);
            return this;
        }

        public async UniTask<Result<HttpResponse>> ExecuteAsync(HttpRequest request)
        {
            int index = -1;

            async UniTask<Result<HttpResponse>> Next(HttpRequest req)
            {
                index++;
                if (index < _middlewares.Count)
                    return await _middlewares[index].InvokeAsync(req, Next);

                try
                {
                    var resp = await _transport.SendAsync(req);
                    return Result<HttpResponse>.Success(resp);
                }
                catch (System.Exception ex)
                {
                    return Result<HttpResponse>.Fail(new Error
                    {
                        Code = "TransportError",
                        Message = ex.Message,
                        Details = ex.ToString()
                    });
                }
            }

            return await Next(request);
        }
    }
}
