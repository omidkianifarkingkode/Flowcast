using System.Collections.Generic;
using UnityEngine;
using FixedMathSharp;
using System;

namespace FlowPipeline
{
    public class FlowPipelineBuilder : MonoBehaviour
    {
        [TypeDropdown(typeof(IFlowStep))]
        public List<TypeReference> _steps = new();

        public IFlowPipeline Pipeline { get; private set; }

        private void Awake()
        {
            var steps = new List<IFlowStep>();

            foreach (var typeRef in _steps)
            {
                var type = typeRef.Type;
                if (type == null || !typeof(IFlowStep).IsAssignableFrom(type))
                {
                    Debug.LogWarning($"Invalid or missing IFlowStep type: {typeRef.Type}");
                    continue;
                }

                if (Activator.CreateInstance(type) is not IFlowStep instance)
                {
                    Debug.LogError($"Could not instantiate {type.Name}");
                    continue;
                }

                steps.Add(instance);
            }

            Pipeline = new FlowPipeline(steps);
        }

        public void Execute(ulong tick, Fixed64 delta)
        {
            Pipeline?.ProcessFrame(tick, delta);
        }
    }
}
