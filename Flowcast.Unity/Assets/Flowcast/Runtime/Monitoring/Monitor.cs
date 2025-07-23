using Flowcast.Commands;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Flowcast.Monitoring
{
    public class Monitor : MonoBehaviour
    {
        public static Monitor Instace;

        [SerializeField] private TextMeshProUGUI _infoText;
        [SerializeField] private Button _closeButton;
        [SerializeField] private TimeFrameMonitor _timeframeMonitor;
        [SerializeField] private LogMonitor _logMonitor;
        [SerializeField] private Button _pingButton;

        public ulong SegmentInterval = 10;

        public LockstepEngine Flowcast { get; private set; }

        private List<FlowcastLogEntry> _logs = new();

        private ulong _lastTurnSegmented = 0;
        private bool _commandSentSinceLast = false;
        private bool _commandReceivedSinceLast = false;
        private bool _rollbackOccurredSinceLast = false;

        private float _lastStatsUpdateTime = 0f;
        private ulong _lastGameFrame = 0;
        private ulong _lastTurn = 0;

        private float _fps = 0f;
        private float _tps = 0f;



        private void Awake()
        {
            Instace = this;

            _closeButton.onClick.AddListener(Close);
            _pingButton.onClick.AddListener(SendPingCommand);
        }

        public void MonitorFlowcast(LockstepEngine flowcast)
        {
            Flowcast = flowcast;

            Flowcast.LockstepProvider.OnLockstepTurn += OnLockstepTurn;
            Flowcast.CommandChannel.OnCommandsReceived += OnCommandsReceived;
            Flowcast.CommandChannel.OnCommandsSent += OnCommandSent;
            Flowcast.RollbackHandler.OnRollbackPrepared += OnRollback;

            // Optional: hook into command collector dispatch

        }

        private void UpdateInfo(string localPlayer, ulong frame, ulong turn, float simTime, float speed)
        {
            _infoText.text = $"Local Player: {localPlayer}\n" +
                             $"Frame: {frame} ,FPS: {_fps:F1}\n" +
                             $"Turn: {turn}, TPS: {_tps:F1}\n" +
                             $"Sim Time: {simTime:F2}\n" +
                             $"Speed: {speed:F2}";
        }


        private void AddTimeframeSegment(ulong currentTurn, string label)
        {
            _timeframeMonitor.AddSegment(currentTurn, label);
        }

        private void AddLogEntry(FlowcastLogEntry entry)
        {
            _logs.Add(entry);

            _logMonitor.AddLog(entry);
            _timeframeMonitor.AddLog(entry);
        }

        private void OnCommandSent(IReadOnlyCollection<ICommand> commands)
        {
            _commandSentSinceLast = true;

            foreach (var command in commands)
                AddLogEntry(new CommandLogEntry
                {
                    Command = command,
                    Type = LogType.CommandSent,
                    Turn = command.Frame
                });
        }

        private void OnCommandsReceived(IReadOnlyCollection<ICommand> commands)
        {
            _commandReceivedSinceLast = true;
            foreach (var command in commands)
                AddLogEntry(new CommandLogEntry
                {
                    Command = command,
                    Type = LogType.CommandRecieved,
                    Turn = command.Frame
                });
        }

        private void OnRollback(ulong frame)
        {
            _rollbackOccurredSinceLast = true;
            AddLogEntry(new RollbackLogEntry
            {
                Type = LogType.Rollback,
                Turn = frame
            });
        }

        private void OnLockstepTurn()
        {
            ulong currentTurn = Flowcast.LockstepProvider.CurrentLockstepTurn;
            ulong currentFrame = Flowcast.LockstepProvider.CurrentGameFrame;
            float now = Time.realtimeSinceStartup;
            float delta = now - _lastStatsUpdateTime;

            if (delta >= 1f)
            {
                _fps = (currentFrame - _lastGameFrame) / delta;
                _tps = (currentTurn - _lastTurn) / delta;

                _lastGameFrame = currentFrame;
                _lastTurn = currentTurn;
                _lastStatsUpdateTime = now;
            }

            if (currentTurn >= _lastTurnSegmented + SegmentInterval)
            {
                string label = $"Turn {currentTurn}";
                if (_rollbackOccurredSinceLast) label += " 🔴";
                else if (_commandReceivedSinceLast) label += " 🔵";
                else if (_commandSentSinceLast) label += " 🟢";

                AddTimeframeSegment(currentTurn, label);

                _lastTurnSegmented = currentTurn;
                _commandSentSinceLast = false;
                _commandReceivedSinceLast = false;
                _rollbackOccurredSinceLast = false;
            }

            UpdateInfo(
                Flowcast.PlayerProvider.GetLocalPlayerId().ToString(),
                Flowcast.LockstepProvider.CurrentGameFrame,
                currentTurn,
                Flowcast.LockstepProvider.SimulationTimeTicks / 1000f,
                (float)Flowcast.LockstepProvider.SimulationSpeedMultiplier
            );
        }



        private void Close()
        {
            gameObject.SetActive(false);
        }

        private void SendPingCommand()
        {
            var ping = new PingCommand();
            Flowcast.SubmitCommand(ping);
        }

    }

}
