
// using System.Collections;
// using UnityEngine;

// // One frying machine that can fry different ingredient types (Eggplant / Chips).
// public class FryZoneIngredient : MonoBehaviour
// {
//     public enum FryType { Eggplant, Chips }

//     private enum FryState { Empty, Frying, Ready, Burnt }

//     [SerializeField] private bool is_level3 = false; // Controls burn behavior in level 3

//     [Header("Type")]
//     [SerializeField] private FryType currentType = FryType.Eggplant;

//     [Header("Collect Settings")]
//     [SerializeField] private bool collectByClickOnThisObject = true;

//     [Header("Visual (optional)")]
//     [SerializeField] private SpriteRenderer overlayRenderer;
//     [SerializeField] private Sprite fryingSprite;
//     [SerializeField] private Sprite readySprite;
//     [SerializeField] private Sprite burntSprite;

//     [Header("Timing")]
//     [SerializeField] private float fryTimeSeconds = 5f;
//     [SerializeField] private float burnAfterReadySeconds = 10f;

//     [Header("Frying Timer Audio")]
//     [SerializeField] private AudioSource fryingTimerAudioSource;
//     [SerializeField] private AudioClip tickingClockClip;
//     [SerializeField] private AudioClip timerFinishedClip;
//     [SerializeField, Range(0f, 1f)] private float tickingClockVolume = 1f;
//     [SerializeField, Range(0f, 1f)] private float timerFinishedVolume = 1f;

//     [Header("Ready Shake")]
//     [SerializeField] private bool shakeWhenReady = true;

//     [Tooltip("Assign the emoji transform here. If empty, the script will use overlayRenderer.transform, and if that is also empty, it will shake this object.")]
//     [SerializeField] private Transform readyShakeTarget;

//     [Tooltip("If true, shaking will happen only when is_level3 is true.")]
//     [SerializeField] private bool shakeOnlyInLevel3 = true;

//     [SerializeField] private float shakeEverySeconds = 2.5f;
//     [SerializeField] private float shakeDurationSeconds = 0.35f;
//     [SerializeField] private float shakeAmount = 0.08f;
//     [SerializeField] private float shakeSpeed = 45f;

//     [Header("Output trays")]
//     [SerializeField] private FriedTray eggplantTray;
//     [SerializeField] private FriedTray chipsTray;

//     private FryState state = FryState.Empty;

//     private float fryTimer = 0f;
//     private float readyTimer = 0f;

//     private Coroutine readyShakeRoutine;
//     private Vector3 readyShakeOriginalLocalPosition;
//     private bool hasReadyShakeOriginalPosition = false;

//     public FryType CurrentType => currentType;
//     public bool IsFrying => state == FryState.Frying;
//     public bool IsReady => state == FryState.Ready;
//     public bool IsBurnt => state == FryState.Burnt;

//     public float FryProgress =>
//         (state == FryState.Frying && fryTimeSeconds > 0f)
//             ? Mathf.Clamp01(fryTimer / fryTimeSeconds)
//             : (state == FryState.Ready ? 1f : 0f);

//     private void Start()
//     {
//         SetState(FryState.Empty);
//     }

//     private void Update()
//     {
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

//         if (state == FryState.Ready)
//         {
//             readyTimer += Time.deltaTime;

//             if (readyTimer >= burnAfterReadySeconds && !is_level3)
//             {
//                 SetState(FryState.Burnt);
//             }
//         }
//     }

//     private void OnDisable()
//     {
//         StopReadyShake();
//         StopTickingClockSound();
//     }

//     public void SetType(FryType type)
//     {
//         currentType = type;
//     }

//     // Called from LevelGameFlow
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
//         FryState previousState = state;
//         state = newState;

//         if (state != FryState.Frying)
//         {
//             StopTickingClockSound();
//         }

//         if (state != FryState.Ready)
//         {
//             readyTimer = 0f;
//             StopReadyShake();
//         }

//         UpdateVisualByState();

//         if (state == FryState.Frying)
//         {
//             StartTickingClockSound();
//         }

//         if (state == FryState.Ready)
//         {
//             PlayTimerFinishedSound();
//             StartReadyShake();
//         }

//         Debug.Log($"[FryZone] State changed: {previousState} -> {state}");
//     }

//     private void UpdateVisualByState()
//     {
//         if (overlayRenderer == null)
//         {
//             return;
//         }

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

//     private void StartTickingClockSound()
//     {
//         if (fryingTimerAudioSource == null || tickingClockClip == null)
//         {
//             return;
//         }

//         fryingTimerAudioSource.Stop();
//         fryingTimerAudioSource.clip = tickingClockClip;
//         fryingTimerAudioSource.volume = tickingClockVolume;
//         fryingTimerAudioSource.loop = true;
//         fryingTimerAudioSource.Play();
//     }

//     private void StopTickingClockSound()
//     {
//         if (fryingTimerAudioSource == null)
//         {
//             return;
//         }

//         if (fryingTimerAudioSource.isPlaying && fryingTimerAudioSource.clip == tickingClockClip)
//         {
//             fryingTimerAudioSource.Stop();
//         }

//         fryingTimerAudioSource.loop = false;
//     }

//     private void PlayTimerFinishedSound()
//     {
//         if (fryingTimerAudioSource == null || timerFinishedClip == null)
//         {
//             return;
//         }

//         fryingTimerAudioSource.PlayOneShot(timerFinishedClip, timerFinishedVolume);
//     }

//     private void OnReady()
//     {
//         if (TutorialManager.Instance != null)
//         {
//             if (currentType == FryType.Eggplant)
//             {
//                 TutorialManager.Instance.OnEggplantFried(this);
//             }
//             else
//             {
//                 TutorialManager.Instance.OnChipsFried(this);
//             }
//         }
//     }

//     private void Collect()
//     {
//         if (currentType == FryType.Eggplant)
//         {
//             if (eggplantTray != null)
//             {
//                 eggplantTray.FillFromPan();
//             }
//         }
//         else
//         {
//             if (chipsTray != null)
//             {
//                 chipsTray.FillFromPan();
//             }
//         }
//     }

//     private void OnMouseDown()
//     {
//         if (!collectByClickOnThisObject)
//         {
//             return;
//         }

//         if (state == FryState.Ready)
//         {
//             Collect();
//             ClearPan();
//             return;
//         }

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

//     private void StartReadyShake()
//     {
//         if (!shakeWhenReady)
//         {
//             return;
//         }

//         if (shakeOnlyInLevel3 && !is_level3)
//         {
//             return;
//         }

//         Transform target = GetReadyShakeTarget();

//         if (target == null)
//         {
//             return;
//         }

//         if (!hasReadyShakeOriginalPosition)
//         {
//             readyShakeOriginalLocalPosition = target.localPosition;
//             hasReadyShakeOriginalPosition = true;
//         }

//         if (readyShakeRoutine != null)
//         {
//             StopCoroutine(readyShakeRoutine);
//         }

//         readyShakeRoutine = StartCoroutine(ReadyShakeLoop());
//     }

//     private void StopReadyShake()
//     {
//         if (readyShakeRoutine != null)
//         {
//             StopCoroutine(readyShakeRoutine);
//             readyShakeRoutine = null;
//         }

//         RestoreReadyShakePosition();
//     }

//     private IEnumerator ReadyShakeLoop()
//     {
//         while (state == FryState.Ready)
//         {
//             yield return ShakeOnce();

//             float timer = 0f;

//             while (state == FryState.Ready && timer < shakeEverySeconds)
//             {
//                 timer += Time.deltaTime;
//                 yield return null;
//             }
//         }

//         RestoreReadyShakePosition();
//         readyShakeRoutine = null;
//     }

//     private IEnumerator ShakeOnce()
//     {
//         Transform target = GetReadyShakeTarget();

//         if (target == null)
//         {
//             yield break;
//         }

//         if (!hasReadyShakeOriginalPosition)
//         {
//             readyShakeOriginalLocalPosition = target.localPosition;
//             hasReadyShakeOriginalPosition = true;
//         }

//         float timer = 0f;

//         while (state == FryState.Ready && timer < shakeDurationSeconds)
//         {
//             timer += Time.deltaTime;

//             float offsetX = Mathf.Sin(timer * shakeSpeed) * shakeAmount;
//             target.localPosition = readyShakeOriginalLocalPosition + new Vector3(offsetX, 0f, 0f);

//             yield return null;
//         }

//         RestoreReadyShakePosition();
//     }

//     private void RestoreReadyShakePosition()
//     {
//         Transform target = GetReadyShakeTarget();

//         if (target == null || !hasReadyShakeOriginalPosition)
//         {
//             return;
//         }

//         target.localPosition = readyShakeOriginalLocalPosition;
//     }

//     private Transform GetReadyShakeTarget()
//     {
//         if (readyShakeTarget != null)
//         {
//             return readyShakeTarget;
//         }

//         if (overlayRenderer != null)
//         {
//             return overlayRenderer.transform;
//         }

//         return transform;
//     }
// }
using System.Collections;
using UnityEngine;

// One frying machine that can fry different ingredient types (Eggplant / Chips).
public class FryZoneIngredient : MonoBehaviour
{
    public enum FryType { Eggplant, Chips }

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

    [Header("Frying Timer Audio")]
    [SerializeField] private AudioSource fryingTimerAudioSource;
    [SerializeField] private AudioClip tickingClockClip;
    [SerializeField] private AudioClip timerFinishedClip;
    [SerializeField, Range(0f, 1f)] private float tickingClockVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float timerFinishedVolume = 1f;

    [Header("First Timer Finished Voice")]
    [Tooltip("Optional. If empty, the script will try to use the Frying Timer Audio Source.")]
    [SerializeField] private AudioSource firstTimerFinishedAudioSource;

    [Tooltip("Clip that plays only the first time the eggplant timer finishes.")]
    [SerializeField] private AudioClip eggplantFirstTimerFinishedClip;

    [Tooltip("Clip that plays only the first time the chips timer finishes.")]
    [SerializeField] private AudioClip chipsFirstTimerFinishedClip;

    [SerializeField, Range(0f, 5f)] private float firstTimerFinishedVolume = 1f;

    [Header("Burn Warning Audio")]
    [Tooltip("Optional. If empty, the script will try to use the Frying Timer Audio Source.")]
    [SerializeField] private AudioSource burnWarningAudioSource;

    [Tooltip("Clip that plays before the eggplant becomes burnt.")]
    [SerializeField] private AudioClip eggplantBurnWarningClip;

    [Tooltip("How many seconds before the eggplant burns the warning clip should play.")]
    [SerializeField, Min(0f)] private float secondsBeforeBurntWarning = 3f;

    [SerializeField, Range(0f, 2f)] private float burnWarningVolume = 2f;

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

    private bool burnWarningPlayed = false;

    private bool eggplantFirstTimerFinishedClipPlayed = false;
    private bool chipsFirstTimerFinishedClipPlayed = false;

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

            if (!is_level3)
            {
                TryPlayEggplantBurnWarning();

                if (readyTimer >= burnAfterReadySeconds)
                {
                    SetState(FryState.Burnt);
                }
            }
        }
    }

    private void OnDisable()
    {
        StopReadyShake();
        StopTickingClockSound();
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
        burnWarningPlayed = false;

        SetState(FryState.Frying);
    }

    public void ClearPan()
    {
        SetState(FryState.Empty);
    }

    private void SetState(FryState newState)
    {
        FryState previousState = state;
        state = newState;

        if (state != FryState.Frying)
        {
            StopTickingClockSound();
        }

        if (state != FryState.Ready)
        {
            readyTimer = 0f;
            burnWarningPlayed = false;
            StopReadyShake();
        }

        if (state == FryState.Ready)
        {
            readyTimer = 0f;
            burnWarningPlayed = false;
        }

        UpdateVisualByState();

        if (state == FryState.Frying)
        {
            StartTickingClockSound();
        }

        if (state == FryState.Ready)
        {
            PlayTimerFinishedSound();
            PlayFirstTimerFinishedClipIfNeeded();
            StartReadyShake();
        }

        Debug.Log($"[FryZone] State changed: {previousState} -> {state}");
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

    private void StartTickingClockSound()
    {
        if (fryingTimerAudioSource == null || tickingClockClip == null)
        {
            return;
        }

        fryingTimerAudioSource.Stop();
        fryingTimerAudioSource.clip = tickingClockClip;
        fryingTimerAudioSource.volume = tickingClockVolume;
        fryingTimerAudioSource.loop = true;
        fryingTimerAudioSource.Play();
    }

    private void StopTickingClockSound()
    {
        if (fryingTimerAudioSource == null)
        {
            return;
        }

        if (fryingTimerAudioSource.isPlaying && fryingTimerAudioSource.clip == tickingClockClip)
        {
            fryingTimerAudioSource.Stop();
        }

        fryingTimerAudioSource.loop = false;
    }

    private void PlayTimerFinishedSound()
    {
        if (fryingTimerAudioSource == null || timerFinishedClip == null)
        {
            return;
        }

        fryingTimerAudioSource.PlayOneShot(timerFinishedClip, timerFinishedVolume);
    }

    private void PlayFirstTimerFinishedClipIfNeeded()
    {
        AudioSource sourceToUse = firstTimerFinishedAudioSource;

        if (sourceToUse == null)
        {
            sourceToUse = fryingTimerAudioSource;
        }

        if (sourceToUse == null)
        {
            return;
        }

        if (currentType == FryType.Eggplant)
        {
            if (eggplantFirstTimerFinishedClipPlayed)
            {
                return;
            }

            eggplantFirstTimerFinishedClipPlayed = true;

            if (eggplantFirstTimerFinishedClip == null)
            {
                return;
            }

            sourceToUse.PlayOneShot(eggplantFirstTimerFinishedClip, firstTimerFinishedVolume);
            return;
        }

        if (currentType == FryType.Chips)
        {
            if (chipsFirstTimerFinishedClipPlayed)
            {
                return;
            }

            chipsFirstTimerFinishedClipPlayed = true;

            if (chipsFirstTimerFinishedClip == null)
            {
                return;
            }

            sourceToUse.PlayOneShot(chipsFirstTimerFinishedClip, firstTimerFinishedVolume);
        }
    }

    private void TryPlayEggplantBurnWarning()
    {
        if (burnWarningPlayed)
        {
            return;
        }

        if (currentType != FryType.Eggplant)
        {
            return;
        }

        if (burnAfterReadySeconds <= 0f)
        {
            return;
        }

        float warningTime = burnAfterReadySeconds - secondsBeforeBurntWarning;

        if (warningTime < 0f)
        {
            warningTime = 0f;
        }

        if (readyTimer < warningTime)
        {
            return;
        }

        burnWarningPlayed = true;

        AudioSource sourceToUse = burnWarningAudioSource;

        if (sourceToUse == null)
        {
            sourceToUse = fryingTimerAudioSource;
        }

        if (sourceToUse == null || eggplantBurnWarningClip == null)
        {
            return;
        }

        sourceToUse.PlayOneShot(eggplantBurnWarningClip, burnWarningVolume);
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
        burnWarningPlayed = false;

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