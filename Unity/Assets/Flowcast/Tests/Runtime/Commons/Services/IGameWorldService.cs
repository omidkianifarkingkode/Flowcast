namespace Flowcast.Tests.Runtime.Commons.Services
{
    public interface IGameWorldService
    {
        bool IsMovementAllowed(float x, float y);
    }

    public class GameWorldService : IGameWorldService
    {
        public bool IsMovementAllowed(float x, float y)
        {
            // Simple bounds check
            return x >= 0 && x <= 10 && y >= 0 && y <= 10;
        }
    }

}
