using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Session.Application.Commands;
using Session.Domain;
using Shared.Application.Messaging;
using Shared.Presentation.Endpoints;
using SharedKernel;

namespace Session.Presentation.Endpoints.V1;

public sealed class CreateEndpoint : IEndpoint
{
    public const string Method = "POST";
    public const string Route = "/sessions";

    public record Request(
        string Mode,
        List<Request.Player> Players,
        Request.MatchSettings? GameSettings,
        string StartBarrier = "ConnectedAndLoaded",   // ConnectedOnly|ConnectedAndLoaded|Timer
        int? JoinDeadlineSeconds = 15                 // ignored unless Waiting
        )
    {
        public record Player(Guid Id, string DisplayName);
        public record MatchSettings
        {
            public int TickRate { get; init; } = 60;
            public int InputDelayFrames { get; init; } = 2;
        }
    }

    public record Response(Guid SessionId);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(Route,
            async (Request request, ICommandHandler<CreateSessionCommand, SessionId> handler,
                   HttpContext httpContext, CancellationToken ct) =>
        {
            var command = ToCommand(request);

            var result = await handler.Handle(command, ct);

            return result.Match(
                id => Results.Created($"{Route}/{id.Value}", new Response(id.Value)),
                error => CustomResults.Problem(error, httpContext)
            );
        })
        .RequireAuthorization()
        .WithTags(Consts.Tag)
        .WithSummary("Create a game session")
        .WithDescription("Creates a session using the provided players and settings.")
        .MapToApiVersion(1.0);
    }


    private static CreateSessionCommand ToCommand(Request request)
    {
        var players = request.Players
            .Select(p => new CreateSessionCommand.PlayerInfo(p.Id, p.DisplayName))
            .ToList();

        MatchSettings? settings = request.GameSettings is null
            ? null
            : new MatchSettings
            {
                TickRate = request.GameSettings.TickRate,
                InputDelayFrames = request.GameSettings.InputDelayFrames
            };

        return new CreateSessionCommand(players, request.Mode, settings);
    }
}