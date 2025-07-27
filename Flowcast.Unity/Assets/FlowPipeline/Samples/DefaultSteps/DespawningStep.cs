using System.Collections.Generic;
using System;
using FixedMathSharp;

namespace FlowPipeline
{
    public class DespawningStep : FlowStep<IDespawnable>
    {
        private readonly List<Action<IDespawnable>> _unregisterCallbacks = new();

        public void RegisterUnlinker<T>(IFlowStep<T> step) where T : class
        {
            _unregisterCallbacks.Add(unit =>
            {
                if (unit is T typed)
                    step.Remove(typed);
            });
        }

        public override void Process(ulong frame, Fixed64 deltaTime)
        {
            for (int i = _units.Count - 1; i >= 0; i--)
            {
                var unit = _units[i];
                if (unit.ShouldDespawn)
                {
                    foreach (var unlink in _unregisterCallbacks)
                        unlink(unit);

                    _units.RemoveAt(i);
                }
            }
        }
    }
}
