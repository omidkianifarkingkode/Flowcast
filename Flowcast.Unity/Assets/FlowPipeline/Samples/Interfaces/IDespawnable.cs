using FixedMathSharp;

namespace FlowPipeline
{
    public interface IDespawnable 
    {
        bool ShouldDespawn { get; }

        void RegisterStep(IFlowStep<IDespawnable> step);
        void UnregisterStep(IFlowStep<IDespawnable> step);
    }

}
