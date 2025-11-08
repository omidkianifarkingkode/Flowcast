namespace Flowcast.Synchronization
{
    public interface ISnapshotRepository
    {
        void CaptureAndSyncSnapshot(ulong frame);
        bool TryGetSnapshot(ulong frame, out SnapshotEntry entry);
        bool TryGetLatestSyncedSnapshot(out SnapshotEntry entry);
        bool ResetToLastSyncedSnapShot(out SnapshotEntry entry);
    }
}
