using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flowcast.Player
{
    public class PlayerInfo
    {
        public long Id { get; }
        public int Index { get; }
        public bool IsLocal { get; }

        public PlayerInfo(long id, int index, bool isLocal)
        {
            Id = id;
            Index = index;
            IsLocal = isLocal;
        }
    }


    public interface IPlayerProvider
    {
        long GetLocalPlayerId();
        IReadOnlyList<long> GetAllPlayerIds();
        bool IsLocalPlayer(long playerId);
        int GetPlayerCount();
    }

    public class PlayerProvider : IPlayerProvider
    {
        private long _localPlayerId;
        private IReadOnlyList<long> _playerIds;

        public void Initialize(long localId, IReadOnlyList<long> allIds)
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
