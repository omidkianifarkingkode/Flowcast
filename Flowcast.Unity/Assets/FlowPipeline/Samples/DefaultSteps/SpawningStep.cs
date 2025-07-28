using Flowcast.FlowPipeline;

namespace FlowPipeline
{
    public class SpawningStep : FlowStep<ISpawnable, SimulationContext>
    {
        public override void Process(SimulationContext context)
        {
            foreach (var unit in _entities)
            {
                if (unit.ShouldSpawn(context))
                {
                    unit.OnSpawned(context);
                }
            }
        }
    }
}
