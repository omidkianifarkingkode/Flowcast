using System.Collections.Generic;

namespace Flowcast.Data
{
    public interface IPlayerProvider
    {
        long GetLocalPlayerId();
        IReadOnlyList<long> GetAllPlayerIds();
        bool IsLocalPlayer(long playerId);
        int GetPlayerCount();
    }

    public class PlayerProvider : IPlayerProvider
    {
        private readonly long _localPlayerId;
        private readonly IReadOnlyList<long> _playerIds;

        public PlayerProvider(long localId, IReadOnlyList<long> allIds)
        {
            _localPlayerId = localId;
            _playerIds = allIds;
        }

        public long GetLocalPlayerId() => _localPlayerId;

        public IReadOnlyList<long> GetAllPlayerIds() => _playerIds;

        public bool IsLocalPlayer(long playerId) => _localPlayerId == playerId;

        public int GetPlayerCount() => _playerIds.Count;
    }

}
