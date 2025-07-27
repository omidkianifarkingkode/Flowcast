using Flowcast.Commons;
using Flowcast.Network;
using Flowcast.Rollback;
using Flowcast.Serialization;
using Newtonsoft.Json;
using System;
using FixedMathSharp;

namespace Flowcast.Synchronization
{
    public class GameStateSyncOptionsBuilder : IGameStateSyncSnapshotSerializer, IGameStepConfigurer, IGameStateSyncRollbackConfigurer, IGameStateSyncOptionalSettings
    {
        public IGameStateSerializer Serializer { get; private set; }
        public IHasher Hasher { get; private set; }
        public IRollbackHandler RollbackHandler { get; private set; }
        public INetworkGameStateSyncService NetworkService { get; private set; }

        public Action<ISerializableGameState, ulong> RollbackCallback { get; private set; }
        public Action<ulong, Fixed64> StepCallback { get; private set; }


        public IGameStepConfigurer SetGameStateSerializer(IGameStateSerializer serializer)
        {
            Serializer = serializer;
            return this;
        }

        public IGameStepConfigurer UseBinarySerializer<T>(T gameState)
            where T : IBinarySerializableGameState, new()
        {
            Serializer = new BinarySerializer<T>(() => gameState);
            return this;
        }

        public IGameStepConfigurer UseJsonSerializer<T>(T gameState, JsonSerializerSettings settings = null)
            where T : ISerializableGameState, new()
        {
            Serializer = new JsonSerializer<T>(() => gameState, settings);
            return this;
        }

        public IGameStateSyncRollbackConfigurer OnStep(Action<ulong, Fixed64> onStep)
        {
            StepCallback = onStep;

            return this;
        }

        public IGameStateSyncOptionalSettings OnRollback(Action<ISerializableGameState, ulong> onRollback)
        {
            RollbackCallback = onRollback;

            return this;
        }

        public IGameStateSyncOptionalSettings OnRollback<T>(Action<T, ulong> onRollback) where T : ISerializableGameState
        {
            RollbackCallback = (state, frame) =>
            {
                if (state is T typed)
                    onRollback(typed, frame);
                else
                    throw new InvalidCastException($"Rollback state is not of expected type {typeof(T).Name}");
            };
            return this;
        }

        

        public IGameStateSyncOptionalSettings SetHasher(IHasher hasher)
        {
            Hasher = hasher;
            return this;
        }

        public IGameStateSyncOptionalSettings SetRollbackHandler(IRollbackHandler rollbackHandler)
        {
            RollbackHandler = rollbackHandler;
            return this;
        }

        public IGameStateSyncOptionalSettings SetNetworkSync(INetworkGameStateSyncService networkService)
        {
            NetworkService = networkService;
            return this;
        }

        public void Build()
        {
            Hasher ??= new XorHasher();
        }
    }
}
