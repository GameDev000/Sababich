using UnityEngine;
//Represents a clickable ingredient. Handles selection and updates when clicked.
public class Item : MonoBehaviour
{
    [SerializeField] private string ingredientName = "pitta"; //For each click able item-> Updating his name in the Inspector
    [SerializeField] private EggplantFlyToOilFX_World eggplantToOilFx; // optional FX for eggplantrow

    private bool isClickable = true; //Item availability indication at a click


    public void OnClick()
    {
        if (!isClickable) return;

        Debug.Log("Clicked on ingredient: " + ingredientName);

        if (SelectionList.Instance != null)
        {
            string lower = ingredientName.ToLower();
            if (lower == "eggplantrow" || lower == "potatoes")
            {
                if (eggplantToOilFx != null)
                    eggplantToOilFx.Play();
            }


            if (lower != "eggplantrow" && lower != "fryzone" && lower != "potatoes") //Item that are not "pitta_in_hands". potatoes for level3
            {
                bool added = SelectionList.Instance.TryAddIngredient(ingredientName);

                if (!added) //Check that the item was actually added to list
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
        //Bottle animation according to component
        var bottlePhysics = GetComponent<SauceBottleWobble>();
        if (bottlePhysics != null)
        {
            bottlePhysics.Shake();

        }

        //Update Tutorial
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.OnIngredientClicked(this, ingredientName);

        }

        //Update GameFlow for levels 1-3
        if (LevelGameFlow.Instance != null)
            LevelGameFlow.Instance.OnIngredientClickedFromItem(this, ingredientName);
    }

    public void SetClickable(bool value)
    {
        isClickable = value;
    }


}
