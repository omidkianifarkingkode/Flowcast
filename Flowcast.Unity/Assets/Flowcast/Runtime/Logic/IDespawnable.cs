using FixedMathSharp;

namespace Flowcast.Pipeline
{
    public interface IDespawnable 
    {
        bool ShouldDespawn { get; }
    }

    public interface ISpawnable
    {
        bool ShouldSpawn(ulong frame, Fixed64 deltaTime);
        void OnSpawned(ulong frame, Fixed64 deltaTime);
    }

    public interface IPathfindable
    {
        bool NeedsNewPath(ulong frame, Fixed64 deltaTime);
        void CalculatePath();
    }

    public interface ICollidable
    {
        void CheckCollision(ulong frame, Fixed64 deltaTime);
    }

    public interface ILogicUnit
    {
        void ExecuteLogic(ulong frame, Fixed64 deltaTime);
    }

}
