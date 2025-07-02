using Flowcast.Serialization;

namespace Flowcast.Builders
{
    public interface IRequireGameState
    {
        IOptionalSettings SetGameStateModel(ISerializableGameState state);
    }
}
