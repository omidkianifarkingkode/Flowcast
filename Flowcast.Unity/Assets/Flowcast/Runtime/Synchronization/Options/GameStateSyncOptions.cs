using Flowcast.Serialization;
using System;
using UnityEngine;

namespace Flowcast.Synchronization
{
    [Serializable]
    public class GameStateSyncOptions : IGameStateSyncOptions
    {
        public int GameFramesPerSecond { get; set; } = 50;

        public int GameFramesPerLockstepTurn { get; set; } = 5;
        
        public int SnapshotHistoryLimit { get; set; } = 20;

        public int DesyncToleranceFrames { get; set; } = 5;

        public float MinCatchupSpeed { get; set; } = 1.5f;
        public float MaxCatchupSpeed { get; set; } = 5.0f;
        public int FarRollbackThreshold { get; set; } = 20;

        public bool EnableLocalAutoRollback { get; set; } = false;

        public bool EnableRollbackLog { get; set; } = false;

        [Newtonsoft.Json.JsonIgnore]
        public Action<ISerializableGameState, ulong> OnRollback { get; set; }
    }
}
