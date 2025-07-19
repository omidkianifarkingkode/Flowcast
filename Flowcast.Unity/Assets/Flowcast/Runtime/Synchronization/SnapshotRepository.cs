using Flowcast.Collections;
using Flowcast.Commons;
using Flowcast.Logging;
using Flowcast.Network;
using Flowcast.Serialization;
using System;

namespace Flowcast.Synchronization
{

    public class SnapshotRepository : ISnapshotRepository
    {
        private readonly CircularBuffer<SnapshotEntry> _buffer;
        private readonly IGameStateSerializer _gameStateSerializer;
        private readonly IHasher _hasher;
        private readonly INetworkGameStateSyncService _networkSyncService;

        private readonly IGameStateSyncOptions _options;
        private readonly ILogger _logger;

        public SnapshotRepository(IGameStateSerializer gameStateSerializer, IHasher hasher, INetworkGameStateSyncService networkSyncService, IGameStateSyncOptions options, ILogger logger)
        {
            _options = options;
            _logger = logger;
            _gameStateSerializer = gameStateSerializer;
            _hasher = hasher;
            _buffer = new CircularBuffer<SnapshotEntry>(_options.SnapshotHistoryLimit);
            _networkSyncService = networkSyncService;

            _networkSyncService.OnSyncStatusReceived += SetSynced;
        }

        public void CaptureAndSyncSnapshot(ulong frame)
        {
            var serializedGameState = _gameStateSerializer.SerializeSnapshot();

            uint hash = _hasher.ComputeHash(serializedGameState);

            var entry = new SnapshotEntry
            {
                Tick = frame,
                Data = serializedGameState,
                Hash = hash,
                IsSynced = false
            };

            _buffer.Add(entry);

            _networkSyncService.SendStateHash(new StateHashReport
            {
                Frame = frame,
                Hash = hash
            });
        }

        public bool TryGetSnapshot(ulong frame, out SnapshotEntry entry)
        {
            for (int i = 0; i < _buffer.Count; i++)
            {
                entry = _buffer.GetAt(i);
                if (entry.Tick == frame)
                    return true;
            }

            entry = default;
            return false;
        }

        public bool TryGetLatestSyncedSnapshot(out SnapshotEntry entry)
        {
            for (int i = 0; i < _buffer.Count; i++)
            {
                var e = _buffer.GetAt(i);
                if (e.IsSynced)
                {
                    entry = e;
                    return true;
                }
            }

            entry = default;
            return false;
        }

        public bool ResetToLastSyncedSnapShot(out SnapshotEntry entry)
        {
            if (!TryGetLatestSyncedSnapshot(out entry))
            {
                _buffer.Clear();
                return false;
            }

            ClearAfter(entry.Tick);

            return true;
        }

        private void SetSynced(SyncStatus syncStatus)
        {
            for (int i = 0; i < _buffer.Count; i++)
            {
                ref var entry = ref _buffer.RefAt(i);
                if (entry.Tick == syncStatus.Frame)
                {
                    entry.IsSynced = syncStatus.IsSynced;
                    return;
                }
            }
        }

        private void ClearAfter(ulong tickExclusive)
        {
            int kept = 0;

            for (int i = 0; i < _buffer.Count; i++)
            {
                ref var entry = ref _buffer.RefAt(i);
                if (entry.Tick <= tickExclusive)
                {
                    kept++;
                }
                else
                {
                    entry = default;
                }
            }

            _buffer.TrimToLatest(kept);
        }
    }
}
