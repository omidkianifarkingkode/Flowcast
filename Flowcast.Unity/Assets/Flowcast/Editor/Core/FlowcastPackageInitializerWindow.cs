using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Flowcast.Core.Environments;
using Flowcast.Rest.Bootstrap;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Flowcast.Editor.Core
{
    /// <summary>
    /// Utility window to help initialize Flowcast package dependencies and assets.
    /// </summary>
    public sealed class FlowcastPackageInitializerWindow : EditorWindow
    {
        private const string FlowcastUnitaskDefine = "FLOWCAST_UNITASK";
        private const string FlowcastNewtonsoftDefine = "FLOWCAST_NEWTONSOFT_JSON";
        private const string SettingsFolderPath = "Assets/Content/Flowcast";
        private const string SettingsAssetPath = SettingsFolderPath + "/FlowcastRestSettings.asset";
        private const string DevEnvironmentPath = SettingsFolderPath + "/Environment.Dev.asset";
        private const string ReleaseEnvironmentPath = SettingsFolderPath + "/Environment.Release.asset";

        [MenuItem("Flowcast/Package Setup", priority = 10)]
        public static void ShowWindow()
        {
            var window = GetWindow<FlowcastPackageInitializerWindow>(true, "Flowcast Package Setup");
            window.minSize = new Vector2(360f, 220f);
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Dependencies", EditorStyles.boldLabel);
            if (GUILayout.Button("Resolve Define Symbols"))
            {
                ResolveDefineSymbols();
            }

            EditorGUILayout.Space(12f);

            EditorGUILayout.LabelField("REST Settings", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Creates or locates FlowcastRestSettings along with default Dev and Release environments in Assets/Content/Flowcast.",
                MessageType.Info);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Initialize"))
                {
                    InitializeRestSettings();
                }

                if (GUILayout.Button("Locate"))
                {
                    LocateRestSettings();
                }
            }

            EditorGUILayout.Space(12f);

            EditorGUILayout.LabelField("Scene", EditorStyles.boldLabel);
            if (GUILayout.Button("Create Flowcast Bootstrapper GameObject"))
            {
                CreateBootstrapperGameObject();
            }
        }

        private static void ResolveDefineSymbols()
        {
            bool hasUniTask = HasDependency("com.cysharp.unitask", "UniTask");
            bool hasNewtonsoft = HasDependency("com.unity.nuget.newtonsoft-json", "Newtonsoft.Json", "Newtonsoft");

            SetDefineForAllTargets(FlowcastUnitaskDefine, hasUniTask);
            SetDefineForAllTargets(FlowcastNewtonsoftDefine, hasNewtonsoft);

            Debug.Log($"[Flowcast] Define symbols updated. {FlowcastUnitaskDefine}={(hasUniTask ? "ON" : "OFF")}, {FlowcastNewtonsoftDefine}={(hasNewtonsoft ? "ON" : "OFF")}");
        }

        private static bool HasDependency(string manifestKey, params string[] assetSearchTerms)
        {
            if (ManifestContains(manifestKey))
            {
                return true;
            }

            foreach (var term in assetSearchTerms)
            {
                if (HasAsset(term))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ManifestContains(string manifestKey)
        {
            if (string.IsNullOrEmpty(manifestKey))
                return false;

            var root = Path.GetDirectoryName(Application.dataPath);
            if (string.IsNullOrEmpty(root))
                return false;

            var manifestPath = Path.Combine(root, "Packages", "manifest.json");
            if (!File.Exists(manifestPath))
                return false;

            try
            {
                var text = File.ReadAllText(manifestPath);
                return text.IndexOf(manifestKey, StringComparison.OrdinalIgnoreCase) >= 0;
            }
            catch (IOException)
            {
                return false;
            }
        }

        private static bool HasAsset(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return false;

            var guids = AssetDatabase.FindAssets(searchTerm);
            return guids.Any(guid =>
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var name = Path.GetFileNameWithoutExtension(path);
                return name != null && name.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0;
            });
        }

        private static void SetDefineForAllTargets(string define, bool enabled)
        {
            if (string.IsNullOrEmpty(define))
                return;

            var targetGroups = GetRelevantBuildTargetGroups();
            foreach (var group in targetGroups)
            {
                var symbolList = new List<string>(PlayerSettings
                    .GetScriptingDefineSymbolsForGroup(group)
                    .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries));

                bool changed;
                if (enabled)
                {
                    if (!symbolList.Contains(define))
                    {
                        symbolList.Add(define);
                        changed = true;
                    }
                    else
                    {
                        changed = false;
                    }
                }
                else
                {
                    changed = symbolList.Remove(define);
                }

                if (changed)
                {
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(group, string.Join(";", symbolList));
                }
            }
        }

        private static IEnumerable<BuildTargetGroup> GetRelevantBuildTargetGroups()
        {
            var groups = new HashSet<BuildTargetGroup>
            {
                EditorUserBuildSettings.selectedBuildTargetGroup,
                BuildTargetGroup.Standalone
            };

            groups.Remove(BuildTargetGroup.Unknown);
            return groups;
        }

        private static void InitializeRestSettings()
        {
            EnsureFolderExists(SettingsFolderPath);

            var devEnv = EnsureEnvironmentAsset(DevEnvironmentPath, "dev", "Development", "https://api.dev.example.com");
            var releaseEnv = EnsureEnvironmentAsset(ReleaseEnvironmentPath, "release", "Release", "https://api.example.com");

            var settings = AssetDatabase.LoadAssetAtPath<FlowcastRestSettings>(SettingsAssetPath);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<FlowcastRestSettings>();
                AssetDatabase.CreateAsset(settings, SettingsAssetPath);
            }

            var serializedSettings = new SerializedObject(settings);
            var environmentsProp = serializedSettings.FindProperty("environments");
            if (environmentsProp != null)
            {
                environmentsProp.arraySize = 0;
                environmentsProp.InsertArrayElementAtIndex(0);
                environmentsProp.GetArrayElementAtIndex(0).objectReferenceValue = devEnv;
                environmentsProp.InsertArrayElementAtIndex(1);
                environmentsProp.GetArrayElementAtIndex(1).objectReferenceValue = releaseEnv;
            }

            var defaultEnvProp = serializedSettings.FindProperty("defaultEnvironment");
            if (defaultEnvProp != null)
            {
                defaultEnvProp.objectReferenceValue = devEnv;
            }

            var preferredEnvProp = serializedSettings.FindProperty("preferredEnvironment");
            if (preferredEnvProp != null)
            {
                preferredEnvProp.objectReferenceValue = devEnv;
            }

            serializedSettings.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(settings);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = settings;
            EditorGUIUtility.PingObject(settings);

            Debug.Log("[Flowcast] FlowcastRestSettings initialized under Assets/Content/Flowcast.");
        }

        private static Environment EnsureEnvironmentAsset(string assetPath, string id, string displayName, string baseUrl)
        {
            var environment = AssetDatabase.LoadAssetAtPath<Environment>(assetPath);
            if (environment == null)
            {
                environment = ScriptableObject.CreateInstance<Environment>();
                environment.Id = id;
                environment.DisplayName = displayName;
                environment.BaseUrl = baseUrl;
                AssetDatabase.CreateAsset(environment, assetPath);
            }
            else
            {
                environment.Id = id;
                environment.DisplayName = displayName;
                environment.BaseUrl = baseUrl;
            }

            EditorUtility.SetDirty(environment);
            return environment;
        }

        private static void LocateRestSettings()
        {
            var settings = FindRestSettings();
            if (settings == null)
            {
                Debug.LogWarning("[Flowcast] No FlowcastRestSettings asset found. Use Initialize to create one.");
                return;
            }

            Selection.activeObject = settings;
            EditorGUIUtility.PingObject(settings);
        }

        private static FlowcastRestSettings FindRestSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<FlowcastRestSettings>(SettingsAssetPath);
            if (settings != null)
            {
                return settings;
            }

            var guids = AssetDatabase.FindAssets("t:Flowcast.Rest.Bootstrap.FlowcastRestSettings");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<FlowcastRestSettings>(path);
                if (asset != null)
                {
                    return asset;
                }
            }

            return null;
        }

        private static void CreateBootstrapperGameObject()
        {
            var settings = FindRestSettings();
            if (settings == null)
            {
                Debug.LogWarning("[Flowcast] Cannot create bootstrapper - FlowcastRestSettings not found. Initialize settings first.");
                return;
            }

            var existing = GameObject.Find("Flowcast");
            if (existing == null)
            {
                existing = new GameObject("Flowcast");
                Undo.RegisterCreatedObjectUndo(existing, "Create Flowcast GameObject");
            }

            var bootstrapper = existing.GetComponent<FlowcastRestBootstrapper>();
            if (bootstrapper == null)
            {
                bootstrapper = Undo.AddComponent<FlowcastRestBootstrapper>(existing);
            }

            var so = new SerializedObject(bootstrapper);
            var settingsProp = so.FindProperty("settings");
            if (settingsProp != null)
            {
                settingsProp.objectReferenceValue = settings;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            EditorSceneManager.MarkSceneDirty(existing.scene);
            Selection.activeGameObject = existing;
            EditorGUIUtility.PingObject(existing);

            Debug.Log("[Flowcast] Flowcast GameObject with FlowcastRestBootstrapper ready in the current scene.");
        }

        private static void EnsureFolderExists(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            var segments = folderPath.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
            var current = "";
            for (int i = 0; i < segments.Length; i++)
            {
                current = i == 0 ? segments[i] : current + "/" + segments[i];
                if (!AssetDatabase.IsValidFolder(current))
                {
                    var parent = Path.GetDirectoryName(current);
                    if (string.IsNullOrEmpty(parent))
                    {
                        continue;
                    }

                    var folderName = Path.GetFileName(current);
                    if (!string.IsNullOrEmpty(folderName))
                    {
                        AssetDatabase.CreateFolder(parent, folderName);
                    }
                }
            }
        }
    }
}
