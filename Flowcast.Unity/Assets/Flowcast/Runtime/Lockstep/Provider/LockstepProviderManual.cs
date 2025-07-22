using LogKit;
using System.Diagnostics;

namespace Flowcast.Lockstep
{
    public class LockstepProviderManual : LockstepProviderBase
    {
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private ulong _lastElapsedMs = 0;
        private ulong _accumulatedMs = 0;
        private readonly ulong _frameDurationMs;

        public LockstepProviderManual(ILockstepSettings settings, ILogger logger)
            : base(settings, logger)
        {
            _frameDurationMs = (ulong)(1000 / settings.GameFramesPerSecond);
            _stopwatch.Start();
            _lastElapsedMs = (ulong)_stopwatch.ElapsedMilliseconds;
        }

        public override void Tick()
        {
            ulong now = (ulong)_stopwatch.ElapsedMilliseconds;
            ulong delta = now - _lastElapsedMs;
            _lastElapsedMs = now;

            _accumulatedMs += delta;

            while (_accumulatedMs >= _frameDurationMs)
            {
                Step();
                _accumulatedMs -= _frameDurationMs;
            }
        }
    }
}
