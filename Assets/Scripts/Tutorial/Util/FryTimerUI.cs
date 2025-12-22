using UnityEngine;
using UnityEngine.UI;

//Shows and updates the timer image, based on the frying progress of a FryZone
public class FryTimerUI : MonoBehaviour
{
    //frying zone of eggplant or chips
    [SerializeField] private FryZoneIngredient fryZone;
    //The UI Image of timer
    [SerializeField] private Image timerImage;

    private void Start()
    {
        if (timerImage == null)
            timerImage = GetComponent<Image>();

        if (timerImage != null)
        {
            timerImage.enabled = false; //Hide timer at game start
            timerImage.fillAmount = 1f; //Full circle
        }
    }

    private void Update()
    {
        if (fryZone == null || !fryZone.IsFrying)
        {
            if (timerImage != null && timerImage.enabled)
                timerImage.enabled = false;

            return;
        }

        if (timerImage != null)
        {
            if (!timerImage.enabled)
                timerImage.enabled = true;

            timerImage.fillAmount = 1f - fryZone.FryProgress;
        }
    }
}
