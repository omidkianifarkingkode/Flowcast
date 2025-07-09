using Flowcast.Commands;
using System;

namespace Flowcast.Monitoring
{
    public enum LogType
    {
        None = 0,
        CommandSent,
        CommandRecieved,
        Rollback,
        Snapshot
    }

    public class FlowcastLogEntry
    {
        public DateTime Timestamp;
        public ulong Turn;        
        public LogType Type;      
    }

    public class CommandLogEntry : FlowcastLogEntry
    {
        public ICommand Command;
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
