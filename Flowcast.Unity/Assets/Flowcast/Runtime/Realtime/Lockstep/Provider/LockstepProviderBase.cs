using FixedMathSharp;
using System;
using System.Diagnostics;
using ILogger = LogKit.ILogger;

namespace Flowcast.Lockstep
{
    public abstract class LockstepProviderBase : ILockstepProvider
    {
        public ILockstepSettings Settings { get; }
        public ILogger Logger { get; }

        public ulong CurrentGameFrame { get; protected set; }
        public ulong CurrentLockstepTurn { get; protected set; }
        public ulong SimulationTimeTicks => CurrentGameFrame * (ulong)(1000 / Settings.GameFramesPerSecond);

        public Fixed64 SimulationSpeedMultiplier { get; private set; } = Fixed64.One;

        public Fixed64 FixedDeltaTime { get; }
        public double DeltaTime { get; private set; }

        public event Action OnGameFrame;
        public event Action OnLockstepTurn;

        private Stopwatch _stepTimer;

        protected LockstepProviderBase(ILockstepSettings settings, ILogger logger)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (Settings.GameFramesPerSecond <= 0)
                throw new ArgumentException("GameFramesPerSecond must be > 0.");

            FixedDeltaTime = Fixed64.One / Settings.GameFramesPerSecond;
            _stepTimer = Stopwatch.StartNew();
        }

        /// <summary>
        /// Subclasses must implement this. It is called by MonoBehaviour or custom loop.
        /// </summary>
        public abstract void Tick();

        /// <summary>
        /// Call this when time has accumulated enough for one fixed simulation step.
        /// </summary>
        protected void Step()
        {
            var now = _stepTimer.Elapsed;
            _stepTimer.Restart();
            DeltaTime = now.TotalSeconds;

            if (CurrentGameFrame % (ulong)Settings.GameFramesPerLockstepTurn == 0)
            {
                // Logger.Log($"[LockstepTurn] {CurrentLockstepTurn}");
                OnLockstepTurn?.Invoke();
                CurrentLockstepTurn++;
            }

            //Logger.Log($"[GameFrame] {CurrentGameFrame}");
            OnGameFrame?.Invoke();
            CurrentGameFrame++;
        }

        public void SetFastModeSimulation()
        {
            SimulationSpeedMultiplier = Settings.MaxRecoverySpeed;
        }

        public void SetNormalModeSimulation()
        {
            SimulationSpeedMultiplier = Fixed64.One;
        }

        public ulong GetCurrentFrame() => CurrentGameFrame;

        public virtual void ResetFrameTo(ulong frame)
        {
            CurrentGameFrame = frame;
            CurrentLockstepTurn = frame / (ulong)Settings.GameFramesPerLockstepTurn;
        }

    }
}
