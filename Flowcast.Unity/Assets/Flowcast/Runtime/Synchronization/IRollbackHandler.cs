using Flowcast.Logging;
using Flowcast.Serialization;
using System;

namespace Flowcast.Synchronization
{
    public interface IRollbackHandler
    {
        void Rollback(SnapshotEntry snapshot);
    }

    public class RollbackHandler : IRollbackHandler
    {
        private readonly IGameStateSerializer _serializer;
        private readonly ILogger _logger;
        private readonly IGameStateSyncOptions _options;

        public RollbackHandler(IGameStateSerializer serializer, ILogger logger , IGameStateSyncOptions options)
        {
            _serializer = serializer;
            _logger = logger;
            _options = options;
        }

        public void Rollback(SnapshotEntry snapshot)
        {
            var state = _serializer.DeserializeSnapshot(snapshot.Data);

            if (_options.EnableRollbackLog)
                _logger.Log("[Rollback] Game state reverted.");

            _options.OnRollback?.Invoke(state);
        }
    }
}
