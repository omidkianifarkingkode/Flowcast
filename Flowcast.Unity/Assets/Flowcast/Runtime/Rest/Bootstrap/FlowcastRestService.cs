// Runtime/Rest/Bootstrap/FlowcastRestBootstrapper.cs
using Flowcast.Rest.Client;

namespace Flowcast.Rest.Bootstrap
{
    /// <summary> Default implementation wrapping RestClient. </summary>
    public sealed class FlowcastRestService : IFlowcastRest
    {
        private readonly RestClient _client;
        public FlowcastRestService(RestClient client) { _client = client; }

        public RestClient.RequestBuilder Send(string method, string pathOrAbsoluteUrl)
            => _client.Send(method, pathOrAbsoluteUrl);

        public System.Threading.Tasks.Task<Flowcast.Core.Common.Result<T>> GetAsync<T>(string pathOrUrl)
            => _client.Send("GET", pathOrUrl).AsResultAsync<T>();

        public System.Threading.Tasks.Task<Flowcast.Core.Common.Result<T>> PostAsync<TReq, T>(string pathOrUrl, TReq body, string contentType = "application/json")
            => _client.Send("POST", pathOrUrl).WithBody(body, contentType).AsResultAsync<T>();
    }
}
