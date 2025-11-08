using FixedMathSharp;
using UnityEngine;
using ILogger = LogKit.ILogger;

namespace Flowcast.Lockstep
{
    public class LockstepProviderUpdate : LockstepProviderBase
    {
        private Fixed64 _accumulatedTime;
        private Fixed64 _frameDuration;

        public LockstepProviderUpdate(ILockstepSettings settings, ILogger logger)
            : base(settings, logger)
        {
            _frameDuration = Fixed64.One / Settings.GameFramesPerSecond;
        }

        public override void Tick()
        {
            _accumulatedTime += (Fixed64)Time.deltaTime * SimulationSpeedMultiplier;

            while (_accumulatedTime >= _frameDuration)
            {
                Step();
                _accumulatedTime -= _frameDuration;
            }
        }

        public override void ResetFrameTo(ulong frame)
        {
            base.ResetFrameTo(frame);
            _accumulatedTime = Fixed64.Zero;
        }

        public float GetDelay()
        {
            var expectedTime = CurrentGameFrame * (float)_frameDuration;
            var actualTime = Time.timeSinceLevelLoad;
            return actualTime - expectedTime;
        }
    }
}
