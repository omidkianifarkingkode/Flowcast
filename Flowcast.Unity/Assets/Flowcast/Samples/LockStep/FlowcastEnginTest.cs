using Flowcast.Data;
using System;
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
                        // factory.MapLazy(()=>new SpawnValidator());
                    })
                    .SetupProcessorFactory(factory =>
                    {
                        factory.AutoMap();
                        // factory.MapManual<SpawnCommand, SpawnProcessor>();
                    }))
                .SynchronizeGameState(syncSetup => syncSetup
                    .UseDefaultOptions()
                    .UseBinarySerializer(gameState)
                    .OnRollback<MyGameState>((snapshot) =>
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


}
