using FixedMathSharp;
using System.Collections.Generic;

namespace Flowcast.Pipeline
{
    public class CollisionStep : ISimulationStep<ICollidable>
    {
        private readonly List<ICollidable> _units = new();

        public void Add(ICollidable unit) => _units.Add(unit);
        public void Remove(ICollidable unit) => _units.Remove(unit);
        public void Clear() => _units.Clear();

        public void ProcessFrame(ulong frame, Fixed64 deltaTime)
        {
            foreach (var unit in _units)
            {
                unit.CheckCollision(frame, deltaTime);
            }
        }
    }
}
