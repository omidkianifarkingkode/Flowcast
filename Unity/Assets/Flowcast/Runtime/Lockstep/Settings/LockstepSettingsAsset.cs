using UnityEngine;

namespace Flowcast.Lockstep
{
    [CreateAssetMenu(menuName = "Lockstep/Lockstep Settings")]
    public class LockstepSettingsAsset : ScriptableObject, ILockstepSettings
    {
        [field:SerializeField] public int GameFramesPerSecond { get; set; } = 20;

        [field: SerializeField] public int GameFramesPerLockstepTurn { get; set; } = 5;
    }
}
