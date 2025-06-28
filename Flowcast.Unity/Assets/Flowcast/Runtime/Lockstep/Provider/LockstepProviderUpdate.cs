using UnityEngine;
using ILogger = Flowcast.Logging.ILogger;

namespace Flowcast.Lockstep
{
    public class LockstepProviderUpdate : LockstepProviderBase
    {
        private float _accumulatedTime = 0f;
        private float _frameDuration;

        public LockstepProviderUpdate(ILockstepSettings settings, ILogger logger)
            : base(settings, logger)
        {
            _frameDuration = 1f / Settings.GameFramesPerSecond;
        }

        public override void Tick()
        {
            _accumulatedTime += Time.deltaTime;

            while (_accumulatedTime >= _frameDuration)
            {
                Step();
                _accumulatedTime -= _frameDuration;
            }
        }


        public float GetDelay()
        {
            float expectedTime = CurrentGameFrame * _frameDuration;
            float actualTime = Time.timeSinceLevelLoad;
            return actualTime - expectedTime;
        }
    }
}
