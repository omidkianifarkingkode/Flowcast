using System.Collections.Generic;

namespace Flowcast.Pipeline
{
    /// <summary>
    /// Deterministic execution pipeline that processes game logic in a fixed order.
    /// Each handler type (movement, pathfinding, etc.) is processed in a hardcoded sequence.
    /// </summary>
    public interface IGameUpdatePipeline 
    {
        public void AddStep(ISimulationStep step);

        public void ProcessFrame(ulong frame);
    }
    
    public class GameUpdatePipeline : IGameUpdatePipeline
    {
        private readonly List<ISimulationStep> _steps = new();

        public void AddStep(ISimulationStep step)
        {
            _steps.Add(step);
        }

        public void ProcessFrame(ulong frame)
        {
            foreach (var system in _steps)
                system.ProcessFrame(frame);
        }
    }

    public static class SimulationPipelineBuilder
    {
        public static GameUpdatePipeline BuildDefault()
        {
            var pipeline = new GameUpdatePipeline();

            pipeline.AddStep(new SpawnStep());
            pipeline.AddStep(new PathfindingStep());
            pipeline.AddStep(new MovementStep());
            pipeline.AddStep(new CollisionStep());
            pipeline.AddStep(new LogicStep());
            pipeline.AddStep(new DespawnStep());

            return pipeline;
        }
    }

}
