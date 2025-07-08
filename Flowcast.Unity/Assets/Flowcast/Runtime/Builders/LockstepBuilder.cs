using Flowcast.Commons;
using Flowcast.Data;
using Flowcast.Inputs;
using Flowcast.Lockstep;
using Flowcast.Logging;
using Flowcast.Network;
using Flowcast.Pipeline;
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
        private ILogger _logger;
        private ILockstepSettings _settings;

        private GameStateSyncOptionsBuilder _gameStateSyncBuilder;
        private NetworkBuilder _networkBuilder;
        private GameUpdatePipelineBuilder _gameUpdatePipelineBuilder;

        public IRequireGameState SetGameSession(GameSessionData gameSessionData)
        {
            _gameSessionData = gameSessionData;
            return this;
        }

        public IRequireNetwork SynchronizeGameState(Action<IGameStateSyncOptionsBuilder> setup)
        {
            _gameStateSyncBuilder = new();

            setup?.Invoke(_gameStateSyncBuilder);

            _gameStateSyncBuilder.Build();

            return this;
        }

        public IOptionalSettings SetupNetworkServices(Action<INetworkBuilder> setup)
        {
            _networkBuilder = new();

            setup?.Invoke(_networkBuilder);

            _networkBuilder.Build();

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

        public IOptionalSettings SetupProcessPipeline(Action<IGameUpdatePipelineBuilder> setup) 
        {
            _gameUpdatePipelineBuilder = new();

            setup?.Invoke(_gameUpdatePipelineBuilder);

            return this;
        }

        public ILockstepEngine BuildAndStart()
        {
            _settings ??= LockstepSettingsAsset.Instance;
            _logger ??= new UnityLogger();

            var playerProvider = new PlayerProvider(
                _gameSessionData.LocalPlayerId,
                _gameSessionData.Players.Select(x => x.PlayerId).ToArray());

            IInputValidatorFactory validatorFactory = new InputValidatorFactory(builder => builder.AutoMap());
            IRemoteInputChannel remoteCollector = new RemoteInputChannel(_networkBuilder.InputTransportService);

            _gameUpdatePipelineBuilder ??= new();
            IGameUpdatePipeline pipeline = _gameUpdatePipelineBuilder.Build();

            var rollbackHandler = new RollbackHandler(_gameStateSyncBuilder.Serializer, _logger, _gameStateSyncBuilder.Options);
            IGameStateSyncService syncService = new GameStateSyncService(_gameStateSyncBuilder.GameState, _gameStateSyncBuilder.Serializer, _gameStateSyncBuilder.Hasher, rollbackHandler, _networkBuilder.SimulationSyncService, _gameStateSyncBuilder.Options);
            var lockstepProvider = new LockstepProviderUpdate(_settings, _logger);

            IFrameProvider frameProvider = lockstepProvider;
            IIdGenerator idGenerator = new SequentialIdGenerator();

            ILocalInputCollector inputCollector = new LocalInputCollector(
                validatorFactory, playerProvider, frameProvider, idGenerator
            );

            var engine = new LockstepEngine(
                inputCollector,
                remoteCollector,
                pipeline,
                syncService,
                lockstepProvider,
                _logger,
                _gameStateSyncBuilder.Serializer,
                playerProvider
            );

            var flowcastRunner = new GameObject("Flowcast Engin", typeof(FlowcastRunner)).GetComponent<FlowcastRunner>();
            flowcastRunner.SetEngine(engine);

            return engine;
        }
    }
}
