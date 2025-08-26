//using Application.MatchMakings.Shared;
//using Application.Realtime.Messaging;
//using Application.Realtime.Messaging.Contracts;
//using Application.Realtime.Services;
//using Domain.Sessions;
//using Realtime.Transport.Messaging.Sender;
//using Match = Domain.Matchmaking.Match;

//namespace Infrastructure.Persistence.Matchmaking.Services;

//public sealed class MatchmakingNotifier(IRealtimeMessageSender sender, IRealtimeMessageFactory factory) : IMatchmakingNotifier
//{
//    public async Task MatchFound(PlayerId player, Match match, DateTime readyDeadlineUtc, CancellationToken ct)
//    {
//        var payload = new MatchFoundCmd
//        {
//            MatchId = match.Id.Value,
//            Mode = match.Mode,
//            Players = match.Players.Select(p => p.Value).ToArray(),
//            ReadyDeadlineUnixMs = new DateTimeOffset(readyDeadlineUtc).ToUnixTimeMilliseconds()
//        };

//        var message = factory.Create(RealtimeMessageType.MatchFound, payload);

//        await sender.SendToUserAsync(player.Value, message, ct);
//    }

//    public async Task MatchAborted(PlayerId player, Match match, string reason, CancellationToken ct)
//    {
//        var payload = new MatchAbortedCmd
//        {
//            MatchId = match.Id.Value,
//            Reason = reason
//        };

//        var message = factory.Create(RealtimeMessageType.MatchAborted, payload);

//        await sender.SendToUserAsync(player.Value, message, ct);
//    }

//    public async Task MatchConfirmed(PlayerId player, Match match, CancellationToken ct)
//    {
//        var payload = new MatchConfirmedCmd
//        {
//            MatchId = match.Id.Value,
//            Mode = match.Mode,
//            Players = match.Players.Select(p => p.Value).ToArray()
//        };

//        var message = factory.Create(RealtimeMessageType.MatchConfirmed, payload);

//        await sender.SendToUserAsync(player.Value, message, ct);
//    }
//}
