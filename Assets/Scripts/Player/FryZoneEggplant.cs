using UnityEngine;

/// <summary>
/// Manages the frying zone for eggplants, handling frying states and interactions.
/// </summary>
public class FryZoneEggplant : MonoBehaviour
{
    public enum FryState { Empty, Frying, Ready } // Frying states

    [Header("Sprites")]
    [SerializeField] private SpriteRenderer overlayRenderer; // Reference to the overlay SpriteRenderer
    [SerializeField] private Sprite rawEggplantSprite; // Sprite to display when frying
    [SerializeField] private Sprite friedEggplantSprite; // Sprite to display when fried

    [Header("Timing")]
    [SerializeField] private float fryTimeSeconds = 5f; // Time required to fry the eggplant

    [Header("Tray")]
    [SerializeField] private EggplantTray eggplantTray; // Reference to the eggplant tray
    private FryState state = FryState.Empty; // Current frying state
    public float timer = 0f;

    public bool IsReady => state == FryState.Ready;
    public bool IsFrying => state == FryState.Frying;
    public float FryProgress => (state == FryState.Frying && fryTimeSeconds > 0f) ? Mathf.Clamp01(timer / fryTimeSeconds) : (state == FryState.Ready ? 1f : 0f);

    private void Start()
    {
        Debug.Log("[FryZone] StartFry, current state: " + state);

        if (state != FryState.Empty)
            return;

        timer = 0f;
        SetState(FryState.Empty); // Initialize to empty state
    }

    private void Update()
    {
        if (state == FryState.Frying)
        {
            timer += Time.deltaTime;
            if (timer >= fryTimeSeconds) // Check if frying is complete
            {
                SetState(FryState.Ready); // Transition to ready state
            }
        }
    }

    public void StartFry()
    {
        Debug.Log("[FryZone] StartFry, current state: " + state);

        if (state != FryState.Empty)
            return;

        timer = 0f;
        SetState(FryState.Frying);// Transition to frying state 
    }

    public void ClearPan()
    {
        SetState(FryState.Empty); // Transition to empty state
    }


    /// <summary>
    /// Sets the current frying state and updates the overlay sprite accordingly.
    /// </summary>
    private void SetState(FryState newState)
    {
        Debug.Log("[FryZone] SetState: " + state + " -> " + newState);
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
                overlayRenderer.sprite = rawEggplantSprite;
                break;

            case FryState.Ready:
                overlayRenderer.enabled = true;
                overlayRenderer.sprite = friedEggplantSprite;
                if (TutorialManager.Instance != null)
                {
                    TutorialManager.Instance.OnEggplantFried(this);
                }
                break;
        }
    }

    /// <summary>
    /// Handles mouse click interactions on the frying zone.
    /// </summary>
    private void OnMouseDown()
    {
        if (state != FryState.Ready) return;

        Debug.Log("[FryZone] Pan clicked while ready");

        ClearPan(); // Clear the pan

        if (eggplantTray != null)
        {
            eggplantTray.FillFromPan(); // Fill the tray with fried eggplant
        }

    }


}
