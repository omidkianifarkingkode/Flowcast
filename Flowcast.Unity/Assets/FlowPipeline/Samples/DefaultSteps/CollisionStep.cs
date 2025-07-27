using FixedMathSharp;

namespace FlowPipeline
{
    public class CollisionStep : FlowStep<ICollidable>
    {
        public override void Process(ulong tick, Fixed64 delta)
        {
            foreach (var unit in _units)
            {
                unit.CheckCollision(tick, delta);
            }
        }
    }
}
