using FixedMathSharp;
using System;
using UnityEngine;

namespace Flowcast.Lockstep
{
    [Serializable]
    public class LockstepSettings : ILockstepSettings
    {
        public int GameFramesPerSecond { get; set; } = 20;

        public int GameFramesPerLockstepTurn { get; set; } = 5;

        public Fixed64 MinRecoverySpeed { get; set; } = Fixed64.Two;
        public Fixed64 MaxRecoverySpeed { get; set; } = Fixed64.One * 10;
        public int FarRecoveryThreshold { get; set; } = 20;
    }
}
