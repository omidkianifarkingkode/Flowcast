using NUnit.Framework;

namespace Flowcast.Serialization
{
    public interface IGameStateSerializer
    {
        byte[] SerializeSnapshot();
        ISerializableGameState DeserializeSnapshot(byte[] data);
    }
}

