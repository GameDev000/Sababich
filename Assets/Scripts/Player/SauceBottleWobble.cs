using System.Collections;
using UnityEngine;

public class SauceBottleWobble : MonoBehaviour
{
    [Header("Tilt settings")]
    [SerializeField] private float tiltAngle = 20f;      // כמה מעלות כל צד
    [SerializeField] private float wobbleDuration = 0.15f; // זמן לכל חצי תנועה
    [SerializeField] private int wobbleCycles = 2;       // כמה פעמים לזגזג

    private bool isWobbling = false;
    private Quaternion initialRotation;

    private void Awake()
    {
        initialRotation = transform.rotation;
    }

    public void Shake()
    {
        if (!isWobbling)
            StartCoroutine(WobbleRoutine());
    }

    private IEnumerator WobbleRoutine()
    {
        isWobbling = true;

        Quaternion left  = initialRotation * Quaternion.Euler(0, 0, tiltAngle);
        Quaternion right = initialRotation * Quaternion.Euler(0, 0, -tiltAngle);

        for (int i = 0; i < wobbleCycles; i++)
        {
            yield return RotateOverTime(initialRotation, left,  wobbleDuration);
            yield return RotateOverTime(left,           right, wobbleDuration * 2f);
            yield return RotateOverTime(right,          initialRotation, wobbleDuration);
        }

        transform.rotation = initialRotation;
        isWobbling = false;
    }

    private IEnumerator RotateOverTime(Quaternion from, Quaternion to, float time)
    {
        float t = 0f;
        while (t < time)
        {
            t += Time.deltaTime;
            float lerp = t / time;
            transform.rotation = Quaternion.Lerp(from, to, lerp);
            yield return null;
        }
    }
}
