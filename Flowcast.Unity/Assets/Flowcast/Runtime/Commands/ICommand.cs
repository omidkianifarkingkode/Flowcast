using System;

namespace Flowcast.Commands
{
    public interface ICommand
    {
        long Id { get; }
        ulong Frame { get; } // Or Tick, Turn, Timestamp depending on game
        long PlayerId { get; }
        long CreateTime { get; } // Timestamp in milliseconds or ticks
    }
}

