using Flowcast.FlowPipeline;

namespace FlowPipeline
{
    public class LogicStep : FlowStep<ILogicUnit, SimulationContext>
    {
        public override void Process(SimulationContext context)
        {
            foreach (var unit in _entities)
            {
                unit.ExecuteLogic(context);
            }
        }
    }
}
