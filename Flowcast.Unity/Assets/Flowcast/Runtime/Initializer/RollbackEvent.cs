using Flowcast.Serialization;
using System;
using UnityEngine.Events;

namespace Flowcast
{
    [Serializable]
    public class RollbackEvent : UnityEvent<RollbackWrapper> { }

    [Serializable]
    public class RollbackWrapper
    {
        public ulong Tick;
        public ISerializableGameState State;

        public RollbackWrapper(ulong tick, ISerializableGameState state)
        {
            Tick = tick;
            State = state;
        }
    }
}
