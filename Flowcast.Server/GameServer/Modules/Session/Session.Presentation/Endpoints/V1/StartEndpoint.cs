using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Session.Application.Commands;
using Session.Domain;
using Shared.API.Endpoints;
using Shared.Application.Messaging;
using SharedKernel;

namespace Session.Presentation.Endpoints.V1;

public sealed class StartEndpoint : IEndpoint
{
    public const string Method = "POST";
    public const string Route = "/sessions/{sessionId:guid}/start";

    public record Response(
        Guid SessionId,
        string Status,           // Waiting|InProgress|Aborted|Ended
        DateTime? StartedAtUtc
    );

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(Route,
            async ([FromRoute] Guid sessionId,
                   ICommandHandler<TryStartSessionCommand, SessionEntity> handler,
                   HttpContext httpContext, CancellationToken ct) =>
            {
                var command = ToCommand(sessionId);
                var result = await handler.Handle(command, ct);

                return result.Match(
                    session => Results.Ok(ToResponse(session)),
                    error => CustomResults.Problem(error, httpContext)
                );
            })
        .RequireAuthorization()
        .WithTags(Consts.Tag)
        .WithSummary("Start a session")
        .WithDescription("Attempts to transition the session to InProgress.")
        .MapToApiVersion(1.0);
    }

    // Mapping Section

    public static TryStartSessionCommand ToCommand(Guid sessionId)
        => new(new SessionId(sessionId));

    public static Response ToResponse(SessionEntity session)
        => new(
            SessionId: session.Id.Value,
            Status: session.Status.ToString(),
            StartedAtUtc: session.StartedAtUtc
        );
}
