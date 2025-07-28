namespace FlowPipeline
{
    /// <summary>
    /// Handles deterministic, per-frame game logic (e.g., movement, pathfinding).
    /// Entities can implement this interface to participate in the simulation step.
    /// </summary>
    public interface IFlowStep<TContext> where TContext : struct
    {
        void Process(TContext deltaTime);
    }

    public interface IFlowStep<TEntity, TContext> : IFlowStep<TContext> where TContext : struct
    {
        void Add(TEntity entity);
        void Remove(TEntity entity);
        void Clear();
    }
}
