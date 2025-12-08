using UnityEngine;
using System.Collections.Generic;

public class CheckOrder : MonoBehaviour
{
    [SerializeField] private SelectionList selectionList;
    [SerializeField] private List<string> correctOrder = new List<string> { "pitta", "tahini", "eggplant", "egg", "salad", "amba" };
    private void OnMouseDown()
    {
        if (selectionList == null)
        {
            Debug.LogWarning("SelectionList is not assigned to CheckOrder on " + gameObject.name);
            return;
        }

        bool isCorrect = selectionList.IsSelectionMatching(correctOrder);

        if (isCorrect)
        {
            GetComponent<CustomerMoodTimer>().CustomerServed();
            selectionList.ClearIngredients();
            Debug.Log("Correct!");

            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.CustomerOrderServed();
            }
        }
        else
        {
            Debug.Log("Wrong! - The entered order is" + string.Join(", ", selectionList.GetSelectedIngredients()));
            Debug.Log("Correct order is" + string.Join(", ", correctOrder));
        }

    }
}