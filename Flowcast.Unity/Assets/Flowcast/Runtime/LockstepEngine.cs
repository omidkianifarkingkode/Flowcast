using Flowcast.Data;
using Flowcast.Commands;
using Flowcast.Lockstep;
using Flowcast.Logging;
using Flowcast.Pipeline;
using Flowcast.Serialization;
using Flowcast.Synchronization;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

namespace Flowcast
{
    public interface ILockstepEngine
    {
        bool IsInRecovery { get; }

        ILocalCommandCollector CommandCollector { get; }
        IRemoteCommandChannel CommandChannel { get; }
        IGameUpdatePipeline GameUpdatePipeline { get; }
        IGameStateSyncService GameStateSyncService { get; }
        IGameStateSerializer GameStateSerializer { get; }
        ILockstepProvider LockstepProvider { get; }
        IPlayerProvider PlayerProvider { get; }
        ILogger Logger { get; }

        void SubmitCommand(ICommand command);
        void StartTicking(); // Begin simulation
        void StopTicking();  // Optional for pause/leave
    }

    public class LockstepEngine : ILockstepEngine
    {
        public static ILockstepEngine Instance { get; private set; }

        public bool IsInRecovery => _isInRecovery;

        public ILocalCommandCollector CommandCollector => _localCommandCollector;
        public IRemoteCommandChannel CommandChannel => _remoteCommandChannel;
        public IGameUpdatePipeline GameUpdatePipeline => _gameUpdatePipeline;
        public IGameStateSyncService GameStateSyncService => _gameStateSyncService;
        public IGameStateSerializer GameStateSerializer => _gameStateSerializer;
        public ILockstepProvider LockstepProvider => _lockstepProvider;
        public IPlayerProvider PlayerProvider => _playerProvider;
        public ILogger Logger => _logger;

        private readonly ICommandManager _commandManager;
        private readonly ILocalCommandCollector _localCommandCollector;
        private readonly IRemoteCommandChannel _remoteCommandChannel;
        private readonly IGameUpdatePipeline _gameUpdatePipeline;
        private readonly IGameStateSyncService _gameStateSyncService;
        private readonly IGameStateSerializer _gameStateSerializer;
        private readonly LockstepProviderBase _lockstepProvider;
        private readonly IPlayerProvider _playerProvider;
        private readonly ILogger _logger;

        private bool _isTicking;

        private bool _isInRecovery;
        private ulong _targetRecoveryFrame;

        public LockstepEngine(
            ICommandManager commandManager,
            ILocalCommandCollector commandCollector,
            IRemoteCommandChannel commandChannel,
            IGameUpdatePipeline gameUpdatePipeline,
            IGameStateSyncService gameStateSyncService,
            LockstepProviderBase lockstepProvider,
            ILogger logger,
            IGameStateSerializer gameStateSerializer,
            IPlayerProvider playerProvider)
        {
            Instance = this;

            _commandManager = commandManager;
            _localCommandCollector = commandCollector;
            _remoteCommandChannel = commandChannel;
            _gameUpdatePipeline = gameUpdatePipeline;
            _gameStateSyncService = gameStateSyncService;
            _lockstepProvider = lockstepProvider;
            _logger = logger;

            // Hook up event-driven "sagas"
            _lockstepProvider.OnGameFrame += OrchestrateGameFrame;
            _lockstepProvider.OnLockstepTurn += OrchestrateLockstepTurn;
            _gameStateSerializer = gameStateSerializer;
            _playerProvider = playerProvider;
        }

        public void SubmitCommand(ICommand command)
        {
            if (_isInRecovery)
            {
                _logger.LogWarning("Ignoring input during rollback recovery phase.");
                return;
            }

            var result = _localCommandCollector.Collect(command);

            if (!result.IsSuccess)
                _logger.LogWarning($"Command rejected: {result.Error}");
        }

        public void StartTicking()
        {
            _isTicking = true;
        }

        public void StopTicking()
        {
            _isTicking = false;
        }

        public void Tick()
        {
            if (_isTicking)
                _lockstepProvider.Tick();
        }

        private void OrchestrateGameFrame()
        {
            // Begin recovery if rollback was requested
            if (CheckAndRecoverRollback())
                return;

            // Process Commands and Update Gameplay
            ProcessGameFrame();
        }

        private void OrchestrateLockstepTurn()
        {
            var step = _lockstepProvider.CurrentLockstepTurn;

            // Process Commands
            _commandManager.ProcessOnLockstep(step);

            // SyncGameState
            _gameStateSyncService.CaptureAndSyncSnapshot(_lockstepProvider.CurrentGameFrame);
        }

        private bool CheckAndRecoverRollback()
        {
            if (_isInRecovery)
            {
                FinalizeRollback();
                return false;
            }

            if (!_gameStateSyncService.IsRollbackPending)
            {
                return false;
            }

            if (!_gameStateSyncService.TryGetLatestSyncedSnapshot(out var entry))
                return false;

            ulong rollbackStart = entry.Tick;
            ulong networkTarget = _gameStateSyncService.TargetRecoveryFrame;

            _lockstepProvider.AdjustSimulationSpeed(networkTarget - rollbackStart);

            _targetRecoveryFrame = _gameStateSyncService.ApplyPendingRollback(_lockstepProvider.SimulationSpeedMultiplier);
            _isInRecovery = true;

            return true;
        }

        private void FinalizeRollback()
        {
            if (_lockstepProvider.CurrentGameFrame < _targetRecoveryFrame)
            {
                return;
            }

            _lockstepProvider.SimulationSpeedMultiplier = 1f;
            _isInRecovery = false;

            _logger.Log($"[Recovery] Caught up to frame {_targetRecoveryFrame}. Resuming normal speed.");
        }

        private void ProcessGameFrame() 
        {
            var frame = _lockstepProvider.CurrentGameFrame;

            // Process Commands
            _commandManager.ProcessOnFrame(frame);

            // Update Gameplay
            _gameUpdatePipeline.ProcessFrame(frame);
        }
    }

    public static class CatchupEstimator
    {
        public static ulong EstimateTargetFrame(
            ulong rollbackStartFrame,
            ulong networkTargetFrame,
            float speedMultiplier,
            float gameFps)
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
