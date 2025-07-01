using Flowcast.Inputs;
using Flowcast.Lockstep;
using Flowcast.Logging;
using Flowcast.Pipeline;
using Flowcast.Player;
using Flowcast.Serialization;
using Flowcast.Synchronization;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Flowcast
{
    public interface IFlowcastEngine
    {
        void Initialize(long localId, IReadOnlyList<long> allIds);
        void SubmitInput(IInput input);
        void StartTicking(); // Begin simulation
        void StopTicking();  // Optional for pause/leave
    }

    public class FlowcastEngine : IFlowcastEngine
    {
        private readonly ILocalInputCollector _inputCollector;
        private readonly IRemoteInputCollector _inputChannel;
        private readonly IGameUpdatePipeline _gameUpdatePipeline;
        private readonly IGameStateSyncService _gameStateSyncService;
        private readonly IGameStateSerializer _gameStateSerializer;
        private readonly LockstepProviderBase _lockstepProvider;
        private readonly PlayerProvider _playerProvider;
        private readonly ILogger _logger;
        private bool _isTicking;

        public FlowcastEngine(
            ILocalInputCollector inputCollector,
            IRemoteInputCollector inputChannel,
            IGameUpdatePipeline gameUpdatePipeline,
            IGameStateSyncService gameStateSyncService,
            LockstepProviderBase lockstepProvider,
            PlayerProvider playerProvider,
            ILogger logger,
            IGameStateSerializer gameStateSerializer)
        {
            _inputCollector = inputCollector;
            _inputChannel = inputChannel;
            _gameUpdatePipeline = gameUpdatePipeline;
            _gameStateSyncService = gameStateSyncService;
            _lockstepProvider = lockstepProvider;
            _playerProvider = playerProvider;
            _logger = logger;

            // Hook up event-driven "sagas"
            _lockstepProvider.OnGameFrame += OrchestrateGameFrame;
            _lockstepProvider.OnLockstepTurn += OrchestrateLockstepTurn;
            _gameStateSerializer = gameStateSerializer;
        }

        public void Initialize(long localId, IReadOnlyList<long> allIds)
        {
            _playerProvider.Initialize(localId, allIds);
            // Optionally forward to other modules
        }

        public void SubmitInput(IInput input)
        {
            var result = _inputCollector.Collect(input);

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
            var buffered = _inputCollector.ConsumeBufferedInputs();
            if (buffered.Count > 0)
            {
                _inputChannel.SendInputs(buffered);
                _logger.Log($"[InputDispatch] Sent {buffered.Count} inputs");
            }
        }

        private void ProcessInputs(ulong frame)
        {
            var inputs = _inputChannel.GetInputsForFrame(frame);
            foreach (var input in inputs)
                _inputCollector.Collect(input); // Apply remote inputs

            _inputChannel.RemoveInputsForFrame(frame); // Clean up
        }

        private void UpdateGameState(ulong frame)
        {
            _gameUpdatePipeline.ProcessFrame(frame);
        }

        private void SyncGameState(ulong turn)
        {
            var serializedGameState = _gameStateSerializer.SerializeSnapshot();

            _gameStateSyncService.SaveSnapshot(_lockstepProvider.CurrentGameFrame, serializedGameState);
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
