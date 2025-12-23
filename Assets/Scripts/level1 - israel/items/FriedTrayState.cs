using UnityEngine;

public class FriedTrayState : MonoBehaviour
{
    [SerializeField] private SpriteRenderer trayRenderer;

    [SerializeField] private Sprite fullSprite;
    [SerializeField] private Sprite midSprite;
    [SerializeField] private Sprite lowSprite;   // "few left"

    [SerializeField] private int usesPerStage = 2;
    [SerializeField] private Item[] trayItems;   // items you click to take from this tray

    // 0 = full, 1 = mid, 2 = low, 3 = empty (no overlay)
    private int stage = 3;
    private int usesLeft = 0;

    private void Awake()
    {
        ApplyStage();                 // start empty: overlay OFF
        SetTrayItemsClickable(false); // cannot click at game start
    }

    public void Refill()
    {
        stage = 0;
        usesLeft = usesPerStage;
        ApplyStage();
        SetTrayItemsClickable(true);
    }

    public void ConsumeOne()
    {
        if (stage >= 3) return; // already empty

        usesLeft--;

        if (usesLeft <= 0)
        {
            usesLeft = usesPerStage;
            stage++;
            ApplyStage();

            if (stage >= 3)
                SetTrayItemsClickable(false);
        }
    }

    private void ApplyStage()
    {
        if (trayRenderer == null) return;

        if (stage == 0)
        {
            trayRenderer.enabled = true;
            trayRenderer.sprite = fullSprite;
        }
        else if (stage == 1)
        {
            trayRenderer.enabled = true;
            trayRenderer.sprite = midSprite;
        }
        else if (stage == 2)
        {
            trayRenderer.enabled = true;
            trayRenderer.sprite = lowSprite;
        }
        else
        {
            
            SetTrayItemsClickable(false);
            trayRenderer.sprite = null;
            trayRenderer.enabled = false; // show only background
        }
    }

    private void SetTrayItemsClickable(bool value)
    {
        if (trayItems == null) return;

        foreach (var it in trayItems)
            if (it != null) it.SetClickable(value);
    }
}
