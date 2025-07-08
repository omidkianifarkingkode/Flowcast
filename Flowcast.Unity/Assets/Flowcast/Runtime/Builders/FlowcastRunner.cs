using Flowcast.Monitoring;
using UnityEngine;

namespace Flowcast.Builders
{
    [RequireComponent(typeof(FlowcastStatsUI))]
    public class FlowcastRunner : MonoBehaviour
    {
        private LockstepEngine _engine;
        private FlowcastStatsUI _statusUI;
        [SerializeField] Monitor _monitor;

        private void OnEnable()
        {
            _statusUI = GetComponent<FlowcastStatsUI>();
            _monitor = FindAnyObjectByType<Monitor>();
        }

        public void SetEngine(LockstepEngine engine)
        {
            _engine = engine;
            _engine.StartTicking();
            _statusUI.SetEngine(engine);
            _monitor.MonitorFlowcast(engine);
        }

        private void Update()
        {
            _engine.Tick();
        }
    }
}
