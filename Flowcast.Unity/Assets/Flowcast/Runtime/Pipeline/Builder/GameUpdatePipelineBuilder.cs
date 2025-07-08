using System;
using System.Collections.Generic;

namespace Flowcast.Pipeline
{
    public interface IGameUpdatePipelineBuilder
    {
        void UseDefaultSteps();
        void UseCustomSteps(Action<CustomPipelineBuilder> stepSteup);
    }

    public class CustomPipelineBuilder
    {
        private readonly List<ISimulationStep> _steps = new();

        public CustomPipelineBuilder Add(ISimulationStep step) 
        {
            _steps.Add(step);
            return this;
        }
        public CustomPipelineBuilder Add(IEnumerable<ISimulationStep> steps) 
        {
            _steps.AddRange(steps);
            return this;
        }

        internal List<ISimulationStep> Build() => _steps;
    }

    public class GameUpdatePipelineBuilder : IGameUpdatePipelineBuilder
    {
        private readonly List<ISimulationStep> _steps = new();

        public void UseDefaultSteps()
        {
            _steps.Clear();

            _steps.Add(new SpawnStep());
            _steps.Add(new PathfindingStep());
            _steps.Add(new MovementStep());
            _steps.Add(new CollisionStep());
            _steps.Add(new LogicStep());
            _steps.Add(new DespawnStep());
        }

        public void UseCustomSteps(Action<CustomPipelineBuilder> stepSteup)
        {
            var piplineBuilder = new CustomPipelineBuilder();

            stepSteup?.Invoke(piplineBuilder);

            _steps.Clear();
            _steps.AddRange(piplineBuilder.Build());
        }

        internal GameUpdatePipeline Build()
        {
            if(_steps.Count == 0)
                UseDefaultSteps();

            var pipeline = new GameUpdatePipeline(_steps);

            return pipeline;
        }
    }
}
