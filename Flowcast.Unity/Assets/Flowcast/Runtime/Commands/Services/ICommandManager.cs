using Flowcast.Logging;

namespace Flowcast.Commands
{
    public interface ICommandManager
    {
        void DispatchLocalCommands();
        void ProcessOnFrame(ulong tick);
        void ProcessOnLockstep(ulong tick);
    }

    public class CommandManager : ICommandManager
    {
        private readonly CommandOptions _options;
        private readonly ILocalCommandCollector _localCommandCollector;
        private readonly IRemoteCommandChannel _remoteCommandChannel;
        private readonly ICommandProcessorFactory _commandProcessorFactory;
        private readonly ILogger _logger;

        public CommandManager(CommandOptions options, ILocalCommandCollector localCommandCollector, IRemoteCommandChannel remoteCommandChannel, ICommandProcessorFactory commandProcessorFactory, ILogger logger)
        {
            _options = options;
            _localCommandCollector = localCommandCollector;
            _remoteCommandChannel = remoteCommandChannel;
            _commandProcessorFactory = commandProcessorFactory;
            _logger = logger;
        }

        public void DispatchLocalCommands()
        {
            var bufferedCommands = _localCommandCollector.ConsumeBufferedCommands();
            if (bufferedCommands.Count > 0)
            {
                _remoteCommandChannel.SendCommands(bufferedCommands);
                _logger.Log($"[CommandDispatch] Sent {bufferedCommands.Count} commands");
            }
        }

        public void ProcessOnFrame(ulong tick)
        {
            if (!_options.HandleOnGameFrame)
                return;

            ProcessCommands(tick);
        }

        public void ProcessOnLockstep(ulong tick)
        {
            if (!_options.HandleOnLockstepTurn)
                return;

            ProcessCommands(tick);
        }

        private void ProcessCommands(ulong tick)
        {
            DispatchLocalCommands();

            var commands = _remoteCommandChannel.GetCommandsForFrame(tick);

            _remoteCommandChannel.RemoveCommandsForFrame(tick); // Clean up

            foreach (var command in commands)
            {
                var processor = _commandProcessorFactory.GetProcessor(command.GetType());

                processor?.Process(command);

                _options.OnCommandReceived?.Invoke(command);
            }
        }
    }
}

