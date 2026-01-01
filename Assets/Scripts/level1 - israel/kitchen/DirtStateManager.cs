// using UnityEngine;
// using UnityEngine.UI;
// /// <summary>
// /// Manages the dirtiness state of the kitchen, including cleaning.
// /// Tracks clicks that contribute to dirtiness and updates visual elements accordingly.
// /// </summary>
// public class DirtStateManager : MonoBehaviour
// {
//     public static DirtStateManager Instance { get; private set; }

//     [Header("Accumulation")]
//     [SerializeField] private int clicksToGetDirty = 6;// Number of clicks needed to become dirty

//     [Header("Overlay UI")]
//     [SerializeField] private GameObject dirtOverlayObject;// The overlay to show when dirty

//     [Header("Mirror")]
//     [SerializeField] private SpriteRenderer mirrorRenderer;// The mirror sprite renderer
//     [SerializeField] private Sprite mirrorCleanSprite;// Clean mirror sprite
//     [SerializeField] private Sprite mirrorDirtySprite;// Dirty mirror sprite


//     [Header("Tutorial Hook (optional)")]
//     [SerializeField] private bool triggerTutorialOnDirty = true;// Whether to trigger tutorial when dirty
//     [SerializeField] private TutorialManager tutorialOverride;// Optional tutorial manager override
//     private bool tutorialTriggeredThisDirty = false;

//     private int clickCount;
//     public bool IsDirty { get; private set; }

//     private void Awake()
//     {
//         if (Instance != null && Instance != this) { Destroy(gameObject); return; }
//         Instance = this;
//     }

//     private void Start()
//     {
//         ApplyVisuals();// Initialize visuals based on current state
//     }

//     /// <summary>
//     /// Registers a click on a messy item, potentially increasing dirtiness.
//     /// </summary>
//     public void RegisterMessyClick()
//     {
//         if (IsDirty) return;

//         clickCount++;

//         if (clickCount >= clicksToGetDirty)// if reached threshold apply dirty state
//         {
//             IsDirty = true;
//             ApplyVisuals();
//             TryTriggerTutorialDirtyStep();
//         }
//     }

//     /// <summary>
//     /// Cleans the kitchen, resetting dirtiness state and visuals.
//     /// </summary>
//     public void Clean()
//     {
//         IsDirty = false;
//         clickCount = 0;
//         tutorialTriggeredThisDirty = false;

//         ApplyVisuals();// Update visuals to clean state
//     }

//     /// <summary>
//     /// Applies visual changes based on the current dirtiness state.
//     /// Updates the dirt overlay and mirror sprite accordingly.
//     /// </summary>
//     private void ApplyVisuals()
//     {
//         if (dirtOverlayObject != null)
//             dirtOverlayObject.SetActive(IsDirty);// Show or hide dirt overlay

//         if (mirrorRenderer != null)
//             mirrorRenderer.sprite = IsDirty ? mirrorDirtySprite : mirrorCleanSprite;// Update mirror sprite
//     }

//     /// <summary>
//     /// Attempts to trigger the tutorial step for dirtiness if conditions are met.
//     /// </summary>
//     private void TryTriggerTutorialDirtyStep()
//     {
//         if (!triggerTutorialOnDirty) return;
//         if (tutorialTriggeredThisDirty) return;

//         TutorialManager tuto = tutorialOverride != null ? tutorialOverride : TutorialManager.Instance;
//         if (tuto == null) return;

//         tutorialTriggeredThisDirty = true;
//         tuto.TriggerDirtTutorial();// Trigger the dirt tutorial step
//     }
// }


using UnityEngine;

/// <summary>
/// Manages the dirtiness state of the kitchen, including gradual dirt stages and cleaning.
/// Every N clicks increases the dirt stage (up to 4 stages total: 0..3).
/// </summary>
public class DirtStateManager : MonoBehaviour
{
    public static DirtStateManager Instance { get; private set; }

    [Header("Accumulation")]
    [SerializeField] private int clicksPerStage = 6; // 6 clicks -> stage 1, 12 -> stage 2, 18 -> stage 3

    [Header("Overlay UI")]
    [SerializeField] private GameObject dirtOverlayObject; // UI image object (overlay)
    [SerializeField] private SpriteRenderer dirtOverlayRenderer; // Optional: SpriteRenderer if overlay is in world (not UI Image)

    [Tooltip("4 sprites total: [0]=clean (or null), [1]=dirt1, [2]=dirt2, [3]=dirt3")]
    [SerializeField] private Sprite[] dirtStageSprites = new Sprite[4];

    [Header("Mirror")]
    [SerializeField] private SpriteRenderer mirrorRenderer;
    [SerializeField] private Sprite mirrorCleanSprite;
    [SerializeField] private Sprite mirrorDirtySprite; // used for stages 1..3

    [Header("Tutorial Hook (optional)")]
    [SerializeField] private bool triggerTutorialOnFirstDirtStage = true;
    [SerializeField] private TutorialManager tutorialOverride;
    private bool tutorialTriggeredThisDirty = false;

    private int clickCount;
    public int DirtStage { get; private set; } = 0; // 0..3

    public bool IsDirty => DirtStage > 0;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        ApplyVisuals();
    }

    /// <summary>
    /// Registers a click on a messy item, potentially increasing dirt stage (0..3).
    /// </summary>
    public void RegisterMessyClick()
    {
        if (DirtStage >= 3) return;

        clickCount++;

        int newStage = Mathf.Clamp(clickCount / clicksPerStage, 0, 3); // 0 for 0..5, 1 for 6..11, 2 for 12..17, 3 for 18+
        if (newStage != DirtStage)
        {
            DirtStage = newStage;
            ApplyVisuals();

            // Trigger tutorial exactly when we reach the FIRST dirt sprite (stage 1)
            if (DirtStage == 1)
                TryTriggerTutorialDirtyStep();
        }
    }

    /// <summary>
    /// Cleans the kitchen completely: resets stage and counter.
    /// </summary>
    public void Clean()
    {
        clickCount = 0;
        DirtStage = 0;
        tutorialTriggeredThisDirty = false;

        ApplyVisuals();
    }

    /// <summary>
    /// Applies visual changes based on current dirt stage.
    /// </summary>
    private void ApplyVisuals()
    {
        // Overlay show/hide
        if (dirtOverlayObject != null)
            dirtOverlayObject.SetActive(IsDirty);

        // Overlay sprite (for world sprite overlays)
        if (dirtOverlayRenderer != null && dirtStageSprites != null && dirtStageSprites.Length >= 4)
        {
            dirtOverlayRenderer.sprite = dirtStageSprites[DirtStage];
        }

        // Mirror sprite
        if (mirrorRenderer != null)
            mirrorRenderer.sprite = IsDirty ? mirrorDirtySprite : mirrorCleanSprite;
    }

    private void TryTriggerTutorialDirtyStep()
    {
        if (!triggerTutorialOnFirstDirtStage) return;
        if (tutorialTriggeredThisDirty) return;

        TutorialManager tuto = tutorialOverride != null ? tutorialOverride : TutorialManager.Instance;
        if (tuto == null) return;

        tutorialTriggeredThisDirty = true;
        tuto.TriggerDirtTutorial();
    }
}
