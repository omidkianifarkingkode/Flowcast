using FixedMathSharp;

namespace Flowcast.Pipeline
{
    public interface IMovable
    {
        void Move(ulong frame, Fixed64 deltaTime);
    }

}
