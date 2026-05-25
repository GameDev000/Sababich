
// using UnityEngine;

// // One frying machine that can fry different ingredient types (Eggplant / Chips).
// public class FryZoneIngredient : MonoBehaviour
// {
//     public enum FryType { Eggplant, Chips }

//     // Added Burnt state
//     private enum FryState { Empty, Frying, Ready, Burnt }
//     [SerializeField] private bool is_level3 = false; // to control burn behavior in level3

//     [Header("Type")]
//     [SerializeField] private FryType currentType = FryType.Eggplant; // default for 0-2 levels

//     // Controls whether clicking this object collects the fried item (level3-> the emojy true)
//     [Header("Collect Settings")]
//     [SerializeField] private bool collectByClickOnThisObject = true;

//     [Header("Visual (optional)")]
//     [SerializeField] private SpriteRenderer overlayRenderer; // can be null if you don't want Visual frying
//     [SerializeField] private Sprite fryingSprite;            // Optional for level3
//     [SerializeField] private Sprite readySprite;             // Optional for level3
//     [SerializeField] private Sprite burntSprite;             // Sprite for burnt state

//     [Header("Timing")]
//     [SerializeField] private float fryTimeSeconds = 5f;
//     [SerializeField] private float burnAfterReadySeconds = 10f; // Time until burnt after ready

//     [Header("Output trays")]
//     [SerializeField] private FriedTray eggplantTray;
//     [SerializeField] private FriedTray chipsTray;

//     private FryState state = FryState.Empty;

//     private float fryTimer = 0f;   // Frying timer
//     private float readyTimer = 0f; // Burn timer (starts when Ready)

//     //Indications for the frying process
//     public FryType CurrentType => currentType;
//     public bool IsFrying => state == FryState.Frying;
//     public bool IsReady => state == FryState.Ready;
//     public bool IsBurnt => state == FryState.Burnt;

//     public float FryProgress =>
//         (state == FryState.Frying && fryTimeSeconds > 0f)
//             ? Mathf.Clamp01(fryTimer / fryTimeSeconds) // Frying percentage of the entire process
//             : (state == FryState.Ready ? 1f : 0f); //1->100%, 0->not start

//     private void Start()
//     {
//         SetState(FryState.Empty);
//     }

//     private void Update()
//     {
//         // Frying process update
//         if (state == FryState.Frying)
//         {
//             fryTimer += Time.deltaTime;
//             if (fryTimer >= fryTimeSeconds)
//             {
//                 SetState(FryState.Ready);
//                 OnReady();
//             }
//             return;
//         }

//         // Burn process update (only when Ready)
//         if (state == FryState.Ready)
//         {
//             readyTimer += Time.deltaTime;
//             if (readyTimer >= burnAfterReadySeconds && !is_level3) /// only burn in levels 0-2
//             {
//                 SetState(FryState.Burnt);
//             }
//         }
//     }

//     public void SetType(FryType type)
//     {
//         // Optional: block changing while frying
//         // if (IsFrying) return;
//         currentType = type;
//     }

//     // Called on LevelGameFlow
//     public void StartFry()
//     {
//         Debug.Log($"[FryZone] StartFry on {name} id={GetInstanceID()} state={state} type={currentType}");

//         if (state != FryState.Empty)
//         {
//             Debug.LogWarning($"[FryZone] StartFry BLOCKED on {name} state={state}");
//             return;
//         }
//         fryTimer = 0f;
//         readyTimer = 0f;
//         SetState(FryState.Frying);
//     }

//     public void ClearPan()
//     {
//         SetState(FryState.Empty);
//     }

//     private void SetState(FryState newState)
//     {
//         state = newState;

//         if (state != FryState.Ready)
//             readyTimer = 0f;

//         if (overlayRenderer == null) return;

//         switch (state)
//         {
//             case FryState.Empty:
//                 overlayRenderer.enabled = false;
//                 overlayRenderer.sprite = null;
//                 break;

//             case FryState.Frying:
//                 overlayRenderer.enabled = true;
//                 overlayRenderer.sprite = fryingSprite;
//                 break;

//             case FryState.Ready:
//                 overlayRenderer.enabled = true;
//                 overlayRenderer.sprite = readySprite;
//                 break;

//             case FryState.Burnt:
//                 overlayRenderer.enabled = true;
//                 overlayRenderer.sprite = burntSprite;
//                 break;
//         }
//     }

//     private void OnReady()
//     {
//         // Tutorial hook (optional)
//         if (TutorialManager.Instance != null)
//         {
//             if (currentType == FryType.Eggplant)
//                 TutorialManager.Instance.OnEggplantFried(this);
//             else
//                 TutorialManager.Instance.OnChipsFried(this);
//         }
//     }

//     private void Collect()
//     {
//         if (currentType == FryType.Eggplant)
//         {
//             if (eggplantTray != null) eggplantTray.FillFromPan();
//         }
//         else
//         {
//             if (chipsTray != null) chipsTray.FillFromPan();
//         }
//     }

//     private void OnMouseDown()
//     {
//         // NEW: allow disabling collect by clicking this object (so collect can be only via emoji)
//         if (!collectByClickOnThisObject) return;

//         // Ready → collect and clear
//         if (state == FryState.Ready)
//         {

//             Collect();
//             ClearPan();
//             return;
//         }

//         // Burnt → clear only (discard)
//         if (state == FryState.Burnt)
//         {
//             ClearPan();
//         }
//     }

//     public void ResetFryProcess()
//     {
//         Debug.Log($"[FryZone] ResetFryProcess on {name} id={GetInstanceID()} type={currentType}");

//         fryTimer = 0f;
//         readyTimer = 0f;

//         SetState(FryState.Empty);
//     }
// }


using System.Collections;
using UnityEngine;

// One frying machine that can fry different ingredient types (Eggplant / Chips).
public class FryZoneIngredient : MonoBehaviour
{
    public enum FryType { Eggplant, Chips }

    // Added Burnt state
    private enum FryState { Empty, Frying, Ready, Burnt }

    [SerializeField] private bool is_level3 = false; // Controls burn behavior in level 3

    [Header("Type")]
    [SerializeField] private FryType currentType = FryType.Eggplant;

    [Header("Collect Settings")]
    [SerializeField] private bool collectByClickOnThisObject = true;

    [Header("Visual (optional)")]
    [SerializeField] private SpriteRenderer overlayRenderer;
    [SerializeField] private Sprite fryingSprite;
    [SerializeField] private Sprite readySprite;
    [SerializeField] private Sprite burntSprite;

    [Header("Timing")]
    [SerializeField] private float fryTimeSeconds = 5f;
    [SerializeField] private float burnAfterReadySeconds = 10f;

    [Header("Ready Shake")]
    [SerializeField] private bool shakeWhenReady = true;

    [Tooltip("Assign the emoji transform here. If empty, the script will use overlayRenderer.transform, and if that is also empty, it will shake this object.")]
    [SerializeField] private Transform readyShakeTarget;

    [Tooltip("If true, shaking will happen only when is_level3 is true.")]
    [SerializeField] private bool shakeOnlyInLevel3 = true;

    [SerializeField] private float shakeEverySeconds = 2.5f;
    [SerializeField] private float shakeDurationSeconds = 0.35f;
    [SerializeField] private float shakeAmount = 0.08f;
    [SerializeField] private float shakeSpeed = 45f;

    [Header("Output trays")]
    [SerializeField] private FriedTray eggplantTray;
    [SerializeField] private FriedTray chipsTray;

    private FryState state = FryState.Empty;

    private float fryTimer = 0f;
    private float readyTimer = 0f;

    private Coroutine readyShakeRoutine;
    private Vector3 readyShakeOriginalLocalPosition;
    private bool hasReadyShakeOriginalPosition = false;

    public FryType CurrentType => currentType;
    public bool IsFrying => state == FryState.Frying;
    public bool IsReady => state == FryState.Ready;
    public bool IsBurnt => state == FryState.Burnt;

    public float FryProgress =>
        (state == FryState.Frying && fryTimeSeconds > 0f)
            ? Mathf.Clamp01(fryTimer / fryTimeSeconds)
            : (state == FryState.Ready ? 1f : 0f);

    private void Start()
    {
        SetState(FryState.Empty);
    }

    private void Update()
    {
        if (state == FryState.Frying)
        {
            fryTimer += Time.deltaTime;

            if (fryTimer >= fryTimeSeconds)
            {
                SetState(FryState.Ready);
                OnReady();
            }

            return;
        }

        if (state == FryState.Ready)
        {
            readyTimer += Time.deltaTime;

            if (readyTimer >= burnAfterReadySeconds && !is_level3)
            {
                SetState(FryState.Burnt);
            }
        }
    }

    private void OnDisable()
    {
        StopReadyShake();
    }

    public void SetType(FryType type)
    {
        currentType = type;
    }

    // Called from LevelGameFlow
    public void StartFry()
    {
        Debug.Log($"[FryZone] StartFry on {name} id={GetInstanceID()} state={state} type={currentType}");

        if (state != FryState.Empty)
        {
            Debug.LogWarning($"[FryZone] StartFry BLOCKED on {name} state={state}");
            return;
        }

        fryTimer = 0f;
        readyTimer = 0f;

        SetState(FryState.Frying);
    }

    public void ClearPan()
    {
        SetState(FryState.Empty);
    }

    private void SetState(FryState newState)
    {
        state = newState;

        if (state != FryState.Ready)
        {
            readyTimer = 0f;
            StopReadyShake();
        }

        UpdateVisualByState();

        if (state == FryState.Ready)
        {
            StartReadyShake();
        }
    }

    private void UpdateVisualByState()
    {
        if (overlayRenderer == null)
        {
            return;
        }

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

            case FryState.Burnt:
                overlayRenderer.enabled = true;
                overlayRenderer.sprite = burntSprite;
                break;
        }
    }

    private void OnReady()
    {
        if (TutorialManager.Instance != null)
        {
            if (currentType == FryType.Eggplant)
            {
                TutorialManager.Instance.OnEggplantFried(this);
            }
            else
            {
                TutorialManager.Instance.OnChipsFried(this);
            }
        }
    }

    private void Collect()
    {
        if (currentType == FryType.Eggplant)
        {
            if (eggplantTray != null)
            {
                eggplantTray.FillFromPan();
            }
        }
        else
        {
            if (chipsTray != null)
            {
                chipsTray.FillFromPan();
            }
        }
    }

    private void OnMouseDown()
    {
        if (!collectByClickOnThisObject)
        {
            return;
        }

        if (state == FryState.Ready)
        {
            Collect();
            ClearPan();
            return;
        }

        if (state == FryState.Burnt)
        {
            ClearPan();
        }
    }

    public void ResetFryProcess()
    {
        Debug.Log($"[FryZone] ResetFryProcess on {name} id={GetInstanceID()} type={currentType}");

        fryTimer = 0f;
        readyTimer = 0f;

        SetState(FryState.Empty);
    }

    private void StartReadyShake()
    {
        if (!shakeWhenReady)
        {
            return;
        }

        if (shakeOnlyInLevel3 && !is_level3)
        {
            return;
        }

        Transform target = GetReadyShakeTarget();

        if (target == null)
        {
            return;
        }

        if (!hasReadyShakeOriginalPosition)
        {
            readyShakeOriginalLocalPosition = target.localPosition;
            hasReadyShakeOriginalPosition = true;
        }

        if (readyShakeRoutine != null)
        {
            StopCoroutine(readyShakeRoutine);
        }

        readyShakeRoutine = StartCoroutine(ReadyShakeLoop());
    }

    private void StopReadyShake()
    {
        if (readyShakeRoutine != null)
        {
            StopCoroutine(readyShakeRoutine);
            readyShakeRoutine = null;
        }

        RestoreReadyShakePosition();
    }

    private IEnumerator ReadyShakeLoop()
    {
        while (state == FryState.Ready)
        {
            yield return ShakeOnce();

            float timer = 0f;

            while (state == FryState.Ready && timer < shakeEverySeconds)
            {
                timer += Time.deltaTime;
                yield return null;
            }
        }

        RestoreReadyShakePosition();
        readyShakeRoutine = null;
    }

    private IEnumerator ShakeOnce()
    {
        Transform target = GetReadyShakeTarget();

        if (target == null)
        {
            yield break;
        }

        if (!hasReadyShakeOriginalPosition)
        {
            readyShakeOriginalLocalPosition = target.localPosition;
            hasReadyShakeOriginalPosition = true;
        }

        float timer = 0f;

        while (state == FryState.Ready && timer < shakeDurationSeconds)
        {
            timer += Time.deltaTime;

            float offsetX = Mathf.Sin(timer * shakeSpeed) * shakeAmount;
            target.localPosition = readyShakeOriginalLocalPosition + new Vector3(offsetX, 0f, 0f);

            yield return null;
        }

        RestoreReadyShakePosition();
    }

    private void RestoreReadyShakePosition()
    {
        Transform target = GetReadyShakeTarget();

        if (target == null || !hasReadyShakeOriginalPosition)
        {
            return;
        }

        target.localPosition = readyShakeOriginalLocalPosition;
    }

    private Transform GetReadyShakeTarget()
    {
        if (readyShakeTarget != null)
        {
            return readyShakeTarget;
        }

        if (overlayRenderer != null)
        {
            return overlayRenderer.transform;
        }

        return transform;
    }
}
