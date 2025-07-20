using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Flowcast.Data;
using Flowcast.Commands;
using UnityEngine;
using System.Linq;

namespace Flowcast.Network
{

    [Serializable]
    public class DummyNetworkServer : INetworkManager
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
        public event Action<IReadOnlyCollection<ICommand>> OnCommandsHistoryReceived;

        public bool IsConnected { get; private set; }

        public TimeSpan EstimatedLatency => Options.GetRandomLatency();

        // Internal command storage
        private readonly Dictionary<ulong, List<ICommand>> _commandHistory = new();
        // Frame → PlayerId → Hash
        private readonly Dictionary<ulong, Dictionary<long, uint>> _stateHashes = new();


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

        public void RequestRollback(ulong currentServerFrame)
        {
            SimulateRollback(currentServerFrame).Forget();
        }

        public void RequestCommandsHistory()
        {
            SimulateRequestCommandsHistory().Forget();
        }

        public void Dispose()
        {
            Disconnect();
        }

        private async UniTaskVoid SimulateCommandDelivery(IReadOnlyCollection<ICommand> commands)
        {
            // Simulate uplink (client to server)
            await UniTask.Delay(Options.GetRandomLatency());
            if (Options.ShouldDropPacket()) return;

            // Store command on the simulated server
            foreach (var cmd in commands)
            {
                cmd.Frame += 10;

                if (!_commandHistory.TryGetValue(cmd.Frame, out var frameCommands))
                {
                    frameCommands = new List<ICommand>();
                    _commandHistory[cmd.Frame] = frameCommands;
                }

                bool isDuplicate = frameCommands.Exists(existing =>
                    existing.Id == cmd.Id &&
                    existing.PlayerId == cmd.PlayerId);

                if (!isDuplicate)
                {
                    frameCommands.Add(cmd);
                }
                else
                {
                    Debug.LogWarning($"Duplicate command ignored: Frame={cmd.Frame}, Id={cmd.Id}, PlayerId={cmd.PlayerId}");
                }
            }


            // Simulate downlink (server to client)
            await UniTask.Delay(Options.GetRandomLatency());
            await UniTask.SwitchToMainThread();
            if (Options.ShouldDropPacket()) return;

            OnCommandsReceived?.Invoke(commands);
        }

        private async UniTaskVoid SimulateSyncStatus(StateHashReport report)
        {
            await UniTask.Delay(Options.GetRandomLatency());
            await UniTask.SwitchToMainThread();

            if (Options.ShouldDropPacket()) return;

            // Store the player's hash
            if (!_stateHashes.TryGetValue(report.Frame, out var playerHashes))
            {
                playerHashes = new Dictionary<long, uint>();
                _stateHashes[report.Frame] = playerHashes;
            }

            playerHashes[report.PlayerId] = report.Hash;

            // Check if all hashes for this frame match
            bool isSynced = true;
            if (playerHashes.Count > 1)
            {
                var firstHash = playerHashes.Values.First();
                isSynced = playerHashes.Values.All(h => h == firstHash);
            }

            if(report.Frame > 40)
                isSynced= false;

            // Notify client of sync status
            OnSyncStatusReceived?.Invoke(new SyncStatus
            {
                Frame = report.Frame,
                IsSynced = isSynced
            });

            // Trigger rollback if enabled and desync detected
            if (!isSynced && Options.ShouldTriggerRollback())
            {
                OnRollbackRequested?.Invoke(new RollbackRequest
                {
                    CurrentServerFrame = report.Frame,
                    Reason = "Hash mismatch detected"
                });
            }
        }

        private async UniTaskVoid SimulateRequestCommandsHistory()
        {
            await UniTask.Delay(Options.GetRandomLatency());
            await UniTask.SwitchToMainThread();

            if (Options.ShouldDropPacket()) return;

            // Flatten all commands across frames
            var allCommands = new List<ICommand>();
            foreach (var frameCommands in _commandHistory.Values)
            {
                allCommands.AddRange(frameCommands);
            }

            // Optional: sort by frame
            allCommands.Sort((a, b) => a.Frame.CompareTo(b.Frame));

            OnCommandsHistoryReceived?.Invoke(allCommands);
        }

        private async UniTaskVoid SimulatePing()
        {
            await UniTask.Delay(Options.GetRandomLatency());
            await UniTask.SwitchToMainThread();

            OnPingResult?.Invoke(TimeSpan.FromMilliseconds(Options.BaseLatencyMs));
        }

        private async UniTaskVoid SimulateRollback(ulong currentServerFrame)
        {
            await UniTask.Delay(Options.GetRandomLatency());
            await UniTask.SwitchToMainThread();

            if (Options.ShouldDropPacket()) return;
            OnRollbackRequested?.Invoke(new RollbackRequest() 
            {
                CurrentServerFrame = currentServerFrame
            });
        }

        public Task RequestMatchAsync(string gameMode, object customData = null)
        {
            throw new NotImplementedException("Matchmaking simulation not implemented.");
        }

#if UNITY_EDITOR
        // Convenience for triggering from editor
        public void Editor_SendPing() => SendPing();
#endif
    }
}
