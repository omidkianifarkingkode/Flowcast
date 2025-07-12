using System;
using System.IO;

namespace Flowcast.Serialization
{
    public class BinarySerializer : IGameStateSerializer
    {
        private readonly Func<IBinarySerializableGameState> _gameStateFactory;

        public BinarySerializer(Func<IBinarySerializableGameState> gameStateFactory)
        {
            _gameStateFactory = gameStateFactory;
        }

        public byte[] SerializeSnapshot()
        {
            var gameState = _gameStateFactory();
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);
            gameState.WriteTo(writer);
            return stream.ToArray();
        }

        public ISerializableGameState DeserializeSnapshot(byte[] data)
        {
            var gameState = _gameStateFactory();
            using var stream = new MemoryStream(data);
            using var reader = new BinaryReader(stream);
            gameState.ReadFrom(reader);
            return gameState;
        }
    }

}

