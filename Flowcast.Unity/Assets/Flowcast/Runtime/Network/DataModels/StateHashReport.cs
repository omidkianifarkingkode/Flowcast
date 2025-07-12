namespace Flowcast.Network
{
    public class StateHashReport
    {
        public ulong Frame { get; set; }
        public uint Hash { get; set; }
    }

    public class SyncStatus
    {
        public ulong Frame { get; set; }
        public bool IsSynced { get; set; }
    }

    public class RollbackRequest
    {
        public ulong CurrentNetworkFrame { get; set; }
        public string Reason { get; set; }
    }
}
