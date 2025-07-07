using UnityEngine;

namespace Flowcast.Monitoring
{
    public class LogMonitor : MonoBehaviour
    {
        [SerializeField] private Transform _parent;
        [SerializeField] private LogRecord _recordPrefab;

        private void Awake()
        {
            _recordPrefab.gameObject.SetActive(false);
        }

        public void AddLog(FlowcastLogEntry entry)
        {
            var record = Instantiate(_recordPrefab, _parent);
            record.SetData(entry);
        }
    }

}
