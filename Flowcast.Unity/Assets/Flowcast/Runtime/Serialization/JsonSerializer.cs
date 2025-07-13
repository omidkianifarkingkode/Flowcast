using System;
using System.Text;
using System.Threading.Tasks;
using Flowcast.Commons;
using Newtonsoft.Json;

namespace Flowcast.Serialization
{
    public class JsonSerializer<T> : IGameStateSerializer<T>
        where T : ISerializableGameState, new()
    {
        private readonly Func<T> _gameStateFactory;
        private readonly JsonSerializerSettings _settings;

        public JsonSerializer(Func<T> gameStateFactory, JsonSerializerSettings settings = default)
        {
            _gameStateFactory = gameStateFactory ?? throw new ArgumentNullException(nameof(gameStateFactory));
            _settings = settings;
        }

        // --- Exception-based methods ---

        public byte[] SerializeSnapshot()
        {
            var gameState = _gameStateFactory();
            var json = JsonConvert.SerializeObject(gameState, _settings);
            return Encoding.UTF8.GetBytes(json);
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
            var json = Encoding.UTF8.GetString(data);
            JsonConvert.PopulateObject(json, targetInstance, _settings);
        }

        public Result TryDeserializeInto(byte[] data, T targetInstance)
        {
            return Result.Try(() => DeserializeInto(data, targetInstance), "JSON in-place deserialization failed.");
        }

        public Task<Result> DeserializeIntoAsync(byte[] data, T targetInstance)
        {
            return Task.FromResult(TryDeserializeInto(data, targetInstance));
        }

        // --- Result<T>-based methods ---

        public Result<byte[]> TrySerializeSnapshot()
        {
            return Result<byte[]>.Try(() => SerializeSnapshot(), "JSON serialization failed.");
        }

        public Result<ISerializableGameState> TryDeserializeSnapshot(byte[] data)
        {
            return Result<ISerializableGameState>.Try(() => DeserializeSnapshot(data), "JSON deserialization failed.");
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
