using Flowcast.Commons;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Flowcast.Serialization
{
    public class BinarySerializer<T> : IGameStateSerializer<T>
        where T : IBinarySerializableGameState, new()
    {
        private readonly Func<T> _gameStateFactory;

        public BinarySerializer(Func<T> gameStateFactory)
        {
            _gameStateFactory = gameStateFactory ?? throw new ArgumentNullException(nameof(gameStateFactory));
        }

        // --- Exception-based methods ---

        public byte[] SerializeSnapshot()
        {
            var gameState = _gameStateFactory();
            var estimatedSize = Math.Max(_gameStateFactory().GetEstimatedSize(), 128); // fallback to 128 bytes
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);
            gameState.WriteTo(writer);
            return stream.ToArray();
        }

        public ISerializableGameState DeserializeSnapshot(byte[] data)
        {
            var gameState = new T();
            DeserializeInto(data, gameState);
            return gameState;
        }

        T IGameStateSerializer<T>.DeserializeSnapshot(byte[] data)
        {
            return (T)DeserializeSnapshot(data);
        }

        // --- In-place deserialization ---

        public void DeserializeInto(byte[] data, T targetInstance)
        {
            using var stream = new MemoryStream(data);
            using var reader = new BinaryReader(stream);
            targetInstance.ReadFrom(reader);
        }

        public Result TryDeserializeInto(byte[] data, T targetInstance)
        {
            return Result.Try(() => DeserializeInto(data, targetInstance), "Binary in-place deserialization failed.");
        }

        public Task<Result> DeserializeIntoAsync(byte[] data, T targetInstance)
        {
            return Task.FromResult(TryDeserializeInto(data, targetInstance));
        }

        // --- Result<T>-based methods ---

        public Result<byte[]> TrySerializeSnapshot()
        {
            return Result<byte[]>.Try(() => SerializeSnapshot(), "Binary serialization failed.");
        }

        public Result<ISerializableGameState> TryDeserializeSnapshot(byte[] data)
        {
            return Result<ISerializableGameState>.Try(() => DeserializeSnapshot(data), "Binary deserialization failed.");
        }

        Result<T> IGameStateSerializer<T>.TryDeserializeSnapshot(byte[] data)
        {
            var result = TryDeserializeSnapshot(data);

            return result.IsSuccess
                ? Result<T>.Success((T)result.Value)
                : Result<T>.Failure(result.Error);
        }


        // --- Async wrappers ---

        public Task<Result<byte[]>> TrySerializeSnapshotAsync()
        {
            return Task.FromResult(TrySerializeSnapshot());
        }

        public Task<Result<ISerializableGameState>> TryDeserializeSnapshotAsync(byte[] data)
        {
            return Task.FromResult(TryDeserializeSnapshot(data));
        }

        Task<Result<T>> IGameStateSerializer<T>.TryDeserializeSnapshotAsync(byte[] data)
        {
            return Task.FromResult(((IGameStateSerializer<T>)this).TryDeserializeSnapshot(data));
        }
    }
}
