// Runtime/Rest/Client/ApiResponse.cs
using Flowcast.Core.Common;

namespace Flowcast.Rest.Client
{
    public sealed class ApiResponse
    {
        public int Status { get; internal set; }
        public Headers Headers { get; internal set; } = new();
        public byte[] BodyBytes { get; internal set; }
        public string MediaType { get; internal set; }
        public long DurationMs { get; internal set; }
    }
}
