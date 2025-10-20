using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Session.Application.Commands;
using Session.Domain;
using Shared.Application.Messaging;
using Shared.Presentation.Endpoints;
using SharedKernel;
using SharedKernel.Primitives;

namespace Session.Presentation.Endpoints.V1;

public sealed class JoinEndpoint : IEndpoint
{
    public const string Method = "POST";
    public const string Route = "/sessions/{sessionId:guid}/join";

    public record Request(Guid PlayerId, string JoinToken, string? BuildHash = null, string? DisplayName = null);

    public record Response(Guid SessionId, Response.ParticipantInfo Participant, string SessionStatus)
    {
        public record ParticipantInfo(Guid Id, string DisplayName, string Status);
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(Route,
            async ([FromRoute] Guid sessionId, Request request, ICommandHandler<JoinSessionCommand, JoinSessionResult> handler,
                   HttpContext httpContext, CancellationToken ct) =>
        {
            var command = ToCommand(sessionId, request);

            var result = await handler.Handle(command, ct);

            return result.Match(
                joinResult => Results.Ok(ToResponse(joinResult)),
                error => CustomResults.Problem(error, httpContext)
            );
        })
        // .RequireAuthorization() // enable if joining requires auth beyond the join token
        .WithTags(Consts.Tag)
        .WithSummary("Join a session")
        .WithDescription("Adds a participant to the session using a join token.")
        .MapToApiVersion(1.0);
    }

    // Mapping Section

    public static JoinSessionCommand ToCommand(Guid sessionId, Request request)
        => new(
            SessionId: new SessionId(sessionId),
            PlayerId: new PlayerId(request.PlayerId),
            JoinToken: request.JoinToken,
            BuildHash: request.BuildHash,
            DisplayName: request.DisplayName
        );

    public static Response ToResponse(JoinSessionResult joinResult)
        => new(
            SessionId: joinResult.Session.Id.Value,
            Participant: new Response.ParticipantInfo(
                Id: joinResult.Participant.Id.Value,
                DisplayName: joinResult.Participant.DisplayName,
                Status: joinResult.Participant.Status.ToString()
            ),
            SessionStatus: joinResult.Session.Status.ToString()
        );
}
