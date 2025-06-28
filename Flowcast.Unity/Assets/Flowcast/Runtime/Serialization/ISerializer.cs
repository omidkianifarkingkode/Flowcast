namespace Flowcast.Serialization
{
    public interface ISerializer
    {
        byte[] Serialize(object input);
        T Deserialize<T>();
    }
}

