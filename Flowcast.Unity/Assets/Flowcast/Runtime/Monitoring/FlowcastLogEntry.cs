using Flowcast.Inputs;
using System;

namespace Flowcast.Monitoring
{
    public enum LogType
    {
        None = 0,
        InputSent,
        InputRecieved,
        Rollback,
        Snapshot
    }

    public class FlowcastLogEntry
    {
        public DateTime Timestamp;
        public ulong Turn;        
        public LogType Type;      
    }

    public class InputLogEntry : FlowcastLogEntry
    {
        public IInput Input;
    }

    public class RollbackLogEntry : FlowcastLogEntry
    {
        public ulong ToTurn;
    }

    public class SnapshotLogEntry : FlowcastLogEntry
    {
        public uint Hash;
        public bool IsSynced;
    }
}
