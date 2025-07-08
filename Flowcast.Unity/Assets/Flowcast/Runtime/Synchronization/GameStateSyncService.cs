using Flowcast.Collections;
using Flowcast.Commons;
using Flowcast.Network;
using Flowcast.Serialization;
using System;

namespace Flowcast.Synchronization
{
    public interface IGameStateSyncService
    {
        event Action<ulong> OnRollback;

        void CaptureAndSyncSnapshot(ulong frame);
        bool TryGetSnapshot(ulong frame, out SnapshotEntry entry);
        bool TryGetLatestSyncedSnapshot(out SnapshotEntry entry);

        bool NeedsRollback();
        void RollbackToVerifiedFrame();
    }

    public class GameStateSyncService : IGameStateSyncService
    {
        public event Action<ulong> OnRollback;

        private readonly CircularBuffer<SnapshotEntry> _buffer;
        private readonly ISerializableGameState _gameState;
        private readonly IGameStateSerializer _gameStateSerializer;
        private readonly IHasher _hasher;
        private readonly IRollbackHandler _rollbackHandler;
        private readonly INetworkGameStateSyncService _networkSyncService;

        private readonly IGameStateSyncOptions _options;

        public GameStateSyncService(ISerializableGameState gameState, IGameStateSerializer gameStateSerializer, IHasher hasher, IRollbackHandler rollbackHandler, INetworkGameStateSyncService networkSyncService, IGameStateSyncOptions options)
        {
            _options = options;
            _gameState = gameState;
            _gameStateSerializer = gameStateSerializer;
            _hasher = hasher;
            _buffer = new CircularBuffer<SnapshotEntry>(_options.SnapshotHistoryLimit);
            _rollbackHandler = rollbackHandler;
            _networkSyncService = networkSyncService;

            _networkSyncService.OnSyncStatusReceived += SetSynced;
            _networkSyncService.OnRollbackRequested += HandleRollbackRequest;
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

            _networkSyncService.SendStateHash(frame, hash);
        }

        private void SetSynced(ulong frame, bool isSynced)
        {
            for (int i = 0; i < _buffer.Count; i++)
            {
                ref var entry = ref _buffer.RefAt(i);
                if (entry.Tick == frame)
                {
                    entry.IsSynced = true;
                    return;
                }
            }
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

        public bool NeedsRollback()
        {
            if (_buffer.Count <= _options.DesyncToleranceFrames)
                return false;

            // Skip newest N frames
            for (int i = _options.DesyncToleranceFrames; i < _buffer.Count; i++)
            {
                var entry = _buffer.GetAt(i);
                if (!entry.IsSynced)
                    return true;
            }

            return false;
        }

        public void RollbackToVerifiedFrame()
        {
            for (int i = 0; i < _buffer.Count; i++)
            {
                var entry = _buffer.GetAt(i);
                if (entry.IsSynced)
                {
                    _rollbackHandler.Rollback(entry);
                    ClearAfter(entry.Tick);

                    OnRollback?.Invoke(entry.Tick);

                    return;
                }
            }

            throw new InvalidOperationException("No synced snapshot available for rollback.");
        }

        private void HandleRollbackRequest(ulong frame)
        {
            // Optionally verify frame is known
            if (TryGetSnapshot(frame, out var entry))
            {
                _rollbackHandler.Rollback(entry);
                ClearAfter(frame);

                OnRollback?.Invoke(entry.Tick);
            }
            else
            {
                throw new InvalidOperationException($"No snapshot available for rollback to frame {frame}.");
            }
        }
    }
}
