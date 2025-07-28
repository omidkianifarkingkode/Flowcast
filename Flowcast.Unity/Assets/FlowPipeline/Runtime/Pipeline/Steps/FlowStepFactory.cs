using System;

namespace FlowPipeline
{
    public static class FlowStepFactory
    {
        public static IFlowStep<TEntity, TContext> CreateStep<TEntity, TContext>(Action<TEntity, TContext> logic)
            where TContext : struct
            => new FlowStep<TEntity, TContext>(logic);
    }
}
