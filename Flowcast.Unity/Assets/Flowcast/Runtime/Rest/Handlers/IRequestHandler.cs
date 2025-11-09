// Runtime/Rest/Client/IRequestHandler.cs
using System.Threading;
using System.Threading.Tasks;
using Flowcast.Core.Common;

namespace Flowcast.Rest.Client
{
    public interface IRequestHandler<TResponse>
    {
        /// Build a complete ApiRequest (absolute URL recommended).
        ApiRequest Build();

        /// Map the low-level ApiResponse to a domain Result<TResponse>.
        Task<Result<TResponse>> MapAsync(ApiResponse response, CancellationToken ct);
    }
}
