using UnityEngine;
using Flowcast.Inputs;
using Flowcast.Logging;
using Flowcast.Lockstep;
using Flowcast.Pipeline;
using Flowcast.Serialization;
using Flowcast.Synchronization;
using Flowcast.Network;
using Flowcast.Commons;
using Flowcast.Player;
using ILogger = Flowcast.Logging.ILogger;
using System.Reflection.Emit;
using System.Collections.Generic;
using System;

namespace Flowcast
{
    [DefaultExecutionOrder(-1000)]
    public class FlowcastBootstrap : MonoBehaviour
    {
        [Header("Simulation Units (Optional)")]
        public MonoBehaviour[] simulationUnits;

        public static FlowcastEngine Engine { get; private set; }

        private void Awake()
        {
            if (Engine != null)
            {
                Debug.LogWarning("FlowcastEngine already initialized.");
                return;
            }

            if (LockstepSettingsAsset.Instance == null)
            {
                Debug.LogError("FlowcastSettings is not assigned.");
                return;
            }

            // Dependencies
            ILogger logger = new UnityLogger();
            IInputValidatorFactory validatorFactory = new InputValidatorFactory(builder =>
            { 
                builder.AutoMap(); // Assumes validators are discoverable
            });

            PlayerProvider playerProvider = new PlayerProvider();

            IRemoteInputCollector remoteCollector = new RemoteInputCollector();

            IGameUpdatePipeline pipeline = SimulationPipelineBuilder.BuildDefault();

            var serializer = new GenericStateSerializer<ExampleGameState>(() => new ExampleGameState());
            IGameStateSerializer serializerWrapper = new GameStateSerializerWrapper<ExampleGameState>(serializer);

            var rollbackHandler = new RollbackHandler<ExampleGameState>(serializer, state =>
            {
                // Apply the deserialized state here
                Debug.Log("Rollback state applied."); 
            });

            IGameStateSyncService syncService = new GameStateSyncService(new XorHasher(), rollbackHandler);

            LockstepProviderBase lockstepProvider = new LockstepProviderUpdate(LockstepSettingsAsset.Instance , logger);

            IFrameProvider frameProvider = lockstepProvider;
            IIdGenerator idGenerator = new SequentialIdGenerator();

            ILocalInputCollector inputCollector = new LocalInputCollector(validatorFactory, playerProvider, frameProvider, idGenerator);

            // Create the engine
            Engine = new FlowcastEngine(
                inputCollector,
                remoteCollector,
                pipeline,
                syncService,
                lockstepProvider,
                playerProvider,
                logger,
                serializerWrapper
            );

            Debug.Log("[FlowcastBootstrap] Engine initialized.");
        }

        private void Update()
        {
            Engine.Tick();
        }
    }

    public class GameStateInstaller
    {
        public static (IGameStateSerializer, IRollbackHandler) CreateHandlers<T>(
            Func<T> stateFactory,
            Action<T> applyState
        )
        where T : class, ISerializableGameState
        {
            var serializer = new GenericStateSerializer<T>(stateFactory);
            var wrapper = new GameStateSerializerWrapper<T>(serializer);
            var rollbackHandler = new RollbackHandler<T>(serializer, applyState);

            return (wrapper, rollbackHandler);
        }
    }

}
