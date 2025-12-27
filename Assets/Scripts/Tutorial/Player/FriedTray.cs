using UnityEngine;

// Represents a fried food tray that receives items from the fryer, updates its visual state, and notifies the tutorial and game flow when it is filled
public class FriedTray : MonoBehaviour
{
    public enum TrayType { Eggplant, Chips }

    [Header("Type")]
    [SerializeField] private TrayType trayType = TrayType.Eggplant; //Default-> Eggplant

    [Header("State")]
    [SerializeField] private FriedTrayState trayState;

    public void FillFromPan()
    {
        Debug.Log($"[Tray:{trayType}] FillFromPan CALLED");

        if (trayState != null)
            trayState.Refill();

        // Update Tutorial
        if (TutorialManager.Instance != null)
        {
            if (trayType == TrayType.Eggplant)
            {
                TutorialManager.Instance.OnEggplantTrayFull();
            }
            else
            {
                TutorialManager.Instance.OnChipsTrayFull();
            }
        }
        // Update LevelGameFlow
        if (LevelGameFlow.Instance != null)
        {
            if (trayType == TrayType.Eggplant)
            {
                LevelGameFlow.Instance.OnTrayFilled();
            }
            else
            {
                LevelGameFlow.Instance.OnChipsTrayFilled();
            }
        }
    }
}
