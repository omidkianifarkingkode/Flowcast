#if UNITY_EDITOR

using Flowcast.Options;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Flowcast.Lockstep
{
    [CustomEditor(typeof(LockstepEngineOptionsAsset))]
    public class LockstepEngineOptionsAssetEditor : Editor
    {
        public const string DefaultFileName = LockstepEngineOptionsAsset.FileName + ".asset";
        public const string AssetFolderPath = "Assets/Resources/Flowcast/";
        public const string AssetFullPath = AssetFolderPath + DefaultFileName;
        public const string CreateAssetMenuPath = "Window/Flowcast/Create Lockstep Engine Settings Asset";

        private bool _lockstepFoldout = true;
        private bool _rollbackFoldout = true;
        private bool _snapshotFoldout = true;

        public override void OnInspectorGUI()
        {
            var asset = (LockstepEngineOptionsAsset)target;

            // Lockstep Settings
            _lockstepFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_lockstepFoldout, "Lockstep");
            if (_lockstepFoldout)
            {
                asset.GameFramesPerSecond = EditorGUILayout.IntField(new GUIContent("Game FPS", "Frames per second of the simulation."), asset.GameFramesPerSecond);
                asset.GameFramesPerLockstepTurn = EditorGUILayout.IntField(new GUIContent("Frames per Lockstep", "Game frames in one lockstep turn."), asset.GameFramesPerLockstepTurn);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            // Rollback Settings
            _rollbackFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_rollbackFoldout, "Rollback");
            if (_rollbackFoldout)
            {
                asset._minRecoverySpeed = EditorGUILayout.FloatField(new GUIContent("Min Recovery Speed", "Minimum simulation speed multiplier during catch-up."), asset._minRecoverySpeed);
                asset._maxRecoverySpeed = EditorGUILayout.FloatField(new GUIContent("Max Recovery Speed", "Maximum simulation speed multiplier during catch-up."), asset._maxRecoverySpeed);
                asset.FarRecoveryThreshold = EditorGUILayout.IntField(new GUIContent("Far Recovery Threshold", "Rollback distance threshold to enable fast catch-up."), asset.FarRecoveryThreshold);
                asset.EnableLocalAutoRollback = EditorGUILayout.Toggle(new GUIContent("Enable Local Auto-Rollback", "Allow client to rollback without server instruction."), asset.EnableLocalAutoRollback);
                asset.EnableRollbackLog = EditorGUILayout.Toggle(new GUIContent("Enable Rollback Logging", "Enable logging when rollback occurs."), asset.EnableRollbackLog);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            // Snapshot Settings
            _snapshotFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_snapshotFoldout, "Snapshot");
            if (_snapshotFoldout)
            {
                asset.SnapshotHistoryLimit = EditorGUILayout.IntField(new GUIContent("Snapshot History Limit", "Maximum snapshots to retain."), asset.SnapshotHistoryLimit);
                asset.DesyncToleranceFrames = EditorGUILayout.IntField(new GUIContent("Desync Tolerance Frames", "Frames to tolerate before triggering rollback."), asset.DesyncToleranceFrames);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorUtility.SetDirty(asset); // Mark dirty to persist changes
        }

        [MenuItem(CreateAssetMenuPath)]
        public static void CreateAssetIfNotExists()
        {
            if (!Directory.Exists(AssetFolderPath))
                Directory.CreateDirectory(AssetFolderPath);

            var existing = AssetDatabase.LoadAssetAtPath<LockstepEngineOptionsAsset>(AssetFullPath);
            if (existing != null)
            {
                Debug.Log("LockstepEngineOptions already exists at: " + AssetFullPath);
                Selection.activeObject = existing;
                return;
            }

            var asset = CreateInstance<LockstepEngineOptionsAsset>();
            AssetDatabase.CreateAsset(asset, AssetFullPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("LockstepEngineOptions created at: " + AssetFullPath);
            Selection.activeObject = asset;
        }
    }
}

#endif