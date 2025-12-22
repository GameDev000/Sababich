using UnityEngine;

public class FryZoneEggplant : MonoBehaviour
{
    [SerializeField] private FryZoneIngredient fryZone;
    [SerializeField] private EggplantTray eggplantTray;

    private void Awake()
    {
        if (fryZone == null)
            fryZone = GetComponent<FryZoneIngredient>();
    }

    public void StartFry()
    {
        if (fryZone == null) return;

        fryZone.SetType(FryZoneIngredient.FryType.Eggplant);
        fryZone.StartFry();
    }

    // נקרא ע"י FryZoneIngredient כשהטיגון מוכן
    public void OnReady()
    {
        if (TutorialManager.Instance != null)
            TutorialManager.Instance.OnEggplantFried(fryZone);
    }

    // נקרא כשאוספים
    public void Collect()
    {
        if (eggplantTray != null)
            eggplantTray.FillFromPan();
    }
}
