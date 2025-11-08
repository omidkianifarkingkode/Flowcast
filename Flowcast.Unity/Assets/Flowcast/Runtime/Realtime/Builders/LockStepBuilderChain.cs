using Flowcast.Commands;
using Flowcast.Data;
using Flowcast.Network;
using Flowcast.Options;
using Flowcast.Synchronization;
using System;

namespace Flowcast.Builders
{
    public interface IRequireConfiguration
    {
        IRequireMatchInfo ConfigureWithDefault();
        IRequireMatchInfo ConfigureFromResources();
        IRequireMatchInfo ConfigureWithCustomOptions(ILockstepEngineOptions options);
    }

    public interface IRequireMatchInfo
    {
        IRequireCommand SetMatchInfo(MatchInfo matchInfo);
    }

    public interface IRequireCommand
    {
        IRequireGameState ConfigureCommandSystem(Action<ICommandOptionsBuilderStart> command);
    }

    public interface IRequireGameState
    {
        IRequireNetwork SynchronizeGameState(Action<IGameStateSyncSnapshotSerializer> gameState);
    }

    public interface IRequireNetwork
    {
        public IOptionalSettings SetupNetworkServices(Action<INetworkBuilder> network);
    }

    public interface IOptionalSettings
    {
        ILockstepEngine BuildAndStart();
    }
}
