using MessagePack;
using Realtime.TestHost.Messaging;
using Realtime.Transport.Messaging;
using Realtime.Transport.Messaging.Sender;
using SharedKernel;

namespace Realtime.TestHost.Chat
{
    public static class ChatMessageType
    {
        public const ushort Chat = 10;
    }

    [MessagePackObject]
    [RealtimeMessage(ChatMessageType.Chat)]
    public class ChatCommand : IPayload, ICommand
    {
        [Key(0)] public ulong SenderId { get; set; }
        [Key(1)] public string Message { get; set; }
    }

    public class ChatCommandHandler(IRealtimeClientMessenger messenger) : ICommandHandler<ChatCommand>
    {
        public Task<Result> Handle(ChatCommand command, CancellationToken cancellationToken)
        {
            var chat = new ChatCommand
            {
                SenderId = 1,
                Message = "Hi"
            };

            messenger.SendAsync(ChatMessageType.Chat, chat, cancellationToken);

            return Task.FromResult(Result.Success());
        }
    }
}
