// Runtime/Core/Serialization/XmlTextSerializer.cs
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace Flowcast.Core.Serialization
{
    public sealed class XmlTextSerializer : ISerializer
    {
        public string MediaType => "application/xml";

        public byte[] Serialize<T>(T value)
        {
            var ser = new XmlSerializer(typeof(T));
            using var ms = new MemoryStream();
            ser.Serialize(ms, value);
            return ms.ToArray();
        }

        public T Deserialize<T>(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0) return default;
            var ser = new XmlSerializer(typeof(T));
            using var ms = new MemoryStream(bytes);
            return (T)ser.Deserialize(ms);
        }
    }
}
