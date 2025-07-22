namespace Flowcast.Commands
{
    public interface ICommandManager
    {
        void DispatchLocalCommands();
        void ProcessOnFrame(ulong tick);
        void ProcessOnLockstep(ulong tick);
    }
}

