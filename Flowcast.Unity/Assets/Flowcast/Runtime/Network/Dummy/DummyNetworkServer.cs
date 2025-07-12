using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Flowcast.Data;
using Flowcast.Commands;
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
        public event Action<IReadOnlyCollection<ICommand>> OnCommandsReceived;
        public event Action<SyncStatus> OnSyncStatusReceived;
        public event Action<RollbackRequest> OnRollbackRequested;
        public event Action<TimeSpan> OnPingResult;
        public event Action<MatchInfo> OnMatchFound;

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

        public void SendCommands(IReadOnlyCollection<ICommand> commands)
        {
            if (!Options.EchoCommands) return;
            SimulateCommandDelivery(commands).Forget();
        }

        public void SendStateHash(StateHashReport report)
        {
            SimulateSyncStatus(report).Forget();
        }

        public void SendPing()
        {
            SimulatePing().Forget();
        }

        public void RequestRollback()
        {
            SimulateRollback().Forget();
        }

        public void Dispose()
        {
            Disconnect();
        }

        private async UniTaskVoid SimulateCommandDelivery(IReadOnlyCollection<ICommand> commands)
        {
            await UniTask.Delay(Options.GetRandomLatency());
            await UniTask.SwitchToMainThread();

            if (Options.ShouldDropPacket()) return;

            OnCommandsReceived?.Invoke(commands);
        }

        private async UniTaskVoid SimulateSyncStatus(StateHashReport stateHashReport)
        {
            await UniTask.Delay(Options.GetRandomLatency());
            await UniTask.SwitchToMainThread();

            if (Options.ShouldDropPacket()) return;

            OnSyncStatusReceived?.Invoke(new SyncStatus
            {
                Frame = stateHashReport.Frame,
                IsSynced = true
            });

            if (Options.ShouldTriggerRollback())
            {
                OnRollbackRequested?.Invoke(new RollbackRequest());
            }
        }

        private async UniTaskVoid SimulatePing()
        {
            await UniTask.Delay(Options.GetRandomLatency());
            await UniTask.SwitchToMainThread();

            OnPingResult?.Invoke(TimeSpan.FromMilliseconds(Options.BaseLatencyMs));
        }

        private async UniTaskVoid SimulateRollback()
        {
            await UniTask.Delay(Options.GetRandomLatency());
            await UniTask.SwitchToMainThread();

            if (Options.ShouldDropPacket()) return;
            OnRollbackRequested?.Invoke(new RollbackRequest());
        }

        public Task RequestMatchAsync(string gameMode, object customData = null)
        {
            throw new NotImplementedException("Matchmaking simulation not implemented.");
        }

#if UNITY_EDITOR
        // Convenience for triggering from editor
        public void Editor_SendPing() => SendPing();
        public void Editor_RequestRollback() => RequestRollback();
#endif
    }
}
