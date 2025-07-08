using Flowcast.Serialization;
using System;
using UnityEngine;

namespace Flowcast.Synchronization
{
    public class GameStateSyncOptionsAsset : ScriptableObject, IGameStateSyncOptions
    {
        /// <summary>
        /// Maximum number of snapshots to store in the circular buffer.
        /// </summary>
        [field:SerializeField] public int SnapshotHistoryLimit { get; set; } = 128;

        /// <summary>
        /// Number of most recent frames to skip before considering rollback (to tolerate latency jitter).
        /// </summary>
        [field: SerializeField] public int DesyncToleranceFrames { get; set; } = 5;

        /// <summary>
        /// Whether the client is allowed to auto-rollback without explicit server instruction.
        /// </summary>
        [field: SerializeField] public bool EnableLocalAutoRollback { get; set; } = false;

        /// <summary>
        /// Enables debug logging when rollback occurs.
        /// </summary>
        [field: SerializeField] public bool EnableRollbackLog { get; set; } = false;

        /// <summary>
        /// Hook to apply deserialized game state during rollback.
        /// </summary>
        public Action<ISerializableGameState> OnRollback { get; set; }
    }

    
}
