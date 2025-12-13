using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField] private string ingredientName = "pitta";
    private bool isClickable = true;

    public void OnClick()
    {
        if (!isClickable) return;

        Debug.Log("Clicked on ingredient: " + ingredientName);

        if (SelectionList.Instance != null)
        {
            string lower = ingredientName.ToLower();

            if (lower != "eggplantrow" && lower != "fryzone")
            {
                bool added = SelectionList.Instance.TryAddIngredient(ingredientName);

                if (!added)
                {
                    Debug.Log("Ingredient " + ingredientName + " was not added to selection list.");
                    return;
                }
            }
        }
        else
        {
            Debug.LogWarning("SelectionList.Instance is null!");
        }

        var bottlePhysics = GetComponent<SauceBottleWobble>();
        if (bottlePhysics != null)
            bottlePhysics.Shake();

        if (TutorialManager.Instance != null)
            TutorialManager.Instance.OnIngredientClicked(this, ingredientName);

        if (Level1GameFlow.Instance != null)
            Level1GameFlow.Instance.OnIngredientClickedFromItem(this, ingredientName);
    }

    public void SetClickable(bool value)
    {
        isClickable = value;
    }
}
