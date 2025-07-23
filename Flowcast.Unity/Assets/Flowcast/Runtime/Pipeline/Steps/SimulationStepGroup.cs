using FixedMathSharp;
using System.Collections.Generic;

namespace Flowcast.Pipeline
{
    public class SimulationStepGroup<T> : ISimulationStep<T>
    {
        private readonly List<T> _units = new();
        private readonly List<ISimulationStep> _subSteps = new();

        public void AddSubStep(ISimulationStep step) => _subSteps.Add(step);

        public void ProcessFrame(ulong frame, Fixed64 deltaTime)
        {
            foreach (var step in _subSteps)
                step.ProcessFrame(frame, deltaTime);
        }

        public void Add(T unit)
        {
            _units.Add(unit);
        }

        public void Remove(T unit)
        {
            _units.Remove(unit);
        }

        public void Clear()
        {
            _units.Clear();
        }
    }

}
