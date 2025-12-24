using UnityEngine;

//Manages the overall level flow by coordinating frying machines, tray states, and item interactions across different level setups.
public class LevelGameFlow : MonoBehaviour
{
    public static LevelGameFlow Instance { get; private set; } // This line defines a Singleton instance that provides global access to the level’s gameManager logic

    [Header("Frying Machine (0-2 levels)")]
    [SerializeField] private FryZoneIngredient fryZone; // Single machine reference

    [Header("Level 3: Separate fryers (assign Emoji_Eggplant / Emoji_Chips)")]
    [SerializeField] private FryZoneIngredient eggplantFryer; // Eggplant machine reference
    [SerializeField] private FryZoneIngredient chipsFryer;  // Chips machine reference

    [Header("Tray Clickables")]
    [SerializeField] private Item[] trayEggplantItems; // For case that we want click on specific eggplant (there is currently one collider)

    [Header("Tray States (optional per level)")]
    [SerializeField] private FriedTrayState eggplantTrayState;
    [SerializeField] private FriedTrayState chipsTrayState;

    private bool trayReady = false; // Tray usage indication

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

    //Both methods signal that a tray is ready and enable item interaction, called on FriedTray
    public void OnTrayFilled()
    {
        trayReady = true;
        SetTrayEggplantsClickable(true);
    }

    public void OnChipsTrayFilled()
    {
        OnTrayFilled();
    }

    //The get method allows access to machine/machines according to level
    private FryZoneIngredient GetEggplantFryer()
    {
        return eggplantFryer != null ? eggplantFryer : fryZone;
    }

    private FryZoneIngredient GetChipsFryer()
    {
        return chipsFryer != null ? chipsFryer : fryZone;
    }

    //Called on ItemScrips
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
