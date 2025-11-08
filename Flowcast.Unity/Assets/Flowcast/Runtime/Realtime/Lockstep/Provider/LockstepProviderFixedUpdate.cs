using LogKit;

namespace Flowcast.Lockstep
{
    public class LockstepProviderFixedUpdate : LockstepProviderBase
    {
        public LockstepProviderFixedUpdate(ILockstepSettings settings, ILogger logger)
            : base(settings, logger)
        {
            // Optional: ensure Unity's Fixed Timestep matches
            // Time.fixedDeltaTime = 1f / settings.GameFramesPerSecond;
        }

        public override void Tick()
        {
            Step(); // one step per FixedUpdate call
        }
    }
}
