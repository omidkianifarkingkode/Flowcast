using System;
using System.Text;
using Newtonsoft.Json;

namespace Flowcast.Serialization
{
    public class JsonGameStateSerializer<T> : IGameStateSerializer where T : ISerializableGameState, new()
    {
        private readonly Func<ISerializableGameState> _gameStateFactory;
        private readonly JsonSerializerSettings _settings;

        public JsonGameStateSerializer(Func<ISerializableGameState> gameStateFactory, JsonSerializerSettings settings)
        {
            _gameStateFactory = gameStateFactory;
            _settings = settings;
        }

        public byte[] SerializeSnapshot()
        {
            var gameState = _gameStateFactory();
            var json = JsonConvert.SerializeObject(gameState, _settings);
            return Encoding.UTF8.GetBytes(json);
        }

        public ISerializableGameState DeserializeSnapshot(byte[] data)
        {
            var json = Encoding.UTF8.GetString(data);
            var gameState = _gameStateFactory();
            JsonConvert.PopulateObject(json, gameState, _settings);
            return gameState;
        }
    }

}

