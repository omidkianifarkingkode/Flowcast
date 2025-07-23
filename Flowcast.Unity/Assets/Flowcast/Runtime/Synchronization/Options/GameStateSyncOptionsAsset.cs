using FixedMathSharp;
using Flowcast.Serialization;
using System;
using UnityEngine;

namespace Flowcast.Synchronization
{
    public class GameStateSyncOptionsAsset : ScriptableObject, IGameStateSyncOptions
    {
        [Tooltip("Number of simulation frames per second (e.g., 20 means 50ms per frame).")]
        [field: SerializeField]
        public int GameFramesPerSecond { get; set; } = 50;

        [Tooltip("Number of game frames in one lockstep turn (e.g., 5 = 100ms lockstep).")]
        [field: SerializeField]
        public int GameFramesPerLockstepTurn { get; set; } = 5;

        [Tooltip("Maximum number of snapshots to store in the circular buffer.")]
        [field:SerializeField]
        public int SnapshotHistoryLimit { get; set; } = 20;

        [Tooltip("Number of most recent frames to skip before considering rollback (to tolerate latency jitter).")]
        [field: SerializeField]
        public int DesyncToleranceFrames { get; set; } = 5;

        public Fixed64 MinRecoverySpeed => (Fixed64)_minCatchupSpeed;
        [SerializeField] float _minCatchupSpeed = 2;
        public Fixed64 MaxRecoverySpeed => (Fixed64)_maxCatchupSpeed;
        [SerializeField] float _maxCatchupSpeed = 10;

        [field: SerializeField] 
        public int FarRecoveryThreshold { get; set; } = 20;

        [Tooltip("Whether the client is allowed to auto-rollback without explicit server instruction.")]
        [field: SerializeField]
        public bool EnableLocalAutoRollback { get; set; } = false;

        [Tooltip("nables debug logging when rollback occurs.")]
        [field: SerializeField]
        public bool EnableRollbackLog { get; set; } = false;

        public Action<ISerializableGameState, ulong> OnRollback { get; set; }
    }

    
}
