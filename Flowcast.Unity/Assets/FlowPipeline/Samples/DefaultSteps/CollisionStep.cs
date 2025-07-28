using Flowcast.FlowPipeline;

namespace FlowPipeline
{
    public class CollisionStep : FlowStep<ICollidable, SimulationContext>
    {
        public override void Process(SimulationContext context)
        {
            foreach (var unit in _entities)
            {
                unit.CheckCollision(context);
            }
        }
    }
}
