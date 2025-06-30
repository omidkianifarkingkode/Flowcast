using Flowcast.Inputs;
using Flowcast.Lockstep;
using Flowcast.Logging;
using Flowcast.Pipeline;
using Flowcast.Serialization;
using Flowcast.Synchronization;

namespace Flowcast
{
    public interface IFlowcastEngine
    {
        void SubmitInput(IInput input);

    }

    public class FlowcastEngine : IFlowcastEngine
    {
        private readonly ILocalInputCollector _inputCollector;
        private readonly IRemoteInputCollector _inputChannel;
        private readonly IGameUpdatePipeline _gameUpdatePipeline;
        private readonly IGameStateSyncService _gameStateSyncService;
        private readonly IGameStateSerializer _gameStateSerializer;
        private readonly ILockstepProvider _lockstepProvider;
        private readonly ILogger _logger;

        public FlowcastEngine(
            ILocalInputCollector inputCollector,
            IRemoteInputCollector inputChannel,
            IGameUpdatePipeline gameUpdatePipeline,
            IGameStateSyncService gameStateSyncService,
            ILockstepProvider lockstepProvider,
            ILogger logger,
            IGameStateSerializer gameStateSerializer)
        {
            _inputCollector = inputCollector;
            _inputChannel = inputChannel;
            _gameUpdatePipeline = gameUpdatePipeline;
            _gameStateSyncService = gameStateSyncService;
            _lockstepProvider = lockstepProvider;
            _logger = logger;

            // Hook up event-driven "sagas"
            _lockstepProvider.OnGameFrame += OrchestrateGameFrame;
            _lockstepProvider.OnLockstepTurn += OrchestrateLockstepTurn;
            _gameStateSerializer = gameStateSerializer;
        }

        public void SubmitInput(IInput input)
        {
            var result = _inputCollector.Collect(input);

            if (!result.IsSuccess)
                _logger.LogWarning($"Input rejected: {result.Error}");
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
