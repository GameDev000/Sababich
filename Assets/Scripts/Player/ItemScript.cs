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
    
            if( lower != "eggplantrow" && lower != "fryzone")
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
