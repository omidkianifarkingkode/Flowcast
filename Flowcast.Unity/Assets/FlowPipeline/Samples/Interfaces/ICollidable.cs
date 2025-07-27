using FixedMathSharp;

namespace FlowPipeline
{
    public interface ICollidable
    {
        void CheckCollision(ulong frame, Fixed64 deltaTime);
    }
}
