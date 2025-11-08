using Flowcast.Commons;
using System.Collections.Generic;
using System.Linq;

namespace Flowcast.Commands
{
    public class LocalCommandCollector : ILocalCommandCollector
    {
        private readonly ICommandValidatorFactory _validatorFactory;
        private readonly Queue<ICommand> _commandQueue = new();

        private readonly IFrameProvider _frameProvider;
        private readonly IIdGenerator _idGenerator;

        public IReadOnlyCollection<ICommand> BufferedCommands => _commandQueue.ToList().AsReadOnly();

        public LocalCommandCollector(ICommandValidatorFactory validatorFactory, IFrameProvider frameProvider, IIdGenerator idGenerator)
        {
            _validatorFactory = validatorFactory;
            _frameProvider = frameProvider;
            _idGenerator = idGenerator;
        }

        public Result Collect(ICommand command)
        {
            if (command == null)
                return Result.Failure("Command is null.");

            if (command is BaseCommand baseCommand)
            {
                baseCommand.Id = _idGenerator.Generate();
                baseCommand.Frame = _frameProvider.GetCurrentFrame();
            }

            var validator = _validatorFactory.GetValidator(command.GetType());

            if (validator != null && (validator.Validate(command) is var result && result.IsFailure))
                return result;

            _commandQueue.Enqueue(command);

            return Result.Success();
        }

        public IReadOnlyCollection<ICommand> ConsumeBufferedCommands()
        {
            var commands = _commandQueue.ToArray();
            _commandQueue.Clear();

            return commands;
        }
    }
}

