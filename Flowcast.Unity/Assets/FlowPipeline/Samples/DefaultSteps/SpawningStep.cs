using FixedMathSharp;

namespace FlowPipeline
{
    public class SpawningStep : FlowStep<ISpawnable>
    {
        public override void Process(ulong frame, Fixed64 deltaTime)
        {
            foreach (var unit in _units)
            {
                if (unit.ShouldSpawn(frame, deltaTime))
                {
                    unit.OnSpawned(frame, deltaTime);
                }
            }
        }
    }
}
