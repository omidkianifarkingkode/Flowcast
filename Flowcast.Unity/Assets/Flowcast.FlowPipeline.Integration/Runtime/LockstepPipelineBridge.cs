using FlowPipeline;
using UnityEngine;

namespace Flowcast.FlowPipeline
{
    public class LockstepPipelineBridge : MonoBehaviour
    {
        [SerializeField] private LockstepInitializer lockstepInitializer;
        [SerializeField] private LockstepPipelineBuilder flowPipelineInitializer;

        private void Awake()
        {
            lockstepInitializer ??= FindAnyObjectByType<LockstepInitializer>();
            flowPipelineInitializer ??= FindAnyObjectByType<LockstepPipelineBuilder>();
        }

        private void OnEnable()
        {
            if (lockstepInitializer != null && flowPipelineInitializer != null)
            {
                lockstepInitializer.OnTick.AddListener(OnLockstepTick);
            }
        }

        private void OnDisable()
        {
            if (lockstepInitializer != null)
            {
                lockstepInitializer.OnTick.RemoveListener(OnLockstepTick);
            }
        }

        private void OnLockstepTick(TickWrapper bundle)
        {
            var context = new SimulationContext(bundle.Tick, bundle.DeltaTime);
            flowPipelineInitializer.Pipeline.ProcessFrame(context);
        }
    }
}
