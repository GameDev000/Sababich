using UnityEngine;

public class MessyClickSource : MonoBehaviour
{
    private void OnMouseDown()
    {
        if (DirtStateManager.Instance != null)
            DirtStateManager.Instance.RegisterMessyClick();
    }
}