// Runtime/Rest/Pipeline/LoggingBehavior.cs
using Flowcast.Core.Environments;
using Flowcast.Rest.Client;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace Flowcast.Rest.Pipeline
{
    public sealed class LoggingBehavior : IPipelineBehavior
    {
        private readonly IEnvironmentProvider _env;
        public LoggingBehavior(IEnvironmentProvider env) { _env = env; }

        public async Task<ApiResponse> HandleAsync(ApiRequest req, CancellationToken ct, PipelineNext next)
        {
            if (req.Policy == null || !req.Policy.Features.Has(RequestFeatures.Logging))
                return await next(req, ct).ConfigureAwait(false);

            var enable = _env.Current != null && _env.Current.EnableLogging;
            long startedMs = 0;
            if (enable) startedMs = Stopwatch.GetTimestamp();

            var resp = await next(req, ct);

            if (enable)
            {
                var elapsedMs = (long)(1000.0 * (Stopwatch.GetTimestamp() - startedMs) / Stopwatch.Frequency);
                var sb = new StringBuilder();
                sb.Append("[Flowcast] ")
                  .Append(req.Method).Append(' ').Append(req.Url)
                  .Append(" -> ").Append(resp.Status)
                  .Append(" (").Append(elapsedMs).Append(" ms)");
                Debug.Log(sb.ToString());
            }

            return resp;
        }
    }
}
