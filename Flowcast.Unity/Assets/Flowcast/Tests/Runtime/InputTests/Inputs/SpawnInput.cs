using Flowcast.Commons;
using Flowcast.Inputs;

namespace Flowcast.Tests.Runtime.InputTests
{
    public class SpawnInput : InputBase
    {
        public int ObjectId { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public SpawnInput(int objectId, int x, int y)
        {
            ObjectId = objectId;
            X = x;
            Y = y;
        }
    }

    public class SpawnInputValidator : InputValidatorBase<SpawnInput>
    {
        public override Result Validate(SpawnInput input)
        {
            return input switch
            {
                { ObjectId: > 0, X: >= 0, Y: >= 0 } => Result.Success(),
                _ => Result.Failure("Invalid SpawnInput parameters.")
            };
        }
    }
}

