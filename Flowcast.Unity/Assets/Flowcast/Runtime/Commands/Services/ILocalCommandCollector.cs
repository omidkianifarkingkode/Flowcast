using Flowcast.Commons;
using System.Collections.Generic;

namespace Flowcast.Commands
{
    /// <summary>
    /// Handles collection, validation, and buffering of local player commands.
    /// This module is responsible for accepting commands from the player (or AI),
    /// validating them if necessary, and preparing them for dispatch to the server.
    /// 
    /// It is the authoritative source for all client-side commands before they are sent.
    /// </summary>
    public interface ILocalCommandCollector
    {
        /// <summary>
        /// Collects and buffers a new local command.
        /// </summary>
        Result Collect(ICommand command);

        /// <summary>
        /// Returns and clears all buffered commands that have not yet been dispatched.
        /// </summary>
        IReadOnlyCollection<ICommand> ConsumeBufferedCommands();

        /// <summary>
        /// Exposes current buffered commands for inspection/debugging (read-only).
        /// </summary>
        IReadOnlyCollection<ICommand> BufferedCommands { get; }
    }

}

