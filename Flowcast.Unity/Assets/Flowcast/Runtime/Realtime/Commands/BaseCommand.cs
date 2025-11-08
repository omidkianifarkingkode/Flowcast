namespace Flowcast.Commands
{
    public abstract class BaseCommand : ICommand
    {
        public long Id { get; set; }
        public ulong Frame { get; set; }
        public long CreateTime { get; set; }
        public long PlayerId { get; set; }

        protected BaseCommand() { }

        protected BaseCommand(long id, ulong frame, long time, long playerId)
        {
            Id = id;
            Frame = frame;
            CreateTime = time;
            PlayerId = playerId;
        }
    }
}

