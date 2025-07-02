using Flowcast.Data;

namespace Flowcast.Builders
{
    public interface IRequireGameSession
    {
        IRequireGameState SetGameSession(GameSessionData gameSessionData);
    }
}
