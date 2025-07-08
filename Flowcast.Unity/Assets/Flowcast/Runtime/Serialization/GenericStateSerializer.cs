using System;
using System.IO;

namespace Flowcast.Serialization
{
    public class GameStateSerializer : IGameStateSerializer
    {
        // for get current game state via some IStateProvider
        private readonly Func<ISerializableGameState> _stateFactory;

        public GameStateSerializer(Func<ISerializableGameState> stateFactory)
        {
            _stateFactory = stateFactory;
        }

        public byte[] SerializeSnapshot()
        {
            var state = _stateFactory();
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);
            state.WriteTo(writer);
            return stream.ToArray();
        }

        public ISerializableGameState DeserializeSnapshot(byte[] data)
        {
            var state = _stateFactory();
            using var stream = new MemoryStream(data);
            using var reader = new BinaryReader(stream);
            state.ReadFrom(reader);
            return state;
        }
    }

}

