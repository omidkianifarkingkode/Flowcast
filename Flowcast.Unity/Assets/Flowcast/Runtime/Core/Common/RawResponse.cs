// Runtime/Core/Common/Result/RawResponse.cs
namespace Flowcast.Core.Common
{
    public sealed class RawResponse
    {
        public int Status { get; internal set; }

        public string MediaType { get; internal set; }
        public byte[] BodyBytes { get; internal set; }
        public string BodyText { get; internal set; } // lazily filled when text
        public Headers Headers { get; internal set; } = new Headers();

        public RawResponse(int status, string mediaType, byte[] bodyBytes, Headers headers)
        {
            Status = status;
            MediaType = mediaType;
            BodyBytes = bodyBytes;
            Headers = headers;
        }

        public RawResponse(int status, string mediaType, string bodyText, Headers headers)
        {
            Status = status;
            MediaType = mediaType;
            BodyText = bodyText;
            Headers = headers;
        }

        public override string ToString() => $"Status={Status}, MediaType={MediaType}, Bytes={(BodyBytes?.Length ?? 0)}";
    }
}
