using FixedMathSharp;
using Flowcast.Serialization;
using System;
using UnityEngine;

namespace Flowcast.Options
{
    public class LockstepEngineOptionsAsset : ScriptableObject, ILockstepEngineOptions 
    {
        public const string FileName = "LockstepEngineOptions";
        public const string ResourceLoadPath = "Flowcast/" + FileName;

        [field: SerializeField] public int GameFramesPerSecond { get; set; } = 50;

        [field: SerializeField] public int GameFramesPerLockstepTurn { get; set; } = 5;

        public float _minRecoverySpeed = 2;
        public Fixed64 MinRecoverySpeed => (Fixed64)_minRecoverySpeed;

        public float _maxRecoverySpeed = 10;
        public Fixed64 MaxRecoverySpeed => (Fixed64)_maxRecoverySpeed;

        [field: SerializeField] public int FarRecoveryThreshold { get; set; } = 20;


        [field: SerializeField] public int SnapshotHistoryLimit { get; set; } = 20;

        [field: SerializeField] public int DesyncToleranceFrames { get; set; } = 5;

        
        [field: SerializeField] public bool EnableLocalAutoRollback { get; set; } = false;

        [Tooltip("nables debug logging when rollback occurs.")]
        [field: SerializeField] public bool EnableRollbackLog { get; set; } = false;

        public Action<ISerializableGameState, ulong> OnRollback { get; set; }
    }
}
