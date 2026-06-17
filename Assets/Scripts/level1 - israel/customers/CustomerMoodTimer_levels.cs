// using System;
// using UnityEngine;
// using System.Collections;

// /// <summary>
// /// Manages the mood timer for a customer, handling happy and angry states.
// /// The seconds per mood stage can be changed at runtime from the control panel.
// /// </summary>
// public class CustomerMoodTimer_levels : MonoBehaviour
// {
//     public event Action<bool> OnCustomerFinished; // bool served (true = served, false = angry left)

//     private const float DEFAULT_SECONDS_PER_STAGE = 7f;
//     private const float MIN_SECONDS_PER_STAGE = 7f;
//     private const float MAX_SECONDS_PER_STAGE = 12f;

//     private static float runtimeSecondsPerStage = DEFAULT_SECONDS_PER_STAGE;

//     public static float RuntimeSecondsPerStage => runtimeSecondsPerStage;

//     private Sprite happySprite;
//     private Sprite[] angryStages;

//     [SerializeField] private SpriteRenderer spriteRenderer; // To display the customer's face
//     [SerializeField] private Sprite allergyFace;

//     [Header("Mood Timing")]
//     [SerializeField] private float secondsPerStage = DEFAULT_SECONDS_PER_STAGE; // Time before switching to each angry face
//     [SerializeField] private float maxAngryWait = 3f; // Time to wait after the last angry face before leaving

//     private bool isDone;
//     private bool wasServed;

//     private Coroutine routine;

//     private void Awake()
//     {
//         if (spriteRenderer == null)
//             spriteRenderer = GetComponent<SpriteRenderer>();
//     }

//     private void Start()
//     {
//         if (spriteRenderer == null)
//             spriteRenderer = GetComponent<SpriteRenderer>();
//     }

//     /// <summary>
//     /// Updates the global runtime seconds per stage.
//     /// This affects only customers that are initialized after saving the setting.
//     /// </summary>
//     public static void SetRuntimeSecondsPerStage(float value)
//     {
//         runtimeSecondsPerStage = Mathf.Clamp(Mathf.Round(value), MIN_SECONDS_PER_STAGE, MAX_SECONDS_PER_STAGE);
//         Debug.Log($"[CustomerMoodTimer_levels] Runtime seconds per stage set to {runtimeSecondsPerStage}");
//     }

//     /// <summary>
//     /// Resets the global runtime seconds per stage back to the default value.
//     /// </summary>
//     public static void ResetRuntimeSecondsPerStage()
//     {
//         runtimeSecondsPerStage = DEFAULT_SECONDS_PER_STAGE;
//         Debug.Log($"[CustomerMoodTimer_levels] Runtime seconds per stage reset to {runtimeSecondsPerStage}");
//     }

//     /// <summary>
//     /// Returns the real total time from the moment the mood timer starts
//     /// until the customer leaves angrily.
//     ///
//     /// Formula:
//     /// secondsPerStage * angryFacesCount + maxAngryWait
//     ///
//     /// Examples:
//     /// Normal customer:
//     /// 2 angry faces, secondsPerStage = 7, maxAngryWait = 3
//     /// 7 * 2 + 3 = 17 seconds
//     ///
//     /// Forbidden/gluten customer:
//     /// 1 angry face, secondsPerStage = 7, maxAngryWait = 3
//     /// 7 * 1 + 3 = 10 seconds
//     /// </summary>
//     public float GetTotalAngerDurationSeconds()
//     {
//         int angryStagesCount = angryStages != null ? angryStages.Length : 0;

//         if (angryStagesCount <= 0)
//             return maxAngryWait;

//         return (secondsPerStage * angryStagesCount) + maxAngryWait;
//     }

//     /// <summary>
//     /// Returns the total number of mood faces, including the happy face.
//     /// </summary>
//     public int GetTotalMoodFacesCount()
//     {
//         int angryStagesCount = angryStages != null ? angryStages.Length : 0;
//         return 1 + angryStagesCount;
//     }

//     /// <summary>
//     /// Sets the sprite of the customer.
//     /// </summary>
//     private void SetSprite(Sprite s)
//     {
//         if (spriteRenderer != null && s != null)
//             spriteRenderer.sprite = s;
//     }

//     /// <summary>
//     /// Runs the mood timer, transitioning through angry stages and eventually timing out.
//     /// </summary>
//     private IEnumerator Run()
//     {
//         if (angryStages == null || angryStages.Length == 0)
//             yield break;

//         for (int i = 0; i < angryStages.Length; i++)
//         {
//             float t = 0f;

//             while (t < secondsPerStage)
//             {
//                 if (isDone)
//                     yield break;

//                 t += Time.deltaTime;
//                 yield return null;
//             }

//             if (isDone)
//                 yield break;

//             SetSprite(angryStages[i]);
//         }

//         float w = 0f;

//         while (w < maxAngryWait)
//         {
//             if (isDone)
//                 yield break;

//             w += Time.deltaTime;
//             yield return null;
//         }

//         isDone = true;
//         OnCustomerFinished?.Invoke(false); // Customer left angrily
//     }

//     /// <summary>
//     /// Marks the customer as served, stopping the mood timer.
//     /// </summary>
//     public void CustomerServed()
//     {
//         if (isDone)
//             return;

//         wasServed = true;
//         isDone = true;

//         SetSprite(happySprite);
//         OnCustomerFinished?.Invoke(true);
//     }

//     /// <summary>
//     /// Forces the customer to show an angry face immediately.
//     /// Used for wrong order feedback or forbidden customer feedback.
//     /// </summary>
//     public void ShowAngryNow(Customer target = null)
//     {
//         if (target == null || target.Data == null)
//             return;

//         if (!target.Data.scoreIfNotServed)
//         {
//             if (spriteRenderer == null)
//                 spriteRenderer = GetComponent<SpriteRenderer>();

//             if (angryStages == null || angryStages.Length == 0)
//                 return;

//             SetSprite(angryStages[0]);
//         }
//         else
//         {
//             if (allergyFace != null)
//             {
//                 SetSprite(allergyFace);
//             }
//             else
//             {
//                 if (angryStages == null || angryStages.Length == 0)
//                     return;

//                 SetSprite(angryStages[0]);
//             }
//         }
//     }

//     /// <summary>
//     /// Resets the mood timer to the initial happy state.
//     /// </summary>
//     public void ResetTimer()
//     {
//         if (routine != null)
//             StopCoroutine(routine);

//         isDone = false;
//         wasServed = false;

//         SetSprite(happySprite);
//         routine = StartCoroutine(Run());
//     }

//     /// <summary>
//     /// Configures the mood timer with happy and angry sprites.
//     /// </summary>
//     public void Configure(Sprite happy, Sprite[] angry)
//     {
//         if (angry == null || angry.Length == 0)
//             Debug.LogWarning("No angry faces assigned for this customer type!");

//         happySprite = happy;
//         angryStages = angry;

//         // Apply the current control-panel value.
//         // This affects only the customer currently being initialized.
//         secondsPerStage = runtimeSecondsPerStage;

//         ResetTimer();
//     }

//     public void SetRenderer(SpriteRenderer r)
//     {
//         spriteRenderer = r;
//     }
// }


using System;
using UnityEngine;
using System.Collections;

/// <summary>
/// Manages the mood timer for a customer, handling happy and angry states.
/// The seconds per mood stage can be changed at runtime from the control panel.
/// If the customer patience timer is disabled from the control panel,
/// the customer will not progress through anger stages and will not leave automatically.
/// </summary>
public class CustomerMoodTimer_levels : MonoBehaviour
{
    public event Action<bool> OnCustomerFinished; // bool served (true = served, false = angry left)

    private const float DEFAULT_SECONDS_PER_STAGE = 7f;
    private const float MIN_SECONDS_PER_STAGE = 7f;
    private const float MAX_SECONDS_PER_STAGE = 20f;

    private static float runtimeSecondsPerStage = DEFAULT_SECONDS_PER_STAGE;

    public static float RuntimeSecondsPerStage => runtimeSecondsPerStage;

    private Sprite happySprite;
    private Sprite[] angryStages;

    [SerializeField] private SpriteRenderer spriteRenderer; // To display the customer's face
    [SerializeField] private Sprite allergyFace;

    [Header("Mood Timing")]
    [SerializeField] private float secondsPerStage = DEFAULT_SECONDS_PER_STAGE; // Time before switching to each angry face
    [SerializeField] private float maxAngryWait = 3f; // Time to wait after the last angry face before leaving

    private bool isDone;
    private bool wasServed;

    private Coroutine routine;

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    private void Start()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    /// <summary>
    /// Updates the global runtime seconds per stage.
    /// This affects only customers that are initialized after saving the setting.
    /// </summary>
    public static void SetRuntimeSecondsPerStage(float value)
    {
        runtimeSecondsPerStage = Mathf.Clamp(
            Mathf.Round(value),
            MIN_SECONDS_PER_STAGE,
            MAX_SECONDS_PER_STAGE
        );

        Debug.Log($"[CustomerMoodTimer_levels] Runtime seconds per stage set to {runtimeSecondsPerStage}");
    }

    /// <summary>
    /// Resets the global runtime seconds per stage back to the default value.
    /// </summary>
    public static void ResetRuntimeSecondsPerStage()
    {
        runtimeSecondsPerStage = DEFAULT_SECONDS_PER_STAGE;
        Debug.Log($"[CustomerMoodTimer_levels] Runtime seconds per stage reset to {runtimeSecondsPerStage}");
    }

    /// <summary>
    /// Returns the real total time from the moment the mood timer starts
    /// until the customer leaves angrily.
    ///
    /// Formula:
    /// secondsPerStage * angryFacesCount + maxAngryWait
    ///
    /// If the customer patience timer is disabled, this value is theoretical only,
    /// because the timer will pause and the customer will not leave automatically.
    /// </summary>
    public float GetTotalAngerDurationSeconds()
    {
        int angryStagesCount = angryStages != null ? angryStages.Length : 0;

        if (angryStagesCount <= 0)
        {
            return maxAngryWait;
        }

        return (secondsPerStage * angryStagesCount) + maxAngryWait;
    }

    /// <summary>
    /// Returns the total number of mood faces, including the happy face.
    /// </summary>
    public int GetTotalMoodFacesCount()
    {
        int angryStagesCount = angryStages != null ? angryStages.Length : 0;
        return 1 + angryStagesCount;
    }

    /// <summary>
    /// Sets the sprite of the customer.
    /// </summary>
    private void SetSprite(Sprite s)
    {
        if (spriteRenderer != null && s != null)
        {
            spriteRenderer.sprite = s;
        }
    }

    /// <summary>
    /// Returns true when the customer patience timer should currently run.
    /// When false, the customer stays in the same mood state and does not leave automatically.
    /// </summary>
    private bool ShouldMoodTimerRun()
    {
        return ControlPanelUI.CustomerPatienceTimerEnabled;
    }

    /// <summary>
    /// Runs the mood timer, transitioning through angry stages and eventually timing out.
    /// If the timer is disabled from the control panel, this coroutine pauses in place.
    /// </summary>
    private IEnumerator Run()
    {
        if (angryStages == null || angryStages.Length == 0)
        {
            yield break;
        }

        for (int i = 0; i < angryStages.Length; i++)
        {
            float t = 0f;

            while (t < secondsPerStage)
            {
                if (isDone)
                {
                    yield break;
                }

                if (!ShouldMoodTimerRun())
                {
                    yield return null;
                    continue;
                }

                t += Time.deltaTime;
                yield return null;
            }

            if (isDone)
            {
                yield break;
            }

            SetSprite(angryStages[i]);
        }

        float w = 0f;

        while (w < maxAngryWait)
        {
            if (isDone)
            {
                yield break;
            }

            if (!ShouldMoodTimerRun())
            {
                yield return null;
                continue;
            }

            w += Time.deltaTime;
            yield return null;
        }

        isDone = true;
        OnCustomerFinished?.Invoke(false); // Customer left angrily
    }

    /// <summary>
    /// Marks the customer as served, stopping the mood timer.
    /// This still works even when the patience timer is disabled.
    /// </summary>
    public void CustomerServed()
    {
        if (isDone)
        {
            return;
        }

        wasServed = true;
        isDone = true;

        SetSprite(happySprite);
        OnCustomerFinished?.Invoke(true);
    }

    /// <summary>
    /// Forces the customer to show an angry face immediately.
    /// Used for wrong order feedback or forbidden customer feedback.
    /// </summary>
    public void ShowAngryNow(Customer target = null)
    {
        if (target == null || target.Data == null)
        {
            return;
        }

        if (!target.Data.scoreIfNotServed)
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (angryStages == null || angryStages.Length == 0)
            {
                return;
            }

            SetSprite(angryStages[0]);
        }
        else
        {
            if (allergyFace != null)
            {
                SetSprite(allergyFace);
            }
            else
            {
                if (angryStages == null || angryStages.Length == 0)
                {
                    return;
                }

                SetSprite(angryStages[0]);
            }
        }
    }

    /// <summary>
    /// Resets the mood timer to the initial happy state.
    /// </summary>
    public void ResetTimer()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
        }

        isDone = false;
        wasServed = false;

        SetSprite(happySprite);
        routine = StartCoroutine(Run());
    }

    /// <summary>
    /// Configures the mood timer with happy and angry sprites.
    /// </summary>
    public void Configure(Sprite happy, Sprite[] angry)
    {
        if (angry == null || angry.Length == 0)
        {
            Debug.LogWarning("No angry faces assigned for this customer type!");
        }

        happySprite = happy;
        angryStages = angry;

        // Apply the current control-panel value.
        // This affects only the customer currently being initialized.
        secondsPerStage = runtimeSecondsPerStage;

        ResetTimer();
    }

    public void SetRenderer(SpriteRenderer r)
    {
        spriteRenderer = r;
    }
}