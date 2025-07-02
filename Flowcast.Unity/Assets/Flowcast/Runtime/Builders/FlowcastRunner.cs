using UnityEngine;

namespace Flowcast.Builders
{
    [RequireComponent(typeof(FlowcastStatsUI))]
    public class FlowcastRunner : MonoBehaviour
    {
        private FlowcastEngine _engine;
        private FlowcastStatsUI _statusUI;

        private void OnEnable()
        {
            _statusUI = GetComponent<FlowcastStatsUI>();
        }

        public void SetEngine(FlowcastEngine engine)
        {
            _engine = engine;
            _engine.StartTicking();
            _statusUI.SetEngine(engine);
        }

        private void Update()
        {
            _engine.Tick();
        }
    }
}
