//using Flowcast.Rest.Bootstrap;
//using UnityEditor;
//using UnityEngine;

//namespace Flowcast.Core.Environments.Editor
//{
//    public sealed class EnvironmentSwitcherWindow : EditorWindow
//    {
//        private FlowcastRestSettings _settings;
//        private Vector2 _scroll;

//        [MenuItem("Flowcast/Environment Switcher")]
//        private static void Open()
//        {
//            GetWindow<EnvironmentSwitcherWindow>("Flowcast Environments");
//        }

//        private void OnEnable()
//        {
//            _settings = LoadSettingsAsset();
//            EnvironmentProvider.Instance.Changed += OnEnvironmentChanged;
//        }

//        private void OnDisable()
//        {
//            EnvironmentProvider.Instance.Changed -= OnEnvironmentChanged;
//        }

//        private void OnFocus()
//        {
//            _settings = LoadSettingsAsset();
//            Repaint();
//        }

//        private void OnEnvironmentChanged(Flowcast.Core.Environments.Environment _)
//        {
//            Repaint();
//        }

//        private void OnGUI()
//        {
//            if (_settings == null)
//            {
//                EditorGUILayout.HelpBox(
//                    "Create a FlowcastRestSettings asset and assign it to your FlowcastRestBootstrapper.",
//                    MessageType.Info);
//                if (GUILayout.Button("Create Settings Asset"))
//                {
//                    CreateSettingsAsset();
//                }
//                return;
//            }

//            var list = _settings.Environments;
//            if (list == null || list.Count == 0)
//            {
//                EditorGUILayout.HelpBox("Assign at least one Environment in FlowcastRestSettings.", MessageType.Warning);
//                return;
//            }

//            var current = EnvironmentProvider.Instance.Current;

//            EditorGUILayout.LabelField("Active Environment:", EditorStyles.boldLabel);
//            EditorGUILayout.Space(4);

//            _scroll = EditorGUILayout.BeginScrollView(_scroll);
//            foreach (var env in list)
//            {
//                if (env == null) continue;

//                EditorGUILayout.BeginHorizontal();
//                bool isActive = current == env;
//                GUI.enabled = !isActive;
//                if (GUILayout.Button(isActive ? $"● {env.DisplayName} ({env.Id})" : $"○ {env.DisplayName} ({env.Id})",
//                                     GUILayout.Height(24)))
//                {
//                    EnvironmentProvider.Instance.Set(env);
//                }
//                GUI.enabled = true;
//                EditorGUILayout.EndHorizontal();
//            }
//            EditorGUILayout.EndScrollView();

//            EditorGUILayout.Space();
//            EditorGUILayout.BeginHorizontal();
//            if (GUILayout.Button("Clear Persisted Selection"))
//            {
//                EnvironmentProvider.Instance.ClearPersistedSelection();
//            }
//            if (GUILayout.Button("Ping Base URL"))
//            {
//                if (current != null)
//                    Debug.Log($"[Flowcast] Base URL: {current.BaseUrl}");
//            }
//            EditorGUILayout.EndHorizontal();
//        }

//        private static FlowcastRestSettings LoadSettingsAsset()
//        {
//            var guids = AssetDatabase.FindAssets("t:Flowcast.Rest.Bootstrap.FlowcastRestSettings");
//            if (guids.Length == 0) return null;
//            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
//            return AssetDatabase.LoadAssetAtPath<FlowcastRestSettings>(path);
//        }

//        private static void CreateSettingsAsset()
//        {
//            var settings = ScriptableObject.CreateInstance<FlowcastRestSettings>();
//            const string dir = "Assets/Flowcast/Rest";
//            const string path = dir + "/FlowcastRestSettings.asset";
//            if (!AssetDatabase.IsValidFolder("Assets/Flowcast"))
//                AssetDatabase.CreateFolder("Assets", "Flowcast");
//            if (!AssetDatabase.IsValidFolder(dir))
//                AssetDatabase.CreateFolder("Assets/Flowcast", "Rest");
//            AssetDatabase.CreateAsset(settings, path);
//            AssetDatabase.SaveAssets();
//            Selection.activeObject = settings;
//            EditorGUIUtility.PingObject(settings);
//            Debug.Log($"[Flowcast] Created {path}. Assign your Environment assets there.");
//        }
//    }
//}
