using Application.Abstractions.Authentication;
using Application.Abstractions.Realtime;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net.WebSockets;
using System.Text;

namespace Infrastructure.Realtime;

public class WebSocketHandler(
    IUserConnectionRegistry connectionRegistry,
    IRealtimeMessageReceiver receiver,
    ILogger<WebSocketHandler> logger)
{
    public async Task HandleConnectionAsync(HttpContext context, CancellationToken cancellationToken)
    {
        using var webSocket = await context.WebSockets.AcceptWebSocketAsync().ConfigureAwait(false);

        var userContext = context.RequestServices.GetRequiredService<IUserContext>();

        var userId = userContext.GetUserId(context);
        if (userId == null)
        {
            await webSocket.CloseAsync(WebSocketCloseStatus.PolicyViolation, "Unauthorized", cancellationToken).ConfigureAwait(false);
            return;
        }

        var connectionId = Guid.NewGuid().ToString();
        connectionRegistry.Register(connectionId, userId.Value, webSocket);

        try
        {
            await ReceiveLoopAsync(webSocket, connectionId, userId.Value, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            connectionRegistry.Unregister(connectionId);
        }
    }

    private async Task ReceiveLoopAsync(WebSocket socket, string connectionId, Guid userId, CancellationToken cancellationToken)
    {
        var buffer = new byte[1024 * 4];
        var messageBuffer = new ArraySegment<byte>(buffer);

        using var ms = new MemoryStream(); // For fragmented message assembly

        try
        {
            while (!cancellationToken.IsCancellationRequested && socket.State == WebSocketState.Open)
            {
                var (flowControl, result) = await FetchSocketReceiveResult(socket, connectionId, messageBuffer, cancellationToken).ConfigureAwait(false); ;

                if (!flowControl)
                    break;

                // Handle Close message immediately
                if (result!.MessageType == WebSocketMessageType.Close)
                {
                    logger.LogInformation($"Close message received from {connectionId}: {result.CloseStatus} - {result.CloseStatusDescription}");
                    await CloseSocketAsync(socket, result.CloseStatus, result.CloseStatusDescription, cancellationToken).ConfigureAwait(false);
                    break;
                }

                await AppendFragmentToMessageBuffer(userId, messageBuffer, ms, result, cancellationToken).ConfigureAwait(false);
            }
        }
        finally
        {
            await CleanupConnectionAsync(socket, connectionId).ConfigureAwait(false);
        }
    }

    private async Task AppendFragmentToMessageBuffer(Guid userId, ArraySegment<byte> messageBuffer, MemoryStream ms, WebSocketReceiveResult result, CancellationToken cancellationToken)
    {
        ms.Write(messageBuffer.Array!, messageBuffer.Offset, result.Count);

        if (result.EndOfMessage)
        {
            ms.Seek(0, SeekOrigin.Begin);

            if (result.MessageType == WebSocketMessageType.Text)
            {
                using var reader = new StreamReader(ms, Encoding.UTF8, leaveOpen: true);
                var message = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);

                await receiver.OnMessageReceivedAsync(userId, message, cancellationToken).ConfigureAwait(false);
            }
            else if (result.MessageType == WebSocketMessageType.Binary)
            {
                var binaryData = ms.ToArray();

                await receiver.OnMessageReceivedAsync(userId, binaryData, cancellationToken).ConfigureAwait(false);
            }

            // Reset memory stream for next message
            ms.SetLength(0);
        }
    }

    private async Task<(bool flowControl, WebSocketReceiveResult? value)> FetchSocketReceiveResult(
        WebSocket socket,
        string connectionId,
        ArraySegment<byte> messageBuffer,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await socket.ReceiveAsync(messageBuffer, cancellationToken).ConfigureAwait(false);
            return (true, result);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation($"Receive loop cancelled for {connectionId}");
            return (false, null);
        }
        catch (WebSocketException ex)
        {
            logger.LogError($"WebSocket error on {connectionId}: {ex.Message}", ex);
            return (false, null);
        }
        catch (Exception ex)
        {
            logger.LogError($"Unexpected error on {connectionId}: {ex.Message}", ex);
            return (false, null);
        }
    }

    private async Task CloseSocketAsync(WebSocket socket, WebSocketCloseStatus? status, string? description, CancellationToken cancellationToken)
    {
        if (socket.State == WebSocketState.Open || socket.State == WebSocketState.CloseReceived)
        {
            try
            {
                await socket.CloseAsync(
                    status ?? WebSocketCloseStatus.NormalClosure,
                    description ?? "Closed by server",
                    cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogError("Error while closing WebSocket", ex);
            }
        }
    }

    private Task CleanupConnectionAsync(WebSocket socket, string connectionId)
    {
        logger.LogInformation($"Cleaning up connection {connectionId}");
        try
        {
            socket.Dispose();
            // Remove from connection dictionary, release resources, notify other services, etc.
        }
        catch (Exception ex)
        {
            logger.LogError($"Error during cleanup for {connectionId}", ex);
        }

        return Task.CompletedTask;
    }
}
