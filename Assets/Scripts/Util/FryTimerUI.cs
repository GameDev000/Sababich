using UnityEngine;
using UnityEngine.UI;

public class FryTimerUI : MonoBehaviour
{
    [SerializeField] private FryZoneEggplant fryZone;
    [SerializeField] private Image timerImage;

    private void Start()
    {

        if (timerImage == null)
            timerImage = GetComponent<Image>();


        if (timerImage != null)
        {
            timerImage.enabled = false;
            timerImage.fillAmount = 1f;
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
