using Flowcast.FlowPipeline;
using System;
using System.Collections.Generic;

namespace FlowPipeline
{
    public class DespawningStep : FlowStep<IDespawnable, SimulationContext>
    {
        private readonly List<Action<IDespawnable>> _unregisterCallbacks = new();

        public void RegisterUnlinker<T>(IFlowStep<T, SimulationContext> step) where T : class
        {
            _unregisterCallbacks.Add(unit =>
            {
                if (unit is T typed)
                    step.Remove(typed);
            });
        }

        public override void Process(SimulationContext context)
        {
            for (int i = _entities.Count - 1; i >= 0; i--)
            {
                var unit = _entities[i];
                if (unit.ShouldDespawn)
                {
                    foreach (var unlink in _unregisterCallbacks)
                        unlink(unit);

                    _entities.RemoveAt(i);
                }
            }
        }
    }
}
