using System.Collections;
using UnityEngine;

public class FaucetSpriteToggle : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private SpriteRenderer targetRenderer;
    [SerializeField] private Sprite idleSprite;
    [SerializeField] private Sprite runningWaterSprite;

    [Header("Timing")]
    [SerializeField] private float seconds = 5f;

    private Coroutine routine;

    private void Awake()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<SpriteRenderer>();

        if (idleSprite == null && targetRenderer != null)
            idleSprite = targetRenderer.sprite;
    }

    private void OnMouseDown()
    {
        if (targetRenderer == null || runningWaterSprite == null) return;

        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(RunWater());
    }

    private IEnumerator RunWater()
    {
        targetRenderer.sprite = runningWaterSprite;

        yield return new WaitForSeconds(seconds);

        targetRenderer.sprite = idleSprite;
        routine = null;
    }
}
