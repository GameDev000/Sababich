using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Updates the frying timer UI based on the frying progress in the FryZoneEggplant.
/// </summary>
public class FryTimerUI : MonoBehaviour
{
    [SerializeField] private FryZoneEggplant fryZone; // Reference to the FryZoneEggplant component
    [SerializeField] private Image timerImage; // Reference to the UI Image component representing the timer

    private void Start()
    {

        if (timerImage == null)
            timerImage = GetComponent<Image>(); // Get the Image component if not assigned


        if (timerImage != null)
        {
            timerImage.enabled = false; // Initially hide the timer image
            timerImage.fillAmount = 1f; // Set the fill amount to full
        }
    }

    private void Update()
    {   // Update the timer UI based on the frying progress
        if (fryZone == null || !fryZone.IsFrying)
        {
            if (timerImage != null && timerImage.enabled)
                timerImage.enabled = false;

            return;
        }
        // Update the timer fill amount based on frying progress
        if (timerImage != null)
        {
            if (!timerImage.enabled)
                timerImage.enabled = true;

            timerImage.fillAmount = 1f - fryZone.FryProgress;// Fill amount decreases as frying progresses
        }
    }
}
