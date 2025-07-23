using FixedMathSharp;
using Flowcast.Commands;
using Flowcast.Data;
using Flowcast.Serialization;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Flowcast
{
    [System.Serializable]
    public class CommandEvent : UnityEvent<CommandWrapper> { }

    [System.Serializable]
    public class TickEvent : UnityEvent<TickWrapper> { }

    [System.Serializable]
    public class RollbackEvent : UnityEvent<RollbackWrapper> { }

    [System.Serializable]
    public class CommandWrapper
    {
        public ICommand Command;

        public CommandWrapper(ICommand command)
        {
            Command = command;
        }
    }

    [System.Serializable]
    public class TickWrapper
    {
        public ulong Tick;
        public Fixed64 DeltaTime;

        public TickWrapper(ulong tick, Fixed64 deltaTime)
        {
            Tick = tick;
            DeltaTime = deltaTime;
        }
    }

    [System.Serializable]
    public class RollbackWrapper
    {
        public ulong Tick;
        public ISerializableGameState State;

        public RollbackWrapper(ulong tick, ISerializableGameState state)
        {
            Tick = tick;
            State = state;
        }
    }

    public class LockstepInitializer : MonoBehaviour
    {
        [Header("Match Setup")]
        [SerializeField] int localPlayerId = 1;
        [SerializeField] string localPlayerName = "Player1";
        [SerializeField] string remotePlayerName = "Player2";

        [Header("Lockstep Settings")]
        [SerializeField] int ticksPerSecond = 50;
        [SerializeField] float tickDuration = 0.02f;

        [Header("Dummy Network Settings")]
        [SerializeField] int baseLatencyMs = 100;
        [SerializeField] bool echoCommands = true;

        [Header("Callbacks")]
        [SerializeField] CommandEvent onCommandReceived;
        [SerializeField] TickEvent onTick;
        [SerializeField] RollbackEvent onRollback;

        public CommandEvent OnCommandReceived => onCommandReceived;
        public TickEvent OnTick => onTick;
        public RollbackEvent OnRollback => onRollback;

        public ILockstepEngine Initialize<T>(T gameState, MatchInfo matchInfo) where T : IBinarySerializableGameState, new()
        {
            var flowcast = FlowcastBuilder.CreateLockstep()
                .SetMatchInfo(matchInfo)
                .ConfigureCommandSystem(command => command
                    .OnCommandReceived(cmd => onCommandReceived?.Invoke(new CommandWrapper(cmd)))
                    .HandleCommandsOnGameFrame())
                .SynchronizeGameState(syncSetup => syncSetup
                    .LoadOptionsFromResources()
                    .UseBinarySerializer(gameState)
                    .OnRollback<T>((snapshot, frame) => onRollback?.Invoke(new RollbackWrapper(frame, snapshot))))
                .SetupNetworkServices(networkSetup => networkSetup
                    .UseDummyServer(new()
                    {
                        BaseLatencyMs = baseLatencyMs,
                        EchoCommands = echoCommands,
                    }))
                .ConfigureSimulationPipeline(pipelineSetup => pipelineSetup
                    .HandleStepManually((tick,deltaTime) =>
                    {
                        onTick?.Invoke(new TickWrapper(tick, deltaTime));
                    }))
                .BuildAndStart();

            return flowcast;
        }
    }


}
