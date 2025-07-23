using Flowcast.Commands;
using Flowcast.Data;
using Flowcast.Lockstep;
using Flowcast.Pipeline;
using Flowcast.Rollback;
using Flowcast.Serialization;
using Flowcast.Synchronization;
using System;
using ILogger = LogKit.ILogger;

namespace Flowcast
{

    public interface ILockstepEngine : ILockstepScheduler
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
        public ISnapshotRepository GameStateSyncService => _snapshotRepository;
        public IRollbackHandler RollbackHandler => _rollbackHandler;
        public IGameStateSerializer GameStateSerializer => _gameStateSerializer;
        public ILockstepProvider LockstepProvider => _lockstepProvider;
        public ILockstepScheduler LockstepScheduler => _scheduler;
        public IPlayerProvider PlayerProvider => _playerProvider;
        public ILogger Logger => _logger;

        private readonly ICommandManager _commandManager;
        private readonly ILocalCommandCollector _localCommandCollector;
        private readonly IRemoteCommandChannel _remoteCommandChannel;
        private readonly IGameUpdatePipeline _gameUpdatePipeline;
        private readonly ISnapshotRepository _snapshotRepository;
        private readonly IRollbackHandler _rollbackHandler;
        private readonly IGameStateSerializer _gameStateSerializer;
        private readonly ILockstepProvider _lockstepProvider;
        private readonly LockstepScheduler _scheduler;
        private readonly IPlayerProvider _playerProvider;
        private readonly ILogger _logger;

        private bool _isTicking;

        public LockstepEngine(
            ICommandManager commandManager,
            ILocalCommandCollector commandCollector,
            IRemoteCommandChannel commandChannel,
            IGameUpdatePipeline gameUpdatePipeline,
            ISnapshotRepository snapshotRepository,
            IRollbackHandler rollbackHandler,
            ILockstepProvider lockstepProvider,
            LockstepScheduler lockstepScheduler,
            ILogger logger,
            IGameStateSerializer gameStateSerializer,
            IPlayerProvider playerProvider)
        {
            Instance = this;

            _commandManager = commandManager;
            _localCommandCollector = commandCollector;
            _remoteCommandChannel = commandChannel;
            _gameUpdatePipeline = gameUpdatePipeline;
            _snapshotRepository = snapshotRepository;
            _rollbackHandler = rollbackHandler;
            _lockstepProvider = lockstepProvider;
            _scheduler = lockstepScheduler;
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

        public void Schedule(Action action, int delayMs) => _scheduler.Schedule(action, delayMs);
        public void Schedule(ulong frame, Action action) => _scheduler.Schedule(frame, action);

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
                    _scheduler.ResetToFrame(toFrame);

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
            var deltaTime = _lockstepProvider.FixedDeltaTime;

            _scheduler.UpdateFrame(frame);

            // Process Commands
            _commandManager.ProcessOnFrame(frame);

            // Update Gameplay
            _gameUpdatePipeline.ProcessFrame(frame, deltaTime);
        }

        private void OrchestrateLockstepTurn()
        {
            var step = _lockstepProvider.CurrentLockstepTurn;

            // Process Commands
            _commandManager.ProcessOnLockstep(step);

            // SyncGameState
            _snapshotRepository.CaptureAndSyncSnapshot(_lockstepProvider.CurrentGameFrame);
        }
    }

}
