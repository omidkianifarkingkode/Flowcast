using LogKit;
using System.Collections;
using UnityEngine;

namespace Flowcast.Lockstep
{
    public class LockstepTestRunner : MonoBehaviour
    {
        [SerializeField] public bool simulateHeavyFrame = false;
        [SerializeField] private bool alignRight = false;
        [SerializeField] private float lagIntensity = 0f;

        public LockstepProviderUpdate Lockstep;
        private int _gameFrames = 0;
        private int _lockstepTurns = 0;

        private float _elapsed = 0f;
        private float _logInterval = 1f;

        private float _totalDelay = 0f;
        private int _delaySamples = 0;

        private int _totalCatchUpEvents = 0;
        private int _maxCatchUpSeen = 0;

        private void Start()
        {
            var settings = LockstepSettingsAsset.Instance;
            Lockstep = new LockstepProviderUpdate(settings, LoggerFactory.Create("Core"));

            Lockstep.OnGameFrame += () => _gameFrames++;
            Lockstep.OnLockstepTurn += () => _lockstepTurns++;

            StartCoroutine(LockstepLoop());
        }

        private IEnumerator LockstepLoop()
        {
            while (true)
            {
                if (simulateHeavyFrame && lagIntensity > 0f && Time.frameCount % 60 == 0)
                {
                    float end = Time.realtimeSinceStartup + lagIntensity;
                    while (Time.realtimeSinceStartup < end)
                        yield return null;  // لگ به اندازه lagIntensity ثانیه
                }

                Lockstep.Tick();
                _elapsed += Time.deltaTime;

                float delay = Lockstep.GetDelay();
                _totalDelay += delay;
                _delaySamples++;

                yield return null;
            }
        }


        private void OnGUI()
        {
            if (Lockstep == null) return;

            int width = 250;
            int height = 160;
            int x = alignRight ? Screen.width - width - 10 : 10;
            int y = 10;

            GUILayout.BeginArea(new Rect(x, y, width, height), GUI.skin.box);
            GUILayout.Label($"Game Frame: {Lockstep.CurrentGameFrame}");
            GUILayout.Label($"Lockstep Turn: {Lockstep.CurrentLockstepTurn}");
            GUILayout.Label($"Delay: {Lockstep.GetDelay():F3}s");
            GUILayout.Label($"Profile: {(simulateHeavyFrame ? "HeavyFrame" : "Normal")}");
            GUILayout.Space(10);
            GUILayout.Label($"Avg Delay: {_totalDelay / Mathf.Max(1, _delaySamples):F3}s");
            GUILayout.Label($"Catch-Up Events: {_totalCatchUpEvents}");
            GUILayout.Label($"Max Catch-Up Seen: {_maxCatchUpSeen}");
            GUILayout.EndArea();
        }
    }
}
