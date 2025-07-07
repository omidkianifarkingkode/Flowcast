using Flowcast.Collections;
using Flowcast.Inputs;
using Flowcast.Network;
using Flowcast.Serialization;
using System;
using System.Collections.Generic;

namespace Flowcast.Synchronization
{
    public interface IGameStateSyncService
    {
        event Action<ulong> OnRollback;

        void CaptureAndSyncSnapshot(ulong tick, byte[] serializedState);
        void SetSynced(ulong tick, bool isSynced);
        bool TryGetSnapshot(ulong tick, out SnapshotEntry entry);
        bool TryGetLatestSyncedSnapshot(out SnapshotEntry entry);
        void ClearAfter(ulong tickExclusive);

        bool NeedsRollback();
        void RollbackToVerifiedFrame();
    }

    public class GameStateSyncService : IGameStateSyncService
    {
        public event Action<ulong> OnRollback;

        private readonly CircularBuffer<SnapshotEntry> _buffer;
        private readonly IHasher _hasher;
        private readonly IRollbackHandler _rollbackHandler;
        private readonly ISimulationSyncService _simulationSyncService;

        public GameStateSyncService(IHasher hasher, IRollbackHandler rollbackHandler, ISimulationSyncService simulationSyncService, int maxEntries = 128)
        {
            _hasher = hasher;
            _buffer = new CircularBuffer<SnapshotEntry>(maxEntries);
            _rollbackHandler = rollbackHandler;
            _simulationSyncService = simulationSyncService;

            _simulationSyncService.OnSyncStatusReceived += SetSynced;
            _simulationSyncService.OnRollbackRequested += HandleRollbackRequest;
        }

        public void CaptureAndSyncSnapshot(ulong frame, byte[] serializedState)
        {
            uint hash = ComputeHash(serializedState);

            var entry = new SnapshotEntry
            {
                Tick = frame,
                Data = serializedState,
                Hash = hash,
                IsSynced = false
            };

            _buffer.Add(entry);

            _simulationSyncService.SendStateHash(frame, hash);
        }

        public void SetSynced(ulong frame, bool isSynced)
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

        public void ClearAfter(ulong tickExclusive)
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

        private uint ComputeHash(byte[] data)
        {
            if (data == null || data.Length == 0)
                return 0;

            _hasher.Reset();
            _hasher.WriteBytes(data);
            return _hasher.GetHash();
        }

        public bool NeedsRollback()
        {
            const int graceFrames = 5;

            if (_buffer.Count <= graceFrames)
                return false;

            // Skip newest N frames
            for (int i = graceFrames; i < _buffer.Count; i++)
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
                    _rollbackHandler.ApplySnapshot(entry);
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
                _rollbackHandler.ApplySnapshot(entry);
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
