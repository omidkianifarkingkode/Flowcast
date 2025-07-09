using System;

namespace Flowcast.Commands
{
    public class CommandOptions
    {
        /// <summary>
        /// Required. Setup for command validator mapping and creation.
        /// </summary>
        public CommandValidatorFactoryOptions ValidatorFactoryOptions { get; set; }

        /// <summary>
        /// Required. Setup for command processor mapping and creation.
        /// </summary>
        public CommandProcessorFactoryOptions CommandFactoryOptions { get; set; }

        /// <summary>
        /// Optional. Called immediately when command is received (before processing).
        /// </summary>
        public Action<ICommand> OnCommandReceived { get; set; }

        /// <summary>
        /// If true, command are processed during each simulation frame.
        /// </summary>
        public bool HandleOnGameFrame { get; set; }

        /// <summary>
        /// If true, command are processed once per lockstep turn.
        /// </summary>
        public bool HandleOnLockstepTurn { get; set; }
    }

}
