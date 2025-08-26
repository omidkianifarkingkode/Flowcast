using MessagePack;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Realtime.Transport.Messaging.Codec;

public sealed partial class Codec : ICodec
{
    private readonly JsonSerializerOptions _jsonWriteSerializerOptions;
    private readonly JsonSerializerOptions _jsonReadSerializerOptions;
    private readonly MessagePackSerializerOptions _messagePackSerializerOptions;

    public Codec(
        JsonSerializerOptions? jsonWriteSerializerOptions = null,
        JsonSerializerOptions? jsonReadSerializerOptions = null,
        MessagePackSerializerOptions? messagePackSerializerOptions = null)
    {
        _jsonWriteSerializerOptions = jsonWriteSerializerOptions ?? new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };

        _jsonReadSerializerOptions = jsonReadSerializerOptions ?? new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };

        _messagePackSerializerOptions = messagePackSerializerOptions ?? MessagePackSerializerOptions.Standard;
    }

    private static T? CreateDefaultIfPossible<T>() where T : IPayload
    {
        var t = typeof(T);
        // Allow parameterless construction for empty commands like PongCommand/PingCommand
        return (T?)Activator.CreateInstance(t);
    }
}
