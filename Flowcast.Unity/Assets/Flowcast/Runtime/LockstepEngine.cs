using Flowcast.Data;
using Flowcast.Commands;
using Flowcast.Lockstep;
using Flowcast.Logging;
using Flowcast.Pipeline;
using Flowcast.Serialization;
using Flowcast.Synchronization;
using Flowcast.Rollback;
using static UnityEditor.Progress;

namespace Flowcast
{
    public interface ILockstepEngine
    {
        ILocalCommandCollector CommandCollector { get; }
        IRemoteCommandChannel CommandChannel { get; }
        IGameUpdatePipeline GameUpdatePipeline { get; }
        ISnapshotRepository GameStateSyncService { get; }
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

        public ILocalCommandCollector CommandCollector => _localCommandCollector;
        public IRemoteCommandChannel CommandChannel => _remoteCommandChannel;
        public IGameUpdatePipeline GameUpdatePipeline => _gameUpdatePipeline;
        public ISnapshotRepository GameStateSyncService => _gameStateSyncService;
        public IRollbackHandler RollbackHandler => _rollbackHandler;
        public IGameStateSerializer GameStateSerializer => _gameStateSerializer;
        public ILockstepProvider LockstepProvider => _lockstepProvider;
        public IPlayerProvider PlayerProvider => _playerProvider;
        public ILogger Logger => _logger;

        private readonly ICommandManager _commandManager;
        private readonly ILocalCommandCollector _localCommandCollector;
        private readonly IRemoteCommandChannel _remoteCommandChannel;
        private readonly IGameUpdatePipeline _gameUpdatePipeline;
        private readonly ISnapshotRepository _gameStateSyncService;
        private readonly IRollbackHandler _rollbackHandler;
        private readonly IGameStateSerializer _gameStateSerializer;
        private readonly LockstepProviderBase _lockstepProvider;
        private readonly IPlayerProvider _playerProvider;
        private readonly ILogger _logger;

        private bool _isTicking;

        public LockstepEngine(
            ICommandManager commandManager,
            ILocalCommandCollector commandCollector,
            IRemoteCommandChannel commandChannel,
            IGameUpdatePipeline gameUpdatePipeline,
            ISnapshotRepository gameStateSyncService,
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
            if (_rollbackHandler.IsInRecovery)
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
            // Begin recovery if rollback was requested
            _rollbackHandler.CheckAndApplyRollback(_lockstepProvider.CurrentGameFrame,
                onPreparing: () =>
                {
                    StopTicking();
                },
                onStarted: (toFrame, commandsHistory) => 
                {
                    _remoteCommandChannel.ResetWith(commandsHistory);

                    _lockstepProvider.ResetFrameTo(toFrame);

                    _lockstepProvider.SetFastModeSimulation();

                    StartTicking();
                },
                onFinished: () => 
                {
                    _lockstepProvider.SetNormalModeSimulation();
                });

            if (_isTicking)
                _lockstepProvider.Tick();
        }

        private void OrchestrateGameFrame()
        {
            var frame = _lockstepProvider.CurrentGameFrame;

            // Process Commands
            _commandManager.ProcessOnFrame(frame);

            // Update Gameplay
            _gameUpdatePipeline.ProcessFrame(frame);
        }

        private void OrchestrateLockstepTurn()
        {
            var step = _lockstepProvider.CurrentLockstepTurn;

            // Process Commands
            _commandManager.ProcessOnLockstep(step);

            // SyncGameState
            _gameStateSyncService.CaptureAndSyncSnapshot(_lockstepProvider.CurrentGameFrame);
        }
    }

}
