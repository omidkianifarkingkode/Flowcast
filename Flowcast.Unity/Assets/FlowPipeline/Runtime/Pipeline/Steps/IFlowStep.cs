using FixedMathSharp;

namespace FlowPipeline
{
    /// <summary>
    /// Handles deterministic, per-frame game logic (e.g., movement, pathfinding).
    /// Units can implement this interface to participate in the simulation step.
    /// </summary>
    public interface IFlowStep
    {
        void Process(ulong frame, Fixed64 deltaTime);
    }

    public interface IFlowStep<T> : IFlowStep
    {
        void Add(T unit);
        void Remove(T unit);
        void Clear();
    }
}
