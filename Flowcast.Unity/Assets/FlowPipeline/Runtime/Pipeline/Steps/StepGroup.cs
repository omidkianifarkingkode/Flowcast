using FixedMathSharp;
using System.Collections.Generic;

namespace FlowPipeline
{
    public class StepGroup<T> : IFlowStep<T>
    {
        private readonly List<IFlowStep> _subSteps = new();

        public void AddSubStep(IFlowStep step) => _subSteps.Add(step);

        public void Process(ulong frame, Fixed64 deltaTime)
        {
            foreach (var step in _subSteps)
                step.Process(frame, deltaTime);
        }

        public void Add(T unit)
        {
            foreach (var step in _subSteps)
                if (step is IFlowStep<T> typed)
                    typed.Add(unit);
        }

        public void Remove(T unit)
        {
            foreach (var step in _subSteps)
                if (step is IFlowStep<T> typed)
                    typed.Remove(unit);
        }

        public void Clear()
        {
            foreach (var step in _subSteps)
                if (step is IFlowStep<T> typed)
                    typed.Clear();
        }
    }

}
