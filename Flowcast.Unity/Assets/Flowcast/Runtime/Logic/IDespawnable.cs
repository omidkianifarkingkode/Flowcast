namespace Flowcast.Pipeline
{
    public interface IDespawnable 
    {
        bool ShouldDespawn { get; }
    }

    public interface ISpawnable
    {
        bool ShouldSpawn(ulong frame);
        void OnSpawned(ulong frame);
    }

    public interface IPathfindable
    {
        bool NeedsNewPath(ulong frame);
        void CalculatePath();
    }

    public interface ICollidable
    {
        void CheckCollision(ulong frame);
    }

    public interface ILogicUnit
    {
        void ExecuteLogic(ulong frame);
    }

}
