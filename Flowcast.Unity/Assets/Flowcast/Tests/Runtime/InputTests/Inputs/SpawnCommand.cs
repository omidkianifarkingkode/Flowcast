using Flowcast.Commons;
using Flowcast.Commands;

namespace Flowcast.Tests.Runtime.CommandTests
{
    public class SpawnCommand : BaseCommand
    {
        public int ObjectId { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public SpawnCommand(int objectId, int x, int y)
        {
            ObjectId = objectId;
            X = x;
            Y = y;
        }
    }

    public class SpawnCommandValidator : ICommandValidator<SpawnCommand>
    {
        public Result Validate(SpawnCommand command)
        {
            return command switch
            {
                { ObjectId: > 0, X: >= 0, Y: >= 0 } => Result.Success(),
                _ => Result.Failure("Invalid SpawnCommand parameters.")
            };
        }
    }
}

