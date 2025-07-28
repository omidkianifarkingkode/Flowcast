using Flowcast.Synchronization;
using FlowPipeline;
using System;
using UnityEngine;

namespace Flowcast.FlowPipeline
{
    public static class FlowPipelineExtensions
    {
        /// <summary>
        /// Configures a default game step callback to run the flow pipeline logic each tick.
        /// </summary>
        /// <param name="configurer">The pipeline configuration chain.</param>
        /// <returns>The next stage of rollback configuration.</returns>
        public static IGameStateSyncRollbackConfigurer UseFlowPipelineFromAsset(this IGameStepConfigurer configurer, PipelineOptionsAsset asset = default)
        {
            asset ??= PipelineOptionsAsset.Load();

            var steps = asset.GetSteps<SimulationContext>();

            var pipeline = new FlowPipeline<SimulationContext>(steps);

            return configurer.UseFlowPipeline(pipeline);
        }

        /// <summary>
        /// Configures and uses a pipeline using fluent step configuration.
        /// </summary>
        public static IGameStateSyncRollbackConfigurer UseFlowPipeline(this IGameStepConfigurer configurer, Action<PipelineOptions> setup)
        {
            var options = new PipelineOptions();
            setup(options);

            var pipeline = new FlowPipeline<SimulationContext>(options.GetSteps<SimulationContext>());

            return configurer.UseFlowPipeline(pipeline);
        }

        /// <summary>
        /// Uses a custom pipeline instance directly.
        /// </summary>
        public static IGameStateSyncRollbackConfigurer UseFlowPipeline(this IGameStepConfigurer configurer, IFlowPipeline<SimulationContext> pipeline)
        {
            return configurer.OnStep((tick, delta) =>
            {
                var context = new SimulationContext(tick, delta);
                pipeline.ProcessFrame(context);
            });
        }
    }
}
