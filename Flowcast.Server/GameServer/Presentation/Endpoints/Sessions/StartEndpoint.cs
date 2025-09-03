using Application.Abstractions.Messaging;
using Application.Sessions.Commands;
using Contracts.V1.Sessions;
using Domain.Sessions;
using Presentation.Infrastructure;
using SharedKernel;

namespace Presentation.Endpoints.Sessions;

internal sealed class StartEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(Start.Route,
            async (Start.Request request,
                   ICommandHandler<TryStartSessionCommand, Session> handler,
                   CancellationToken ct) =>
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

    public static TryStartSessionCommand ToCommand(Start.Request request)
        => new(new SessionId(request.SessionId));

    public static Start.Response ToResponse(Session session)
        => new(
            SessionId: session.Id.Value,
            Status: session.Status.ToString(),
            StartedAtUtc: session.StartedAtUtc
        );
}
