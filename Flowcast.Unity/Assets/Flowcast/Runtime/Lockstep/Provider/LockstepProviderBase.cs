using FixedMathSharp;
using System;
using UnityEngine;
using ILogger = Flowcast.Logging.ILogger;

namespace Flowcast.Lockstep
{
    public abstract class LockstepProviderBase : ILockstepProvider
    {
        public ILockstepSettings Settings { get; }
        public ILogger Logger { get; }

        public ulong CurrentGameFrame { get; protected set; }
        public ulong CurrentLockstepTurn { get; protected set; }
        public ulong SimulationTimeTicks => CurrentGameFrame * (ulong)(1000 / Settings.GameFramesPerSecond);

        public float SimulationSpeedMultiplier
        {
            get => _simulationSpeedMultiplier;
            private set => _simulationSpeedMultiplier = Mathf.Max(0, value);
        }
        private float _simulationSpeedMultiplier = 1;

        public event Action OnGameFrame;
        public event Action OnLockstepTurn;

        protected LockstepProviderBase(ILockstepSettings settings, ILogger logger)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (Settings.GameFramesPerSecond <= 0)
                throw new ArgumentException("GameFramesPerSecond must be > 0.");
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
            _simulationSpeedMultiplier = Settings.MaxCatchupSpeed;
        }

        public void SetNormalModeSimulation()
        {
            _simulationSpeedMultiplier = 1;
        }

        public ulong GetCurrentFrame() => CurrentGameFrame;

        public void ResetFrameTo(ulong frame)
        {
            throw new NotImplementedException();
        }
    }
}
