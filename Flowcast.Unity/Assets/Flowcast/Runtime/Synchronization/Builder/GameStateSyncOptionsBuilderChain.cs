using Flowcast.Commons;
using Flowcast.Network;
using Flowcast.Rollback;
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
        IGameStateSyncRollbackConfigurer UseBinarySerializer<T>(T gameState) where T : IBinarySerializableGameState, new();
        IGameStateSyncRollbackConfigurer UseJsonSerializer<T>(T gameState, JsonSerializerSettings settings = null) where T : ISerializableGameState, new();
    }

    public interface IGameStateSyncRollbackConfigurer
    {
        IGameStateSyncOptionalSettings OnRollback(Action<ISerializableGameState, ulong> onRollback);
        IGameStateSyncOptionalSettings OnRollback<T>(Action<T, ulong> onRollback) where T : ISerializableGameState;
    }

    public interface IGameStateSyncOptionalSettings
    {
        IGameStateSyncOptionalSettings SetHasher(IHasher hasher);
        IGameStateSyncOptionalSettings SetRollbackHandler(IRollbackHandler rollbackHandler);
        IGameStateSyncOptionalSettings SetNetworkSync(INetworkGameStateSyncService networkService);
    }
}
