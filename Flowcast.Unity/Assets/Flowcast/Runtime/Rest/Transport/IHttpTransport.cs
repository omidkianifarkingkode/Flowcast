using Cysharp.Threading.Tasks;

namespace Flowcast.Rest.Transport
{
    public interface IHttpTransport
    {
        UniTask<HttpResponse> SendAsync(HttpRequest request);
    }

    public class UnityWebRequestTransport : IHttpTransport
    {
        public async UniTask<HttpResponse> SendAsync(HttpRequest request)
        {
            // TODO: implement real UnityWebRequest logic
            await UniTask.Delay(10);
            return new HttpResponse { StatusCode = 200, Body = "{}" };
        }
    }
}
