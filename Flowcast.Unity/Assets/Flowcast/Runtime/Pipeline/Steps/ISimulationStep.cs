namespace Flowcast.Pipeline
{
    /// <summary>
    /// Handles deterministic, per-frame game logic (e.g., movement, pathfinding).
    /// Units can implement this interface to participate in the simulation step.
    /// </summary>
    public interface ISimulationStep
    {
        void ProcessFrame(ulong frameNumber);
    }

    public interface ISimulationStep<T> : ISimulationStep
    {
        void Add(T unit);
        void Remove(T unit);
        void Clear();
    }
}
