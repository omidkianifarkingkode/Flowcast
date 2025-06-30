namespace Flowcast.Synchronization
{
    public struct SnapshotEntry
    {
        public ulong Tick;
        public byte[] Data;
        public uint Hash;
        public bool IsSynced;
    }
}
