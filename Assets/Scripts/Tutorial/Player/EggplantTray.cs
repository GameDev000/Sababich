using UnityEngine;

/// <summary>
/// Manages the eggplant tray, allowing it to be filled and notifying the tutorial manager.
/// </summary>
public class EggplantTray : MonoBehaviour
{
    [SerializeField] private SpriteRenderer trayRenderer; // Reference to the SpriteRenderer component
    [SerializeField] private Sprite fullTraySprite; // Sprite to display when the tray is full
    [SerializeField] private EggplantTrayState trayState;

    private void Start()
    {
        SetEmpty();// Initialize the tray as empty at the start
    }

    private void SetEmpty()
    {
        if (trayRenderer != null)
        {
            trayRenderer.enabled = true;
            trayRenderer.sprite = null;
        }
    }

    /// <summary>
    /// Fills the tray with eggplant from the pan and notifies the tutorial manager.
    /// </summary>
    public void FillFromPan()
    {
        Debug.Log("[Tray] FillFromPan CALLED");



        if (trayRenderer != null && fullTraySprite != null)
        {
            trayRenderer.enabled = true;
            trayRenderer.sprite = fullTraySprite; // Set the tray sprite to the full tray sprite
        }
        if (trayState != null)
            trayState.Refill();

        if (TutorialManager.Instance != null)
        {
            Debug.Log("[Tray] Calling TutorialManager.OnEggplantTrayFull()");
            TutorialManager.Instance.OnEggplantTrayFull(); // Notify the tutorial manager that the tray is full
        }
        if (Level1GameFlow.Instance != null)
        {
            Debug.Log("[Tray] Calling Level1GameFlow.OnTrayFilled()");// Notify the game flow that the tray is filled
            Level1GameFlow.Instance.OnTrayFilled();
        }
    }



}
