using Flowcast.FlowPipeline;

namespace FlowPipeline
{
    public interface ISpawnable
    {
        bool ShouldSpawn(SimulationContext context);
        void OnSpawned(SimulationContext context);
    }

}
