using UnityEngine;

public class RotateAfterDelay : MonoBehaviour
{
    [SerializeField] public float delay = 2f;
    [SerializeField] public float rotationAngle = 15f;

    private void Start()
    {
        Invoke(nameof(Rotate), delay);
    }

    private void Rotate()
    {
        transform.Rotate(0, 0, -rotationAngle); 
    }
}
