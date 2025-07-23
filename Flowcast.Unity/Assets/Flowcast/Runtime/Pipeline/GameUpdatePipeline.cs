using FixedMathSharp;
using System;
using System.Collections.Generic;

namespace Flowcast.Pipeline
{
    /// <summary>
    /// Deterministic execution pipeline that processes game logic in a fixed order.
    /// Each handler type (movement, pathfinding, etc.) is processed in a hardcoded sequence.
    /// </summary>
    public interface IGameUpdatePipeline 
    {
        public void ProcessFrame(ulong frame, Fixed64 deltaTime);
    }
    
    public class GameUpdatePipeline : IGameUpdatePipeline
    {
        private readonly List<ISimulationStep> _steps = new();

        public GameUpdatePipeline(List<ISimulationStep> steps)
        {
            _steps = steps;
        }

        public void ProcessFrame(ulong frame, Fixed64 deltaTime)
        {
            foreach (var system in _steps)
                system.ProcessFrame(frame, deltaTime);
        }
    }

    public class SimpleGameUpdatePipeline : IGameUpdatePipeline
    {
        private readonly Action<ulong,Fixed64> _onTick;

        public SimpleGameUpdatePipeline(Action<ulong, Fixed64> onTick)
        {
            _onTick = onTick ?? throw new ArgumentNullException(nameof(onTick));
        }

        public void ProcessFrame(ulong frame, Fixed64 deltaTime)
        {
            _onTick(frame, deltaTime);
        }
    }

}
