using Flowcast.Commons;
using Flowcast.Network;
using Flowcast.Rollback;
using Flowcast.Serialization;
using Newtonsoft.Json;
using System;
using FixedMathSharp;

namespace Flowcast.Synchronization
{
    public interface IGameStateSyncSnapshotSerializer
    {
        IGameStepConfigurer SetGameStateSerializer(IGameStateSerializer serializer);
        IGameStepConfigurer UseBinarySerializer<T>(T gameState) where T : IBinarySerializableGameState, new();
        IGameStepConfigurer UseJsonSerializer<T>(T gameState, JsonSerializerSettings settings = null) where T : ISerializableGameState, new();
    }

    public interface IGameStepConfigurer
    {
        IGameStateSyncRollbackConfigurer OnStep(Action<ulong, Fixed64> onStep);
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
