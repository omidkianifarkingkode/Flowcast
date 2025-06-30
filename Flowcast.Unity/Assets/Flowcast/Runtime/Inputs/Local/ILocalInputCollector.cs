using Flowcast.Commons;
using System.Collections.Generic;

namespace Flowcast.Inputs
{
    /// <summary>
    /// Handles collection, validation, and buffering of local player inputs.
    /// This module is responsible for accepting inputs from the player (or AI),
    /// validating them if necessary, and preparing them for dispatch to the server.
    /// 
    /// It is the authoritative source for all client-side inputs before they are sent.
    /// </summary>
    public interface ILocalInputCollector
    {
        /// <summary>
        /// Collects and buffers a new local input.
        /// </summary>
        Result Collect(IInput input);

        /// <summary>
        /// Returns and clears all buffered inputs that have not yet been dispatched.
        /// </summary>
        IReadOnlyCollection<IInput> ConsumeBufferedInputs();

        /// <summary>
        /// Exposes current buffered inputs for inspection/debugging (read-only).
        /// </summary>
        IReadOnlyCollection<IInput> BufferedInputs { get; }
    }

}

