using System;
using UnityEngine;
using System.Collections;
/// <summary>
/// Manages the mood timer for a customer, handling happy and angry states.
/// </summary>
public class CustomerMoodTimer_levels : MonoBehaviour
{
    public event Action<bool> OnCustomerFinished; // bool served (true=served, false=angry left)

    private Sprite happySprite;
    private Sprite[] angryStages;
    [SerializeField] private SpriteRenderer spriteRenderer;// To display the customer's face

    [SerializeField] private float secondsPerStage = 7f;// Time per angry stage
    [SerializeField] private float maxAngryWait = 3f;// Time to wait after last angry stage

    private bool isDone;
    private bool wasServed;

    private Coroutine routine;

    private void Start()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
    }
    /// <summary>
    /// Sets the sprite of the customer.
    /// </summary>
    /// <param name="s"></param>
    private void SetSprite(Sprite s)
    {
        if (spriteRenderer != null && s != null) spriteRenderer.sprite = s;
    }

    /// <summary>
    /// Runs the mood timer, transitioning through angry stages and eventually timing out.
    /// </summary>
    /// <returns></returns>
    private IEnumerator Run()
    {
        if (angryStages == null || angryStages.Length == 0) yield break;

        for (int i = 0; i < angryStages.Length; i++)
        {
            float t = 0f;
            while (t < secondsPerStage)
            {
                if (isDone) yield break;
                t += Time.deltaTime;
                yield return null;
            }

            if (isDone) yield break;
            SetSprite(angryStages[i]);// Update to next angry stage
        }

        float w = 0f;
        while (w < maxAngryWait)
        {
            if (isDone) yield break;
            w += Time.deltaTime;
            yield return null;
        }

        isDone = true; // Mark as done due to timeout
        OnCustomerFinished?.Invoke(false); // Notify that customer left angrily
    }

    /// <summary>
    /// Marks the customer as served, stopping the mood timer.
    /// </summary>
    public void CustomerServed()
    {
        if (isDone) return;

        wasServed = true;
        isDone = true;

        SetSprite(happySprite);// Set to happy face
        OnCustomerFinished?.Invoke(true);
    }

    /// <summary>
    /// Forces the customer to show an angry face immediately (wrong order feedback),
    /// </summary>
    public void ShowAngryNow()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (angryStages == null || angryStages.Length == 0) return;

        // show first angry stage
        SetSprite(angryStages[0]);
    }


    /// <summary>
    /// Resets the mood timer to the initial happy state.
    /// </summary>
    public void ResetTimer()
    {
        if (routine != null) StopCoroutine(routine);
        isDone = false;
        wasServed = false;
        SetSprite(happySprite);// Start with happy face
        routine = StartCoroutine(Run());
    }

    /// <summary>
    /// Configures the mood timer with happy and angry sprites.
    /// </summary>
    public void Configure(Sprite happy, Sprite[] angry)
    {
        if (angry == null || angry.Length == 0)
            Debug.LogWarning("No angry faces assigned for this customer type!");

        happySprite = happy;
        angryStages = angry;
        ResetTimer();// Start the timer
    }

}
