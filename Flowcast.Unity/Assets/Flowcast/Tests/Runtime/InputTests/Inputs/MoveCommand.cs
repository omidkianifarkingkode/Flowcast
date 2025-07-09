using Flowcast.Commons;
using Flowcast.Commands;
using Flowcast.Tests.Runtime.Commons.Services;

namespace Flowcast.Tests.Runtime.CommandTests
{
    public class MoveCommand : BaseCommand
    {
        public float X { get; set; }
        public float Y { get; set; }


        public MoveCommand(float x, float y)
        {
            X = x;
            Y = y;
        }
    }

    public class MoveCommandValidator : ICommandValidator<MoveCommand>
    {
        private readonly IGameWorldService _gameWorldService;

        public MoveCommandValidator(IGameWorldService gameWorldService)
        {
            _gameWorldService = gameWorldService;
        }

        public Result Validate(MoveCommand command)
        {
            bool allowed = _gameWorldService.IsMovementAllowed(command.X, command.Y);

            return allowed
                ? Result.Success()
                : Result.Failure("Movement out of bounds.");
        }
    }
}

