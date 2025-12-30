using System.Collections;
using UnityEngine;
/// <summary>
/// Handles mouse click events on the faucet to toggle its sprite for running water effect.
/// </summary>
public class FaucetSpriteToggle : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private SpriteRenderer targetRenderer;// The SpriteRenderer to change sprites on
    [SerializeField] private Sprite idleSprite;// The sprite to use when faucet is idle
    [SerializeField] private Sprite runningWaterSprite;// The sprite to use when water is running

    [Header("Timing")]
    [SerializeField] private float seconds = 5f;// Duration to show running water sprite

    private Coroutine routine;

    private void Awake()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<SpriteRenderer>();// Try to get the SpriteRenderer if not assigned

        if (idleSprite == null && targetRenderer != null)
            idleSprite = targetRenderer.sprite;
    }
    /// <summary>
    /// Handles mouse click to start the water running effect.
    /// </summary>
    private void OnMouseDown()
    {
        if (targetRenderer == null || runningWaterSprite == null) return;

        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(RunWater());// Start the water running effect
    }
    /// <summary>
    /// Coroutine to switch the faucet sprite to running water and back after a delay.
    /// </summary>
    /// <returns></returns>
    private IEnumerator RunWater()
    {
        targetRenderer.sprite = runningWaterSprite;// Set to running water sprite

        yield return new WaitForSeconds(seconds);// Wait for specified duration

        targetRenderer.sprite = idleSprite;// Revert to idle sprite
        routine = null;
    }
}
