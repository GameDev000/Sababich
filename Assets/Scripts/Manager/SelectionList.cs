using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Manages the list of selected ingredients for an order.
/// </summary>
public class SelectionList : MonoBehaviour
{
    public static SelectionList Instance { get; private set; } // Singleton instance

    [SerializeField] private TextMeshProUGUI selectedText; // Reference to the UI text component - will remove in later versions

    private List<string> selectedIngredients = new List<string>(); // List of selected ingredients

    private void Awake()
    {
        if (Instance != null && Instance != this) // Ensure singleton pattern
        {
            Destroy(gameObject);
            return;
        }
        Instance = this; // Set the singleton instance
    }

    private void Start()
    {
        UpdateText(); // Initialize the selected ingredients text (empty at start)
    }

    /// <summary>
    /// Attempts to add an ingredient to the selection list.
    /// </summary>
    /// <param name="ingredientName">The name of the ingredient to add.</param>
    /// <returns>True if the ingredient was added; otherwise, false.</returns>
    public bool TryAddIngredient(string ingredientName)
    {
        string lower = ingredientName.ToLower();


        if (selectedIngredients.Count == 0 && lower != "pitta") // First ingredient must be Pitta for tutorial purposes
        {
            Debug.Log("First ingredient must be Pitta. ");
            return false;
        }

        Debug.Log("Adding ingredient: " + lower);
        selectedIngredients.Add(lower); // Add the ingredient to the list
        UpdateText(); // Update the UI text
        return true;
    }

    public void ClearIngredients()
    {
        selectedIngredients.Clear(); // Clear the list of selected ingredients
        UpdateText(); // Update the UI text
    }

    private void UpdateText()
    {
        if (selectedText == null) return;

        string result = "Selected items:\n";
        foreach (string ing in selectedIngredients)
        {
            result += "- " + ing + "\n"; // Append each ingredient to the result string
        }

        selectedText.text = result; // Update the UI text component
    }

    /// <summary>
    /// Checks if the current selection matches the correct order.
    /// </summary>
    /// <param name="correctOrder">The correct order of ingredients.</param>
    /// <returns>True if the selection matches; otherwise, false.</returns>
    public bool IsSelectionMatching(List<string> correctOrder)
    {
        if (selectedIngredients.Count != correctOrder.Count)
            return false;

        foreach (string item in correctOrder)
        {
            Debug.Log("Check item: " + item);


            if (!selectedIngredients.Contains(item.ToLower())) // Check if the item is in the selected ingredients
            {
                Debug.Log("Missing item: " + item);
                return false;
            }

        }

        return true;
    }

    public List<string> GetSelectedIngredients()
    {
        return new List<string>(selectedIngredients);
    }
}
