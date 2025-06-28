using System;

namespace Flowcast.Lockstep
{
    [Serializable]
    public class LockstepSettings : ILockstepSettings
    {
        public int GameFramesPerSecond { get; set; } = 20;

        public int GameFramesPerLockstepTurn { get; set; } = 5;
    }
}
