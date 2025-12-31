using UnityEngine;
/// <summary>
/// Handles camera movement when an arrow is clicked.
/// using an arrow to move the camera to a target point in the kitchen or main area.
/// </summary>
public class ArrowMoveCamera : MonoBehaviour
{
    [SerializeField] private Transform targetPoint;// The point to move the camera to
    [SerializeField] private float moveSpeed = 6f;// Speed of camera movement

    [SerializeField] private TutorialManager tutorial;// Reference to the tutorial manager

    public enum ArrowType { ToKitchen, ToMain }// Types of arrows

    [SerializeField] private ArrowType arrowType;// Type of arrow
    public ArrowType Type => arrowType;

    private Camera cam;

    private bool moving;

    private void Start()
    {
        cam = Camera.main;// Get the main camera
        if (tutorial == null)
            tutorial = TutorialManager.Instance;
    }

    /// <summary>
    /// Handles mouse click to start moving the camera.
    /// </summary>
    private void OnMouseDown()
    {
        if (targetPoint == null) return;

        if (tutorial != null)
            tutorial.OnArrowClicked(this); // Notify tutorial manager of the arrow click

        moving = true;// Start moving the camera
    }

    private void Update()
    {
        if (!moving) return;

        // Smoothly move the camera towards the target point
        cam.transform.position = Vector3.Lerp(
            cam.transform.position, new Vector3(targetPoint.position.x, targetPoint.position.y,
            cam.transform.position.z), moveSpeed * Time.deltaTime
        );
        // Check if the camera is close enough to the target point
        if (Vector2.Distance(cam.transform.position,
            new Vector2(targetPoint.position.x, targetPoint.position.y)) < 0.05f)
        {
            cam.transform.position = new Vector3(targetPoint.position.x, targetPoint.position.y,
                cam.transform.position.z); // Snap to target position
            moving = false; // Stop moving
        }
    }

}
