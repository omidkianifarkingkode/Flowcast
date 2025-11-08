// Runtime/Rest/Transport/UnityWebRequestTransport.cs  (add timeout line)
using System.Threading;
using System.Threading.Tasks;
using Flowcast.Rest.Client;
using UnityEngine.Networking;

namespace Flowcast.Rest.Transport
{
    public sealed class UnityWebRequestTransport : ITransport
    {
        public async Task<ApiResponse> SendAsync(ApiRequest req, CancellationToken ct)
        {
            var uwr = new UnityWebRequest(req.Url, req.Method);
            if (req.TimeoutSeconds.HasValue && req.TimeoutSeconds.Value > 0)
                uwr.timeout = req.TimeoutSeconds.Value;

            if (req.BodyBytes != null && req.BodyBytes.Length > 0)
            {
                uwr.uploadHandler = new UploadHandlerRaw(req.BodyBytes);
                if (!string.IsNullOrEmpty(req.MediaType))
                    uwr.SetRequestHeader("Content-Type", req.MediaType);
            }
            uwr.downloadHandler = new DownloadHandlerBuffer();

            foreach (var kv in req.Headers.Pairs)
                uwr.SetRequestHeader(kv.Key, kv.Value);

            var op = uwr.SendWebRequest();
            while (!op.isDone && !ct.IsCancellationRequested) await Task.Yield();
            if (ct.IsCancellationRequested) uwr.Abort();

            var resp = new ApiResponse
            {
                Status = (int)uwr.responseCode,
                BodyBytes = uwr.downloadHandler?.data,
                MediaType = uwr.GetResponseHeader("Content-Type") ?? "",
            };

            var headers = uwr.GetResponseHeaders();
            if (headers != null)
                foreach (var h in headers) resp.Headers.Set(h.Key, h.Value);

            uwr.Dispose();
            return resp;
        }
    }
}
