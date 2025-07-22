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

        public TickWrapper(ulong tick)
        {
            Tick = tick;
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
        public int localPlayerId = 1;
        public string localPlayerName = "Player1";
        public string remotePlayerName = "Player2";

        [Header("Lockstep Settings")]
        public int ticksPerSecond = 50;
        public float tickDuration = 0.02f;

        [Header("Dummy Network Settings")]
        public int baseLatencyMs = 100;
        public bool echoCommands = true;

        [Header("Callbacks")]
        public CommandEvent onCommandReceived;
        public TickEvent onTick;
        public RollbackEvent onRollback;

        public ILockstepEngine Initialize<T>(T gameState, MatchInfo matchInfo) where T : ISerializableGameState, new()
        {
            var flowcast = FlowcastBuilder.CreateLockstep()
                .SetMatchInfo(matchInfo)
                .ConfigureCommandSystem(command => command
                    .OnCommandReceived(cmd => onCommandReceived?.Invoke(new CommandWrapper(cmd)))
                    .HandleCommandsOnGameFrame())
                .SynchronizeGameState(syncSetup => syncSetup
                    .UseDefaultOptions()
                    .UseJsonSerializer(gameState)
                    .OnRollback<T>((snapshot, frame) => onRollback?.Invoke(new RollbackWrapper(frame, snapshot))))
                .SetupNetworkServices(networkSetup => networkSetup
                    .UseDummyServer(new()
                    {
                        BaseLatencyMs = baseLatencyMs,
                        EchoCommands = echoCommands,
                    }))
                .ConfigureSimulationPipeline(pipelineSetup => pipelineSetup
                    .HandleStepManually(tick =>
                    {
                        onTick?.Invoke(new TickWrapper(tick));
                    }))
                .BuildAndStart();

            return flowcast;
        }

        public ILockstepEngine InitializeAsBinary<T>(T gameState, MatchInfo matchInfo) where T : IBinarySerializableGameState, new()
        {
            var flowcast = FlowcastBuilder.CreateLockstep()
                .SetMatchInfo(matchInfo)
                .ConfigureCommandSystem(command => command
                    .OnCommandReceived(cmd => onCommandReceived?.Invoke(new CommandWrapper(cmd)))
                    .HandleCommandsOnGameFrame())
                .SynchronizeGameState(syncSetup => syncSetup
                    .UseDefaultOptions()
                    .UseBinarySerializer(gameState)
                    .OnRollback<T>((snapshot, frame) => onRollback?.Invoke(new RollbackWrapper(frame, snapshot))))
                .SetupNetworkServices(networkSetup => networkSetup
                    .UseDummyServer(new()
                    {
                        BaseLatencyMs = baseLatencyMs,
                        EchoCommands = echoCommands,
                    }))
                .ConfigureSimulationPipeline(pipelineSetup => pipelineSetup
                    .HandleStepManually(tick =>
                    {
                        onTick?.Invoke(new TickWrapper(tick));
                    }))
                .BuildAndStart();

            return flowcast;
        }
    }


}
