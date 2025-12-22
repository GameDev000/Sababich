using UnityEngine;

public class LevelGameFlow : MonoBehaviour
{
    public static LevelGameFlow Instance { get; private set; }

    [Header("Frying Machine (legacy - old levels)")]
    [SerializeField] private FryZoneIngredient fryZone; // old single machine reference

    [Header("Level 3: Separate fryers (assign Emoji_Eggplant / Emoji_Chips)")]
    [SerializeField] private FryZoneIngredient eggplantFryer;
    [SerializeField] private FryZoneIngredient chipsFryer;

    [Header("Tray Clickables (Eggplant pieces on tray)")]
    [SerializeField] private Item[] trayEggplantItems;

    [Header("Tray States (optional per level)")]
    [SerializeField] private FriedTrayState eggplantTrayState;
    [SerializeField] private FriedTrayState chipsTrayState;

    private bool trayReady = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        SetTrayEggplantsClickable(false);
    }

    private void SetTrayEggplantsClickable(bool value)
    {
        if (trayEggplantItems == null) return;
        foreach (var item in trayEggplantItems)
            if (item != null) item.SetClickable(value);
    }

    //Backwards compatibility (tutorial/old scripts expect these)
    public void OnTrayFilled()
    {
        trayReady = true;
        SetTrayEggplantsClickable(true);
    }

    public void OnChipsTrayFilled()
    {
        OnTrayFilled();
    }

    private FryZoneIngredient GetEggplantFryer()
    {
        // If level3 fryers are assigned, use them; otherwise fall back to legacy single machine.
        return eggplantFryer != null ? eggplantFryer : fryZone;
    }

    private FryZoneIngredient GetChipsFryer()
    {
        return chipsFryer != null ? chipsFryer : fryZone;
    }

    public void OnIngredientClickedFromItem(Item item, string ingredientName)
    {
        string lower = ingredientName.ToLowerInvariant();

        // Start frying eggplant
        if (lower == "eggplantrow")
        {
            var f = GetEggplantFryer();
            if (f != null && !f.IsFrying)
            {
                f.SetType(FryZoneIngredient.FryType.Eggplant);
                f.StartFry();
            }
            return;
        }

        // Start frying chips
        if (lower == "potatoes")
        {
            var f = GetChipsFryer();
            if (f != null && !f.IsFrying)
            {
                f.SetType(FryZoneIngredient.FryType.Chips);
                f.StartFry();
            }
            return;
        }

        // Consume from trays
        if (lower == "eggplant" && eggplantTrayState != null)
            eggplantTrayState.ConsumeOne();

        if (lower == "chips" && chipsTrayState != null)
            chipsTrayState.ConsumeOne();
    }
}
