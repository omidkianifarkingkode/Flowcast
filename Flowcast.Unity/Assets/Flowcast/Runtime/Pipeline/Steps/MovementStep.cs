using FixedMathSharp;
using System.Collections.Generic;

namespace Flowcast.Pipeline
{
    public class MovementStep : ISimulationStep<IMovable>
    {
        private readonly List<IMovable> _units = new();

        public void Add(IMovable unit) => _units.Add(unit);
        public void Remove(IMovable unit) => _units.Remove(unit);
        public void Clear() => _units.Clear();

        public void ProcessFrame(ulong frame, Fixed64 deltaTime)
        {
            foreach (var unit in _units)
            {
                unit.Move(frame, deltaTime);
            }
        }
    }
}
