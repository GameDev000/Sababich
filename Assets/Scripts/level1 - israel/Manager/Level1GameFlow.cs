using UnityEngine;

public class Level1GameFlow : MonoBehaviour
{
    public static Level1GameFlow Instance { get; private set; }

    [SerializeField] private FryZoneEggplant fryZone;
    [SerializeField] private Item[] trayEggplantItems;
    [SerializeField] private EggplantTrayState trayState;


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
        {
            if (item != null)
                item.SetClickable(value);
        }
    }

    public void OnTrayFilled()
    {
        trayReady = true;
        SetTrayEggplantsClickable(true);
    }

public void OnIngredientClickedFromItem(Item item, string ingredientName)
{
    string lower = ingredientName.ToLowerInvariant();

    if (lower == "eggplantrow")
    {
        if (fryZone != null && !fryZone.IsFrying)
            fryZone.StartFry();
        return;
    }

    if (lower == "eggplant" && trayState != null)
        trayState.ConsumeOne();
}

}
