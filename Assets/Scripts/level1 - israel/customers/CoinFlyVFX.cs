using System.Collections;
using UnityEngine;

/// <summary>
/// Spawns small UI coins that fly from a WORLD position (customer)
/// to a UI target (the big coin icon).
/// Works with Screen Space - Camera canvas.
/// </summary>
public class CoinFlyVFX : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Canvas uiCanvas;                 // Your Screen Space - Camera canvas
    [SerializeField] private RectTransform targetCoinUI;      // The big coin icon (destination)
    [SerializeField] private RectTransform spawnParent;       // Where flying coins will be spawned under (FlyingCoinsLayer)

    [Header("Prefab")]
    [SerializeField] private RectTransform flyingCoinPrefab;  // UI Image prefab (RectTransform)

    [Header("Animation Settings")]
    [SerializeField] private int coinsPerReward = 5;          // How many coins to spawn per reward event
    [SerializeField] private float duration = 0.6f;           // Flight duration
    [SerializeField] private float spreadPixels = 60f;        // Random start spread around the customer (screen pixels)
    [SerializeField] private float arcPixels = 120f;          // Arc height in pixels (Bezier control point)
    [SerializeField] private float endScale = 0.6f;           // Scale at the end (small "go into" coin)
    [SerializeField] private float endPunch = 0.08f;          // Optional small punch at the end (UI feel)
    int indexreword = 5;

    private Coroutine punchRoutine;


    /// <summary>
    /// Call this when you add money.
    /// worldFrom = customer transform (world position).
    /// </summary>
    public void PlayCoinsFromWorld(Transform worldFrom)
    {
        if (uiCanvas == null || targetCoinUI == null || spawnParent == null || flyingCoinPrefab == null)
        {
            Debug.LogWarning("CoinFlyVFX: Missing references in Inspector.");
            return;
        }

        if (worldFrom == null)
        {
            Debug.LogWarning("CoinFlyVFX: worldFrom is null.");
            return;
        }

        // Spawn multiple flying coins for a nicer effect
        for (int i = 0; i < coinsPerReward; i++)
        {
            StartCoroutine(SpawnAndFlyOneCoin(worldFrom.position));
        }
    }

    /// <summary>
    /// Spawns one coin and animates it from world position to UI target.
    /// </summary>
    private IEnumerator SpawnAndFlyOneCoin(Vector3 worldPos)
    {
        // 1) Convert world position (customer) -> screen point (pixels)
        Camera cam = uiCanvas.worldCamera != null ? uiCanvas.worldCamera : Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("CoinFlyVFX: No camera found (canvas.worldCamera and Camera.main are null).");
            yield break;
        }

        Vector2 startScreen = cam.WorldToScreenPoint(worldPos);

        // Add a small random spread so coins don't stack perfectly
        startScreen += Random.insideUnitCircle * spreadPixels;

        // 2) Convert screen point -> local position inside the canvas (or spawnParent)
        RectTransform canvasRect = uiCanvas.GetComponent<RectTransform>();

        // We will place the coin under spawnParent, so convert into spawnParent local space
        Vector2 startLocal;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                spawnParent, startScreen, cam, out startLocal))
        {
            yield break;
        }

        // 3) Instantiate the coin under spawnParent
        RectTransform coin = Instantiate(flyingCoinPrefab, spawnParent);
        coin.anchoredPosition = startLocal;
        coin.localScale = Vector3.one;

        // Optional: fade control
        CanvasGroup cg = coin.GetComponent<CanvasGroup>();
        if (cg != null) cg.alpha = 1f;

        // 4) Compute target position in the SAME local space (spawnParent)
        Vector2 targetLocal;
        Vector2 targetScreen = RectTransformUtility.WorldToScreenPoint(cam, targetCoinUI.position);

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                spawnParent, targetScreen, cam, out targetLocal))
        {
            Destroy(coin.gameObject);
            yield break;
        }

        // 5) Bezier arc control point (mid point + arc up)
        Vector2 mid = (startLocal + targetLocal) * 0.5f;
        mid += Vector2.up * (arcPixels + Random.Range(-20f, 20f)); // Slight randomness

        // 6) Animate over time
        float t = 0f;
        Vector3 startScale = Vector3.one;

        // Small random delay so all coins don't move perfectly together
        float randomDelay = Random.Range(0f, 0.08f);
        if (randomDelay > 0f) yield return new WaitForSeconds(randomDelay);

        while (t < duration)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / duration);

            // Smooth step for nicer motion
            float s = u * u * (3f - 2f * u);

            // Quadratic Bezier: B(s) = (1-s)^2 * A + 2(1-s)s * M + s^2 * B
            Vector2 p = Bezier2(startLocal, mid, targetLocal, s);
            coin.anchoredPosition = p;

            // Scale down towards end
            coin.localScale = Vector3.Lerp(startScale, Vector3.one * endScale, s);

            // Optional fade near the end
            if (cg != null && s > 0.7f)
            {
                float fadeT = Mathf.InverseLerp(0.7f, 1f, s);
                cg.alpha = Mathf.Lerp(1f, 0f, fadeT);
            }

            yield return null;
        }

        // Snap to target at end
        coin.anchoredPosition = targetLocal;

        // Optional: small "punch" on the target coin (visual feedback)
        // NOTE: This is a lightweight punch without tween libs.
        if (endPunch > 0f)
        {
            if (indexreword == 1)
            {
                StartCoroutine(PunchTargetCoin());
                indexreword = 5;
            }
            else
            {
                indexreword--;
            }
        }

        Destroy(coin.gameObject);
    }


    private IEnumerator PunchTargetCoin()
    {
        // NOTE: If multiple coins arrive, we may start this coroutine many times.
        // We ensure only one punch runs at a time (handled where we start it).

        Vector3 original = targetCoinUI.localScale;            // Current scale when punch starts
        Vector3 bigger = original * (1f + endPunch);           // Target bigger scale

        float punchTime = 0.12f;
        float half = punchTime * 0.5f;

        // Scale up
        float t = 0f;
        while (t < half)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / half);
            targetCoinUI.localScale = Vector3.Lerp(original, bigger, u);
            yield return null;
        }

        // Scale back
        t = 0f;
        while (t < half)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / half);
            targetCoinUI.localScale = Vector3.Lerp(bigger, original, u);
            yield return null;
        }

        // Always restore exact original scale at the end
        targetCoinUI.localScale = original;

        // Mark punch as finished
        punchRoutine = null;
    }


    /// <summary>
    /// Quadratic Bezier helper.
    /// </summary>
    private Vector2 Bezier2(Vector2 a, Vector2 m, Vector2 b, float t)
    {
        float it = 1f - t;
        return it * it * a + 2f * it * t * m + t * t * b;
    }
}
