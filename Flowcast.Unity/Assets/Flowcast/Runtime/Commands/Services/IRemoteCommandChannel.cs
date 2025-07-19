using System;
using System.Collections.Generic;

namespace Flowcast.Commands
{
    /// <summary>
    /// Handles incoming player commands from the server and exposes them by frame.
    /// This module is responsible for buffering remote commands received over the network,
    /// and making them available to the simulation at the correct game frame.
    /// 
    /// It should only contain command from other players (or authoritative echoes).
    /// </summary>
    public interface IRemoteCommandChannel
    {
        /// <summary>
        /// Sends local commands to the server.
        /// Usually called once per lockstep frame.
        /// </summary>
        void SendCommands(IReadOnlyCollection<ICommand> commands);

        /// <summary>
        /// Returns all remote commands for the specified game frame, if available.
        /// </summary>
        IReadOnlyCollection<ICommand> GetCommandsForFrame(ulong frame);

        /// <summary>
        /// Removes remote commands for a specific frame after they’ve been processed.
        /// </summary>
        void RemoveCommandsForFrame(ulong frame);

        void ResetWith(IReadOnlyCollection<ICommand> commands);

        /// <summary>
        /// Optional event triggered when remote commands are received.
        /// Useful for debugging, analytics, or custom command routing.
        /// </summary>
        event Action<IReadOnlyCollection<ICommand>> OnCommandsReceived;

        event Action<IReadOnlyCollection<ICommand>> OnCommandsSent;
    }
}
