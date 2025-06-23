using Flowcast.Commons;
using Flowcast.Inputs;
using Flowcast.Tests.Runtime.Commons.Services;

namespace Flowcast.Tests.Runtime.InputTests
{
    public class MoveInput : InputBase
    {
        public float X { get; set; }
        public float Y { get; set; }


        public MoveInput(float x, float y)
        {
            X = x;
            Y = y;
        }
    }

    public class MoveInputValidator : InputValidatorBase<MoveInput>
    {
        private readonly IGameWorldService _gameWorldService;

        public MoveInputValidator(IGameWorldService gameWorldService)
        {
            _gameWorldService = gameWorldService;
        }

        public override Result Validate(MoveInput input)
        {
            bool allowed = _gameWorldService.IsMovementAllowed(input.X, input.Y);

            return allowed
                ? Result.Success()
                : Result.Failure("Movement out of bounds.");
        }
    }
}

