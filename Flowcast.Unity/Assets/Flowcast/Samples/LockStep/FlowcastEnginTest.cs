using Flowcast.Commons;
using Flowcast.Data;
using Flowcast.Commands;
using Flowcast.Network;
using Flowcast.Pipeline;
using Flowcast.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Flowcast.Tests.Runtime
{
    public class FlowcastEnginTest : MonoBehaviour
    {
        private void Start()
        {
            var matchInfo = new MatchInfo()
            {
                LocalPlayerId = 1,
                MatchId = Guid.NewGuid().ToString(),
                Players = new BasePlayerInfo[]
                {
                    new BasePlayerInfo { PlayerId = 1, DisplayName = "P1" },
                    new BasePlayerInfo { PlayerId = 2, DisplayName = "P2" }
                },
                ServerStartTimeUtc = DateTime.UtcNow,
            };

            var gameState = new MyGameState()
            {
                Health = 1
            };

            ILockstepEngine engine = FlowcastBuilder.CreateLockstep()
                .SetMatchInfo(matchInfo)
                .ConfigureCommandSystem(command => command
                    .OnCommandReceived(command =>
                    {
                        Debug.Log("Received: " + command);
                    })
                    .HandleCommandsOnLockstepTurn()
                    .SetupValidatorFactory(factory =>
                    {
                        factory.AutoMap();
                        factory.MapLazy(()=>new SpawnValidator());
                    })
                    .SetupProcessorFactory(factory =>
                    {
                        factory.AutoMap();
                        factory.MapManual<SpawnCommand, SpawnProcessor>();
                    }))
                .SynchronizeGameState(syncSetup => syncSetup
                    .UseDefaultOptions()
                    .SetGameStateModel(gameState)
                    .OnRollback((snapshot) => 
                    {
                        Debug.Log("Rollback");
                    }))
                .SetupNetworkServices(networkSetup => networkSetup
                    .UseDummyServer(new() 
                    {
                        BaseLatencyMs = 100,
                        EchoCommands = true,
                    }))
                .ConfigureSimulationPipeline(piplineSetup => piplineSetup
                    .HandleStepManually(tick => 
                    {
                        Debug.Log("Process on Tick");
                    }))
                .BuildAndStart();

            var command = new SpawnCommand(Vector2.zero, "1");

            engine.SubmitCommand(command);
        }
    }

    public class MyGameState : ISerializableGameState
    {
        public int Health;

        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(Health);
        }

        public void ReadFrom(BinaryReader reader)
        {
            Health = reader.ReadInt32();
        }
    }

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
