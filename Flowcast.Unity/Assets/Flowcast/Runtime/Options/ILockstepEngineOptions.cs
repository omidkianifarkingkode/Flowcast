using Flowcast.Lockstep;
using Flowcast.Synchronization;
using LogKit;

namespace Flowcast.Options
{
    public interface ILockstepEngineOptions : ILockstepSettings, IGameStateSyncOptions
    {
        ILoggerOptions Logger { get; }
    }
}
