namespace Realtime.Transport.Messaging.Options;

public class MessagingOptions
{
    public WireFormat WireFormat { get; set; } = WireFormat.Json; // used to choose sender
}

public enum WireFormat { Json = 1, MessagePack = 2 }
