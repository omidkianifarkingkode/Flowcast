using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace FlowPipeline
{
    public partial class PipelineOptionsAsset : ScriptableObject, IPipelineOptions
    {
        public const string FileName = "SimulationPipelineOptions";
        public const string ResourceLoadPath = "Flowpipeline/" + FileName;

        [TypeDropdown(typeof(IFlowStep<,>))]
        [SerializeField] List<TypeReference> _steps;

        private static PipelineOptionsAsset _instance;

        public static PipelineOptionsAsset Load() 
        {
            _instance ??= Resources.Load<PipelineOptionsAsset>(ResourceLoadPath);

            if (_instance == null)
                throw new MissingReferenceException($"PipelineOptionsAsset could not be found at Resources/{ResourceLoadPath}");

            return _instance;
        }

        public IEnumerable<IFlowStep<TContext>> GetSteps<TContext>() where TContext : struct
        {
            var steps = new List<IFlowStep<TContext>>();

            foreach (var typeRef in _steps)
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
