using UnityEngine;

public class SignSwing : MonoBehaviour
{
    [SerializeField] public float initialTorque = 5f;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.AddTorque(initialTorque, ForceMode2D.Impulse);
    }
}
