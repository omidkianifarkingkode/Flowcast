using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class MathUtils
{
    public static float EMA(float previous, float newValue, float alpha)
    {
        return alpha * newValue + (1f - alpha) * previous;
    }
}

public class Lockstepline : MonoBehaviour
{
    public float duration = 60f; // Duration of the timer in seconds
    public Image baseLineImage;
    public TextMeshProUGUI tickText;

    private float maxTick;

    private float averageDeltaTime;
    private ulong tickCount;
    private float lastTickTime;
    public float alpha = 0.1f;
    public float fpsTolerance = 1.1f; // Acceptable change threshold
    private int lastDisplayedFps = -1;

    public void Initialize(int fps)
    {
        maxTick = fps * duration;
        baseLineImage.type = Image.Type.Filled;
        baseLineImage.fillMethod = Image.FillMethod.Horizontal;
        baseLineImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        baseLineImage.fillAmount = 0f;

        averageDeltaTime = 0f;
        tickCount = 0;
        lastTickTime = Time.realtimeSinceStartup;
    }

    public void Tick(ulong tick)
    {
        tickCount++;

        float currentTime = Time.realtimeSinceStartup;
        float deltaTime = currentTime - lastTickTime;
        lastTickTime = currentTime;

        if (tickCount == 1)
        {
            averageDeltaTime = deltaTime; // Initialize on first tick
        }
        else
        {
            averageDeltaTime = MathUtils.EMA(averageDeltaTime, deltaTime, alpha);
        }

        float averageFps = averageDeltaTime > 0 ? 1f / averageDeltaTime : 0f;
        int roundedFps = Mathf.RoundToInt(averageFps);

        float progress = Mathf.Clamp01(tick / maxTick);
        baseLineImage.fillAmount = progress;

        if (Mathf.Abs(roundedFps - lastDisplayedFps) >= fpsTolerance || lastDisplayedFps == -1)
        {
            lastDisplayedFps = roundedFps;
        }

        tickText.SetText($"fps:{lastDisplayedFps}, tick:{tick}");
    }
}
