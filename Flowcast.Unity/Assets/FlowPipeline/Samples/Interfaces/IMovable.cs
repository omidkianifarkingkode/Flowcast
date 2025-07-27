using FixedMathSharp;

namespace FlowPipeline
{
    public interface IMovable
    {
        void Move(ulong frame, Fixed64 deltaTime);

        void RegisterStep(IFlowStep<IMovable> step);
        void UnregisterStep(IFlowStep<IMovable> step);
    }
}
