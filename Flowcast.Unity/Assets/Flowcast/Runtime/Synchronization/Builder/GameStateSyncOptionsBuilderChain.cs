using Flowcast.Commons;
using Flowcast.Network;
using Flowcast.Serialization;
using System;

namespace Flowcast.Synchronization
{
    public interface IGameStateSyncOptionsBuilder
    {
        IGameStateSyncSnapshotModeler UseDefaultOptions();
        IGameStateSyncSnapshotModeler LoadOptionsFromResources(string resourcePath = "GameStateSyncOptions");
        IGameStateSyncSnapshotModeler UseCustomOptions(IGameStateSyncOptions options);
    }

    public interface IGameStateSyncSnapshotModeler
    {
        IGameStateSyncRollbackConfigurer SetGameStateModel(ISerializableGameState gameState);
    }

    public interface IGameStateSyncRollbackConfigurer
    {
        IGameStateSyncOptionalSettings OnRollback(Action<ISerializableGameState> onRollback);
    }

    public interface IGameStateSyncOptionalSettings
    {
        IGameStateSyncOptionalSettings SetGameStateSerializer(IGameStateSerializer serializer);
        IGameStateSyncOptionalSettings SetHasher(IHasher hasher);
        IGameStateSyncOptionalSettings SetRollbackHandler(IRollbackHandler rollbackHandler);
        IGameStateSyncOptionalSettings SetNetworkSync(INetworkGameStateSyncService networkService);
    }
}
