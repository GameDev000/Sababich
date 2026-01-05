using Unity.Services.Authentication;
using Unity.Services.Core;
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

    // [Header("Tray Clickables")]
    // [SerializeField] private Item[] trayEggplantItems; // For case that we want click on specific eggplant (there is currently one collider)

    [Header("Tray States (optional per level)")]
    [SerializeField] private FriedTrayState eggplantTrayState;
    [SerializeField] private FriedTrayState chipsTrayState;

    [Header("Instructions UI")]
    [SerializeField] private FeatureHintsSequence instructionManager;
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
        bool shouldRunInstructions_level2 = true;
        bool shouldRunInstructions_level3 = true;


        // if (UnityServices.State == ServicesInitializationState.Initialized &&
        //     AuthenticationService.Instance.IsSignedIn)
        // {
        //     var data = await DatabaseManager.LoadData("resumeScene");
        //     string resumeScene = DatabaseManager.ReadString(data, "resumeScene", "");

        //     if (resumeScene == "Level2 - endScene")
        //         shouldRunInstructions_level2 = false;
        //     // if (resumeScene == "Level3 - endScene")
        //     //     shouldRunInstructions_level3 = false;
        // }
        if (instructionManager != null && shouldRunInstructions_level2)
            instructionManager.OnLevelStarted(2);

        if (instructionManager != null && shouldRunInstructions_level3)
            instructionManager.OnLevelStarted(3);
    }


    public void OnTrayFilled()
    {
        trayReady = true;
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
