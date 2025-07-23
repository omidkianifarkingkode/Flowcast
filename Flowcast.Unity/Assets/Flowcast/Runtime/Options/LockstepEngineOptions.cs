using FixedMathSharp;
using Flowcast.Serialization;
using System;

namespace Flowcast.Options
{
    public class LockstepEngineOptions : ILockstepEngineOptions 
    {
        public int GameFramesPerSecond { get; set; } = 50;
        public int GameFramesPerLockstepTurn { get; set; } = 5;

        public Fixed64 MinRecoverySpeed { get; set; } = Fixed64.Two;
        public Fixed64 MaxRecoverySpeed { get; set; } = Fixed64.One * 5;
        public int FarRecoveryThreshold { get; set; } = 20;

        public int SnapshotHistoryLimit { get; set; } = 50;
        public int DesyncToleranceFrames { get; set; } = 5;

        public bool EnableLocalAutoRollback { get; set; } = false;
        public bool EnableRollbackLog { get; set; } = false;

        [Newtonsoft.Json.JsonIgnore]
        public Action<ISerializableGameState, ulong> OnRollback { get; set; }
    }
}
