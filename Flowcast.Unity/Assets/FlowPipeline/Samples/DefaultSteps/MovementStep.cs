using FixedMathSharp;

namespace FlowPipeline
{
    public class MovementStep : FlowStep<IMovable>
    {
        public override void Process(ulong tick, Fixed64 delta)
        {
            foreach (var unit in _units)
            {
                unit.Move(tick, delta);
            }
        }
    }
}
