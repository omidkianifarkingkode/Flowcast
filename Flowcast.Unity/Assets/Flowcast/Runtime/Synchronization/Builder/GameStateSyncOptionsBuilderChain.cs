using Flowcast.Commons;
using Flowcast.Network;
using Flowcast.Serialization;
using Newtonsoft.Json;
using System;

namespace Flowcast.Synchronization
{
    public interface IGameStateSyncOptionsBuilder
    {
        IGameStateSyncSnapshotSerializer UseDefaultOptions();
        IGameStateSyncSnapshotSerializer LoadOptionsFromResources(string resourcePath = "GameStateSyncOptions");
        IGameStateSyncSnapshotSerializer UseCustomOptions(IGameStateSyncOptions options);
    }

    public interface IGameStateSyncSnapshotSerializer
    {
        IGameStateSyncRollbackConfigurer SetGameStateSerializer(IGameStateSerializer serializer);
        IGameStateSyncRollbackConfigurer UseBinarySerializer(IBinarySerializableGameState gameState);
        IGameStateSyncRollbackConfigurer UseJsonSerializer<T>(T gameState, JsonSerializerSettings settings = null) where T : ISerializableGameState, new();
    }

    public interface IGameStateSyncRollbackConfigurer
    {
        IGameStateSyncOptionalSettings OnRollback(Action<ISerializableGameState> onRollback);
        IGameStateSyncOptionalSettings OnRollback<T>(Action<T> onRollback) where T : ISerializableGameState;
    }

    public interface IGameStateSyncOptionalSettings
    {
        IGameStateSyncOptionalSettings SetHasher(IHasher hasher);
        IGameStateSyncOptionalSettings SetRollbackHandler(IRollbackHandler rollbackHandler);
        IGameStateSyncOptionalSettings SetNetworkSync(INetworkGameStateSyncService networkService);
    }
}
