using System.Collections.Generic;

namespace Flowcast.Pipeline
{
    public class SpawnStep : ISimulationStep<ISpawnable>
    {
        private readonly List<ISpawnable> _units = new();

        public void Add(ISpawnable unit) => _units.Add(unit);
        public void Remove(ISpawnable unit) => _units.Remove(unit);
        public void Clear() => _units.Clear();

        public void ProcessFrame(ulong frameNumber)
        {
            foreach (var unit in _units)
            {
                if (unit.ShouldSpawn(frameNumber))
                {
                    unit.OnSpawned(frameNumber);
                }
            }
        }
    }
}
