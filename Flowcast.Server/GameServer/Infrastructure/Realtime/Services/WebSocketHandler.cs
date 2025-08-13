using Application.Abstractions.Authentication;
using Application.Abstractions.Realtime;
using Application.Abstractions.Realtime.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SharedKernel;
using System.Net.WebSockets;
using System.Text;

namespace Infrastructure.Realtime.Services;

// Main WebSocket handler class managing connection lifecycle and message processing
public class WebSocketHandler(
    IUserConnectionRegistry connectionRegistry,
    IRealtimeMessageReceiver receiver,
    ILogger<WebSocketHandler> logger)
{
    public record ConnectionAuthResult(Guid UserId, string ConnectionId);

    /// <summary>
    /// Accepts and authorizes a WebSocket connection, then starts the receive loop.
    /// </summary>
    public async Task HandleConnectionAsync(HttpContext context, CancellationToken cancellationToken)
    {
        using var webSocket = await context.WebSockets.AcceptWebSocketAsync().ConfigureAwait(false);

        var authResult = await AuthorizeConnection(context, webSocket, cancellationToken).ConfigureAwait(false);

        if (authResult.IsFailure)
            return;

        var (userId, connectionId) = authResult.Value;

        try
        {
            await ReceiveLoopAsync(webSocket, connectionId, userId, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            connectionRegistry.Unregister(connectionId);
        }
    }

    /// <summary>
    /// Authorizes the user from the context and registers the connection.
    /// Returns failure and closes socket if unauthorized.
    /// </summary>
    private async Task<Result<ConnectionAuthResult>> AuthorizeConnection(HttpContext context, WebSocket webSocket, CancellationToken cancellationToken) 
    {
        var userContext = context.RequestServices.GetRequiredService<IUserContext>();

        var userId = userContext.GetUserId(context);

        if (userId == null) 
        {
            await webSocket.CloseAsync(WebSocketCloseStatus.PolicyViolation, "Unauthorized", cancellationToken).ConfigureAwait(false);

            return Result.Failure<ConnectionAuthResult>(Error.DefaultUnauthorized);
        }

        var connectionId = Guid.NewGuid().ToString();
        connectionRegistry.Register(connectionId, userId.Value, webSocket);

        return Result.Success(new ConnectionAuthResult(userId.Value, connectionId));
    }

    /// <summary>
    /// Main receive loop that reads messages until closed or cancelled.
    /// </summary>
    private async Task ReceiveLoopAsync(WebSocket socket, string connectionId, Guid userId, CancellationToken cancellationToken)
    {
        var buffer = new byte[1024 * 4];
        var messageBuffer = new ArraySegment<byte>(buffer);

        using var ms = new MemoryStream(); // For fragmented message assembly

        try
        {
            while (!cancellationToken.IsCancellationRequested && socket.State == WebSocketState.Open)
            {
                var receiveResult = await FetchSocketReceiveResult(socket, connectionId, messageBuffer, cancellationToken).ConfigureAwait(false); ;

                // stop reading if receive failed
                if (receiveResult.IsFailure)
                {
                    logger.LogWarning($"[websocket-handler] Receive loop error for {connectionId}: {receiveResult.Error.Code} - {receiveResult.Error.Description}");
                    break;
                }

                var result = receiveResult.Value;

                // Client requested close, exit loop gracefully
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    logger.LogInformation($"[websocket-handler] Close message received from {connectionId}: {result.CloseStatus} - {result.CloseStatusDescription}");

                    await CloseSocketAsync(socket, result.CloseStatus, result.CloseStatusDescription, cancellationToken).ConfigureAwait(false);
                    break;
                }

                // Accumulate and process message fragments
                await AppendFragmentToMessageBuffer(userId, messageBuffer, ms, result, cancellationToken).ConfigureAwait(false);
            }
        }
        finally
        {
            await CleanupConnectionAsync(socket, connectionId).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Buffers incoming fragments into complete messages, then dispatches them.
    /// </summary>
    private async Task AppendFragmentToMessageBuffer(Guid userId, ArraySegment<byte> messageBuffer, MemoryStream ms, WebSocketReceiveResult result, CancellationToken cancellationToken)
    {
        // accumulate fragment bytes
        ms.Write(messageBuffer.Array!, messageBuffer.Offset, result.Count);

        // message complete, decode & dispatch
        if (result.EndOfMessage)
        {
            ms.Seek(0, SeekOrigin.Begin);

            if (result.MessageType == WebSocketMessageType.Text)
            {
                using var reader = new StreamReader(ms, Encoding.UTF8, leaveOpen: true);
                var textData = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);

                await receiver.ReceiveTextMessage(userId, textData, cancellationToken).ConfigureAwait(false);
            }
            else if (result.MessageType == WebSocketMessageType.Binary)
            {
                var binaryData = ms.ToArray();

                await receiver.ReceiveBinaryMessage(userId, binaryData, cancellationToken).ConfigureAwait(false);
            }

            // reset for next message
            ms.SetLength(0);
        }
    }

    /// <summary>
    /// Receives a message fragment, returning success or failure wrapped in Result.
    /// </summary>
    private async Task<Result<WebSocketReceiveResult>> FetchSocketReceiveResult(
        WebSocket socket,
        string connectionId,
        ArraySegment<byte> messageBuffer,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await socket.ReceiveAsync(messageBuffer, cancellationToken).ConfigureAwait(false);
            return Result.Success(result);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation($"[websocket-handler] Receive loop cancelled for {connectionId}");
            return Result.Failure<WebSocketReceiveResult>(Error.Failure("Receive.Cancelled", "The receive operation was cancelled."));
        }
        catch (WebSocketException ex)
        {
            logger.LogError($"[websocket-handler] WebSocket error on {connectionId}: {ex.Message}", ex);
            return Result.Failure<WebSocketReceiveResult>(Error.Failure("Receive.WebSocketError", $"WebSocket error: {ex.Message}"));
        }
        catch (Exception ex)
        {
            logger.LogError($"[websocket-handler] Unexpected error on {connectionId}: {ex.Message}", ex);
            return Result.Failure<WebSocketReceiveResult>(Error.Failure("Receive.UnexpectedError", $"Unexpected error: {ex.Message}"));
        }
    }

    /// <summary>
    /// Gracefully closes the WebSocket connection if open or closing.
    /// </summary>
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
                logger.LogError("[websocket-handler] Error while closing WebSocket", ex);
            }
        }
    }

    /// <summary>
    /// Cleans up resources and unregisters the connection.
    /// </summary>
    private Task CleanupConnectionAsync(WebSocket socket, string connectionId)
    {
        logger.LogInformation($"[websocket-handler] Cleaning up connection {connectionId}");
        try
        {
            socket.Dispose();
            connectionRegistry.Unregister(connectionId);
        }
        catch (Exception ex)
        {
            logger.LogError($"[websocket-handler] Error during cleanup for {connectionId}", ex);
        }

        return Task.CompletedTask;
    }
}
