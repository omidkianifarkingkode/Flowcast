using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Application.Abstractions.Realtime;
using Application.MatchMakings.Commands;
using Domain.Sessions.ValueObjects;
using System.Net.WebSockets;
using System.Text;

namespace Presentation.WebSockets;

public class GameWebSocketHandler(
    IUserConnectionRegistry connectionRegistry,
    IUserContext userContextService,
    IServiceProvider serviceProvider)
{
    public async Task HandleConnectionAsync(HttpContext context)
    {
        using var webSocket = await context.WebSockets.AcceptWebSocketAsync();

        var userId = userContextService.GetUserId(context);
        if (userId == null)
        {
            await webSocket.CloseAsync(WebSocketCloseStatus.PolicyViolation, "Unauthorized", CancellationToken.None);
            return;
        }

        var connectionId = Guid.NewGuid().ToString();
        connectionRegistry.Register(connectionId, userId.Value, webSocket);

        try
        {
            await ReceiveLoopAsync(webSocket, connectionId, userId.Value);
        }
        finally
        {
            connectionRegistry.Unregister(connectionId);
        }
    }

    private async Task ReceiveLoopAsync(WebSocket socket, string connectionId, Guid userId)
    {
        var buffer = new byte[1024 * 4];
        var receiveResult = await socket.ReceiveAsync(buffer, CancellationToken.None);

        while (!receiveResult.CloseStatus.HasValue)
        {
            var message = Encoding.UTF8.GetString(buffer, 0, receiveResult.Count);
            await HandleMessageAsync(userId, message);

            receiveResult = await socket.ReceiveAsync(buffer, CancellationToken.None);
        }

        await socket.CloseAsync(receiveResult.CloseStatus.Value, receiveResult.CloseStatusDescription, CancellationToken.None);
    }

    private async Task HandleMessageAsync(Guid userId, string message)
    {
        // Here you can parse incoming JSON and send to MediatR
        if (message == "matchmaking")
        {
            var command = new RequestMatchmakingCommand(new PlayerId(userId));
            var handler = serviceProvider.GetRequiredService<ICommandHandler<RequestMatchmakingCommand>>();
            var result = await handler.Handle(command, CancellationToken.None);
            // You can also send a response here using your own SendAsync method
        }
    }
}
