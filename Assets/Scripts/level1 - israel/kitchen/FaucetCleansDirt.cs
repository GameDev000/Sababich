using UnityEngine;

/// <summary>
/// Handles mouse click events on the faucet to clean dirtiness in the kitchen.
/// </summary>
public class FaucetCleansDirt : MonoBehaviour
{
    private void OnMouseDown()
    {
        if (DirtStateManager.Instance != null)
            DirtStateManager.Instance.Clean();
        TutorialManager.Instance?.OnHandsWashed();// Notify tutorial manager that hands were washed

    }
}