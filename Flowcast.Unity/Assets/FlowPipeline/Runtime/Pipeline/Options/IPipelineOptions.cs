using System.Collections.Generic;

namespace FlowPipeline
{
    public interface IPipelineOptions
    {
        IEnumerable<IFlowStep<TContext>> GetSteps<TContext>() where TContext : struct;
    }


}
