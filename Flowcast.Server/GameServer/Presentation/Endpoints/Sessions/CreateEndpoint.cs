using Application.Abstractions.Messaging;
using Application.Sessions.Commands;
using Contracts.V1.Sessions;
using Domain.Sessions;
using Presentation.Infrastructure;
using SharedKernel;

namespace Presentation.Endpoints.Sessions;

internal sealed class CreateEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(Create.Route,
            async (Create.Request request, ICommandHandler<CreateSessionCommand, SessionId> handler, CancellationToken ct) =>
        {
            var command = ToCommand(request);

            var result = await handler.Handle(command, ct);

            return result.Match(
                id => Results.Ok(ToResponse(id)),
                CustomResults.Problem
            );
        })
        .WithTags(Tags.Sessions)
        .MapToApiVersion(1.0);
    }

    // Mapping Section

    public static CreateSessionCommand ToCommand(Create.Request request)
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

    public static Create.Response ToResponse(SessionId id) => new(id.Value);
}