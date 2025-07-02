using System;
using System.Text;
using Newtonsoft.Json;

namespace Flowcast.Serialization
{
    public class JsonGameStateSerializer : IGameStateSerializer
    {
        private readonly Func<ISerializableGameState> _stateFactory;

        public JsonGameStateSerializer(Func<ISerializableGameState> stateFactory)
        {
            _stateFactory = stateFactory;
        }

        public byte[] SerializeSnapshot()
        {
            var state = _stateFactory();
            var json = JsonConvert.SerializeObject(state, Formatting.Indented);
            return Encoding.UTF8.GetBytes(json);
        }

        public ISerializableGameState DeserializeSnapshot(byte[] data)
        {
            var json = Encoding.UTF8.GetString(data);
            var state = _stateFactory();
            JsonConvert.PopulateObject(json, state);
            return state;
        }
    }

}

