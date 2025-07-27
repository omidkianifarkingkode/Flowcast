using Flowcast.Synchronization;

namespace Flowcast.FlowPipeline
{
    public static class FlowPipelineExtensions
    {
        /// <summary>
        /// Configures a default game step callback to run the flow pipeline logic each tick.
        /// </summary>
        /// <param name="configurer">The pipeline configuration chain.</param>
        /// <returns>The next stage of rollback configuration.</returns>
        public static IGameStateSyncRollbackConfigurer UseDefaultFlowPipeline(this IGameStepConfigurer configurer) 
        {
            return configurer.OnStep((tick, delta) =>
            {
                // TODO: Insert default simulation pipeline logic here
            });
        }
    }
}
