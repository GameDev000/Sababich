using UnityEngine;
using UnityEngine.UI;

//Shows and updates the timer image, based on the frying progress of a FryZone
public class FryTimerUI : MonoBehaviour
{
    // Frying zone of eggplant or chips
    [SerializeField] private FryZoneIngredient fryZone;
    // The UI Image of timer
    [SerializeField] private Image timerImage;

    private void Start()
    {
        if (timerImage == null)
            timerImage = GetComponent<Image>();

        if (timerImage != null)
        {
            timerImage.enabled = false; // Hide timer at game start
            timerImage.fillAmount = 1f; // Full circle
        }
    }
    
    // Every frame, the clock on the screen updates according to the frying status of FryZone.
    private void Update()
    {
        if (fryZone == null || !fryZone.IsFrying)
        {
            if (timerImage != null && timerImage.enabled)
                timerImage.enabled = false;

            return;
        }

        // If there is a frying proces
        if (timerImage != null)
        {
            if (!timerImage.enabled)
                timerImage.enabled = true;

            timerImage.fillAmount = 1f - fryZone.FryProgress; // fillAmount of timer according to percentage of the frying process
        }
    }
}
