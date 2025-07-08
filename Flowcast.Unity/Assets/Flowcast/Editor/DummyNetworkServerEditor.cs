using Flowcast.Network;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Flowcast.Editor
{
    [CustomEditor(typeof(DummyNetworkServer))]
    public class DummyNetworkServerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var dummy = (DummyNetworkServer)target;

            GUILayout.Space(10);
            GUILayout.Label("Runtime Tools", EditorStyles.boldLabel);

            if (Application.isPlaying)
            {
                if (GUILayout.Button("🔁 Send Ping"))
                    dummy.Editor_SendPing();

                if (GUILayout.Button("📥 Receive Rollback"))
                    dummy.Editor_RequestRollback();
            }
            else
            {
                EditorGUILayout.HelpBox("Enter Play Mode to test networking actions.", MessageType.Info);
            }
        }
    }
}