using Flowcast.Data;
using Flowcast.Inputs;
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
            var gameSessionData = new GameSessionData()
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
                .SetGameSession(gameSessionData)
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
                        EchoInputs = true,
                    }))
                .SetupProcessPipeline(piplineSetup => piplineSetup
                    .UseDefaultSteps())
                .BuildAndStart();

            // flowcast.SubmitInput();
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

}
