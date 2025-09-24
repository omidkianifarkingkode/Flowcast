using Application.Sessions.Commands;
using Contracts.V1.Sessions;
using Domain.Sessions;
using Presentation.Infrastructure;
using SharedKernel;
using SharedKernel.Messaging;

namespace Presentation.Endpoints.Sessions;

internal sealed class JoinEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(Join.Route,
            async (Join.Request request, ICommandHandler<JoinSessionCommand, JoinSessionResult> handler, CancellationToken ct) =>
        {
            var command = ToCommand(request);

            var result = await handler.Handle(command, ct);

            return result.Match(
                joinResult => Results.Ok(ToResponse(joinResult)),
                CustomResults.Problem
            );
        })
        .WithTags(Tags.Sessions)
        .MapToApiVersion(1.0);
    }

    // Mapping Section

    public static JoinSessionCommand ToCommand(Join.Request request)
        => new(
            SessionId: new SessionId(request.SessionId),
            PlayerId: new PlayerId(request.PlayerId),
            JoinToken: request.JoinToken,
            BuildHash: request.BuildHash,
            DisplayName: request.DisplayName
        );

    public static Join.Response ToResponse(JoinSessionResult joinResult)
        => new(
            SessionId: joinResult.Session.Id.Value,
            Participant: new Join.Response.ParticipantInfo(
                Id: joinResult.Participant.Id.Value,
                DisplayName: joinResult.Participant.DisplayName,
                Status: joinResult.Participant.Status.ToString()
            ),
            SessionStatus: joinResult.Session.Status.ToString()
        );
}
