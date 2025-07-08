using Flowcast.Serialization;
using System;

namespace Flowcast.Synchronization
{
    public interface IGameStateSyncOptions
    {
        /// <summary>
        /// Maximum number of snapshots to store in the circular buffer.
        /// </summary>
        int SnapshotHistoryLimit { get; set; }

        /// <summary>
        /// Number of most recent frames to skip before considering rollback (to tolerate latency jitter).
        /// </summary>
        int DesyncToleranceFrames { get; set; }

        /// <summary>
        /// Whether the client is allowed to auto-rollback without explicit server instruction.
        /// </summary>
        bool EnableLocalAutoRollback { get; set; }

        /// <summary>
        /// Enables debug logging when rollback occurs.
        /// </summary>
        bool EnableRollbackLog { get; set; }

        /// <summary>
        /// Hook to apply deserialized game state during rollback.
        /// </summary>
        Action<ISerializableGameState> OnRollback { get; set; }
    }

    
}
