using Flowcast.Commons;
using Flowcast.Data;
using Flowcast.Commands;
using Flowcast.Lockstep;
using Flowcast.Logging;
using Flowcast.Network;
using Flowcast.Pipeline;
using Flowcast.Synchronization;
using System;
using System.Linq;
using UnityEngine;
using ILogger = Flowcast.Logging.ILogger;
using Flowcast.Rollback;

namespace Flowcast.Builders
{
    public class LockstepBuilder : IRequireMatchInfo, IRequireCommand, IRequireGameState, IRequireNetwork, IRequirePipline, IOptionalSettings
    {
        private MatchInfo _matchInfo;
        private ILogger _logger;

        private CommandOptions _commandOptions;
        private GameStateSyncOptionsBuilder _gameStateSyncBuilder;
        private NetworkBuilder _networkBuilder;
        private GameUpdatePipelineBuilder _gameUpdatePipelineBuilder;

        public IRequireCommand SetMatchInfo(MatchInfo matchInfo)
        {
            _matchInfo = matchInfo;
            return this;
        }

        public IRequireGameState ConfigureCommandSystem(Action<ICommandOptionsBuilderStart> command) 
        {
            var optionBuilder = new CommandOptionsBuilder();

            command?.Invoke(optionBuilder);

            _commandOptions = optionBuilder.Build();

            return this;
        }

        public IRequireNetwork SynchronizeGameState(Action<IGameStateSyncOptionsBuilder> gameState)
        {
            _gameStateSyncBuilder = new();

            gameState?.Invoke(_gameStateSyncBuilder);

            _gameStateSyncBuilder.Build();

            return this;
        }

        public IRequirePipline SetupNetworkServices(Action<INetworkBuilder> network)
        {
            _networkBuilder = new();

            network?.Invoke(_networkBuilder);

            _networkBuilder.Build();

            return this;
        }

        public IOptionalSettings ConfigureSimulationPipeline(Action<IGameUpdatePipelineBuilder> pipline)
        {
            _gameUpdatePipelineBuilder = new();

            pipline?.Invoke(_gameUpdatePipelineBuilder);

            return this;
        }

        public IOptionalSettings SetLogger(ILogger logger)
        {
            _logger = logger;
            return this;
        }

        public ILockstepEngine BuildAndStart()
        {
            if (_commandOptions == null)
                throw new InvalidOperationException("Command-System must be configured before building.");

            if (_gameStateSyncBuilder == null)
                throw new InvalidOperationException("Game state sync must be configured before building.");

            if (_networkBuilder == null)
                throw new InvalidOperationException("Network must be configured before building.");

            _gameStateSyncBuilder.Options = _matchInfo.GameSettings;

            _logger ??= new UnityLogger();

            var playerProvider = new PlayerProvider(
                _matchInfo.LocalPlayerId,
                _matchInfo.Players.Select(x => x.PlayerId).ToArray());

            _gameUpdatePipelineBuilder ??= new();
            IGameUpdatePipeline pipeline = _gameUpdatePipelineBuilder.Build();

            var lockstepProvider = new LockstepProviderUpdate(_gameStateSyncBuilder.Options, _logger);
            IFrameProvider frameProvider = lockstepProvider;
            IIdGenerator idGenerator = new SequentialIdGenerator();

            ICommandValidatorFactory commandValidatorFactory = new CommandValidatorFactory(_commandOptions.ValidatorFactoryOptions);
            ICommandProcessorFactory commandProcessorFactory = new CommandProcessorFactory(_commandOptions.CommandFactoryOptions);
            IRemoteCommandChannel remoteCommandChannel = new RemoteCommandChannel(_networkBuilder.CommandTransportService);
            ILocalCommandCollector localCommandCollector = new LocalCommandCollector(commandValidatorFactory, frameProvider, idGenerator);
            ICommandManager commandManager = new CommandManager(_commandOptions, localCommandCollector, remoteCommandChannel, commandProcessorFactory, _logger);

            ISnapshotRepository snapshotRepository = new SnapshotRepository(_gameStateSyncBuilder.Serializer, _gameStateSyncBuilder.Hasher, _networkBuilder.SimulationSyncService, _gameStateSyncBuilder.Options, _logger);
            IRollbackHandler rollbackHandler = new RollbackHandler(_gameStateSyncBuilder.Serializer, snapshotRepository, _networkBuilder.RollbackService, _logger, _gameStateSyncBuilder.Options);

            var engine = new LockstepEngine(
                commandManager,
                localCommandCollector,
                remoteCommandChannel,
                pipeline,
                snapshotRepository,
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
