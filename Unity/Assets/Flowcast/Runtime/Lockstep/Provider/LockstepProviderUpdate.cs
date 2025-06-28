using UnityEngine;

namespace Flowcast.Lockstep
{
    public class LockstepProviderUpdate : LockstepProviderBase
    {
        private float _accumulatedTime = 0f;
        private readonly float _frameDuration;

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
    }
}
