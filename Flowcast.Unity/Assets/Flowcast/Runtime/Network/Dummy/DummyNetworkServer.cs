using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Flowcast.Data;
using Flowcast.Inputs;
using UnityEngine;

namespace Flowcast.Network
{
    public class DummyNetworkServer : MonoBehaviour, INetworkManager
    {
        [Header("Simulation Options")]
        public DummyNetworkServerOptions Options = new();

        public event Action OnConnected;
        public event Action OnDisconnected;
        public event Action<Exception> OnConnectionError;
        public event Action<IReadOnlyCollection<IInput>> OnInputsReceived;
        public event Action<ulong, bool> OnSyncStatusReceived;
        public event Action<ulong> OnRollbackRequested;
        public event Action<TimeSpan> OnPingResult;
        public event Action<GameSessionData> OnMatchFound;

        public bool IsConnected { get; private set; }

        public TimeSpan EstimatedLatency => Options.GetRandomLatency();

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
            if (!Options.EchoInputs) return;
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

        private async UniTaskVoid SimulateInputDelivery(IReadOnlyCollection<IInput> inputs)
        {
            await UniTask.Delay(Options.GetRandomLatency());
            await UniTask.SwitchToMainThread();

            if (Options.ShouldDropPacket()) return;
            OnInputsReceived?.Invoke(inputs);
        }

        private async UniTaskVoid SimulateSyncStatus(ulong frame)
        {
            await UniTask.Delay(Options.GetRandomLatency());
            await UniTask.SwitchToMainThread();

            if (Options.ShouldDropPacket()) return;

            OnSyncStatusReceived?.Invoke(frame, true);

            if (Options.ShouldTriggerRollback())
            {
                var rollbackFrame = Math.Max(0, (long)frame - 5);
                OnRollbackRequested?.Invoke((ulong)rollbackFrame);
            }
        }

        private async UniTaskVoid SimulatePing()
        {
            await UniTask.Delay(Options.GetRandomLatency());
            await UniTask.SwitchToMainThread();

            OnPingResult?.Invoke(TimeSpan.FromMilliseconds(Options.BaseLatencyMs));
        }

        private async UniTaskVoid SimulateRollback(ulong frame)
        {
            await UniTask.Delay(Options.GetRandomLatency());
            await UniTask.SwitchToMainThread();

            if (Options.ShouldDropPacket()) return;
            OnRollbackRequested?.Invoke(frame);
        }

        public Task RequestMatchAsync(string gameMode, object customData = null)
        {
            throw new NotImplementedException("Matchmaking simulation not implemented.");
        }

#if UNITY_EDITOR
        // Convenience for triggering from editor
        public void Editor_SendPing() => SendPing();
        public void Editor_RequestRollback() => RequestRollback(LockstepEngine.Instance.LockstepProvider.CurrentLockstepTurn - 10);
#endif
    }
}
