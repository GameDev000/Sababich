using UnityEngine;
/// <summary>
/// Handles clicks on messy items to register them with the DirtStateManager.
/// </summary>
public class MessyClickSource : MonoBehaviour
{
    private void OnMouseDown()
    {
        if (DirtStateManager.Instance != null)
            DirtStateManager.Instance.RegisterMessyClick(); // Notify the manager of the click
    }
}