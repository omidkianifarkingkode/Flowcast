#if UNITY_EDITOR

using System.IO;
using UnityEditor;
using UnityEngine;

namespace LogKit
{
    [CreateAssetMenu(fileName = FileName, menuName = CreateAssetMenuPath)]
    public partial class LoggerOptionsAsset 
    {
        public const string DefaultFileName = FileName + ".asset";
        public const string AssetFolderPath = "Assets/Resources/Logkit/";
        public const string AssetFullPath = AssetFolderPath + DefaultFileName;
        public const string CreateAssetMenuPath = "Logkit/Create Logger Config";

        [MenuItem("Window/"+CreateAssetMenuPath)]
        public static void CreateAssetIfNotExists()
        {
            if (!Directory.Exists(AssetFolderPath))
                Directory.CreateDirectory(AssetFolderPath);

            var existing = AssetDatabase.LoadAssetAtPath<LoggerOptionsAsset>(AssetFullPath);
            if (existing != null)
            {
                Debug.Log("LoggerOptionsAsset already exists at: " + AssetFullPath);
                Selection.activeObject = existing;
                return;
            }

            var asset = CreateInstance<LoggerOptionsAsset>();
            AssetDatabase.CreateAsset(asset, AssetFullPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("LoggerOptionsAsset created at: " + AssetFullPath);
            Selection.activeObject = asset;
        }

        [MenuItem("Window/Logkit/Open Log Directory")]
        public static void MenuOpenLogDirectory()
        {
            string dir = Path.Combine(Application.dataPath, "../Logs");
            if (Directory.Exists(dir))
                EditorUtility.RevealInFinder(dir);
        }

        [MenuItem("Window/Logkit/Open Latest Log File")]
        public static void MenuOpenLatestLogFile()
        {
            string[] files = Directory.GetFiles("Logs", "log_*.log");
            if (files.Length > 0)
            {
                string latest = files[^1];
                if (File.Exists(latest))
                    EditorUtility.OpenWithDefaultApp(latest);
            }
        }
    }
}

#endif