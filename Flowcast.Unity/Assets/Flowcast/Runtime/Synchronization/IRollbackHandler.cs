using Flowcast.Logging;
using Flowcast.Serialization;
using System;

namespace Flowcast.Synchronization
{
    public interface IRollbackHandler
    {
        void ApplySnapshot(SnapshotEntry snapshot);
    }

    public class RollbackHandler : IRollbackHandler
    {
        private readonly IGameStateSerializer _serializer;
        private readonly ILogger _logger;
        private readonly RollbackConfig _config;

        public RollbackHandler(IGameStateSerializer serializer, ILogger logger , RollbackConfig config)
        {
            _serializer = serializer;
            _logger = logger;
            _config = config;
        }

        public void ApplySnapshot(SnapshotEntry snapshot)
        {
            var state = _serializer.DeserializeSnapshot(snapshot.Data);

            if (_config.EnableRollbackLog)
                _logger.Log("[Rollback] Game state reverted.");

            _config.OnRollback?.Invoke(state);
        }
    }

    public class RollbackConfig
    {
        public Action<ISerializableGameState> OnRollback { get; set; }
        public bool EnableRollbackLog { get; set; } = false;
    }
}
