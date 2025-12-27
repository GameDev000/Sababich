using UnityEngine;
using UnityEngine.UI;

public class DirtStateManager : MonoBehaviour
{
    public static DirtStateManager Instance { get; private set; }

    [Header("Accumulation")]
    [SerializeField] private int clicksToGetDirty = 6;

    [Header("Overlay UI")]
    [SerializeField] private GameObject dirtOverlayObject;

    [Header("Mirror")]
    [SerializeField] private SpriteRenderer mirrorRenderer;
    [SerializeField] private Sprite mirrorCleanSprite;
    [SerializeField] private Sprite mirrorDirtySprite;


    [Header("Tutorial Hook (optional)")]
    [SerializeField] private bool triggerTutorialOnDirty = true;
    [SerializeField] private TutorialManager tutorialOverride;
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
        ApplyVisuals();
    }

    public void RegisterMessyClick()
    {
        if (IsDirty) return;

        clickCount++;

        if (clickCount >= clicksToGetDirty)
        {
            IsDirty = true;
            ApplyVisuals();
            TryTriggerTutorialDirtyStep();
        }
    }

    public void Clean()
    {
        IsDirty = false;
        clickCount = 0;
        tutorialTriggeredThisDirty = false;

        ApplyVisuals();
    }

    private void ApplyVisuals()
    {
        if (dirtOverlayObject != null)
            dirtOverlayObject.SetActive(IsDirty);

        if (mirrorRenderer != null)
            mirrorRenderer.sprite = IsDirty ? mirrorDirtySprite : mirrorCleanSprite;
    }

    private void TryTriggerTutorialDirtyStep()
    {
        if (!triggerTutorialOnDirty) return;
        if (tutorialTriggeredThisDirty) return;

        TutorialManager tuto = tutorialOverride != null ? tutorialOverride : TutorialManager.Instance;
        if (tuto == null) return;

        tutorialTriggeredThisDirty = true;
        tuto.TriggerDirtTutorial();
    }
}
