using FixedMathSharp;

namespace FlowPipeline
{
    public class LogicStep : FlowStep<ILogicUnit>
    {
        public override void Process(ulong tick, Fixed64 deltaTime)
        {
            foreach (var unit in _units)
            {
                unit.ExecuteLogic(tick, deltaTime);
            }
        }
    }
}
