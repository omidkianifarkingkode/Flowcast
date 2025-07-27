using FixedMathSharp;

namespace FlowPipeline
{
    public class PathfindingStep : FlowStep<IPathfindable>
    {
        public override void Process(ulong tick, Fixed64 delta)
        {
            foreach (var unit in _units)
            {
                if (unit.NeedsNewPath(tick, delta))
                    unit.CalculatePath();
            }
        }
    }
}
