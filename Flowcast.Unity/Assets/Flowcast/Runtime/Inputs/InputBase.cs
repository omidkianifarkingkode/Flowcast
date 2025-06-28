namespace Flowcast.Inputs
{
    public abstract class InputBase : IInput
    {
        public long Id { get; set; }
        public long PlayerId { get; set; }
        public ulong Frame { get; set; }
        public long Time { get; set; }

        protected InputBase() { }

        protected InputBase(long id, long playerId, ulong frame, long time)
        {
            Id = id;
            PlayerId = playerId;
            Frame = frame;
            Time = time;
        }
    }
}

