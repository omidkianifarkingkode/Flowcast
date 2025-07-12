namespace Flowcast.Serialization
{
    public interface IGameStateSerializer
    {
        byte[] SerializeSnapshot();
        ISerializableGameState DeserializeSnapshot(byte[] data);
    }

    public interface IGameStateSerializer<T> : IGameStateSerializer where T : ISerializableGameState
    {
        new T DeserializeSnapshot(byte[] data);
    }
}

