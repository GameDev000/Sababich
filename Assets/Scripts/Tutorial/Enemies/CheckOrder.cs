using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Checks the player's selected ingredients against the correct order when the customer is clicked.
/// If the order is correct, it notifies the CustomerMoodTimer and TutorialManager.
/// </summary>
public class CheckOrder : MonoBehaviour
{
    [SerializeField] private SelectionList selectionList; // Reference to the SelectionList component
    // Define the correct order of ingredients
    [SerializeField] private List<string> correctOrder = new List<string> { "pitta", "tahini", "eggplant", "egg", "salad", "amba" };
    // Reference to the CustomerMoodTimer component
    [SerializeField] private CustomerMoodTimer customerMoodTimer;

    // This method is called when the customer is clicked
    private void OnMouseDown()
    {
        if (selectionList == null)
        {
            Debug.LogWarning("SelectionList is not assigned to CheckOrder on " + gameObject.name);
            return;
        }
        // Check if the selected ingredients match the correct order
        bool isCorrect = selectionList.IsSelectionMatching(correctOrder);

        if (isCorrect)
        {
            if (customerMoodTimer != null)
                customerMoodTimer.CustomerServed(); // Notify the customer mood timer that the customer has been served
            selectionList.ClearIngredients(); // Clear the selected ingredients for the next order
            Debug.Log("Correct!");

            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.CustomerOrderServed(); // Notify the tutorial manager that the customer order was served
            }
        }
        else // If the order is incorrect
        {
            Debug.Log("Wrong! - The entered order is" + string.Join(", ", selectionList.GetSelectedIngredients()));
            Debug.Log("Correct order is" + string.Join(", ", correctOrder));
        }

    }
}