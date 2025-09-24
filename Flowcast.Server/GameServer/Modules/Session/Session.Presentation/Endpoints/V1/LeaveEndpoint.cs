using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Session.Application.Commands;
using Session.Domain;
using Shared.API.Endpoints;
using Shared.Application.Messaging;
using SharedKernel;
using SharedKernel.Primitives;

namespace Session.Presentation.Endpoints.V1;

public sealed class LeaveEndpoint : IEndpoint
{
    public const string Method = "POST";
    public const string Route = "/sessions/{sessionId:guid}/leave";

    public record Request(Guid PlayerId);

    public record Response(Guid SessionId, Response.PlayerInfo Player, bool WasLastPlayer)
    {
        public record PlayerInfo(Guid Id, string DisplayName);
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(Route,
            async ([FromRoute] Guid sessionId, Request request, ICommandHandler<LeaveSessionCommand, LeaveSessionResult> handler,
                   HttpContext httpContext, CancellationToken ct) =>
        {
            var command = ToCommand(sessionId, request);

            var result = await handler.Handle(command, ct);

            return result.Match(
                leaveResult => Results.Ok(ToResponse(leaveResult)),
                error => CustomResults.Problem(error, httpContext)
            );
        })
        // .RequireAuthorization() // enable a policy if needed
        .WithTags(Consts.Tag)
        .WithSummary("Leave a session")
        .WithDescription("Removes the specified player from the session.")
        .MapToApiVersion(1.0);
    }

    // Mapping Section

    public static LeaveSessionCommand ToCommand(Guid sessionId, Request request)
        => new(
            new SessionId(sessionId),
            new PlayerId(request.PlayerId)
        );

    public static Response ToResponse(LeaveSessionResult leaveResult)
        => new(
            SessionId: leaveResult.Session.Id.Value,
            Player: new Response.PlayerInfo(
                               Id: leaveResult.Participant.Id.Value,
                               DisplayName: leaveResult.Participant.DisplayName),
            WasLastPlayer: leaveResult.WasLastPlayer
        );
}