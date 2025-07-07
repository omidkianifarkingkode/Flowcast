using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Flowcast.Monitoring
{
    public class LogDetails : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private Button _closeButton;
        [SerializeField] private TextMeshProUGUI _title;
        [SerializeField] private TextMeshProUGUI _text;

        private void Awake()
        {
            _closeButton.onClick.AddListener(() => gameObject.SetActive(false));
        }

        public void Show(FlowcastLogEntry entry)
        {
            gameObject.SetActive(true);

            if (entry is InputLogEntry inputLogEntry)
            {
                if (entry.Type == LogType.InputSent)
                {
                    _icon.color = Color.cyan;
                    _title.SetText($"Input sent: Frame={inputLogEntry.Turn}, Id={inputLogEntry.Input.Id}");
                    _text.SetText(inputLogEntry.Input.ToString());
                }
                else if (entry.Type == LogType.InputRecieved)
                {
                    _icon.color = Color.green;
                    _title.SetText($"Input recieved: Frame={inputLogEntry.Turn}, Id={inputLogEntry.Input.Id}, PlayerId={inputLogEntry.Input.PlayerId}");
                    _text.SetText(inputLogEntry.Input.ToString());
                }
            }
            else if (entry is RollbackLogEntry rollbackLogEntry)
            {
                _icon.color = Color.red;
                _title.SetText($"Rollback requested: Turn={rollbackLogEntry.Turn}, ToTurn={rollbackLogEntry.ToTurn}");
            }
            else if (entry is SnapshotLogEntry snapshotLogEntry)
            {
                _icon.color = Color.gray;
                _title.SetText($"Snapshot: Turn={snapshotLogEntry.Turn}, Hash={snapshotLogEntry.Hash}");
                if (Monitor.Instace.Flowcast.GameStateSyncService.TryGetSnapshot(snapshotLogEntry.Turn, out var rawSnapshot)) 
                {
                    var snapshot = Monitor.Instace.Flowcast.GameStateSerializer.DeserializeSnapshot(rawSnapshot.Data);
                    _text.SetText(Newtonsoft.Json.JsonConvert.SerializeObject(snapshot));
                }
                else
                    _text.SetText("Not Accessable");

                
            }
        }
    }

}
