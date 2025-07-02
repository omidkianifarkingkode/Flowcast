using NUnit.Framework;

namespace Flowcast.Serialization
{
    public interface IGameStateSerializer
    {
        byte[] SerializeSnapshot();
        ISerializableGameState DeserializeSnapshot(byte[] data);
    }

    //public interface IGameStateSerializer<T> where T : ISerializableGameState
    //{
    //    /// <summary>Capture full game state as a binary snapshot.</summary>
    //    byte[] SerializeSnapshot();

    //    /// <summary>Restore full game state from a snapshot.</summary>
    //    T DeserializeSnapshot(byte[] data);
    //}
}

