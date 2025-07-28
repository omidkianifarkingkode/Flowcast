using Flowcast.FlowPipeline;

namespace FlowPipeline
{
    public interface IPathfindable
    {
        bool NeedsNewPath(SimulationContext context);
        void CalculatePath();
    }

}
