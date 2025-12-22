using UnityEngine;

public class FryCollectEmoji : MonoBehaviour
{
    [SerializeField] private FryZoneIngredient targetFryer;

    private void OnMouseDown()
    {
        if (targetFryer == null) return;
        if (!targetFryer.IsReady) return;

        targetFryer.ClearPan();
        // Collect() אצלך פרטי, אז נשתמש בטריק נקי:
        targetFryer.SendMessage("Collect", SendMessageOptions.DontRequireReceiver);
    }
}
