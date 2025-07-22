using Flowcast.Commands;
using Flowcast.Network;
using UnityEngine;

public class SpawnCommand : BaseCommand
{
    public CharacterType UnitType { get; set; }
    public Vector2 Position { get; set; }

    public SpawnCommand(Vector2 position, CharacterType unitType)
    {
        Position = position;
        UnitType = unitType;
    }

    public override string ToString()
    {
        return $"Spawn {UnitType} at {Position}";
    }
}

//public class SpawnValidator : ICommandValidator<SpawnCommand>
//{
//    public Result Validate(SpawnCommand command)
//    {
//        if (string.IsNullOrEmpty(command.UnitType))
//            return Result.Failure("Missing unit type");

//        return Result.Success();
//    }
//}

//public class SpawnProcessor : ICommandProcessor<SpawnCommand>
//{
//    private readonly INetworkManager networkManager;

//    public SpawnProcessor(INetworkManager networkManager)
//    {
//        this.networkManager = networkManager;
//    }

//    public void Process(SpawnCommand command)
//    {
//        Debug.Log($"Processing {command.UnitType}");
//    }
//}
