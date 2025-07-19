using Flowcast.Network;
using UnityEditor;
using UnityEngine;

namespace Flowcast.Editor
{
    [CustomEditor(typeof(DummerServerRunner))]
    public class DummyNetworkServerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var dummy = (DummerServerRunner)target;

            GUILayout.Space(10);
            GUILayout.Label("Runtime Tools", EditorStyles.boldLabel);

            if (Application.isPlaying)
            {
                if (GUILayout.Button("🔁 Send Ping"))
                    dummy.Server.Editor_SendPing();

                if (GUILayout.Button("📥 Receive Rollback"))
                    dummy.Server.Editor_RequestRollback();
            }
            else
            {
                EditorGUILayout.HelpBox("Enter Play Mode to test networking actions.", MessageType.Info);
            }
        }
    }
}