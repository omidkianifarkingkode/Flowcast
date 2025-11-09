// Runtime/Rest/Pipeline/ResponseDecompressionBehavior.cs
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Flowcast.Rest.Client;

namespace Flowcast.Rest.Pipeline
{
    public sealed class ResponseDecompressionBehavior : IPipelineBehavior
    {
        public async Task<ApiResponse> HandleAsync(ApiRequest req, CancellationToken ct, PipelineNext next)
        {
            var resp = await next(req, ct).ConfigureAwait(false);

            if (req.Policy == null || !req.Policy.Features.Has(RequestFeatures.DecompressResp))
                return resp;

            if (!resp.Headers.TryGet("Content-Encoding", out var enc) || resp.BodyBytes == null) return resp;

            var lower = enc.ToLowerInvariant();
            if (lower.Contains("gzip"))
            {
                using var ms = new MemoryStream(resp.BodyBytes);
                using var gz = new GZipStream(ms, CompressionMode.Decompress);
                using var outMs = new MemoryStream();
                gz.CopyTo(outMs);
                resp.BodyBytes = outMs.ToArray();
                resp.Headers.Set("Content-Encoding", "identity");
            }
            else if (lower.Contains("deflate"))
            {
                using var ms = new MemoryStream(resp.BodyBytes);
                using var df = new DeflateStream(ms, CompressionMode.Decompress);
                using var outMs = new MemoryStream();
                df.CopyTo(outMs);
                resp.BodyBytes = outMs.ToArray();
                resp.Headers.Set("Content-Encoding", "identity");
            }
            return resp;
        }
    }
}
