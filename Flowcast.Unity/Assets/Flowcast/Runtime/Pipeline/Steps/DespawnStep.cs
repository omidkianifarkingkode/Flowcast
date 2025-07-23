using System.Collections.Generic;
using System;
using FixedMathSharp;

namespace Flowcast.Pipeline
{
    public class DespawnStep : ISimulationStep<IDespawnable>
    {
        private readonly List<Action<IDespawnable>> _unregisterCallbacks = new();

        public void RegisterUnlinker<T>(ISimulationStep<T> step) where T : class
        {
            _unregisterCallbacks.Add(unit =>
            {
                if (unit is T typed)
                    step.Remove(typed);
            });
        }

        private readonly List<IDespawnable> _units = new();

        public void Add(IDespawnable unit) => _units.Add(unit);
        public void Remove(IDespawnable unit) => _units.Remove(unit);
        public void Clear() => _units.Clear();

        public void ProcessFrame(ulong frame, Fixed64 deltaTime)
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
