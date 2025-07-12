using Flowcast.Collections;
using Flowcast.Commons;
using Flowcast.Logging;
using Flowcast.Network;
using Flowcast.Serialization;
using System;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

namespace Flowcast.Synchronization
{
    public interface IGameStateSyncService
    {
        event Action<ulong> OnRollback;

        bool IsRollbackPending { get; }
        ulong TargetRecoveryFrame { get; }

        void CaptureAndSyncSnapshot(ulong frame);
        bool TryGetSnapshot(ulong frame, out SnapshotEntry entry);
        bool TryGetLatestSyncedSnapshot(out SnapshotEntry entry);
        void RollbackToVerifiedFrame();
        ulong ApplyPendingRollback(float speedMultiplier);
    }

    public class GameStateSyncService : IGameStateSyncService
    {
        public event Action<ulong> OnRollback;

        public bool IsRollbackPending { get; private set; }
        public ulong TargetRecoveryFrame { get; private set; }

        private readonly CircularBuffer<SnapshotEntry> _buffer;
        private readonly IGameStateSerializer _gameStateSerializer;
        private readonly IHasher _hasher;
        private readonly IRollbackHandler _rollbackHandler;
        private readonly INetworkGameStateSyncService _networkSyncService;

        private readonly IGameStateSyncOptions _options;
        private readonly ILogger _logger;

        private RollbackRequest _pendingRollbackRequest;


        public GameStateSyncService(IGameStateSerializer gameStateSerializer,
                                    IHasher hasher,
                                    IRollbackHandler rollbackHandler,
                                    INetworkGameStateSyncService networkSyncService,
                                    IGameStateSyncOptions options,
                                    ILogger logger)
        {
            _options = options;
            _logger = logger;
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

            _networkSyncService.SendStateHash(new StateHashReport 
            {
                Frame = frame,
                Hash = hash 
            });

            if (_options.EnableLocalAutoRollback)
                CheckStateAndRollback();
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

        public ulong ApplyPendingRollback(float speedMultiplier)
        {
            if (!TryGetLatestSyncedSnapshot(out var entry))
                throw new InvalidOperationException("No synced snapshot available for rollback.");

            _rollbackHandler.Rollback(entry);
            ClearAfter(entry.Tick);
            IsRollbackPending = false;

            TargetRecoveryFrame = CatchupEstimator.EstimateTargetFrame(rollbackStartFrame: entry.Tick,
                                                                       networkTargetFrame: _pendingRollbackRequest.CurrentNetworkFrame,
                                                                       speedMultiplier: speedMultiplier,
                                                                       gameFps: _options.GameFramesPerSecond);

            _logger.Log($"[Recovery] Rolled back to frame {entry.Tick}. Target to catch up: {TargetRecoveryFrame}");

            OnRollback?.Invoke(entry.Tick);

            return TargetRecoveryFrame;
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

        private bool CheckStateAndRollback() 
        {
            if (NeedsRollback())
            {
                _logger.LogWarning("State desync detected. Rolling back...");
                RollbackToVerifiedFrame();

                return true;
            }

            return false;
        }

        private bool NeedsRollback()
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

        private void HandleRollbackRequest(RollbackRequest rollbackRequest)
        {
            IsRollbackPending = true;
            _pendingRollbackRequest = rollbackRequest;
        }
    }
}
