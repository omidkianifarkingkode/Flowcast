// Runtime/Rest/Transport/ITransport.cs
using Flowcast.Rest.Client;
using System.Threading.Tasks;

namespace Flowcast.Rest.Transport
{
    public interface ITransport
    {
        Task<ApiResponse> SendAsync(ApiRequest req, System.Threading.CancellationToken ct);
    }
}
