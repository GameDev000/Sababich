using UnityEngine;

public class FriedTray : MonoBehaviour
{
    public enum TrayType { Eggplant, Chips }

    [Header("Type")]
    [SerializeField] private TrayType trayType = TrayType.Eggplant;

    [Header("State")]
    [SerializeField] private FriedTrayState trayState;

    public void FillFromPan()
    {
        Debug.Log($"[Tray:{trayType}] FillFromPan CALLED");

        if (trayState != null)
            trayState.Refill();

        // Tutorial hooks (keep compatibility)
        if (TutorialManager.Instance != null)
        {
            if (trayType == TrayType.Eggplant)
            {
                TutorialManager.Instance.OnEggplantTrayFull();
            }
            else
            {
                TutorialManager.Instance.OnChipsTrayFull(); // you add this method (see below)
            }
        }
        // Optional: if you want GameFlow notification per type (recommended)
        if (LevelGameFlow.Instance != null)
        {
            if (trayType == TrayType.Eggplant)
            {
                LevelGameFlow.Instance.OnTrayFilled();
            }
            else
            {
                LevelGameFlow.Instance.OnChipsTrayFilled(); // optional (see below)
            }
        }
    }
}
