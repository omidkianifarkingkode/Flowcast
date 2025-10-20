using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Session.Application.Commands;
using Session.Domain;
using Shared.Application.Messaging;
using Shared.Presentation.Endpoints;
using SharedKernel;

namespace Session.Presentation.Endpoints.V1;

public sealed class EndEndpoint : IEndpoint
{
    public const string Method = "POST";
    public const string Route = "/sessions/{sessionId:guid}/end";

    public record Request(string? Reason = null);

    public record Response([FromRoute] Guid SessionId, string Status, DateTime EndedAtUtc, string CloseReason);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(Route,
            async (Guid sessionId, Request request, ICommandHandler<EndSessionCommand, SessionEntity> handler,
                   HttpContext httpContext, CancellationToken ct) =>
        {
            var command = ToCommand(sessionId, request);
            var result = await handler.Handle(command, ct);

            return result.Match(
                session =>
                {
                    if (session.EndedAtUtc is null)
                        return CustomResults.Problem(Error.Conflict("session.not_ended",
                            "Session is not in an ended state."));

                    return Results.Ok(ToResponse(session));
                },
                error => CustomResults.Problem(error, httpContext)
            );
        })
       .RequireAuthorization() 
       .WithTags(Consts.Tag)
       .WithSummary("End a session")
       .WithDescription("Transitions a session to an ended state with an optional reason.")
       .MapToApiVersion(1.0);
    }


    private static EndSessionCommand ToCommand(Guid sessionId, Request req)
        => new(new SessionId(sessionId), req.Reason);

    private static Response ToResponse(SessionEntity session)
        => new(
            SessionId: session.Id.Value,
            Status: session.Status.ToString(),
            EndedAtUtc: session.EndedAtUtc!.Value,
            CloseReason: session.CloseReason?.ToString() ?? string.Empty
        );
}
