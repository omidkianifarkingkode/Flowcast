using System;
using System.Collections.Generic;

namespace Flowcast.Inputs
{
    /// <summary>
    /// Handles incoming player inputs from the server and exposes them by frame.
    /// This module is responsible for buffering remote inputs received over the network,
    /// and making them available to the simulation at the correct game frame.
    /// 
    /// It should only contain inputs from other players (or authoritative echoes).
    /// </summary>
    public interface IRemoteInputChannel
    {
        /// <summary>
        /// Sends local inputs to the server.
        /// Usually called once per lockstep frame.
        /// </summary>
        void SendInputs(IReadOnlyCollection<IInput> inputs);

        /// <summary>
        /// Returns all remote inputs for the specified game frame, if available.
        /// </summary>
        IReadOnlyCollection<IInput> GetInputsForFrame(ulong frame);

        /// <summary>
        /// Removes remote inputs for a specific frame after they’ve been processed.
        /// </summary>
        void RemoveInputsForFrame(ulong frame);

        /// <summary>
        /// Optional event triggered when remote inputs are received.
        /// Useful for debugging, analytics, or custom input routing.
        /// </summary>
        event Action<IReadOnlyCollection<IInput>> OnInputsReceived;
    }
}
