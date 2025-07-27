using FixedMathSharp;
using Flowcast.Commons;
using System;

namespace Flowcast.Lockstep
{
    public interface ILockstepProvider : IFrameProvider
    {
        ILockstepSettings Settings { get; }

        /// <summary>
        /// Total number of game frames that have been processed.
        /// </summary>
        ulong CurrentGameFrame { get; }

        /// <summary>
        /// Total number of lockstep turns that have been completed.
        /// </summary>
        ulong CurrentLockstepTurn { get; }

        /// <summary>
        /// Total simulated time in ticks (e.g., milliseconds).
        /// Equals CurrentGameFrame * FrameDuration
        /// </summary>
        ulong SimulationTimeTicks { get; }

        /// <summary>
        /// Multiplier for game simulation speed. 1.0 = real time. >1.0 = faster. <1.0 = slower.
        /// Used for rollback catch-up and testing.
        /// </summary>
        Fixed64 SimulationSpeedMultiplier { get; }

        /// <summary>
        /// Fixed simulation delta time per game frame in seconds.
        /// Equivalent to Time.fixedDeltaTime.
        /// </summary>
        Fixed64 FixedDeltaTime { get; }

        /// <summary>
        /// The actual simulation time that was advanced in the most recent Step().
        /// Equivalent to Time.deltaTime in lockstep context.
        /// </summary>
        double DeltaTime { get; }


        /// <summary>
        /// Called once every game frame.
        /// </summary>
        event Action OnGameFrame;

        /// <summary>
        /// Called once at the beginning of each new lockstep turn.
        /// </summary>
        event Action OnLockstepTurn;

        void Tick();

        void SetFastModeSimulation();

        void SetNormalModeSimulation();

        void ResetFrameTo(ulong frame);
    }
}
