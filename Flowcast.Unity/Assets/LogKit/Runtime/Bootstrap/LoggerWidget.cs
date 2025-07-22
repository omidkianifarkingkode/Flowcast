#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

namespace LogKit.Bootstrap
{
    public class LoggerWidget : MonoBehaviour 
    {

    }

    [CustomEditor(typeof(LoggerWidget))]
    public class EditorLoggerWidget : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Logger Tools", EditorStyles.boldLabel);

            if (GUILayout.Button("Open Latest Log File"))
            {
                MenuOpenLatestLogFile();
            }

            if (GUILayout.Button("Open Log Directory"))
            {
                MenuOpenLogDirectory();
            }
        }

        [MenuItem("Window/Logger/Open Log Directory")]
        public static void MenuOpenLogDirectory()
        {
            string dir = Path.Combine(Application.dataPath, "../Logs");
            if (Directory.Exists(dir))
                EditorUtility.RevealInFinder(dir);
        }

        [MenuItem("Window/Logger/Open Latest Log File")]
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

        [MenuItem("Window/Logger/Find or Create Logger Config")]
        public static void FindOrCreateLoggerConfig()
        {
            string assetPath = "Assets/Resources/LoggerConfig.asset";

            // Check if it already exists
            var config = AssetDatabase.LoadAssetAtPath<LoggerConfig>(assetPath);
            if (config != null)
            {
                Selection.activeObject = config;
                EditorGUIUtility.PingObject(config);
                return;
            }

            // Ensure the Resources folder exists
            if (!Directory.Exists("Assets/Resources"))
                Directory.CreateDirectory("Assets/Resources");

            // Create and save the asset
            var newConfig = ScriptableObject.CreateInstance<LoggerConfig>();
            AssetDatabase.CreateAsset(newConfig, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("Created new LoggerConfig at " + assetPath);
            Selection.activeObject = newConfig;
            EditorGUIUtility.PingObject(newConfig);
        }

    }
}
#endif