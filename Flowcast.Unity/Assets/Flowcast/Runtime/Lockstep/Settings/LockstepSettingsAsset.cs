using FixedMathSharp;
using UnityEngine;

namespace Flowcast.Lockstep
{
    [CreateAssetMenu(fileName = FileName)]
    public partial class LockstepSettingsAsset : ScriptableObject, ILockstepSettings
    {
        public const string FileName = "LockstepSettings";
        public const string ResourceLoadPath = "Flowcast/" + FileName;

        private static ILockstepSettings _instance;
        public static ILockstepSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<LockstepSettingsAsset>(ResourceLoadPath);
                    if (_instance == null)
                        Debug.LogError($"LockstepSettingsAsset not found at Resources/{ResourceLoadPath}");
                }
                return _instance;
            }
        }

        [field:SerializeField] public int GameFramesPerSecond { get; set; } = 20;

        [field: SerializeField] public int GameFramesPerLockstepTurn { get; set; } = 5;

        public Fixed64 MinRecoverySpeed => (Fixed64)_minCatchupSpeed;
        [SerializeField] float _minCatchupSpeed = 2;
        public Fixed64 MaxRecoverySpeed => (Fixed64)_maxCatchupSpeed;
        [SerializeField] float _maxCatchupSpeed = 10;
        [field: SerializeField] public int FarRecoveryThreshold { get; set; } = 20;
    }
}
