using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Flowcast.Monitoring
{
    public class TimeFrameMonitor : MonoBehaviour
    {
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private Transform _parent;
        [SerializeField] private TimeframeSegment _segmentPrefab;
        [SerializeField] private Image _iconSample;

        private List<TimeframeSegment> _segments = new();

        private void Awake()
        {
            _segmentPrefab.gameObject.SetActive(false);
            _iconSample.gameObject.SetActive(false);
        }

        public void AddSegment(ulong turn, string label)
        {
            var segment = Instantiate(_segmentPrefab, _parent);
            segment.SetLabel(label, turn);
            _segments.Add(segment);

            Canvas.ForceUpdateCanvases();
            _scrollRect.horizontalNormalizedPosition = 1f;
        }

        public void AddLog(FlowcastLogEntry log)
        {
            var segment = FindSegmentForTurn(log.Turn);
            if (segment == null)
            {
                Debug.LogWarning($"No segment found for turn {log.Turn}");
                return;
            }

            var iconLog = Instantiate(_iconSample, segment.GetIconAnchor());
            iconLog.gameObject.SetActive(true);

            switch (log.Type)
            {
                case LogType.CommandSent:
                    iconLog.color = Color.cyan;
                    break;
                case LogType.CommandRecieved:
                    iconLog.color = Color.green;
                    break;
                case LogType.Rollback:
                    iconLog.color = Color.red;
                    break;
            }
        }


        private TimeframeSegment FindSegmentForTurn(ulong turn)
        {
            foreach (var s in _segments)
            {
                if (turn >= s.Turn && turn < s.Turn + Monitor.Instace.SegmentInterval)
                    return s;
            }

            return null;
        }


    }

}
