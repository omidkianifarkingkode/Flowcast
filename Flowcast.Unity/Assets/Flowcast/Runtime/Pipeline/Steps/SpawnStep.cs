using FixedMathSharp;
using System.Collections.Generic;

namespace Flowcast.Pipeline
{
    public class SpawnStep : ISimulationStep<ISpawnable>
    {
        private readonly List<ISpawnable> _units = new();

        public void Add(ISpawnable unit) => _units.Add(unit);
        public void Remove(ISpawnable unit) => _units.Remove(unit);
        public void Clear() => _units.Clear();

        public void ProcessFrame(ulong frame, Fixed64 deltaTime)
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
