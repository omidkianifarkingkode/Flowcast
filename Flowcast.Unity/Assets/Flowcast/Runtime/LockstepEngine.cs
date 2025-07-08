using Flowcast.Data;
using Flowcast.Inputs;
using Flowcast.Lockstep;
using Flowcast.Logging;
using Flowcast.Pipeline;
using Flowcast.Serialization;
using Flowcast.Synchronization;

namespace Flowcast
{
    public interface ILockstepEngine
    {
        ILocalInputCollector InputCollector { get; }
        IRemoteInputChannel InputChannel { get; }
        IGameUpdatePipeline GameUpdatePipeline { get; }
        IGameStateSyncService GameStateSyncService { get; }
        IGameStateSerializer GameStateSerializer { get; }
        ILockstepProvider LockstepProvider { get; }
        IPlayerProvider PlayerProvider { get; }
        ILogger Logger { get; }

        void SubmitInput(IInput input);
        void StartTicking(); // Begin simulation
        void StopTicking();  // Optional for pause/leave
    }

    public class LockstepEngine : ILockstepEngine
    {
        public static ILockstepEngine Instance { get; private set; }

        public ILocalInputCollector InputCollector => _localInputCollector;
        public IRemoteInputChannel InputChannel => _remoteInputChannel;
        public IGameUpdatePipeline GameUpdatePipeline => _gameUpdatePipeline;
        public IGameStateSyncService GameStateSyncService => _gameStateSyncService;
        public IGameStateSerializer GameStateSerializer => _gameStateSerializer;
        public ILockstepProvider LockstepProvider => _lockstepProvider;
        public IPlayerProvider PlayerProvider => _playerProvider;
        public ILogger Logger => _logger;

        private readonly ILocalInputCollector _localInputCollector;
        private readonly IRemoteInputChannel _remoteInputChannel;
        private readonly IGameUpdatePipeline _gameUpdatePipeline;
        private readonly IGameStateSyncService _gameStateSyncService;
        private readonly IGameStateSerializer _gameStateSerializer;
        private readonly LockstepProviderBase _lockstepProvider;
        private readonly IPlayerProvider _playerProvider;
        private readonly ILogger _logger;

        private bool _isTicking;

        public LockstepEngine(
            ILocalInputCollector inputCollector,
            IRemoteInputChannel inputChannel,
            IGameUpdatePipeline gameUpdatePipeline,
            IGameStateSyncService gameStateSyncService,
            LockstepProviderBase lockstepProvider,
            ILogger logger,
            IGameStateSerializer gameStateSerializer,
            IPlayerProvider playerProvider)
        {
            Instance = this;

            _localInputCollector = inputCollector;
            _remoteInputChannel = inputChannel;
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

        public void SubmitInput(IInput input)
        {
            var result = _localInputCollector.Collect(input);

            if (!result.IsSuccess)
                _logger.LogWarning($"Input rejected: {result.Error}");
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
            var frame = _lockstepProvider.CurrentGameFrame;

            DispatchPlayerInputs();
            ProcessInputs(frame);
            UpdateGameState(frame);
        }

        private void OrchestrateLockstepTurn()
        {
            var turn = _lockstepProvider.CurrentLockstepTurn;

            SyncGameState(turn);
            HandleRollback();
        }

        private void DispatchPlayerInputs()
        {
            var buffered = _localInputCollector.ConsumeBufferedInputs();
            if (buffered.Count > 0)
            {
                _remoteInputChannel.SendInputs(buffered);
                _logger.Log($"[InputDispatch] Sent {buffered.Count} inputs");
            }
        }

        private void ProcessInputs(ulong frame)
        {
            var inputs = _remoteInputChannel.GetInputsForFrame(frame);
            foreach (var input in inputs)
                _localInputCollector.Collect(input); // Apply remote inputs

            _remoteInputChannel.RemoveInputsForFrame(frame); // Clean up
        }

        private void UpdateGameState(ulong frame)
        {
            _gameUpdatePipeline.ProcessFrame(frame);
        }

        private void SyncGameState(ulong turn)
        {
            _gameStateSyncService.CaptureAndSyncSnapshot(_lockstepProvider.CurrentGameFrame);
        }

        private bool HandleRollback()
        {
            if (_gameStateSyncService.NeedsRollback())
            {
                _logger.LogWarning("State desync detected. Rolling back...");
                _gameStateSyncService.RollbackToVerifiedFrame();

                return true;
            }

            return false;
        }
    }
}
