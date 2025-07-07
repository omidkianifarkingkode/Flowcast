using Flowcast.Data;
using Flowcast.Network;
using Flowcast.Serialization;
using System;
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

            IFlowcastEngine flowcast = FlowcastBuilder.CreateLockstep()
                .SetGameSession(gameSessionData)
                .SetGameStateModel(gameState)
                .SetNetworkManager(new DummyNetworkServer())
                .ConfigureRollback(config =>
                {
                    config.OnRollback = state => { };
                    config.EnableRollbackLog = true;
                })
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
