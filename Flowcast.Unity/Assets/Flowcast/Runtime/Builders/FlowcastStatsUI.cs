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

        private readonly CircularBuffer<float> _fpsSamples = new(RollingSampleSize);
        private readonly CircularBuffer<float> _tpsSamples = new(RollingSampleSize);

        private float _fps = 0;
        private float _tps = 0;

        private float _averageFps = 0;
        private float _averageTps = 0;
        private int _sampleCount = 0;


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

            var now = Time.realtimeSinceStartup;
            var delta = now - _lastStatsUpdateTime;

            if (delta >= 1f) // one second sample
            {
                var lockstep = flowcast.LockstepProvider;
                ulong currentFrame = lockstep.CurrentGameFrame;
                ulong currentTurn = lockstep.CurrentLockstepTurn;

                _fps = (currentFrame - _lastGameFrame) / delta;
                _tps = (currentTurn - _lastTurn) / delta;

                _fpsSamples.Add(_fps);
                _tpsSamples.Add(_tps);

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
            var input = flowcast.InputCollector;
            var player = flowcast.PlayerProvider;

            GUILayout.BeginArea(new Rect(10, 10, 300, 220), "Flowcast Stats", GUI.skin.window);
            GUILayout.Label($"Local Player: {player.GetLocalPlayerId()}");
            GUILayout.Label($"Frame: {lockstep.CurrentGameFrame}");
            GUILayout.Label($"Turn: {lockstep.CurrentLockstepTurn}");
            GUILayout.Label($"Sim Time (ms): {lockstep.SimulationTimeTicks / 1000}");
            GUILayout.Label($"FPS: {_fps:0.0} (avg: {GetAverage(_fpsSamples):0.0})");
            GUILayout.Label($"TPS: {_tps:0.0} (avg: {GetAverage(_tpsSamples):0.0})");
            GUILayout.Label($"Speed: {lockstep.SimulationSpeedMultiplier:0.00}");
            GUILayout.Label($"Needs Rollback: {sync.NeedsRollback()}");
            GUILayout.Label($"Pending Inputs: {input.BufferedInputs.Count()}");

            //if (_fps < 50 || _tps < _settings.LockstepTurnRate - 1)
            //{
            //    GUILayout.Label("<color=red>⚠️ Simulation under target speed!</color>");
            //}

            GUILayout.EndArea();

            if (GUI.Button(new Rect(buttonPosition.x, buttonPosition.y, buttonSize.x, buttonSize.y), _isVisible ? "<" : ">"))
            {
                _isVisible = !_isVisible;
            }

            var graphRect = new Rect(10, 300, 300, 100);
            GUI.Box(graphRect, "TPS Over Time");

            // Draw background
            EditorGUI.DrawRect(graphRect, Color.black);

            // Draw line graph inside graphRect
            if (_tpsSamples.Count > 1)
            {
                Vector2 prevPoint = Vector2.zero;
                for (int i = 0; i < _tpsSamples.Count; i++)
                {
                    float x = graphRect.x + (i / (float)(RollingSampleSize - 1)) * graphRect.width;
                    float y = graphRect.yMax - (_tpsSamples.GetAt(i) / 50) * graphRect.height;
                    Vector2 currPoint = new Vector2(x, y);

                    if (i > 0)
                        Drawing.DrawLine(prevPoint, currPoint, Color.green, 2);

                    prevPoint = currPoint;
                }
            }
        }

        private float GetAverage(CircularBuffer<float> buffer)
        {
            float sum = 0f;
            foreach (var sample in buffer)
                sum += sample;

            return buffer.Count > 0 ? sum / buffer.Count : 0f;
        }
    }

public static class Drawing
    {
        private static Texture2D _lineTex;

        public static void DrawLine(Vector2 pointA, Vector2 pointB, Color color, float width = 1f)
        {
            if (_lineTex == null)
            {
                _lineTex = new Texture2D(1, 1);
                _lineTex.SetPixel(0, 0, Color.white);
                _lineTex.Apply();
            }

            Matrix4x4 matrix = GUI.matrix;

            float angle = Vector3.Angle(pointB - pointA, Vector2.right);
            if (pointA.y > pointB.y) angle = -angle;

            float length = Vector3.Distance(pointA, pointB);

            GUI.color = color;
            GUIUtility.RotateAroundPivot(angle, pointA);
            GUI.DrawTexture(new Rect(pointA.x, pointA.y, length, width), _lineTex);
            GUI.matrix = matrix;
            GUI.color = Color.white;
        }
    }

}
