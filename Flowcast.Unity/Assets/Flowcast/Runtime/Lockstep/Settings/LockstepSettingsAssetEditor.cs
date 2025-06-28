#if UNITY_EDITOR

using System.IO;
using UnityEditor;
using UnityEngine;

namespace Flowcast.Lockstep
{
    public partial class LockstepSettingsAsset 
    {
        public const string DefaultFileName = "LockstepSettings.asset";
        public const string AssetFolderPath = "Assets/Resources/Flowcast/";
        public const string AssetFullPath = AssetFolderPath + DefaultFileName;
        public const string CreateAssetMenuPath = "Flowcast/Create Lockstep Settings Asset";

        [MenuItem(CreateAssetMenuPath)]
        public static void CreateAssetIfNotExists()
        {
            if (!Directory.Exists(AssetFolderPath))
                Directory.CreateDirectory(AssetFolderPath);

            var existing = AssetDatabase.LoadAssetAtPath<LockstepSettingsAsset>(AssetFullPath);
            if (existing != null)
            {
                Debug.Log("LockstepSettingsAsset already exists at: " + AssetFullPath);
                Selection.activeObject = existing;
                return;
            }

            var asset = CreateInstance<LockstepSettingsAsset>();
            AssetDatabase.CreateAsset(asset, AssetFullPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("LockstepSettingsAsset created at: " + AssetFullPath);
            Selection.activeObject = asset;
        }
    }
}

#endif