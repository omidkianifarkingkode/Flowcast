using System.Collections.Generic;

namespace FlowPipeline
{
    public class StepGroup<TEntity, TContext> : IFlowStep<TEntity, TContext>
        where TContext : struct
    {
        private readonly List<IFlowStep<TContext>> _subSteps = new();

        public void AddSubStep(IFlowStep<TContext> step) => _subSteps.Add(step);

        public void Process(TContext context)
        {
            foreach (var step in _subSteps)
                step.Process(context);
        }

        public void Add(TEntity entity)
        {
            foreach (var step in _subSteps)
                if (step is IFlowStep<TEntity, TContext> typed)
                    typed.Add(entity);
        }

        public void Remove(TEntity entity)
        {
            foreach (var step in _subSteps)
                if (step is IFlowStep<TEntity, TContext> typed)
                    typed.Remove(entity);
        }

        public void Clear()
        {
            foreach (var step in _subSteps)
                if (step is IFlowStep<TEntity, TContext> typed)
                    typed.Clear();
        }
    }

}
