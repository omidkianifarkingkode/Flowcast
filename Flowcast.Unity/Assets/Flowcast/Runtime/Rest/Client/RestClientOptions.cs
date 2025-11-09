// Runtime/Rest/Client/RestClientOptions.cs
using System;

namespace Flowcast.Rest.Client
{
    public sealed class RestClientOptions
    {
        public RequestPolicy DefaultPolicy { get; set; } = RequestPolicy.Default;

        /// <summary>
        /// Optional hook to adjust policy per request (by URL/method, etc.).
        /// Called right after the RequestBuilder is created, before fluent overrides.
        /// </summary>
        public Func<ApiRequest, RequestPolicy, RequestPolicy> PolicySelector { get; set; }
    }
}
