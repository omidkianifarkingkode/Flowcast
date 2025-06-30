using System.Collections.Generic;

namespace Flowcast.Pipeline
{
    public class LogicStep : ISimulationStep<ILogicUnit>
    {
        private readonly List<ILogicUnit> _units = new();

        public void Add(ILogicUnit unit) => _units.Add(unit);
        public void Remove(ILogicUnit unit) => _units.Remove(unit);
        public void Clear() => _units.Clear();

        public void ProcessFrame(ulong frameNumber)
        {
            foreach (var unit in _units)
            {
                unit.ExecuteLogic(frameNumber);
            }
        }
    }
}
