// Runtime/Rest/Pipeline/RequestCompressionBehavior.cs
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Flowcast.Rest.Client;

namespace Flowcast.Rest.Pipeline
{
    public sealed class RequestCompressionBehavior : IPipelineBehavior
    {
        private readonly int _thresholdBytes;
        private readonly bool _always;

        public RequestCompressionBehavior(int thresholdBytes = 8 * 1024, bool always = false)
        { _thresholdBytes = thresholdBytes; _always = always; }

        public async Task<ApiResponse> HandleAsync(ApiRequest req, CancellationToken ct, PipelineNext next)
        {
            if (req.Policy == null || !req.Policy.Features.Has(RequestFeatures.CompressRequest))
                return await next(req, ct).ConfigureAwait(false);

            var always = req.Policy.CompressAlways ?? _always;

            var bytes = req.BodyBytes;
            if (bytes != null && bytes.Length > 0 && (always || bytes.Length >= _thresholdBytes))
            {
                using var ms = new MemoryStream();
                using (var gzip = new GZipStream(ms, CompressionLevel.Fastest, true))
                    gzip.Write(bytes, 0, bytes.Length);
                req.BodyBytes = ms.ToArray();
                req.SetHeader("Content-Encoding", "gzip");
                // (Content-Type remains whatever it was)
            }
            return await next(req, ct);
        }
    }
}
