//using Application.Abstractions.Authentication;
//using Application.Abstractions.Messaging;
//using Application.Abstractions.Realtime;
//using Application.MatchMakings.Commands;
//using Domain.Sessions.ValueObjects;
//using Domain.Users.Services;
//using Microsoft.AspNetCore.SignalR;

//public class GameHub(IUserConnectionRegistry connectionRegistry, IUserContext userContext, IServiceProvider serviceProvider) : Hub
//{
//    public override async Task OnConnectedAsync()
//    {
//        var userId = userContext.GetUserId(Context.GetHttpContext()!);

//        if (userId != null)
//        {
//            // Check if user already connected
//            var existingConnections = connectionRegistry.GetConnectionsForUser(userId.Value);

//            if (existingConnections.Any())
//            {
//                // Handle reconnection
//                connectionRegistry.Register(Context.ConnectionId, userId.Value);
//            }
//            else
//            {
//                // First connection
//                connectionRegistry.Register(Context.ConnectionId, userId.Value);
//            }
//        }

//        await base.OnConnectedAsync();
//    }

//    public override Task OnDisconnectedAsync(Exception? exception)
//    {
//        connectionRegistry.Unregister(Context.ConnectionId);
//        return base.OnDisconnectedAsync(exception);
//    }

//    public async Task RequestMatchmaking()
//    {
//        var userId = userContextService.GetUserId(Context.GetHttpContext()!);

//        if (userId == null)
//        {
//            Context.Abort();
//            return;
//        }

//        var command = new RequestMatchmakingCommand(new PlayerId(userId.Value));
//        var handler = serviceProvider.GetRequiredService<ICommandHandler<RequestMatchmakingCommand>>();
//        var result = await handler.Handle(command, CancellationToken.None);

//        if (result.IsFailure)
//        {
//            await Clients.Caller.SendAsync("MatchmakingFailed", result.Error);
//            return;
//        }

//        await Clients.Caller.SendAsync("MatchmakingQueued");
//    }
//}
