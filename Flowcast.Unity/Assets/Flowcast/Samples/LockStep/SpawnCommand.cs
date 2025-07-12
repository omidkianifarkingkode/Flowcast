using Flowcast.Commands;
using Flowcast.Commons;
using UnityEngine;

namespace Flowcast.Tests.Runtime
{
    public class SpawnCommand : BaseCommand
    {
        public string UnitType { get; set; }
        public Vector2 Position { get; set; }

        public SpawnCommand(Vector2 position, string unitType)
        {
            Position = position;
            UnitType = unitType;
        }
    }

    public class SpawnValidator : ICommandValidator<SpawnCommand>
    {
        public Result Validate(SpawnCommand command)
        {
            if (string.IsNullOrEmpty(command.UnitType))
                return Result.Failure("Missing unit type");

            return Result.Success();
        }
    }

    public class SpawnProcessor : ICommandProcessor<SpawnCommand>
    {
        public void Process(SpawnCommand command)
        {
            Debug.Log($"Processing {command.UnitType}");
        }
    }
}
