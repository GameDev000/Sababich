using UnityEngine;

public class FryZoneEggplant : MonoBehaviour
{
    public enum FryState { Empty, Frying, Ready }

    [Header("Sprites")]
    [SerializeField] private SpriteRenderer overlayRenderer; 
    [SerializeField] private Sprite rawEggplantSprite;       
    [SerializeField] private Sprite friedEggplantSprite;     

    [Header("Timing")]
    [SerializeField] private float fryTimeSeconds = 5f;      

    [Header("Tray")]
    [SerializeField] private EggplantTray eggplantTray;
    private FryState state = FryState.Empty;
    private float timer = 0f;

    public bool IsReady => state == FryState.Ready;
    public bool IsFrying   => state == FryState.Frying;
    public float FryProgress => (state == FryState.Frying && fryTimeSeconds > 0f) ? Mathf.Clamp01(timer / fryTimeSeconds): (state == FryState.Ready ? 1f : 0f);

    private void Start()
    {
        Debug.Log("[FryZone] StartFry, current state: " + state);

        if (state != FryState.Empty)
            return;

        timer = 0f;
        SetState(FryState.Empty);
    }

    private void Update()
    {
        if (state == FryState.Frying)
        {
            timer += Time.deltaTime;
            if (timer >= fryTimeSeconds)
            {
                SetState(FryState.Ready);
            }
        }
    }

    public void StartFry()
    {
        Debug.Log("[FryZone] StartFry, current state: " + state);

        if (state != FryState.Empty)
            return;

        timer = 0f;
        SetState(FryState.Frying);
    }

    public void ClearPan()
    {
        SetState(FryState.Empty);
    }

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

    private void OnMouseDown()
    {
        if (state != FryState.Ready) return;

        Debug.Log("[FryZone] Pan clicked while ready");

        ClearPan();

        if (eggplantTray != null)
        {
            eggplantTray.FillFromPan();
        }

    }
}
