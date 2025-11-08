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

            if (entry is CommandLogEntry commandLogEntry) 
            {
                if (entry.Type == LogType.CommandSent)
                {
                    _icon.color = Color.cyan;
                    _text.SetText($"Command sent: Frame={commandLogEntry.Turn}, Id={commandLogEntry.Command.Id}");
                }else if (entry.Type == LogType.CommandRecieved)
                {
                    _icon.color = Color.green;
                    _text.SetText($"Command recieved: Frame={commandLogEntry.Turn}, Id={commandLogEntry.Command.Id}");
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
