using Flowcast.Commons;
using Flowcast.Data;
using Flowcast.Inputs;
using Flowcast.Lockstep;
using Flowcast.Logging;
using Flowcast.Network;
using Flowcast.Pipeline;
using Flowcast.Serialization;
using Flowcast.Synchronization;
using System;
using System.Linq;
using UnityEngine;
using ILogger = Flowcast.Logging.ILogger;

namespace Flowcast.Builders
{
    public class LockstepBuilder : IRequireGameSession, IRequireGameState, IRequireNetwork, IOptionalSettings
    {
        private GameSessionData _gameSessionData;
        private ISerializableGameState _gameState;
        private ILogger _logger;
        private ILockstepSettings _settings;
        private IGameStateSerializer _gameStateSerializer;

        private INetworkConnectionService _connectionService;
        private IInputTransportService _inputTransportService;
        private ISimulationSyncService _simulationSyncService;
        private INetworkDiagnosticsService _diagnosticsService;

        private RollbackConfig _rollbackConfig = new();

        public IRequireGameState SetGameSession(GameSessionData gameSessionData)
        {
            _gameSessionData = gameSessionData;
            return this;
        }

        public IRequireNetwork SetGameStateModel(ISerializableGameState state)
        {
            _gameState = state;
            return this;
        }

        public IOptionalSettings SetNetworkServices(INetworkConnectionService connectionService,
                                                    IInputTransportService inputTransportService,
                                                    ISimulationSyncService simulationSyncService,
                                                    INetworkDiagnosticsService diagnosticsService)
        {
            _connectionService = connectionService;
            _inputTransportService = inputTransportService;
            _simulationSyncService = simulationSyncService;
            _diagnosticsService = diagnosticsService;
            return this;
        }

        public IOptionalSettings SetLogger(ILogger logger)
        {
            _logger = logger;
            return this;
        }

        public IOptionalSettings SetLockstepSettings(ILockstepSettings settings)
        {
            _settings = settings;
            return this;
        }

        public IOptionalSettings ConfigureRollback(Action<RollbackConfig> config)
        {
            config?.Invoke(_rollbackConfig);
            return this;
        }

        public IOptionalSettings SetGameStateSerializer(IGameStateSerializer serializer)
        {
            _gameStateSerializer = serializer;
            return this;
        }


        public IFlowcastEngine BuildAndStart()
        {
            _settings ??= LockstepSettingsAsset.Instance;
            _logger ??= new UnityLogger();

            var playerProvider = new PlayerProvider(
                _gameSessionData.LocalPlayerId,
                _gameSessionData.Players.Select(x => x.PlayerId).ToArray());

            IInputValidatorFactory validatorFactory = new InputValidatorFactory(builder => builder.AutoMap());
            IRemoteInputChannel remoteCollector = new RemoteInputChannel(_inputTransportService);
            IGameUpdatePipeline pipeline = SimulationPipelineBuilder.BuildDefault();

            _gameStateSerializer ??= new GameStateSerializer(() => _gameState);
            var rollbackHandler = new RollbackHandler(_gameStateSerializer, _logger, _rollbackConfig);
            IGameStateSyncService syncService = new GameStateSyncService(new XorHasher(), rollbackHandler, _simulationSyncService);//todo: set hasher as optinal builder and force to introduce IInetwork
            var lockstepProvider = new LockstepProviderUpdate(_settings, _logger);

            IFrameProvider frameProvider = lockstepProvider;
            IIdGenerator idGenerator = new SequentialIdGenerator();

            ILocalInputCollector inputCollector = new LocalInputCollector(
                validatorFactory, playerProvider, frameProvider, idGenerator
            );

            var engine = new FlowcastEngine(
                inputCollector,
                remoteCollector,
                pipeline,
                syncService,
                lockstepProvider,
                _logger,
                _gameStateSerializer,
                playerProvider
            );

            var flowcastRunner = new GameObject("Flowcast Engin", typeof(FlowcastRunner)).GetComponent<FlowcastRunner>();
            flowcastRunner.SetEngine(engine);

            return engine;
        }
    }
}
