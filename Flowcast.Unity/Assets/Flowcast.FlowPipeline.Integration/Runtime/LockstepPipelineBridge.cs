using FlowPipeline;
using UnityEngine;

namespace Flowcast.FlowPipeline
{
    public class LockstepPipelineBridge : MonoBehaviour
    {
        [SerializeField] private LockstepInitializer lockstepInitializer;
        [SerializeField] private FlowPipelineBuilder flowPipelineInitializer;

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
            flowPipelineInitializer.Execute(bundle.Tick, bundle.DeltaTime);
        }
    }
}
