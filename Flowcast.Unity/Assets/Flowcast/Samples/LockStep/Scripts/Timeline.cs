using System;
using UnityEngine;
using UnityEngine.UI;

public class Timeline : MonoBehaviour
{
    public float duration = 60f; // Duration of the timer in seconds
    public Image baseLineImage;

    public bool autoStart;
    private float timer;
    private bool isRunning = false;

    public TimelineSecondElement secImage;
    public HorizontalLayoutGroup secondsGroup;

    private void Start()
    {
        CreatSeconds();

        if(autoStart)
            StartTimer();
    }

    private void CreatSeconds()
    {
        secImage.SetTime(0);

        for(int i=0; i < duration; i++)
        {
            var image = Instantiate(secImage, secondsGroup.transform);
            image.SetTime(i + 1);
        }
    }

    void Update()
    {
        if (!isRunning)
        {
            return;
        }

        timer += Time.deltaTime;
        float progress = Mathf.Clamp01(timer / duration);
        baseLineImage.fillAmount = progress;

        if (progress >= 1f)
        {
            isRunning = false;
            OnTimerComplete();
        }
    }

    public void StartTimer()
    {
        timer = 0f;
        isRunning = true;
        baseLineImage.type = Image.Type.Filled;
        baseLineImage.fillMethod = Image.FillMethod.Horizontal;
        baseLineImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        baseLineImage.fillAmount = 0f;
    }

    private void OnTimerComplete()
    {
        Debug.Log("Timer Complete!");
        // Add any additional functionality when timer completes
    }
}