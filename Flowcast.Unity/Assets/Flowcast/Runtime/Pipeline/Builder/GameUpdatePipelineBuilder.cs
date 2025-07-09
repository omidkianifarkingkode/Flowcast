using System;
using System.Collections.Generic;

namespace Flowcast.Pipeline
{
    public interface IGameUpdatePipelineBuilder
    {
        void UseDefaultSteps();
        void UseCustomSteps(Action<CustomPipelineBuilder> stepSteup);
        void HandleStepManually(Action<ulong> onTickOnly);
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
        private List<ISimulationStep> _steps;
        private Action<ulong> _simpleTick;

        public void UseDefaultSteps()
        {
            _steps = new()
            {
                new SpawnStep(),
                new PathfindingStep(),
                new MovementStep(),
                new CollisionStep(),
                new LogicStep(),
                new DespawnStep()
            };
        }

        public void UseCustomSteps(Action<CustomPipelineBuilder> stepSteup)
        {
            var piplineBuilder = new CustomPipelineBuilder();

            stepSteup?.Invoke(piplineBuilder);

            _steps = new(piplineBuilder.Build());
        }

        public void HandleStepManually(Action<ulong> onTickOnly)
        {
            _simpleTick = onTickOnly;
        }

        internal IGameUpdatePipeline Build()
        {
            if (_simpleTick != null)
                return new SimpleGameUpdatePipeline(_simpleTick);

            if (_steps.Count == 0)
                UseDefaultSteps();

            var pipeline = new GameUpdatePipeline(_steps);

            return pipeline;
        }
    }
}
