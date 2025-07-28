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

        private static LockstepEngineOptionsAsset _instance;

        public static LockstepEngineOptionsAsset Load() 
        {
            _instance ??= Resources.Load<LockstepEngineOptionsAsset>(ResourceLoadPath);

            if (_instance == null)
                throw new MissingReferenceException($"LockstepSettingsAsset could not be found at Resources/{ResourceLoadPath}");

            return _instance;
        }

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
        public Action<ulong, Fixed64> OnStep { get; set; }
    }
}
