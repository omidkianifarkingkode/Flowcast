using Flowcast.Commons;

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
        float SimulationSpeedMultiplier { get; set; }


        /// <summary>
        /// Called once every game frame.
        /// </summary>
        event System.Action OnGameFrame;

        /// <summary>
        /// Called once at the beginning of each new lockstep turn.
        /// </summary>
        event System.Action OnLockstepTurn;
    }
}
