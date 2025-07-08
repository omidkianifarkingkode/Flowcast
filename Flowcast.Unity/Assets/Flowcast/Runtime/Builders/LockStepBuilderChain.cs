using Flowcast.Data;
using Flowcast.Lockstep;
using Flowcast.Logging;
using Flowcast.Network;
using Flowcast.Pipeline;
using Flowcast.Synchronization;
using System;

namespace Flowcast.Builders
{
    public interface IRequireGameSession
    {
        IRequireGameState SetGameSession(GameSessionData gameSessionData);
    }

    public interface IRequireGameState
    {
        IRequireNetwork SynchronizeGameState(Action<IGameStateSyncOptionsBuilder> setup);
    }

    public interface IRequireNetwork
    {
        public IOptionalSettings SetupNetworkServices(Action<INetworkBuilder> setup);
    }

    public interface IOptionalSettings
    {
        IOptionalSettings SetLogger(ILogger logger);
        IOptionalSettings SetLockstepSettings(ILockstepSettings settings);
        IOptionalSettings SetupProcessPipeline(Action<IGameUpdatePipelineBuilder> setup);
        ILockstepEngine BuildAndStart();
    }
}
