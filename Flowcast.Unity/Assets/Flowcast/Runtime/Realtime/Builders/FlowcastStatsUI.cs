using Flowcast.Collections;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Flowcast.Builders
{
    public class FlowcastStatsUI : MonoBehaviour
    {
        [SerializeField] private Vector2 buttonPosition = new Vector2(15, 15);
        [SerializeField] private Vector2 buttonSize = new Vector2(15, 15);
        [SerializeField] private KeyCode toggleKey = KeyCode.F1;

        private bool _isVisible = true;

        private float _lastStatsUpdateTime = 0;
        private ulong _lastGameFrame = 0;
        private ulong _lastTurn = 0;

        private const int RollingSampleSize = 10;

        private const float SmoothingFactor = 0.1f;
        private float _smoothedFps = 0f;
        private float _smoothedTps = 0f;

        private ILockstepEngine _engine;

        public void SetEngine(ILockstepEngine engine)
        {
            _engine = engine;
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                _isVisible = !_isVisible;
            }

            if (_engine is not LockstepEngine flowcast) return;

            float now = Time.realtimeSinceStartup;
            float delta = now - _lastStatsUpdateTime;

            if (delta >= 1f)
            {
                var lockstep = flowcast.LockstepProvider;
                ulong currentFrame = lockstep.CurrentGameFrame;
                ulong currentTurn = lockstep.CurrentLockstepTurn;

                float fps = (currentFrame - _lastGameFrame) / delta;
                float tps = (currentTurn - _lastTurn) / delta;

                _smoothedFps = Mathf.Lerp(_smoothedFps, fps, SmoothingFactor);
                _smoothedTps = Mathf.Lerp(_smoothedTps, tps, SmoothingFactor);

                _lastGameFrame = currentFrame;
                _lastTurn = currentTurn;
                _lastStatsUpdateTime = now;
            }
        }


        private void OnGUI()
        {
            if (GUI.Button(new Rect(buttonPosition.x, buttonPosition.y, buttonSize.x, buttonSize.y), _isVisible ? "<" : ">"))
            {
                _isVisible = !_isVisible;
            }

            if (!_isVisible || _engine == null)
                return;

            if (_engine is not LockstepEngine flowcast) return;

            var lockstep = flowcast.LockstepProvider;
            var sync = flowcast.GameStateSyncService;
            var command = flowcast.CommandCollector;
            var player = flowcast.PlayerProvider;

            GUILayout.BeginArea(new Rect(10, 10, 300, 220), "Flowcast Stats", GUI.skin.window);
            GUILayout.Label($"Local Player: {player.GetLocalPlayerId()}");
            GUILayout.Label($"Frame: {lockstep.CurrentGameFrame}");
            GUILayout.Label($"Turn: {lockstep.CurrentLockstepTurn}");
            GUILayout.Label($"Sim Time (ms): {lockstep.SimulationTimeTicks / 1000}");
            GUILayout.Label($"FPS: {_smoothedFps:0.0}");
            GUILayout.Label($"TPS: {_smoothedTps:0.0}");
            GUILayout.Label($"Speed: {lockstep.SimulationSpeedMultiplier:0.00}");
            GUILayout.Label($"Pending Commands: {command.BufferedCommands.Count()}");

            //if (_fps < 50 || _tps < _settings.LockstepTurnRate - 1)
            //{
            //    GUILayout.Label("<color=red>⚠️ Simulation under target speed!</color>");
            //}

            GUILayout.EndArea();

            if (GUI.Button(new Rect(buttonPosition.x, buttonPosition.y, buttonSize.x, buttonSize.y), _isVisible ? "<" : ">"))
            {
                _isVisible = !_isVisible;
            }
        }
    }
}
