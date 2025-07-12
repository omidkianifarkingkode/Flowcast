using UnityEngine;

namespace Flowcast.Lockstep
{
    [CreateAssetMenu(fileName = FileName, menuName = CreateAssetMenuPath)]
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

        [field: SerializeField] public float MinCatchupSpeed { get; set; } = 1.5f; 
        [field: SerializeField] public float MaxCatchupSpeed { get; set; } = 5.0f; 
        [field: SerializeField] public int FarRollbackThreshold { get; set; } = 20;
    }
}
