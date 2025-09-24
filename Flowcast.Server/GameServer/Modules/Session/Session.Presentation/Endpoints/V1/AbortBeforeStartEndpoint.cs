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

public sealed class AbortBeforeStartEndpoint : IEndpoint
{
    public const string Method = "POST";
    public const string Route = "/sessions/{sessionId:guid}/abort";

    public record Request(string Reason = "NoShow");

    public record Response(
        Guid SessionId,
        string Status,            // Aborted (or Ended, depending on your domain)
        DateTime EndedAtUtc,
        string CloseReason        // Completed|NoShow|ServerFailure|AdminTerminate|PlayerAbandon
    );

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(Route,
            async ([FromRoute] Guid sessionId, Request request, ICommandHandler<AbortSessionBeforeStartCommand, SessionEntity> handler,
                   HttpContext httpContext, CancellationToken ct) =>
            {
                var command = ToCommand(sessionId, request);
                var result = await handler.Handle(command, ct);

                return result.Match(
                    session => Results.Ok(ToResponse(session)),
                    error => CustomResults.Problem(error, httpContext)
                );
            })
        .RequireAuthorization() // if admin-only, use a policy: .RequireAuthorization("CanAbortSession")
        .WithTags(Consts.Tag)
        .WithSummary("Abort a session before it starts")
        .WithDescription("Aborts a not-yet-started session. Idempotent: repeated calls return the current state.")
        .MapToApiVersion(1.0);
    }


    private static AbortSessionBeforeStartCommand ToCommand(Guid id, Request request)
        => new(new SessionId(id), request.Reason ?? "NoShow");

    private static Response ToResponse(SessionEntity session)
        => new(
            SessionId: session.Id.Value,
            Status: session.Status.ToString(),
            EndedAtUtc: session.EndedAtUtc!.Value,
            CloseReason: session.CloseReason?.ToString() ?? "NoShow"
        );
}
