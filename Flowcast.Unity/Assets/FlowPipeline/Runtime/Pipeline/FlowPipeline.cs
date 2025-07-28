using System.Collections.Generic;
using System.Linq;

namespace FlowPipeline
{
    /// <summary>
    /// Deterministic execution pipeline that processes game logic in a fixed order.
    /// Each handler type (movement, pathfinding, etc.) is processed in a hardcoded sequence.
    /// </summary>
    public interface IFlowPipeline<TContext> where TContext : struct
    {
        bool TryGetStep<TUnit>(out IFlowStep<TUnit, TContext> step);
        public void ProcessFrame(TContext context);
    }

    public class FlowPipeline<TContext> : IFlowPipeline<TContext> where TContext : struct
    {
        private readonly List<IFlowStep<TContext>> _steps = new();

        public FlowPipeline(IEnumerable<IFlowStep<TContext>> steps)
        {
            _steps = steps.ToList();
        }

        public bool TryGetStep<TEntity>(out IFlowStep<TEntity, TContext> step)
        {
            foreach (var s in _steps)
            {
                if (s is IFlowStep<TEntity, TContext> typed)
                {
                    step = typed;
                    return true;
                }
            }
            step = default;
            return false;
        }

        public void ProcessFrame(TContext contenxt)
        {
            foreach (var system in _steps)
                system.Process(contenxt);
        }
    }
}
