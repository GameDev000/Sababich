using UnityEngine;

// One frying machine that can fry different ingredient types (Eggplant / Chips).

public class FryZoneIngredient : MonoBehaviour
{
    public enum FryType { Eggplant, Chips }
    private enum FryState { Empty, Frying, Ready }

    [Header("Type")]
    [SerializeField] private FryType currentType = FryType.Eggplant; // default for 0-2 levels

    // Controls whether clicking this object collects the fried item (level3-> the emojy true)
    [Header("Collect Settings")]
    [SerializeField] private bool collectByClickOnThisObject = true;

    [Header("Visual (optional)")]
    [SerializeField] private SpriteRenderer overlayRenderer; // can be null if you don't want Visual frying
    [SerializeField] private Sprite fryingSprite;            // Optional for level3
    [SerializeField] private Sprite readySprite;             // Optional for level3

    [Header("Timing")]
    [SerializeField] private float fryTimeSeconds = 5f;

    [Header("Output trays")]
    [SerializeField] private FriedTray eggplantTray;
    [SerializeField] private FriedTray chipsTray; 

    private FryState state = FryState.Empty;
    private float timer = 0f;

    //Indications for the frying process
    public FryType CurrentType => currentType;
    public bool IsFrying => state == FryState.Frying;
    public bool IsReady => state == FryState.Ready;

    public float FryProgress =>
        (state == FryState.Frying && fryTimeSeconds > 0f)
            ? Mathf.Clamp01(timer / fryTimeSeconds) // Frying percentage of the entire process
            : (state == FryState.Ready ? 1f : 0f); //1->100%, 0->not start

    private void Start()
    {
        SetState(FryState.Empty);
    }

    private void Update()
    {
        if (state != FryState.Frying) return;
        // Frying process update
        timer += Time.deltaTime;
        if (timer >= fryTimeSeconds)
        {
            SetState(FryState.Ready);
            OnReady();
        }
    }

    public void SetType(FryType type)////////////////////////////////////////////
    {
        // Optional: block changing while frying
        // if (IsFrying) return;
        currentType = type;
    }

    // Called on LevelGameFlow
    public void StartFry()
    {
        if (state != FryState.Empty) return;

        timer = 0f;
        SetState(FryState.Frying);
    }

    public void ClearPan()
    {
        SetState(FryState.Empty);
    }

    private void SetState(FryState newState)
    {
        state = newState;

        if (overlayRenderer == null) return;

        switch (state)
        {
            case FryState.Empty:
                overlayRenderer.enabled = false;
                overlayRenderer.sprite = null;
                break;

            case FryState.Frying:
                overlayRenderer.enabled = true;
                overlayRenderer.sprite = fryingSprite;
                break;

            case FryState.Ready:
                overlayRenderer.enabled = true;
                overlayRenderer.sprite = readySprite;
                break;
        }
    }

    private void OnReady()
    {
        // Tutorial hook (optional)
        if (TutorialManager.Instance != null)
        {
            if (currentType == FryType.Eggplant)
                TutorialManager.Instance.OnEggplantFried(this);
            else
                TutorialManager.Instance.OnChipsFried(this);
        }
    }

    private void Collect()
    {
        if (currentType == FryType.Eggplant)
        {
            if (eggplantTray != null) eggplantTray.FillFromPan();
        }
        else
        {
            if (chipsTray != null) chipsTray.FillFromPan();
        }
    }

    private void OnMouseDown()
    {
        // NEW: allow disabling collect by clicking this object (so collect can be only via emoji)
        if (!collectByClickOnThisObject) return;

        if (state != FryState.Ready) return;

        // Clear first, then output
        ClearPan();
        Collect();
    }
}
