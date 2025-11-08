// Runtime/Rest/Pipeline/IPipelineBehavior.cs
using Flowcast.Rest.Client;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;

namespace Flowcast.Rest.Pipeline
{
    public delegate Task<ApiResponse> PipelineNext(ApiRequest req, CancellationToken ct);

    public interface IPipelineBehavior
    {
        Task<ApiResponse> HandleAsync(ApiRequest req, CancellationToken ct, PipelineNext next);
    }
}
