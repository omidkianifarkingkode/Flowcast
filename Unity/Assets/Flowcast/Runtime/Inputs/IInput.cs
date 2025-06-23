using System;

namespace Flowcast.Inputs
{
    public interface IInput
    {
        long Id { get; }
        long PlayerId { get; }
        ulong Frame { get; } // Or Tick, Turn, Timestamp depending on game
        long Time { get; } // Timestamp in milliseconds or ticks
    }

    public interface IHighPrioritizedInput : IInput { }
}

