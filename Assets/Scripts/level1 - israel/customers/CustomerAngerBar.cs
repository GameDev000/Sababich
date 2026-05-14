using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays a visual anger timer above the customer.
/// The bar fills over time and changes color from green to orange to red.
/// </summary>
public class CustomerAngerBar : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image fillImage;

    [Header("Colors")]
    [SerializeField] private Color calmColor = Color.green;
    [SerializeField] private Color warningColor = new Color(1f, 0.55f, 0f); // Orange
    [SerializeField] private Color angryColor = Color.red;

    private float duration;
    private float elapsedTime;
    private bool isRunning;

    /// <summary>
    /// Starts the anger bar timer.
    /// </summary>
    public void StartBar(float angerDurationSeconds)
    {
        duration = Mathf.Max(0.01f, angerDurationSeconds);
        elapsedTime = 0f;
        isRunning = true;

        gameObject.SetActive(true);

        if (fillImage != null)
        {
            fillImage.fillAmount = 0f;
            fillImage.color = calmColor;
        }
    }



    /// <summary>
    /// Stops and hides the anger bar.
    /// </summary>
    public void StopBar()
    {
        isRunning = false;
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Resets the bar without hiding it.
    /// </summary>
    public void ResetBar()
    {
        elapsedTime = 0f;

        if (fillImage != null)
        {
            fillImage.fillAmount = 0f;
            fillImage.color = calmColor;
        }
    }

    private void Update()
    {
        if (!isRunning)
            return;

        if (fillImage == null)
            return;

        elapsedTime += Time.deltaTime;

        float progress = Mathf.Clamp01(elapsedTime / duration);

        fillImage.fillAmount = progress;
        fillImage.color = GetColorByProgress(progress);

        if (progress >= 1f)
        {
            isRunning = false;
        }
    }

    private Color GetColorByProgress(float progress)
    {
        if (progress < 0.5f)
        {
            float t = progress / 0.5f;
            return Color.Lerp(calmColor, warningColor, t);
        }
        else
        {
            float t = (progress - 0.5f) / 0.5f;
            return Color.Lerp(warningColor, angryColor, t);
        }
    }
}