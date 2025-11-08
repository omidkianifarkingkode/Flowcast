// Runtime/Rest/Client/ApiRequest.cs
using System;
using System.Collections.Generic;
using Flowcast.Core.Common;

namespace Flowcast.Rest.Client
{
    public sealed class ApiRequest
    {
        public string Method { get; set; }   // "GET", "POST", ...
        public Uri Url { get; set; }         // absolute (built using env.BaseUrl + path)
        public Headers Headers { get; } = new Headers();
        public byte[] BodyBytes { get; set; } // optional
        public string MediaType { get; set; } // Content-Type for request
        public int? TimeoutSeconds { get; set; }

        public void SetHeader(string name, string value) => Headers.Set(name, value);
    }
}
