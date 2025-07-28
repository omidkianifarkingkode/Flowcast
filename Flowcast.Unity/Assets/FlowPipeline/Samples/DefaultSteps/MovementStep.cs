using Flowcast.FlowPipeline;

namespace FlowPipeline
{
    public class MovementStep : FlowStep<IMovable, SimulationContext>
    {
        public override void Process(SimulationContext context)
        {
            foreach (var unit in _entities)
            {
                unit.Move(context);
            }
        }
    }
}
