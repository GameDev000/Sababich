using System;
using UnityEngine;
using System.Collections;

public class CustomerMoodTimer_levels : MonoBehaviour
{
    public event Action<bool> OnCustomerFinished; // bool served (true=served, false=angry left)

    private Sprite happySprite;
    private Sprite[] angryStages;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [SerializeField] private float secondsPerStage = 7f;
    [SerializeField] private float maxAngryWait = 3f;

    private bool isDone;
    private bool wasServed;

    private Coroutine routine;

    private void Start()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void SetSprite(Sprite s)
    {
        if (spriteRenderer != null && s != null) spriteRenderer.sprite = s;
    }

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
            SetSprite(angryStages[i]);
        }

        float w = 0f;
        while (w < maxAngryWait)
        {
            if (isDone) yield break;
            w += Time.deltaTime;
            yield return null;
        }

        // לא הוגש בזמן
        isDone = true;
        OnCustomerFinished?.Invoke(false);
    }

    public void CustomerServed()
    {
        if (isDone) return;

        wasServed = true;
        isDone = true;

        SetSprite(happySprite);
        OnCustomerFinished?.Invoke(true);
    }

    public void ResetTimer()
    {
        if (routine != null) StopCoroutine(routine);
        isDone = false;
        wasServed = false;
        SetSprite(happySprite);
        routine = StartCoroutine(Run());
    }

    public void Configure(Sprite happy, Sprite[] angry)
    {
        if (angry == null || angry.Length == 0)
            Debug.LogWarning("No angry faces assigned for this customer type!");

        happySprite = happy;
        angryStages = angry;
        ResetTimer();
    }

}
