using Flowcast.Data;
using Flowcast.Commands;
using Flowcast.Lockstep;
using Flowcast.Logging;
using Flowcast.Network;
using Flowcast.Pipeline;
using Flowcast.Synchronization;
using System;

namespace Flowcast.Builders
{
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
        IRequireNetwork SynchronizeGameState(Action<IGameStateSyncOptionsBuilder> gameState);
    }

    public interface IRequireNetwork
    {
        public IRequirePipline SetupNetworkServices(Action<INetworkBuilder> network);
    }

    public interface IRequirePipline 
    {
        IOptionalSettings ConfigureSimulationPipeline(Action<IGameUpdatePipelineBuilder> pipeline);
    }

    public interface IOptionalSettings
    {
        IOptionalSettings SetLogger(ILogger logger);
        ILockstepEngine BuildAndStart();
    }
}
