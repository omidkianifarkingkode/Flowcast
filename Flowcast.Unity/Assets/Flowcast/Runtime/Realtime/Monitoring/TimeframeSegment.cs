using TMPro;
using UnityEngine;

namespace Flowcast.Monitoring
{
    public class TimeframeSegment : MonoBehaviour
    {
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private TextMeshProUGUI _text;
        public ulong Turn { get; private set; }

        public void SetLabel(string label, ulong turn)
        {
            gameObject.SetActive(true);
            Turn = turn;
            _text.text = label;
        }

        public Transform GetIconAnchor()
        {
            return transform; // Or a child transform if you want precise placement
        }
    }

}
