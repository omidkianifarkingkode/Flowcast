using FixedMathSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FlowPipeline
{
    /// <summary>
    /// Deterministic execution pipeline that processes game logic in a fixed order.
    /// Each handler type (movement, pathfinding, etc.) is processed in a hardcoded sequence.
    /// </summary>
    public interface IFlowPipeline
    {
        IFlowStep<T> GetStep<T>();
        public void ProcessFrame(ulong frame, Fixed64 deltaTime);
    }

    public class FlowPipeline : IFlowPipeline
    {
        private readonly List<IFlowStep> _steps = new();

        public FlowPipeline(List<IFlowStep> steps)
        {
            _steps = steps;
        }

        public IFlowStep<T> GetStep<T>()
        {
            foreach (var step in _steps)
            {
                // Check if the step is of the correct type
                if (step is IFlowStep<T> typedStep)
                {
                    return typedStep;
                }
            }

            // If no step of type T is found, throw an exception or return null
            throw new InvalidOperationException($"No step found for type {typeof(T).Name}");
        }


        public void ProcessFrame(ulong frame, Fixed64 deltaTime)
        {
            foreach (var system in _steps)
                system.Process(frame, deltaTime);
        }
    }
}
