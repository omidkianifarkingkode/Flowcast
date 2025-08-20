using Application.Sessions.Commands;
using Application.Sessions.Queries;
using Contracts.V1.Sessions;
using Domain.Sessions;
using static Contracts.V1.Sessions.Get;

namespace Presentation.Mappings;

public static class CreateSessionMapper
{
    public static CreateSessionCommand MapToCommand(this Create.Request request)
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

        return new (players, request.Mode, settings);
    }

    public static void MapToResponse(this SessionId id, out Create.Response response)
    {
        response = new (id.Value);
    }
}

public static class JoinSessionMapper 
{
    public static void MapToCommand(this Join.Request request, out JoinSessionCommand command)
    {
        command = new (new SessionId(request.SessionId), new PlayerId(request.PlayerId));
    }

    public static void MapToResponse(this JoinSessionResult joinResult, out Join.Response response)
    {
        var player = new Join.Response.PlayerInfo(
            joinResult.Player.Id.Value,
            joinResult.Player.DisplayName
        );

        response = new (joinResult.Session.Id.Value, player);
    }
}

public static class LeaveSessionMapper 
{
    public static void MapToCommand(this Leave.Request request, out LeaveSessionCommand command)
    {
        command = new (
                new SessionId(request.SessionId),
                new PlayerId(request.PlayerId)
            );
    }

    public static void MapToResponse(this LeaveSessionResult leaveResult, out Leave.Response response)
    {
        var player = new Leave.Response.PlayerInfo
            (
                leaveResult.Player.Id.Value,
                leaveResult.Player.DisplayName
            );

        response = new (leaveResult.Session.Id.Value, player, leaveResult.WasLastPlayer);
    }
}

public static class EndSessionMapper 
{
    public static void MapToCommand(this End.Request request, out EndSessionCommand command)
    {
        command = new (new SessionId(request.SessionId));
    }

    public static void MapToResponse(this Session session, out End.Response response)
    {
        response = new (session.Id.Value, session.EndedAtUtc!.Value);
    }
}

public static class PlayerReadyMapper
{
    public static void MapToCommand(this Ready.Request request, out PlayerReadyCommand command)
    {
        command = new(
                new SessionId(request.SessionId),
                new PlayerId(request.PlayerId)
            );
    }

    public static void MapToResponse(this PlayerReadyResult readyResult, out Ready.Response response)
    {
        var player = new Ready.Response.PlayerInfo
            (
                readyResult.Player.Id.Value,
                readyResult.Player.DisplayName
            );

        response = new(readyResult.Session.Id.Value, player, readyResult.AllPlayerReady);
    }
}

public static class GetSessionMapper
{
    public static void MapToQuery(this Guid sessionId, out GetSessionByIdQuery query)
    {
        query = new (new SessionId(sessionId));
    }

    public static void MapToResponse(this Session session, out Get.Response response) 
    {
        var players = session.Players
            .Select(p => new Get.Response.PlayerInfo(p.Id.Value, p.DisplayName, p.Status.ToString()))
            .ToList();

        response = new (session.Id.Value, session.Mode, session.Status.ToString(), session.CreatedAtUtc, players);
    }

    public static void MapToQuery(this Guid playerId, out GetSessionByPlayerQuery query)
    {
        query = new (new PlayerId(playerId));
    }

    public static void MapToResponse(this Session session, out GetByPlayer.Response response) 
    {
        response = new (session.Id.Value, session.Mode, session.Status.ToString(), session.Players.Count, session.CreatedAtUtc);
    }
}
