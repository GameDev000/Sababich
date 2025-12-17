using UnityEngine;

public class ArrowMoveCamera : MonoBehaviour
{
    [SerializeField] private Transform targetPoint;
    [SerializeField] private float moveSpeed = 6f;

    private Camera cam;

    private bool moving;

    private void Start()
    {
        cam = Camera.main;
    }

    private void OnMouseDown()
    {
        if (targetPoint != null)
            moving = true;
    }

    private void Update()
    {
        if (!moving) return;

        cam.transform.position = Vector3.Lerp(
            cam.transform.position,
            new Vector3(
                targetPoint.position.x,
                targetPoint.position.y,
                cam.transform.position.z
            ),
            moveSpeed * Time.deltaTime
        );

        if (Vector2.Distance(
            cam.transform.position,
            new Vector2(targetPoint.position.x, targetPoint.position.y)
        ) < 0.05f)
        {
            cam.transform.position = new Vector3(
                targetPoint.position.x,
                targetPoint.position.y,
                cam.transform.position.z
            );
            moving = false;
        }
    }
}
