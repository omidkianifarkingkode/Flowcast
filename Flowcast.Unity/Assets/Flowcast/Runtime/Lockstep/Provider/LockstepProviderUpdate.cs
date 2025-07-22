using UnityEngine;
using ILogger = LogKit.ILogger;

namespace Flowcast.Lockstep
{
    public class LockstepProviderUpdate : LockstepProviderBase
    {
        private float _accumulatedTime;
        private float _frameDuration;

        public LockstepProviderUpdate(ILockstepSettings settings, ILogger logger)
            : base(settings, logger)
        {
            _frameDuration = 1f / Settings.GameFramesPerSecond;
        }

        public override void Tick()
        {
            _accumulatedTime += Time.deltaTime * SimulationSpeedMultiplier;

            while (_accumulatedTime >= _frameDuration)
            {
                Step();
                _accumulatedTime -= _frameDuration;
            }
        }

        public override void ResetFrameTo(ulong frame)
        {
            base.ResetFrameTo(frame);
            _accumulatedTime = 0f;
        }

        public float GetDelay()
        {
            var expectedTime = (long)CurrentGameFrame * _frameDuration;
            var actualTime = Time.timeSinceLevelLoad;
            return actualTime - expectedTime;
        }
    }
}
