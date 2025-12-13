using UnityEngine;

public class EggplantTrayState : MonoBehaviour
{
    [SerializeField] private SpriteRenderer trayRenderer;

    [SerializeField] private Sprite fullSprite;
    [SerializeField] private Sprite midSprite;
    [SerializeField] private Sprite emptySprite;

    [SerializeField] private int usesPerStage = 2;
    [SerializeField] private Item[] trayEggplantItems;

    private int stage = 0;
    private int usesLeft;

    private void Awake()
    {
        usesLeft = usesPerStage;
        ApplyStage();
        SetTrayItemsClickable(true);
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
        if (stage >= 2) return;

        usesLeft--;

        if (usesLeft <= 0)
        {
            usesLeft = usesPerStage;
            stage++;
            ApplyStage();

            if (stage >= 2)
                SetTrayItemsClickable(false);
        }
    }

    private void ApplyStage()
    {
        if (trayRenderer == null) return;

        if (stage == 0) trayRenderer.sprite = fullSprite;
        else if (stage == 1) trayRenderer.sprite = midSprite;
        else trayRenderer.sprite = emptySprite;

        trayRenderer.enabled = true;
    }

    private void SetTrayItemsClickable(bool value)
    {
        if (trayEggplantItems == null) return;

        foreach (var it in trayEggplantItems)
            if (it != null) it.SetClickable(value);
    }
}
