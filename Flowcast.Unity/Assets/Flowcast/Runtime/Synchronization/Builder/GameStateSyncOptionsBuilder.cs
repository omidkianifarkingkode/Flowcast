using Flowcast.Commons;
using Flowcast.Network;
using Flowcast.Serialization;
using System;
using UnityEngine;

namespace Flowcast.Synchronization
{
    public class GameStateSyncOptionsBuilder : IGameStateSyncOptionsBuilder, IGameStateSyncSnapshotModeler, IGameStateSyncRollbackConfigurer, IGameStateSyncOptionalSettings
    {
        public IGameStateSyncOptions Options { get; private set; }
        public ISerializableGameState GameState { get; private set; }
        public IGameStateSerializer Serializer { get; private set; }
        public IHasher Hasher { get; private set; }
        public IRollbackHandler? RollbackHandler { get; private set; }
        public INetworkGameStateSyncService? NetworkService { get; private set; }

        public IGameStateSyncSnapshotModeler UseDefaultOptions()
        {
            Options = new GameStateSyncOptions();
            return this;
        }

        public IGameStateSyncSnapshotModeler LoadOptionsFromResources(string resourcePath = "GameStateSyncOptions")
        {
            Options = Resources.Load<GameStateSyncOptionsAsset>(resourcePath);
            return this;
        }

        public IGameStateSyncSnapshotModeler UseCustomOptions(IGameStateSyncOptions options)
        {
            Options = options;
            return this;
        }

        public IGameStateSyncRollbackConfigurer SetGameStateModel(ISerializableGameState gameState)
        {
            GameState = gameState;
            return this;
        }

        public IGameStateSyncOptionalSettings OnRollback(Action<ISerializableGameState> onRollback)
        {
            Options.OnRollback = onRollback;
            return this;
        }

        public IGameStateSyncOptionalSettings SetGameStateSerializer(IGameStateSerializer serializer)
        {
            Serializer = serializer;
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

        internal void Build()
        {
            Serializer ??= new GameStateSerializer(() => GameState);
            Hasher ??= new XorHasher();
        }
    }
}
