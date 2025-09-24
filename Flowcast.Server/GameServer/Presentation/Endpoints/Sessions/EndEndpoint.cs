using Application.Sessions.Commands;
using Contracts.V1.Sessions;
using Domain.Sessions;
using Presentation.Infrastructure;
using SharedKernel;
using SharedKernel.Messaging;

namespace Presentation.Endpoints.Sessions;

internal sealed class EndEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(End.Route,
            async (End.Request request, ICommandHandler<EndSessionCommand, Session> handler, CancellationToken ct) =>
        {
            var command = ToCommand(request);

            var result = await handler.Handle(command, ct);

            return result.Match(
                session => Results.Ok(ToResponse(session)),
                CustomResults.Problem
            );
        })
        .WithTags(Tags.Sessions)
        .MapToApiVersion(1.0);
    }

    // Mapping Section

    public static EndSessionCommand ToCommand(End.Request request) 
        => new(new SessionId(request.SessionId), request.Reason);

    public static End.Response ToResponse(Session session) 
        => new(
            SessionId: session.Id.Value,
            Status: session.Status.ToString(),
            EndedAtUtc: session.EndedAtUtc!.Value,
            CloseReason: session.CloseReason?.ToString() ?? string.Empty
        );
}
