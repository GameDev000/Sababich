using UnityEngine;

// Manages the fried tray state by updating its visual stages and controlling item availability based on remaining uses.
public class FriedTrayState : MonoBehaviour
{
    //Sprite is the image, SpriteRenderer is the component that displays it in the scene
    [SerializeField] private SpriteRenderer trayRenderer;

    [SerializeField] private Sprite fullSprite;
    [SerializeField] private Sprite midSprite;
    [SerializeField] private Sprite lowSprite;   

    [SerializeField] private int usesPerStage = 2;
    [SerializeField] private Item[] trayItems;   // For case that we want click on specific eggplant (there is currently one collider)

    // 0 = full, 1 = mid, 2 = low, 3 = empty (no overlay)
    private int stage = 3;
    private int usesLeft = 0;

    public bool IsEmpty => stage >= 3; // For cheack IsEmpty and prevent using this tray

    private void Awake()
    {
        ApplyStage();                 // start empty, no overlay
        SetTrayItemsClickable(false); // cannot click at game start
    }

    public void Refill()
    {
        stage = 0;
        usesLeft = usesPerStage;
        ApplyStage(); // Go to if (stage == 0)
        SetTrayItemsClickable(true);
    }

    //Called on LevelGameFlow and update stage
    public void ConsumeOne()
    {
        if (stage >= 3) return; // already empty

        usesLeft--;

        if (usesLeft <= 0)
        {
            usesLeft = usesPerStage;
            stage++;
            ApplyStage();
        }
}

    private void ApplyStage()
    {
        if (trayRenderer == null) return;
        
        // Sprites will be displayed when there is a sprite render, stage 0-2
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
        else //Stage >= 3 -> empty
        {
        trayRenderer.sprite = null;
        trayRenderer.enabled = false;
        SetTrayItemsClickable(false);
        }
    }

    //Help function for validation
    private void SetTrayItemsClickable(bool value)
    {
        if (trayItems == null) return;

        foreach (var it in trayItems)
            if (it != null) it.SetClickable(value);
    }
}