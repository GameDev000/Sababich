using UnityEngine;

public class EggplantTray : MonoBehaviour
{
    [SerializeField] private SpriteRenderer trayRenderer;
    //[SerializeField] private Sprite emptyTraySprite;
    [SerializeField] private Sprite fullTraySprite;

    //private bool hasEggplant = false;

    private void Start()
    {
        SetEmpty();
    }

    private void SetEmpty()
    {
        //hasEggplant = false;

        if (trayRenderer != null)
        {
            trayRenderer.enabled = true;
            trayRenderer.sprite = null;
        }
    }

    public void FillFromPan()
{
    Debug.Log("[Tray] FillFromPan CALLED");

    //hasEggplant = true;

    if (trayRenderer != null && fullTraySprite != null)
    {
        trayRenderer.enabled = true;
        trayRenderer.sprite = fullTraySprite;
    }

    if (TutorialManager.Instance != null)
    {
        Debug.Log("[Tray] Calling TutorialManager.OnEggplantTrayFull()");
        TutorialManager.Instance.OnEggplantTrayFull();
    }
}



}
