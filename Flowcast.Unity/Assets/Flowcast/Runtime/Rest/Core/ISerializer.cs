namespace Flowcast.Rest.Core
{
    public interface ISerializer
    {
        string Serialize<T>(T obj);
        T Deserialize<T>(string data);
    }

    public class JsonSerializer : ISerializer
    {
        public string Serialize<T>(T obj) =>
            System.Text.Json.JsonSerializer.Serialize(obj);

        public T Deserialize<T>(string data) =>
            System.Text.Json.JsonSerializer.Deserialize<T>(data);
    }
}
