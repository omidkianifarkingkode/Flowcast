using FixedMathSharp;

namespace FlowPipeline
{
    public interface IPathfindable
    {
        bool NeedsNewPath(ulong frame, Fixed64 deltaTime);
        void CalculatePath();
    }

}
