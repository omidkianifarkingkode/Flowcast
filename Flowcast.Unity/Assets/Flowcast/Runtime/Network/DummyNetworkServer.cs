using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Flowcast.Data;
using Flowcast.Inputs;

namespace Flowcast.Network
{
    public class DummyNetworkServer : INetworkManager
    {
        public event Action OnConnected;
        public event Action OnDisconnected;
        public event Action<Exception> OnConnectionError;
        public event Action<IReadOnlyCollection<IInput>> OnInputsReceived;
        public event Action<ulong, bool> OnSyncStatusReceived;
        public event Action<ulong> OnRollbackRequested;
        public event Action<TimeSpan> OnPingResult;
        public event Action<GameSessionData> OnMatchFound;

        public bool IsConnected { get; private set; } = false;
        public TimeSpan EstimatedLatency { get; set; } = TimeSpan.FromMilliseconds(80); // configurable

        public void Connect(string serverAddress)
        {
            IsConnected = true;
            OnConnected?.Invoke();
        }

        public void Disconnect()
        {
            IsConnected = false;
            OnDisconnected?.Invoke();
        }

        public void SendInputs(IReadOnlyCollection<IInput> inputs)
        {
            SimulateInputDelivery(inputs).Forget();
        }

        public void SendStateHash(ulong frame, uint hash)
        {
            SimulateSyncStatus(frame).Forget();
        }

        public void SendPing()
        {
            SimulatePing().Forget();
        }

        public void RequestRollback(ulong rollbackTo)
        {
            SimulateRollback(rollbackTo).Forget();
        }

        public void Dispose()
        {
            Disconnect();
        }

        // ------------------------------
        // UniTask-based simulation logic
        // ------------------------------

        private async UniTaskVoid SimulateInputDelivery(IReadOnlyCollection<IInput> inputs)
        {
            await UniTask.Delay(EstimatedLatency);
            await UniTask.SwitchToMainThread();
            OnInputsReceived?.Invoke(inputs);
        }

        private async UniTaskVoid SimulateSyncStatus(ulong frame)
        {
            await UniTask.Delay(EstimatedLatency);
            await UniTask.SwitchToMainThread();
            OnSyncStatusReceived?.Invoke(frame, true); // always synced
        }

        private async UniTaskVoid SimulatePing()
        {
            await UniTask.Delay(EstimatedLatency);
            await UniTask.SwitchToMainThread();
            OnPingResult?.Invoke(EstimatedLatency);
        }

        private async UniTaskVoid SimulateRollback(ulong frame)
        {
            await UniTask.Delay(EstimatedLatency);
            await UniTask.SwitchToMainThread();
            OnRollbackRequested?.Invoke(frame);
        }

        public Task RequestMatchAsync(string gameMode, object customData = null)
        {
            throw new NotImplementedException();
        }
    }
}
