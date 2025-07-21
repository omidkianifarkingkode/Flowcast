using Flowcast.Commons;
using System.Threading.Tasks;

namespace Flowcast.Serialization
{
    /// <summary>
    /// Base interface for serializing and deserializing game state snapshots.
    /// </summary>
    public interface IGameStateSerializer
    {
        byte[] SerializeSnapshot();
        ISerializableGameState DeserializeSnapshot(byte[] data);

        Result<byte[]> TrySerializeSnapshot();
        Result<ISerializableGameState> TryDeserializeSnapshot(byte[] data);

        Task<Result<byte[]>> TrySerializeSnapshotAsync();
        Task<Result<ISerializableGameState>> TryDeserializeSnapshotAsync(byte[] data);

        ISerializableGameState CreateDefault();
    }

    /// <summary>
    /// Generic interface for type-safe game state serialization.
    /// </summary>
    public interface IGameStateSerializer<T> : IGameStateSerializer where T : ISerializableGameState
    {
        new T DeserializeSnapshot(byte[] data);
        new Result<T> TryDeserializeSnapshot(byte[] data);
        new Task<Result<T>> TryDeserializeSnapshotAsync(byte[] data);

        void DeserializeInto(byte[] data, T targetInstance);
        Result TryDeserializeInto(byte[] data, T targetInstance);
        Task<Result> DeserializeIntoAsync(byte[] data, T targetInstance);
        new T CreateDefault();
    }
}

