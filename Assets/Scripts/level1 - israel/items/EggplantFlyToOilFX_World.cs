using System.Collections;
using UnityEngine;

/// <summary>
/// Spawns an eggplant sprite at spawnPoint and flies it in an arc into targetPoint (boiling oil).
/// World-space version using SpriteRenderer.
/// </summary>
public class EggplantFlyToOilFX_World : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform spawnPoint;     // eggplant pile (left)
    [SerializeField] private Transform targetPoint;    // fryzone / boiling oil
    [SerializeField] private Sprite eggplantSprite;    // eggplant sprite for the flying FX
    [SerializeField] private Transform worldParent;    // optional parent (e.g., Items)

    [Header("Render")]
    [SerializeField] private string sortingLayerName = "Default";
    [SerializeField] private int sortingOrder = 50;

    [Header("Motion")]
    [SerializeField] private float duration = 0.55f;
    [SerializeField] private Vector2 arcOffset = new Vector2(1.2f, 1.4f); // world units (up-right)
    [SerializeField] private float spinDegrees = -540f;

    [Header("Scale")]
    [SerializeField] private float startScale = 1f;
    [SerializeField] private float endScale = 0.25f;

    public void Play()
    {
        if (spawnPoint == null || targetPoint == null || eggplantSprite == null) return;

        GameObject go = new GameObject("FlyingEggplantFX");
        if (worldParent != null) go.transform.SetParent(worldParent, true);

        go.transform.position = spawnPoint.position;
        go.transform.rotation = Quaternion.identity;
        go.transform.localScale = Vector3.one * startScale;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = eggplantSprite;
        sr.sortingLayerName = sortingLayerName;
        sr.sortingOrder = sortingOrder;

        StartCoroutine(FlyArcSpin(go.transform));
    }

    private IEnumerator FlyArcSpin(Transform tr)
    {
        Vector3 start = tr.position;
        Vector3 end = targetPoint.position;
        Vector3 control = start + (Vector3)arcOffset;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / duration);

            float e = u * u * (3f - 2f * u); // smoothstep

            tr.position = Bezier2(start, control, end, e);

            float z = Mathf.Lerp(0f, spinDegrees, e);
            tr.rotation = Quaternion.Euler(0f, 0f, z);

            float s = Mathf.Lerp(startScale, endScale, e);
            tr.localScale = Vector3.one * s;

            yield return null;
        }

        Destroy(tr.gameObject);
    }

    private static Vector3 Bezier2(Vector3 a, Vector3 c, Vector3 b, float t)
    {
        float inv = 1f - t;
        return (inv * inv) * a + 2f * inv * t * c + (t * t) * b;
    }
}
