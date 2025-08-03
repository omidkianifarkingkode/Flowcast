using Application.Sessions.Commands;
using Contracts.V1.Sessions;
using Domain.Sessions;
using Domain.ValueObjects;
using static Contracts.V1.Sessions.Get;

namespace Presentation.Mappings;

public static class SessionMappings
{
    public static CreateSessionCommand ToCommand(this Create.Request request)
    {
        var players = request.Players
            .Select(p => new CreateSessionCommand.PlayerInput(p.Id, p.DisplayName))
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

    public static Create.Response ToResponse(this SessionId id)
        => new(id.Value);

    public static EndSessionCommand ToCommand(this Guid sessionId)
        => new(SessionId.FromGuid(sessionId));

    public static Get.Response ToGetResponse(this Session session) 
    {
        var players = session.Players
            .Select(p => new PlayerResponse(p.PlayerId, p.DisplayName, p.Status.ToString()))
            .ToList();

        return new Response(session.Id.Value, session.Mode, session.Status.ToString(), session.CreatedAtUtc, players);
    }

    public static GetForPlayer.Response ToGetForPlayerResponse(this Session session) 
    {
        return new GetForPlayer.Response(session.Id.Value, session.Mode, session.Status.ToString(), session.Players.Count, session.CreatedAtUtc);
    }
}
