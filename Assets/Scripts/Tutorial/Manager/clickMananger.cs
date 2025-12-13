using UnityEngine;

/// <summary>
/// Manages mouse click interactions, detecting clicks on items and triggering their OnClick behavior.
/// </summary>
public class ClickManager : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Input.mousePosition; // Get mouse position in screen coordinates
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos); // Convert to world coordinates
            Vector2 worldPos2D = new Vector2(worldPos.x, worldPos.y); // Create a 2D vector for raycasting

            RaycastHit2D hit = Physics2D.Raycast(worldPos2D, Vector2.zero); // Perform raycast at mouse position

            if (hit.collider != null)
            {
                Item item = hit.collider.GetComponent<Item>(); // Get the Item component from the hit collider
                if (item != null)
                {
                    item.OnClick(); // Call the OnClick method on the item
                }
            }
        }
    }
}
