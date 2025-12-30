using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Manages the dirtiness state of the kitchen, including cleaning.
/// Tracks clicks that contribute to dirtiness and updates visual elements accordingly.
/// </summary>
public class DirtStateManager : MonoBehaviour
{
    public static DirtStateManager Instance { get; private set; }

    [Header("Accumulation")]
    [SerializeField] private int clicksToGetDirty = 6;// Number of clicks needed to become dirty

    [Header("Overlay UI")]
    [SerializeField] private GameObject dirtOverlayObject;// The overlay to show when dirty

    [Header("Mirror")]
    [SerializeField] private SpriteRenderer mirrorRenderer;// The mirror sprite renderer
    [SerializeField] private Sprite mirrorCleanSprite;// Clean mirror sprite
    [SerializeField] private Sprite mirrorDirtySprite;// Dirty mirror sprite


    [Header("Tutorial Hook (optional)")]
    [SerializeField] private bool triggerTutorialOnDirty = true;// Whether to trigger tutorial when dirty
    [SerializeField] private TutorialManager tutorialOverride;// Optional tutorial manager override
    private bool tutorialTriggeredThisDirty = false;

    private int clickCount;
    public bool IsDirty { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        ApplyVisuals();// Initialize visuals based on current state
    }

    /// <summary>
    /// Registers a click on a messy item, potentially increasing dirtiness.
    /// </summary>
    public void RegisterMessyClick()
    {
        if (IsDirty) return;

        clickCount++;

        if (clickCount >= clicksToGetDirty)// if reached threshold apply dirty state
        {
            IsDirty = true;
            ApplyVisuals();
            TryTriggerTutorialDirtyStep();
        }
    }

    /// <summary>
    /// Cleans the kitchen, resetting dirtiness state and visuals.
    /// </summary>
    public void Clean()
    {
        IsDirty = false;
        clickCount = 0;
        tutorialTriggeredThisDirty = false;

        ApplyVisuals();// Update visuals to clean state
    }

    /// <summary>
    /// Applies visual changes based on the current dirtiness state.
    /// Updates the dirt overlay and mirror sprite accordingly.
    /// </summary>
    private void ApplyVisuals()
    {
        if (dirtOverlayObject != null)
            dirtOverlayObject.SetActive(IsDirty);// Show or hide dirt overlay

        if (mirrorRenderer != null)
            mirrorRenderer.sprite = IsDirty ? mirrorDirtySprite : mirrorCleanSprite;// Update mirror sprite
    }

    /// <summary>
    /// Attempts to trigger the tutorial step for dirtiness if conditions are met.
    /// </summary>
    private void TryTriggerTutorialDirtyStep()
    {
        if (!triggerTutorialOnDirty) return;
        if (tutorialTriggeredThisDirty) return;

        TutorialManager tuto = tutorialOverride != null ? tutorialOverride : TutorialManager.Instance;
        if (tuto == null) return;

        tutorialTriggeredThisDirty = true;
        tuto.TriggerDirtTutorial();// Trigger the dirt tutorial step
    }
}
