using System.Collections.Generic;

namespace Flowcast.Pipeline
{
    public class PathfindingStep : ISimulationStep<IPathfindable>
    {
        private readonly List<IPathfindable> _units = new();

        public void Add(IPathfindable unit) => _units.Add(unit);
        public void Remove(IPathfindable unit) => _units.Remove(unit);
        public void Clear() => _units.Clear();

        public void ProcessFrame(ulong frameNumber)
        {
            foreach (var unit in _units)
            {
                if (unit.NeedsNewPath(frameNumber))
                {
                    unit.CalculatePath();
                }
            }
        }
    }
}
