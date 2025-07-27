using Flowcast.Data;
using Flowcast.Serialization;
using UnityEngine;

namespace Flowcast
{
    public class LockstepInitializer : MonoBehaviour
    {
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
                .ConfigureFromResources()
                .SetMatchInfo(matchInfo)
                .ConfigureCommandSystem(command => command
                    .OnCommandReceived(cmd => onCommandReceived?.Invoke(new CommandWrapper(cmd)))
                    .HandleCommandsOnGameFrame())
                .SynchronizeGameState(syncSetup => syncSetup
                    .UseBinarySerializer(gameState)
                    .OnStep((tick, deltaTime) => onTick.Invoke(new TickWrapper(tick, deltaTime)))
                    .OnRollback<T>((snapshot, frame) => onRollback.Invoke(new RollbackWrapper(frame, snapshot))))
                .SetupNetworkServices(networkSetup => networkSetup
                    .UseDummyServer())
                .BuildAndStart();

            return flowcast;
        }
    }
}
