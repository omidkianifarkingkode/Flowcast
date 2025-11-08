// Runtime/Core/Serialization/Json/JsonSerializer.cs
using System.Text;
using UnityEngine;

namespace Flowcast.Core.Serialization
{
    /// KISS: uses UnityEngine.JsonUtility (no reflection config needed). Note: needs plain POCO types.
    public sealed class UnityJsonSerializer : ISerializer
    {
        public string MediaType => "application/json";
        public byte[] Serialize<T>(T value)
        {
            var json = JsonUtility.ToJson(value);
            return Encoding.UTF8.GetBytes(json ?? "null");
        }
        public T Deserialize<T>(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0) return default;
            var json = Encoding.UTF8.GetString(bytes);
            // JsonUtility requires wrapper for arrays; for MVP we assume objects.
            return JsonUtility.FromJson<T>(json);
        }
    }
}
