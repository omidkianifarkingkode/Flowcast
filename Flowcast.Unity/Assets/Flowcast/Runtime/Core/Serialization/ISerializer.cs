// Runtime/Core/Serialization/ISerializer.cs
namespace Flowcast.Core.Serialization
{
    public interface ISerializer
    {
        string MediaType { get; } // e.g., application/json
        byte[] Serialize<T>(T value);
        T Deserialize<T>(byte[] bytes);
    }
}
