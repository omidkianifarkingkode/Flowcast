using TMPro;
using UnityEngine;

public class TimelineSecondElement : MonoBehaviour
{
    public TextMeshProUGUI text;

    public void SetTime(float time) 
    {
        gameObject.name = "Sec " + (time + 1);
        text.SetText(time.ToString());
    }
}
