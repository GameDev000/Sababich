using UnityEngine;

public class FaucetCleansDirt : MonoBehaviour
{
    private void OnMouseDown()
    {
        if (DirtStateManager.Instance != null)
            DirtStateManager.Instance.Clean();
            TutorialManager.Instance?.OnHandsWashed();

    }
}