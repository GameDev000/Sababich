using UnityEngine;
// For level3 - eggplant&chips at the same time
public class FryCollectEmoji : MonoBehaviour
{
    [SerializeField] private FryZoneIngredient targetFryer; // Connect the emoji in the inspector

    private void OnMouseDown()
    {
        if (targetFryer == null) { Debug.LogWarning("[Emoji] targetFryer is NULL"); return; }

        Debug.Log($"[Emoji] Clicked {name} -> targetFryer={targetFryer.name} id={targetFryer.GetInstanceID()} ready={targetFryer.IsReady}");

        if (!targetFryer.IsReady) return; // Valid to click if IsReady

        //Calls the private Collect() method on the fryer by SendMessage to move the ready item to its tray without exposing the method publicly
        targetFryer.SendMessage("Collect", SendMessageOptions.DontRequireReceiver);
        targetFryer.ClearPan();
    }
}
