using Flowcast.Serialization;
using System;

namespace Flowcast.Synchronization
{
    public interface IRollbackHandler
    {
        void ApplySnapshot(SnapshotEntry snapshot);
    }

    public class RollbackHandler<T> : IRollbackHandler where T : ISerializableGameState
    {
        private readonly IGameStateSerializer<T> _serializer;
        private readonly Action<T> _applyState;

        public RollbackHandler(IGameStateSerializer<T> serializer, Action<T> applyState)
        {
            _serializer = serializer;
            _applyState = applyState;
        }

        public void ApplySnapshot(SnapshotEntry snapshot)
        {
            var state = _serializer.DeserializeSnapshot(snapshot.Data);
            _applyState(state);
        }
    }
}
