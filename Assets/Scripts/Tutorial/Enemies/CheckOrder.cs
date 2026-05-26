using UnityEngine;
using System.Collections.Generic;
using PlasticPipe.PlasticProtocol.Messages;

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
    // voice of char
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] correctClips;
    [SerializeField] private AudioClip[] wrongClips;
    private static int index;

    public static void setIndex(int num)
    {
        index = num;
    }


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
            if (audioSource != null && correctClips != null && index < correctClips.Length)
                audioSource.PlayOneShot(correctClips[index]);

            if (customerMoodTimer != null)
                customerMoodTimer.CustomerServed();
            selectionList.ClearIngredients();
            Debug.Log("Correct!");

            if (TutorialManager.Instance != null)
                TutorialManager.Instance.CustomerOrderServed();
        }
        else
        {
            if (audioSource != null && wrongClips != null && index < wrongClips.Length)
                audioSource.PlayOneShot(wrongClips[index]);

            Debug.Log("Wrong! - The entered order is" + string.Join(", ", selectionList.GetSelectedIngredients()));
            Debug.Log("Correct order is" + string.Join(", ", correctOrder));
        }

    }
}