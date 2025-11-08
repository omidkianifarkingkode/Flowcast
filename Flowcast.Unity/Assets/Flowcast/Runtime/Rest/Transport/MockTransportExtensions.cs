// Runtime/Rest/Transport/MockTransportExtensions.cs
using System.Text;
using Flowcast.Rest.Transport;

namespace Flowcast.Rest.Transport
{
    public static class MockTransportExtensions
    {
        public static MockTransport WhenJson(this MockTransport mt, string method, string absoluteUrl, string json, int status = 200, int latencyMs = 0)
            => mt.When(method, absoluteUrl, MockResponse.Json(json, status, latencyMs));

        public static MockTransport WhenText(this MockTransport mt, string method, string absoluteUrl, string text, int status = 200, int latencyMs = 0)
            => mt.When(method, absoluteUrl, MockResponse.Text(text, status, latencyMs));

        public static MockTransport WhenBytes(this MockTransport mt, string method, string absoluteUrl, byte[] bytes, string mediaType = "application/octet-stream", int status = 200, int latencyMs = 0)
            => mt.When(method, absoluteUrl, MockResponse.Bytes(bytes, mediaType, status, latencyMs));

        public static MockTransport WhenJsonSequence(this MockTransport mt, string method, string absoluteUrl, params string[] jsonSequence)
        {
            var list = new System.Collections.Generic.List<MockResponse>();
            foreach (var j in jsonSequence) list.Add(MockResponse.Json(j));
            return mt.When(method, absoluteUrl, list);
        }
    }
}
