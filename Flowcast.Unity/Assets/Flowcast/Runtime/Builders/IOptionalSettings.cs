using Flowcast.Lockstep;
using Flowcast.Logging;
using Flowcast.Serialization;
using Flowcast.Synchronization;
using System;

namespace Flowcast.Builders
{
    public interface IOptionalSettings
    {
        IOptionalSettings SetLogger(ILogger logger);
        IOptionalSettings SetLockstepSettings(ILockstepSettings settings);
        IOptionalSettings SetGameStateSerializer(IGameStateSerializer serializer);
        IOptionalSettings ConfigureRollback(Action<RollbackConfig> config);
        IFlowcastEngine BuildAndStart();
    }
}
