using UnityEngine;

/// <summary>
/// Up-down bob that plays nicely with an external TutorialManager that moves the arrow.
/// It adds only a vertical offset and keeps the manager-controlled base position.
/// </summary>
public class TutorialArrowBobWorld : MonoBehaviour
{
    [SerializeField] private float amplitude = 0.15f; // world units
    [SerializeField] private float speed = 2f;

    private Vector3 basePos;       // position set by manager (without bob offset)
    private Vector3 lastOffset;    // last applied offset (so we can remove it)
    private bool hasBase;

    private void OnEnable()
    {
        // Assume current position is base when enabled
        basePos = transform.position;
        lastOffset = Vector3.zero;
        hasBase = true;
    }

    private void LateUpdate()
    {
        if (!hasBase) return;

        // If the manager moved us since last frame, detect it and refresh basePos.
        // We do this by removing the lastOffset from the current transform position.
        // If manager moved, this "clean" position will change accordingly.
        Vector3 currentCleanPos = transform.position - lastOffset;
        basePos = currentCleanPos;

        float t = Time.time;
        float y = Mathf.Sin(t * Mathf.PI * 2f * speed) * amplitude;
        Vector3 offset = new Vector3(0f, y, 0f);

        transform.position = basePos + offset;
        lastOffset = offset;
    }
}
