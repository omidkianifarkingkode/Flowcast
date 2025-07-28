using System;
using System.Collections.Generic;
using UnityEngine;

namespace FlowPipeline
{
    [Serializable]
    public class PipelineOptions : IPipelineOptions
    {
        public List<TypeReference> Steps { get; set; } = new();

        public IEnumerable<IFlowStep<TContext>> GetSteps<TContext>() where TContext : struct
        {
            var steps = new List<IFlowStep<TContext>>();

            foreach (var typeRef in Steps)
            {
                if (Activator.CreateInstance(typeRef.Type) is IFlowStep<TContext> step)
                {
                    steps.Add(step);
                }
                else
                {
                    Debug.LogWarning($"Could not create step of type: {typeRef?.Type}");
                }
            }

            return steps;
        }
    }


}
