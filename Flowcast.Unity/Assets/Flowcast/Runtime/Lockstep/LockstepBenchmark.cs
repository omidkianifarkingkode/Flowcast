using UnityEngine;
using System.Collections.Generic;
using System.Text;

namespace Flowcast.Lockstep
{
    public class LockstepBenchmark : MonoBehaviour
    {
        [SerializeField] private LockstepTestRunner[] runners;

        private float _logInterval = 1f;
        private float _elapsed = 0f;

        private void Awake()
        {
            runners = FindObjectsByType<LockstepTestRunner>(FindObjectsSortMode.None);
        }

        void Update()
        {
            _elapsed += Time.deltaTime;

            if (_elapsed >= _logInterval)
            {
                LogComparison();
                _elapsed = 0f;
            }
        }

        void LogComparison()
        {
            if (runners == null || runners.Length == 0) return;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("[Benchmark]");

            foreach (var runner in runners)
            {
                if (runner == null) continue;

                var lockstep = runner.Lockstep;

                sb.AppendLine($"Runner [{(runner.simulateHeavyFrame ? "Heavy" : "Normal")}] => GameFrames: {lockstep.CurrentGameFrame}, LockstepTurns: {lockstep.CurrentLockstepTurn}, Delay: {lockstep.GetDelay():F3}s");
            }

            Debug.Log(sb.ToString());
        }
    }
}
