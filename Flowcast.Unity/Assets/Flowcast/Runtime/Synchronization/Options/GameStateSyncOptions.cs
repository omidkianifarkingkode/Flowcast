using Flowcast.Serialization;
using System;

namespace Flowcast.Synchronization
{

    public class GameStateSyncOptions : IGameStateSyncOptions
    {
        /// <summary>
        /// Maximum number of snapshots to store in the circular buffer.
        /// </summary>
        public int SnapshotHistoryLimit { get; set; } = 128;

        /// <summary>
        /// Number of most recent frames to skip before considering rollback (to tolerate latency jitter).
        /// </summary>
        public int DesyncToleranceFrames { get; set; } = 5;

        /// <summary>
        /// Whether the client is allowed to auto-rollback without explicit server instruction.
        /// </summary>
        public bool EnableLocalAutoRollback { get; set; } = false;

        /// <summary>
        /// Enables debug logging when rollback occurs.
        /// </summary>
        public bool EnableRollbackLog { get; set; } = false;

        /// <summary>
        /// Hook to apply deserialized game state during rollback.
        /// </summary>
        public Action<ISerializableGameState> OnRollback { get; set; }
    }
}
