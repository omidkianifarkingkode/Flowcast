using UnityEngine;

namespace FlowPipeline
{
    public class FlowPipelineBuilder<TContext> : MonoBehaviour
        where TContext : struct
    {
        public IFlowPipeline<TContext> Pipeline { get; private set; }

        private void Awake()
        {
            var asset = PipelineOptionsAsset.Load();

            var steps = asset.GetSteps<TContext>();

            Pipeline = new FlowPipeline<TContext>(steps);
        }
    }
}
