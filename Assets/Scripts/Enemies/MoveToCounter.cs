using UnityEngine;

public class MoveToCounter : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float speed = 2f;

    void Update()
    {
        if (target == null)
        {
            return;
        }
        transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
    }



}
