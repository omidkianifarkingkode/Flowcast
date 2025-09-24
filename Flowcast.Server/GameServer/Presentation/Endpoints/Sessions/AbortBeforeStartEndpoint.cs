using Application.Sessions.Commands;
using Contracts.V1.Sessions;
using Domain.Sessions;
using Presentation.Infrastructure;
using SharedKernel;
using SharedKernel.Messaging;

namespace Presentation.Endpoints.Sessions;

internal sealed class AbortBeforeStartEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(AbortBeforeStart.Route,
            async (AbortBeforeStart.Request request,
                   ICommandHandler<AbortSessionBeforeStartCommand, Session> handler,
                   CancellationToken ct) =>
            {
                // Prefer route's sessionId as source of truth; body carries Reason
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

    public static AbortSessionBeforeStartCommand ToCommand(AbortBeforeStart.Request request)
        => new(new SessionId(request.SessionId), request.Reason ?? "NoShow");

    public static AbortBeforeStart.Response ToResponse(Session session)
        => new(
            SessionId: session.Id.Value,
            Status: session.Status.ToString(),
            EndedAtUtc: session.EndedAtUtc!.Value,
            CloseReason: session.CloseReason?.ToString() ?? "NoShow"
        );
}
