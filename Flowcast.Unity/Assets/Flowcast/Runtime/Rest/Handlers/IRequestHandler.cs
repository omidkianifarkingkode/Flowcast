// Runtime/Rest/Handlers/IRequestHandler.cs
using System.Threading;
using System.Threading.Tasks;
using Flowcast.Core.Common;
using Flowcast.Rest.Client;

namespace Flowcast.Rest.Handlers
{
    public interface IRequestHandler<TResponse>
    {
        /// Build the ApiRequest (you can use env to build URLs if needed).
        ApiRequest Build();
        /// Map ApiResponse -> Result<TResponse> (you can parse or use RawResponse).
        Task<Result<TResponse>> MapAsync(ApiResponse response, CancellationToken ct);
    }
}
