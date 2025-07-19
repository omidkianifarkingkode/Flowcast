using Flowcast.Commons;
using Flowcast.Network;
using Flowcast.Rollback;
using Flowcast.Serialization;
using Newtonsoft.Json;
using System;
using UnityEngine;

namespace Flowcast.Synchronization
{
    public class GameStateSyncOptionsBuilder : IGameStateSyncOptionsBuilder, IGameStateSyncSnapshotSerializer, IGameStateSyncRollbackConfigurer, IGameStateSyncOptionalSettings
    {
        public IGameStateSyncOptions Options { get; set; }
        public IGameStateSerializer Serializer { get; private set; }
        public IHasher Hasher { get; private set; }
        public IRollbackHandler RollbackHandler { get; private set; }
        public INetworkGameStateSyncService NetworkService { get; private set; }

        public IGameStateSyncSnapshotSerializer UseDefaultOptions()
        {
            Options = new GameStateSyncOptions();
            return this;
        }

        public IGameStateSyncSnapshotSerializer LoadOptionsFromResources(string resourcePath = "GameStateSyncOptions")
        {
            Options = Resources.Load<GameStateSyncOptionsAsset>(resourcePath);
            return this;
        }

        public IGameStateSyncSnapshotSerializer UseCustomOptions(IGameStateSyncOptions options)
        {
            Options = options;
            return this;
        }

        public IGameStateSyncOptionalSettings OnRollback(Action<ISerializableGameState> onRollback)
        {
            Options.OnRollback = onRollback;

            return this;
        }

        public IGameStateSyncOptionalSettings OnRollback<T>(Action<T> onRollback) where T : ISerializableGameState
        {
            Options.OnRollback = state =>
            {
                if (state is T typed)
                    onRollback(typed);
                else
                    throw new InvalidCastException($"Rollback state is not of expected type {typeof(T).Name}");
            };
            return this;
        }

        public IGameStateSyncRollbackConfigurer SetGameStateSerializer(IGameStateSerializer serializer)
        {
            Serializer = serializer;
            return this;
        }

        public IGameStateSyncRollbackConfigurer UseBinarySerializer<T>(T gameState)
            where T : IBinarySerializableGameState, new()
        {
            Serializer = new BinarySerializer<T>(() => gameState);
            return this;
        }

        public IGameStateSyncRollbackConfigurer UseJsonSerializer<T>(T gameState, JsonSerializerSettings settings = null)
            where T : ISerializableGameState, new()
        {
            Serializer = new JsonSerializer<T>(() => gameState, settings);
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
