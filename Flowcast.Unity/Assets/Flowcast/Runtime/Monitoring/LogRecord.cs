using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Flowcast.Monitoring
{
    public class LogRecord : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private Button _button;
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] LogDetails _detailsPanel;

        private FlowcastLogEntry _entry;

        public void SetData(FlowcastLogEntry entry)
        {
            gameObject.SetActive(true);
            _entry = entry;

            if (entry is InputLogEntry inputLogEntry) 
            {
                if (entry.Type == LogType.InputSent)
                {
                    _icon.color = Color.cyan;
                    _text.SetText($"Input sent: Frame={inputLogEntry.Turn}, Id={inputLogEntry.Input.Id}");
                }else if (entry.Type == LogType.InputRecieved)
                {
                    _icon.color = Color.green;
                    _text.SetText($"Input recieved: Frame={inputLogEntry.Turn}, Id={inputLogEntry.Input.Id}, PlayerId={inputLogEntry.Input.PlayerId}");
                }
            }
            else if (entry is RollbackLogEntry rollbackLogEntry) 
            {
                _icon.color = Color.red;
                _text.SetText($"Rollback requested: Turn={rollbackLogEntry.Turn}, ToTurn={rollbackLogEntry.ToTurn}");
            }
            else if (entry is SnapshotLogEntry snapshotLogEntry) 
            {
                _icon.color = Color.gray;
                _text.SetText($"Snapshot: Turn={snapshotLogEntry.Turn}, Hash={snapshotLogEntry.Hash}");
            }

            _button.onClick.AddListener(ShowDetails);
        }

        private void ShowDetails()
        {
            _detailsPanel.Show(_entry);
        }
    }

}
