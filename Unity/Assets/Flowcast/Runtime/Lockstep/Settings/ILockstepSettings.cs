namespace Flowcast.Lockstep
{
    public interface ILockstepSettings
    {
        /// <summary>
        /// Number of simulation frames per second (e.g., 20 means 50ms per frame).
        /// </summary>
        int GameFramesPerSecond { get; }

        /// <summary>
        /// Number of game frames in one lockstep turn (e.g., 5 = 100ms lockstep).
        /// </summary>
        int GameFramesPerLockstepTurn { get; }
    }
}
