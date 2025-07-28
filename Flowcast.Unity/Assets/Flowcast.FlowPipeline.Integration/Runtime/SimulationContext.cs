using FixedMathSharp;
using System;

namespace Flowcast.FlowPipeline
{
    /// <summary>
    /// Represents a single simulation step in a deterministic lockstep system.
    /// </summary>
    public readonly struct SimulationContext : IEquatable<SimulationContext>
    {
        public readonly ulong Frame;
        public readonly Fixed64 DeltaTime;

        public SimulationContext(ulong frame, Fixed64 deltaTime)
        {
            Frame = frame;
            DeltaTime = deltaTime;
        }

        public void Deconstruct(out ulong frame, out Fixed64 deltaTime)
        {
            frame = Frame;
            deltaTime = DeltaTime;
        }

        public override string ToString() => $"[Frame: {Frame}, DeltaTime: {DeltaTime}]";

        public bool Equals(SimulationContext other) =>
            Frame == other.Frame && DeltaTime == other.DeltaTime;

        public override bool Equals(object obj) => obj is SimulationContext other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Frame, DeltaTime);
    }
}
