// Runtime/Rest/Bootstrap/FlowcastRestBootstrapper.cs
using Flowcast.Rest.Client;

namespace Flowcast.Rest.Bootstrap
{
    /// <summary> Simple static access for now; DI containers can bind IFlowcastRest to FlowcastRest.Instance later. </summary>
    public static class FlowcastRest
    {
        public static IFlowcastRest Instance { get; private set; }
        internal static void SetInstance(IFlowcastRest svc) => Instance = svc;

        // Shorthand helpers
        public static RestClient.RequestBuilder Send(string method, string pathOrAbsoluteUrl)
            => Instance.Send(method, pathOrAbsoluteUrl);

        public static System.Threading.Tasks.Task<Flowcast.Core.Common.Result<T>> GetAsync<T>(string pathOrUrl)
            => Instance.GetAsync<T>(pathOrUrl);

        public static System.Threading.Tasks.Task<Flowcast.Core.Common.Result<T>> PostAsync<TReq, T>(string pathOrUrl, TReq body, string contentType = "application/json")
            => Instance.PostAsync<TReq, T>(pathOrUrl, body, contentType);
    }
}
