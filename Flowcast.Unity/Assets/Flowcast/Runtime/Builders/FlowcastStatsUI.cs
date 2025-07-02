using System.Linq;
using UnityEngine;

namespace Flowcast.Builders
{
    public class FlowcastStatsUI : MonoBehaviour
    {
        [SerializeField] private Vector2 buttonPosition = new Vector2(15, 15);
        [SerializeField] private Vector2 buttonSize = new Vector2(15, 15);
        [SerializeField] private KeyCode toggleKey = KeyCode.F1;

        private bool _isVisible = true;

        private IFlowcastEngine _engine;

        public void SetEngine(IFlowcastEngine engine)
        {
            _engine = engine;
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                _isVisible = !_isVisible;
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

            if (_engine is not FlowcastEngine flowcast) return;

            var lockstep = flowcast.LockstepProvider;
            var sync = flowcast.GameStateSyncService;
            var input = flowcast.InputCollector;
            var player = flowcast.PlayerProvider;

            GUILayout.BeginArea(new Rect(10, 10, 300, 220), "Flowcast Stats", GUI.skin.window);
            GUILayout.Label($"Local Player: {player.GetLocalPlayerId()}");
            GUILayout.Label($"Frame: {lockstep.CurrentGameFrame}");
            GUILayout.Label($"Turn: {lockstep.CurrentLockstepTurn}");
            GUILayout.Label($"Sim Time (ms): {lockstep.SimulationTimeTicks}");
            GUILayout.Label($"Speed: {lockstep.SimulationSpeedMultiplier:0.00}");
            GUILayout.Label($"Needs Rollback: {sync.NeedsRollback()}");
            GUILayout.Label($"Pending Inputs: {input.BufferedInputs.Count()}");

            GUILayout.EndArea();

            if (GUI.Button(new Rect(buttonPosition.x, buttonPosition.y, buttonSize.x, buttonSize.y), _isVisible ? "<" : ">"))
            {
                _isVisible = !_isVisible;
            }
        }
    }
}
