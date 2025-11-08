using FixedMathSharp;
using Flowcast.Commands;
using Flowcast.Network;
using Flowcast.Options;
using Flowcast.Serialization;
using Flowcast.Synchronization;
using LogKit;
using System;
using System.Collections.Generic;

namespace Flowcast.Rollback
{
    public class RollbackHandler : IRollbackHandler
    {
        private readonly IGameStateSerializer _serializer;
        private readonly ISnapshotRepository _snapshotRepository;
        private readonly INetworkRollbackService _networkService;
        private readonly ILogger _logger;
        private readonly ILockstepEngineOptions _options;

        public bool IsInRecovery => State != RollbackState.None;
        public RollbackState State { get; private set; }

        public event Action<ulong> OnRollbackPrepared;
        public event Action<ulong> OnRollbackStarted;
        public event Action OnRollbackFinished;

        private Action _prepareRollbackCallback;
        private Action<ulong, IReadOnlyCollection<ICommand>> _startRollbackCallback;
        private Action _finishRollbackCallback;

        private RollbackRequest _pendingRollbackRequest;
        private ulong _targetRecoveryFrame;

        public RollbackHandler(IGameStateSerializer serializer, ISnapshotRepository snapshotRepository, INetworkRollbackService networkService, ILockstepEngineOptions options, ILogger logger)
        {
            _serializer = serializer;
            _snapshotRepository = snapshotRepository;
            _networkService = networkService;
            _logger = logger;
            _options = options;

            _networkService.OnRollbackRequested += HandleRollbackRequest;
            _networkService.OnCommandsHistoryReceived += HandleCommandHistoryResponse;
        }

        public void CheckAndApplyRollback(ulong frame, Action onPreparing, Action<ulong, IReadOnlyCollection<ICommand>> onStarted, Action onFinished)
        {
            if (State == RollbackState.Started)
            {
                UpdateRecovery(frame);
            }

            if (State == RollbackState.Pending)
            {
                _prepareRollbackCallback = onPreparing;
                _startRollbackCallback = onStarted;
                _finishRollbackCallback = onFinished;

                PrepareRollback();
            }
        }

        private void PrepareRollback()
        {
            if (State != RollbackState.Pending)
                return;

            State = RollbackState.Preparing;

            _prepareRollbackCallback.Invoke();

            OnRollbackPrepared?.Invoke(_pendingRollbackRequest.CurrentServerFrame);

            _networkService.RequestCommandsHistory();
        }

        private void StartRollback(IReadOnlyCollection<ICommand> commands, SnapshotEntry? snapshotEntry = null)
        {
            if (State != RollbackState.Preparing)
                return;

            var snapshotTick = snapshotEntry is null ? 0 : snapshotEntry.Value.Tick;
            var snapshotData = snapshotEntry is null ? _serializer.CreateDefault() : _serializer.DeserializeSnapshot(snapshotEntry.Value.Data);

            State = RollbackState.Started;

            _targetRecoveryFrame = EstimateTargetFrame(snapshotTick);

            _logger.Log($"[Recovery] Rolled back to frame {snapshotTick}. Target to catch up: {_targetRecoveryFrame}");

            _startRollbackCallback.Invoke(snapshotTick, commands);

            _options.OnRollback?.Invoke(snapshotData, _targetRecoveryFrame);

            OnRollbackStarted?.Invoke(snapshotTick);
        }

        private void UpdateRecovery(ulong frame)
        {
            if (frame < _targetRecoveryFrame)
                return;

            _logger.Log($"[Recovery] Caught up to frame {_targetRecoveryFrame}.");

            State = RollbackState.None;

            _finishRollbackCallback.Invoke();

            _pendingRollbackRequest = null;

            _prepareRollbackCallback = null;
            _startRollbackCallback = null;
            _finishRollbackCallback = null;

            OnRollbackFinished?.Invoke();
        }

        private ulong EstimateTargetFrame(ulong rollbackStartFrame)
        {
            var networkTargetFrame = _pendingRollbackRequest.CurrentServerFrame;
            var speedMultiplier = _options.MaxRecoverySpeed;
            var gameFps = (Fixed64)_options.GameFramesPerSecond;

            if (speedMultiplier <= Fixed64.One || rollbackStartFrame >= networkTargetFrame)
                return networkTargetFrame;

            var gap = (Fixed64)(long)(networkTargetFrame - rollbackStartFrame);

            // Time in seconds it takes to catch up
            var catchupTime = gap / (gameFps * (speedMultiplier - Fixed64.One));

            // How many more frames will pass on the network during catch-up
            var estimatedDrift = catchupTime * gameFps;

            var estimatedTotal = gap + estimatedDrift;

            // Manual ceiling of Fixed64 to ulong
            long intPart = estimatedTotal.m_rawValue >> 32;
            bool hasFraction = (estimatedTotal.m_rawValue & 0xFFFFFFFFL) != 0;
            if (hasFraction) intPart += 1;

            return rollbackStartFrame + (ulong)intPart;
        }

        private void HandleRollbackRequest(RollbackRequest request)
        {
            _pendingRollbackRequest = request;

            State = RollbackState.Pending;

            _logger.Log("[Rollback] Request Pending ...");
        }

        private void HandleCommandHistoryResponse(IReadOnlyCollection<ICommand> commands)
        {
            SnapshotEntry? snapshotEntry = null;
            if (_snapshotRepository.ResetToLastSyncedSnapShot(out var entry))
                snapshotEntry = entry;

            StartRollback(commands, snapshotEntry);
        }
    }
}
