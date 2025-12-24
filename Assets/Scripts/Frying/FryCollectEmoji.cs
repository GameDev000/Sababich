using UnityEngine;

public class FryCollectEmoji : MonoBehaviour
{
    [SerializeField] private FryZoneIngredient targetFryer; // Connect the emoji in the inspector

    private void OnMouseDown()
    {
        if (targetFryer == null) return;
        if (!targetFryer.IsReady) return; // Valid to click if IsReady

        targetFryer.ClearPan();
        //Calls the private Collect() method on the fryer by SendMessage to move the ready item to its tray without exposing the method publicly
        targetFryer.SendMessage("Collect", SendMessageOptions.DontRequireReceiver);
    }
}
