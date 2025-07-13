using Flowcast.Lockstep;
using Flowcast.Logging;
using Flowcast.Network;
using Flowcast.Serialization;
using System;

namespace Flowcast.Synchronization
{
    public interface IRollbackHandler
    {
        void CheckAndApplyRollback(Action onPreparing, Action onStarted);
    }

    public enum RollbackState 
    {
        None,
        Pending,
        Started,
        Finished
    }

    public class RollbackHandler : IRollbackHandler
    {
        private readonly IGameStateSerializer _serializer;
        private readonly IGameStateSyncService _snapshotRepository;
        private readonly INetworkGameStateSyncService _networkSyncService;
        private readonly ILockstepProvider _lockstepProvider;
        private readonly ILogger _logger;
        private readonly IGameStateSyncOptions _options;

        private bool _isInRecovery;
        public RollbackState State 
        {
            get => _state;
            private set 
            {
                if(_state == value) return;

                _state = value;

                _state switch
                {
                    RollbackState.None => (),
                    RollbackState.Pending => OnPendingRollback(),
                    RollbackState.Started => OnStartRollback(),
                    RollbackState.Finished => ()
                };
            }
        }
        private RollbackState _state = RollbackState.None;

        public Action OnPendingRollback;
        public Action OnStartRollback;

        private RollbackRequest _pendingRollbackRequest;
        private ulong _targetRecoveryFrame;

        public RollbackHandler(IGameStateSerializer serializer, IGameStateSyncService gameStateSyncService, INetworkGameStateSyncService networkSyncService, ILockstepProvider lockstepProvider, ILogger logger, IGameStateSyncOptions options)
        {
            _serializer = serializer;
            _snapshotRepository = gameStateSyncService;
            _networkSyncService = networkSyncService;
            _lockstepProvider = lockstepProvider;
            _logger = logger;
            _options = options;

            _networkSyncService.OnRollbackRequested += HandleRollbackRequest;
        }

        private void HandleRollbackRequest(RollbackRequest request)
        {
            _pendingRollbackRequest = request;

            State = RollbackState.Pending;

            _logger.Log("[Rollback] ...");
        }

        public void CheckAndApplyRollback(Action onPreparing, Action onStarted)
        {
            if(_state == RollbackState.Pending) 
            {
                PrepareRollback(onPreparing);
                return;
            }

            if (_isInRecovery)
            {
                UpdateRecovery();
                return;
            }
        }

        private void PrepareRollback(Action onPreparing) 
        {
            _isInRecovery = true;

            if (!_snapshotRepository.ResetToLastSyncedSnapShot(out var entry))
            {
                _networkSyncService.RequestCommandsHistory();
                return;
            }

            var snapshot = _serializer.DeserializeSnapshot(entry.Data);

            _targetRecoveryFrame = EstimateTargetFrame(entry.Tick, _pendingRollbackRequest.CurrentNetworkFrame,
                _options.MaxCatchupSpeed, _options.GameFramesPerSecond);

            _lockstepProvider.ResetFrameTo(entry.Tick);
            _lockstepProvider.SimulationSpeedMultiplier = _options.MaxCatchupSpeed;

            _pendingRollbackRequest = null;

            _logger.Log($"[Recovery] Rolled back to frame {entry.Tick}. Target to catch up: {_targetRecoveryFrame}");

            _options.OnRollback?.Invoke(snapshot);
        }

        private void UpdateRecovery()
        {
            if (_lockstepProvider.CurrentGameFrame < _targetRecoveryFrame)
                return;

            _isInRecovery = false;
            _logger.Log($"[Recovery] Caught up to frame {_targetRecoveryFrame}. Resuming normal speed.");

            _lockstepProvider.SimulationSpeedMultiplier = 1;
        }

        public static ulong EstimateTargetFrame(ulong rollbackStartFrame, ulong networkTargetFrame, float speedMultiplier, float gameFps)
        {
            if (speedMultiplier <= 1f || rollbackStartFrame >= networkTargetFrame)
                return networkTargetFrame;

            float gap = networkTargetFrame - rollbackStartFrame;

            // Time in seconds it takes to catch up
            float catchupTime = gap / (gameFps * (speedMultiplier - 1f));

            // How many more frames will pass on the network during catch-up
            float estimatedDrift = catchupTime * gameFps;

            return rollbackStartFrame + (ulong)System.Math.Ceiling(gap + estimatedDrift);
        }
    }
}
