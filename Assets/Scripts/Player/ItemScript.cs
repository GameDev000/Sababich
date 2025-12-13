using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField] private string ingredientName = "pitta"; // Name of the ingredient represented by this item
    private bool isClickable = true;

    /// <summary>
    /// Handles mouse click interactions on the item.
    /// </summary>
    public void OnClick()
    {
        if (!isClickable) return;

        Debug.Log("Clicked on ingredient: " + ingredientName);

        if (SelectionList.Instance != null)
        {
            string lower = ingredientName.ToLower();

            if (lower != "eggplantrow" && lower != "fryzone") // Exclude non-ingredient items
            {
                bool added = SelectionList.Instance.TryAddIngredient(ingredientName); // Attempt to add the ingredient to the selection list

                if (!added) // If the ingredient was not added, log a message
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

        if (TutorialManager.Instance != null)
        {
            Debug.Log("Calling TutorialManager.OnIngredientClicked");
            TutorialManager.Instance.OnIngredientClicked(this, ingredientName); // Notify the tutorial manager about the clicked ingredient
        }
        else
        {
            Debug.LogWarning("TutorialManager.Instance is null!");
        }


        var bottlePhysics = GetComponent<SauceBottleWobble>();
        if (bottlePhysics != null)
        {
            bottlePhysics.Shake();
        }

        if (TutorialManager.Instance != null)
        {
            Debug.Log("Calling TutorialManager.OnIngredientClicked");
            TutorialManager.Instance.OnIngredientClicked(this, ingredientName);
        }
        else
        {
            Debug.LogWarning("TutorialManager.Instance is null!");
        }
    }

    public void SetClickable(bool value)
    {
        isClickable = value;
    }
}
