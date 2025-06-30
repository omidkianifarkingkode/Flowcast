using Flowcast.Collections;
using Flowcast.Serialization;
using System;

namespace Flowcast.Synchronization
{
    public interface IGameStateSyncService
    {
        void SaveSnapshot(ulong tick, byte[] serializedState);
        void SetSynced(ulong tick, bool isSynced);
        bool TryGetSnapshot(ulong tick, out SnapshotEntry entry);
        bool TryGetLatestSyncedSnapshot(out SnapshotEntry entry);
        void ClearAfter(ulong tickExclusive);

        bool NeedsRollback();
        void RollbackToVerifiedFrame();
    }

    public class GameStateSyncService : IGameStateSyncService
    {
        private readonly CircularBuffer<SnapshotEntry> _buffer;
        private readonly IHasher _hasher;
        private readonly IRollbackHandler _rollbackHandler;

        public GameStateSyncService(IHasher hasher, IRollbackHandler rollbackHandler, int maxEntries = 128)
        {
            _hasher = hasher;
            _buffer = new CircularBuffer<SnapshotEntry>(maxEntries);
            _rollbackHandler = rollbackHandler;
        }

        public void SaveSnapshot(ulong tick, byte[] serializedState)
        {
            uint hash = ComputeHash(serializedState);

            var entry = new SnapshotEntry
            {
                Tick = tick,
                Data = serializedState,
                Hash = hash,
                IsSynced = false
            };

            _buffer.Add(entry);
        }

        public void SetSynced(ulong tick, bool isSynced)
        {
            for (int i = 0; i < _buffer.Count; i++)
            {
                ref var entry = ref _buffer.RefAt(i);
                if (entry.Tick == tick)
                {
                    entry.IsSynced = true;
                    return;
                }
            }
        }

        public bool TryGetSnapshot(ulong tick, out SnapshotEntry entry)
        {
            for (int i = 0; i < _buffer.Count; i++)
            {
                entry = _buffer.GetAt(i);
                if (entry.Tick == tick)
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
            _hasher.Reset();

            // Treat the serialized state as a raw byte stream
            for (int i = 0; i < data.Length; i += 4)
            {
                uint chunk = 0;

                if (i + 3 < data.Length)
                    chunk = BitConverter.ToUInt32(data, i);
                else
                {
                    for (int j = 0; j < data.Length - i; j++)
                    {
                        chunk |= (uint)(data[i + j] << (8 * j));
                    }
                }

                _hasher.Write(chunk);
            }

            return _hasher.GetHash();
        }

        public bool NeedsRollback()
        {
            if (_buffer.Count == 0)
                return false;

            var latest = _buffer.GetAt(0); // Newest is index 0
            return !latest.IsSynced;
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
                    return;
                }
            }

            throw new InvalidOperationException("No synced snapshot available for rollback.");
        }
    }
}
