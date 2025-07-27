using FixedMathSharp;

namespace FlowPipeline
{
    public interface ISpawnable
    {
        bool ShouldSpawn(ulong frame, Fixed64 deltaTime);
        void OnSpawned(ulong frame, Fixed64 deltaTime);
    }

}
