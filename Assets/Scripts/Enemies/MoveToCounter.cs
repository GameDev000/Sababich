using UnityEngine;

/// <summary>
/// Moves an enemy towards a specified target (e.g., a counter) at a defined speed.
/// </summary>
public class MoveToCounter : MonoBehaviour
{
    [SerializeField] private Transform target; // Target to move towards
    [SerializeField] private float speed = 2f; // Movement speed

    void Update()
    {
        if (target == null)
        {
            return;
        }
        // Move the enemy towards the target position
        transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
    }



}
