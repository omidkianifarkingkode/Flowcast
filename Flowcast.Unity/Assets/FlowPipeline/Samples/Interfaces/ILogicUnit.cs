using FixedMathSharp;

namespace FlowPipeline
{
    public interface ILogicUnit
    {
        void ExecuteLogic(ulong tick, Fixed64 deltaTime);
    }
}
