using UnityEditor;
using UnityEngine;

namespace Flowcast.Core.Environments.Editor
{
    public sealed class EnvironmentSwitcherWindow : EditorWindow
    {
        private FlowcastClientSettings _settings;
        private Vector2 _scroll;

        [MenuItem("Flowcast/Environment Switcher")]
        private static void Open()
        {
            GetWindow<EnvironmentSwitcherWindow>("Flowcast Environments");
        }

        private void OnEnable()
        {
            _settings = FlowcastClientSettings.LoadFromResources();
            EnvironmentProvider.Instance.Changed += OnEnvironmentChanged;
        }

        private void OnDisable()
        {
            EnvironmentProvider.Instance.Changed -= OnEnvironmentChanged;
        }

        private void OnFocus()
        {
            _settings = FlowcastClientSettings.LoadFromResources();
            Repaint();
        }

        private void OnEnvironmentChanged(Flowcast.Core.Environments.Environment _)
        {
            Repaint();
        }

        private void OnGUI()
        {
            if (_settings == null)
            {
                EditorGUILayout.HelpBox(
                    "Create a FlowcastClientSettings asset and place it at Resources/Flowcast/ClientSettings.asset",
                    MessageType.Info);
                if (GUILayout.Button("Create Settings Asset"))
                {
                    CreateSettingsAsset();
                }
                return;
            }

            var set = _settings.EnvironmentSet;
            if (set == null || set.Environments.Count == 0)
            {
                EditorGUILayout.HelpBox("Assign an EnvironmentSet with at least one Environment.", MessageType.Warning);
                return;
            }

            var current = EnvironmentProvider.Instance.Current;

            EditorGUILayout.LabelField("Active Environment:", EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            foreach (var env in set.Environments)
            {
                if (env == null) continue;

                EditorGUILayout.BeginHorizontal();
                bool isActive = current == env;
                GUI.enabled = !isActive;
                if (GUILayout.Button(isActive ? $"● {env.DisplayName} ({env.Id})" : $"○ {env.DisplayName} ({env.Id})",
                                     GUILayout.Height(24)))
                {
                    EnvironmentProvider.Instance.Set(env);
                }
                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Clear Persisted Selection"))
            {
                EnvironmentProvider.Instance.ClearPersistedSelection();
            }
            if (GUILayout.Button("Ping Base URL"))
            {
                if (current != null)
                    Debug.Log($"[Flowcast] Base URL: {current.BaseUrl}");
            }
            EditorGUILayout.EndHorizontal();
        }

        private static void CreateSettingsAsset()
        {
            var settings = ScriptableObject.CreateInstance<FlowcastClientSettings>();
            const string dir = "Assets/Resources/Flowcast";
            const string path = dir + "/ClientSettings.asset";
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");
            if (!AssetDatabase.IsValidFolder(dir))
                AssetDatabase.CreateFolder("Assets/Resources", "Flowcast");
            AssetDatabase.CreateAsset(settings, path);
            AssetDatabase.SaveAssets();
            Selection.activeObject = settings;
            EditorGUIUtility.PingObject(settings);
            Debug.Log($"[Flowcast] Created {path}. Assign your EnvironmentSet there.");
        }
    }
}
