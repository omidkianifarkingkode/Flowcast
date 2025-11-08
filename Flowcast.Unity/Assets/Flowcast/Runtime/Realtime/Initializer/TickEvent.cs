using FixedMathSharp;
using System;
using UnityEngine.Events;

namespace Flowcast
{
    [Serializable]
    public class TickEvent : UnityEvent<TickWrapper> { }

    [Serializable]
    public class TickWrapper
    {
        public ulong Tick;
        public Fixed64 DeltaTime;

        public TickWrapper(ulong tick, Fixed64 deltaTime)
        {
            Tick = tick;
            DeltaTime = deltaTime;
        }
    }
}
