using System;
using System.IO;

namespace Flowcast.Serialization
{
    public class GameStateSerializerWrapper<T> : IGameStateSerializer
    where T : ISerializableGameState
    {
        private readonly IGameStateSerializer<T> _inner;

        public GameStateSerializerWrapper(IGameStateSerializer<T> inner)
        {
            _inner = inner;
        }

        public byte[] SerializeSnapshot()
        {
            return _inner.SerializeSnapshot();
        }

        public ISerializableGameState DeserializeSnapshot(byte[] data)
        {
            return _inner.DeserializeSnapshot(data);
        }
    }


    public class GenericStateSerializer<T> : IGameStateSerializer<T> where T : ISerializableGameState
    {
        private readonly Func<T> _stateFactory;

        public GenericStateSerializer(Func<T> stateFactory)
        {
            _stateFactory = stateFactory;
        }

        public byte[] SerializeSnapshot()
        {
            var state = _stateFactory(); // or get current game state via some IStateProvider
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);
            state.WriteTo(writer);
            return stream.ToArray();
        }

        public T DeserializeSnapshot(byte[] data)
        {
            var state = _stateFactory();
            using var stream = new MemoryStream(data);
            using var reader = new BinaryReader(stream);
            state.ReadFrom(reader);
            return state;
        }
    }

}

