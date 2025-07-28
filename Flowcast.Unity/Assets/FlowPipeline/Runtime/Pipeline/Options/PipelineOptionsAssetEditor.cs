#if UNITY_EDITOR

using System.IO;
using UnityEditor;
using UnityEngine;

namespace FlowPipeline
{
    public partial class PipelineOptionsAsset
    {
        public const string DefaultFileName = FileName + ".asset";
        public const string AssetFolderPath = "Assets/Resources/Flowpipeline/";
        public const string AssetFullPath = AssetFolderPath + DefaultFileName;
        public const string CreateAssetMenuPath = "Window/Flowpipeline/Create Simulation Pipeline Options Asset";

        [MenuItem(CreateAssetMenuPath)]
        public static void CreateAssetIfNotExists()
        {
            if (!Directory.Exists(AssetFolderPath))
                Directory.CreateDirectory(AssetFolderPath);

            var existing = AssetDatabase.LoadAssetAtPath<PipelineOptionsAsset>(AssetFullPath);
            if (existing != null)
            {
                Debug.Log("SimulationPipelineOptions already exists at: " + AssetFullPath);
                Selection.activeObject = existing;
                return;
            }

            var asset = CreateInstance<PipelineOptionsAsset>();
            AssetDatabase.CreateAsset(asset, AssetFullPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("SimulationPipelineOptions created at: " + AssetFullPath);
            Selection.activeObject = asset;
        }
    }
}

#endif
