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

            if (lower != "eggplantrow" && lower != "fryzone" && lower != "potatoes")//potatoes for level3
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

        //Bottle animation
        var bottlePhysics = GetComponent<SauceBottleWobble>();
        if (bottlePhysics != null)
            bottlePhysics.Shake(); 

        //Update Tutorial
        if (TutorialManager.Instance != null)
            TutorialManager.Instance.OnIngredientClicked(this, ingredientName);

        //Update GameFlow
        if (LevelGameFlow.Instance != null)
            LevelGameFlow.Instance.OnIngredientClickedFromItem(this, ingredientName);
    }

    //Control which components can be added
    public void SetClickable(bool value)
    {
        isClickable = value;
    }
}
