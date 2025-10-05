using System;
using System.Collections.Generic;

namespace Flowcast.Rest.Transport
{
    public class HttpRequest
    {
        public string Url { get; set; }
        public string Method { get; set; } = "GET";
        public Dictionary<string, string> Headers { get; set; } = new();
        public string Body { get; set; }
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
        public object Payload { get; set; } // raw DTO before serialization

        public Dictionary<string, object> Metadata { get; } = new();
    }

    public class HttpResponse
    {
        public int StatusCode { get; set; }
        public Dictionary<string, string> Headers { get; set; } = new();
        public string Body { get; set; }
        public bool IsSuccess => StatusCode >= 200 && StatusCode < 300;
    }
}
