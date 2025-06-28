namespace Flowcast.Lockstep
{
    public interface ILockstepProvider
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
        /// Called once every game frame.
        /// </summary>
        event System.Action OnGameFrame;

        /// <summary>
        /// Called once at the beginning of each new lockstep turn.
        /// </summary>
        event System.Action OnLockstepTurn;
    }
}
