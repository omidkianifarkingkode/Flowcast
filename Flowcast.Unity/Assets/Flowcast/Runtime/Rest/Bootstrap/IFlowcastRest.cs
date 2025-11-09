// Runtime/Rest/Bootstrap/FlowcastRestBootstrapper.cs
using Flowcast.Core.Common;
using Flowcast.Rest.Client;
using System.Threading.Tasks;

namespace Flowcast.Rest.Bootstrap
{
    // ---------------- Public sending facade + singleton ----------------

    public interface IFlowcastRest
    {
        // full control (fluent builder)
        RestClient.RequestBuilder Send(string method, string pathOrAbsoluteUrl);

        // convenience
        Task<Result<T>> GetAsync<T>(string pathOrUrl);
        Task<Result<T>> PostAsync<TReq, T>(string pathOrUrl, TReq body, string contentType = "application/json");
    }
}
