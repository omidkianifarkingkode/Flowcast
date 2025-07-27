using FixedMathSharp;
using System.Collections.Generic;
using System;

namespace FlowPipeline
{
    public class FlowStep<T> : IFlowStep<T>
    {
        protected readonly List<T> _units = new();
        protected readonly Action<T, ulong, Fixed64> _processUnit;

        public FlowStep() { }

        public FlowStep(Action<T, ulong, Fixed64> processUnit)
        {
            _processUnit = processUnit ?? throw new ArgumentNullException(nameof(processUnit));
        }

        public virtual void Add(T unit) => _units.Add(unit);
        public virtual void Remove(T unit) => _units.Remove(unit);
        public virtual void Clear() => _units.Clear();

        public virtual void Process(ulong tick, Fixed64 delta)
        {
            foreach (var unit in _units)
            {
                _processUnit(unit, tick, delta);
            }
        }
    }

}
