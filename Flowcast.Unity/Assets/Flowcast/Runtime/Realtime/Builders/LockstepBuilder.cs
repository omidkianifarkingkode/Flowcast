using Flowcast.Commands;
using Flowcast.Commons;
using Flowcast.Data;
using Flowcast.Lockstep;
using Flowcast.Network;
using Flowcast.Options;
using Flowcast.Rollback;
using Flowcast.Synchronization;
using LogKit;
using LogKit.Bootstrap;
using System;
using System.Linq;
using UnityEngine;
using ILogger = LogKit.ILogger;

namespace Flowcast.Builders
{
    public class LockstepBuilder : IRequireConfiguration, IRequireMatchInfo, IRequireCommand, IRequireGameState, IRequireNetwork, IOptionalSettings
    {
        private ILockstepEngineOptions _configuration;
        private MatchInfo _matchInfo;

        private CommandOptions _commandOptions;
        private GameStateSyncOptionsBuilder _gameStateSyncBuilder;
        private NetworkBuilder _networkBuilder;

        public IRequireMatchInfo ConfigureWithDefault()
        {
            _configuration = LockstepEngineOptions.Default;

            return this;
        }

        public IRequireMatchInfo ConfigureFromResources()
        {
            _configuration = LockstepEngineOptionsAsset.Load();

            return this;
        }

        public IRequireMatchInfo ConfigureWithCustomOptions(ILockstepEngineOptions options)
        {
            _configuration = options;

            return this;
        }

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

        public IRequireNetwork SynchronizeGameState(Action<IGameStateSyncSnapshotSerializer> gameState)
        {
            _gameStateSyncBuilder = new();

            gameState?.Invoke(_gameStateSyncBuilder);

            _gameStateSyncBuilder.Build();

            return this;
        }

        public IOptionalSettings SetupNetworkServices(Action<INetworkBuilder> network)
        {
            _networkBuilder = new();

            network?.Invoke(_networkBuilder);

            _networkBuilder.Build();

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

            if (_matchInfo.GameSettings is not null)
                _configuration = _matchInfo.GameSettings;

            _configuration.OnRollback = _gameStateSyncBuilder.RollbackCallback;
            _configuration.OnStep = _gameStateSyncBuilder.StepCallback;

            var playerProvider = new PlayerProvider(
                _matchInfo.LocalPlayerId,
                _matchInfo.Players.Select(x => x.PlayerId).ToArray());

            ILockstepProvider lockstepProvider = new LockstepProviderUpdate(_configuration, CreateLogger("Lockstep"));
            IFrameProvider frameProvider = lockstepProvider;
            IIdGenerator idGenerator = new SequentialIdGenerator();

            var commandLogger = CreateLogger("Command");
            ICommandValidatorFactory commandValidatorFactory = new CommandValidatorFactory(_commandOptions.ValidatorFactoryOptions);
            ICommandProcessorFactory commandProcessorFactory = new CommandProcessorFactory(_commandOptions.CommandFactoryOptions);
            IRemoteCommandChannel remoteCommandChannel = new RemoteCommandChannel(_networkBuilder.CommandTransportService, commandLogger);
            ILocalCommandCollector localCommandCollector = new LocalCommandCollector(commandValidatorFactory, frameProvider, idGenerator);
            ICommandManager commandManager = new CommandManager(_commandOptions, localCommandCollector, remoteCommandChannel,
                commandProcessorFactory, commandLogger);

            ISnapshotRepository snapshotRepository = new SnapshotRepository(
                gameStateSerializer: _gameStateSyncBuilder.Serializer,
                hasher: _gameStateSyncBuilder.Hasher,
                networkService: _networkBuilder.SimulationSyncService,
                options: _configuration,
                logger: CreateLogger("Snapshot"));

            IRollbackHandler rollbackHandler = new RollbackHandler(
                serializer: _gameStateSyncBuilder.Serializer,
                snapshotRepository: snapshotRepository,
                networkService: _networkBuilder.RollbackService,
                options: _configuration,
                logger: CreateLogger("Rollback"));

            LockstepScheduler lockstepScheduler = new(_configuration);

            var engine = new LockstepEngine(
                 commandManager: commandManager,
                 commandCollector: localCommandCollector,
                 commandChannel: remoteCommandChannel,
                 snapshotRepository: snapshotRepository,
                 rollbackHandler: rollbackHandler,
                 lockstepProvider: lockstepProvider,
                 lockstepScheduler: lockstepScheduler,
                 logger: CreateLogger("Engine"),
                 gameStateSerializer: _gameStateSyncBuilder.Serializer,
                 playerProvider: playerProvider,
                 options: _configuration
            );

            var flowcastRunner = new GameObject("Flowcast Engin", typeof(FlowcastRunner)).GetComponent<FlowcastRunner>();
            flowcastRunner.SetEngine(engine);

            return engine;
        }

        public ILogger CreateLogger(string moduleName, Action<ILoggerOptions> overrideOptions = null)
        {
            return LoggerFactory.Create($"Flowcast:{moduleName}", options =>
            {
                var defaults = _configuration.Logger;
                
                options.EnableUnitySink = defaults.EnableUnitySink;
                options.EnableFileSink = defaults.EnableFileSink;
                options.MinimumLogLevel = defaults.MinimumLogLevel;
                options.IncludeTimestamp = defaults.IncludeTimestamp;
                options.LogFormat = defaults.LogFormat;
                options.MaxLength = defaults.MaxLength;
                options.MaxLogFiles = defaults.MaxLogFiles;
                options.Color = defaults.Color;

                options.LevelColors.Clear();
                foreach (var colorPair in defaults.LevelColors)
                {
                    options.LevelColors.Add(new LogLevelColor
                    {
                        Level = colorPair.Level,
                        Color = colorPair.Color
                    });
                }

                // Apply optional customizations last
                overrideOptions?.Invoke(options);
            });
        }
    }
}
