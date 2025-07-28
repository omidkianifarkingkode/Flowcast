using Flowcast.FlowPipeline;

namespace FlowPipeline
{
    public interface ICollidable
    {
        void CheckCollision(SimulationContext context);
    }
}
