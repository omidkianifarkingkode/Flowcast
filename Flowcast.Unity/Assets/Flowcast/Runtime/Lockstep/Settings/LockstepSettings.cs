using System;
using UnityEngine;

namespace Flowcast.Lockstep
{
    [Serializable]
    public class LockstepSettings : ILockstepSettings
    {
        public int GameFramesPerSecond { get; set; } = 20;

        public int GameFramesPerLockstepTurn { get; set; } = 5;

        public float MinCatchupSpeed { get; set; } = 1.5f;
        public float MaxCatchupSpeed { get; set; } = 5.0f;
        public int FarRollbackThreshold { get; set; } = 20;
    }
}
