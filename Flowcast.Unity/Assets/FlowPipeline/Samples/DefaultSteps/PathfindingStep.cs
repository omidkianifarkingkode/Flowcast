using Flowcast.FlowPipeline;

namespace FlowPipeline
{
    public class PathfindingStep : FlowStep<IPathfindable, SimulationContext>
    {
        public override void Process(SimulationContext context)
        {
            foreach (var unit in _entities)
            {
                if (unit.NeedsNewPath(context))
                    unit.CalculatePath();
            }
        }
    }
}
