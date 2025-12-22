using UnityEngine;

/// <summary>
/// Allows the eggplant tray to be filled
/// </summary>
public class EggplantTray : MonoBehaviour
{
    [SerializeField] private FriedTrayState trayState; //referance to object that manage state

    /// <summary>
    /// Fills the tray with eggplant from the pan and notifies the tutorial manager.
    /// </summary>
    //FillFromPan() called from FryZoneEggplant()
    public void FillFromPan()
    {
        Debug.Log("[Tray] FillFromPan CALLED");

        if (trayState != null)
            trayState.Refill();

        if (TutorialManager.Instance != null)
        {
            Debug.Log("[Tray] Calling TutorialManager.OnEggplantTrayFull()");
            TutorialManager.Instance.OnEggplantTrayFull(); // Notify the tutorial manager that the tray is full
        }
        if (LevelGameFlow.Instance != null)
        {
            Debug.Log("[Tray] Calling LevelGameFlow.OnTrayFilled()");// Notify the game flow that the tray is filled
            LevelGameFlow.Instance.OnTrayFilled();
        }
    }



}
