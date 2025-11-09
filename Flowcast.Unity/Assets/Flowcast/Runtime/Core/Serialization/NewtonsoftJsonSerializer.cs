// Runtime/Core/Serialization/NewtonsoftJsonSerializer.cs
#if FLOWCAST_NEWTONSOFT_JSON
using Newtonsoft.Json;
using System.Text;

namespace Flowcast.Core.Serialization
{
    public sealed class NewtonsoftJsonSerializer : ISerializer
    {
        private readonly JsonSerializerSettings _settings;
        public string MediaType => "application/json";

        public NewtonsoftJsonSerializer(JsonSerializerSettings settings = null)
        {
            _settings = settings ?? new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };
        }

        public byte[] Serialize<T>(T value)
            => Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value, _settings));

        public T Deserialize<T>(byte[] bytes)
            => bytes == null ? default : JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(bytes), _settings);
    }
}
#endif
