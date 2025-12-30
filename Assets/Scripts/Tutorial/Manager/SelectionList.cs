using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Manages the list of ingredients currently selected by the player while preparing a pita.
/// Responsibilities:
/// - Stores the selected ingredients in the order they were chosen.
/// - Enforces game rules (pita must be first, ingredients cannot be taken from empty trays).
/// - Notifies the PitaBuilder to visually
/// </summary>
public class SelectionList : MonoBehaviour
{
    // Singleton instance 
    public static SelectionList Instance { get; private set; }

    // UI text that shows the player's currently selected ingredients.
    [SerializeField] private TextMeshProUGUI selectedText;

    // Responsible for visually building/updating the pita based on the selected ingredients.
    [SerializeField] private PitaBuilder pitaBuilder;

    // References to tray states used to prevent adding ingredients when their tray is empty.
    [SerializeField] private FriedTrayState eggplantTrayState;
    [SerializeField] private FriedTrayState chipsTrayState;

    private readonly List<string> selectedIngredients = new List<string>();

    private void Awake()
    {
        // Enforce Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        // Initialize the UI and the pita visual to an empty selection.
        UpdateText();
    }

    // Tries to add an ingredient to the selection list
    public bool TryAddIngredient(string ingredientName)
    {
        string lower = ingredientName.ToLower();

        // The first selected ingredient must be "pitta".
        if (selectedIngredients.Count == 0 && lower != "pitta")
        {
            Debug.Log("First ingredient must be Pitta.");
            return false;
        }

        // Prevent adding eggplant if the eggplant tray is empty.
        if (lower == "eggplant" && eggplantTrayState != null && eggplantTrayState.IsEmpty)
        {
            Debug.Log("Eggplant tray is empty.");
            return false;
        }

        // Prevent adding chips if the chips tray is empty.
        if (lower == "chips" && chipsTrayState != null && chipsTrayState.IsEmpty)
        {
            Debug.Log("Chips tray is empty.");
            return false;
        }

        // Add the ingredient and refresh UI + pita visual.
        Debug.Log("Adding ingredient: " + lower);
        selectedIngredients.Add(lower);
        UpdateText();
        return true;
    }

    public void ClearIngredients()
    {
        selectedIngredients.Clear();
        UpdateText();
    }

    // Updates the UI text and tells the PitaBuilder to rebuild the visual pita
    private void UpdateText()
    {
        if (selectedText != null)
        {
            string result = "פריטים נבחרים:\n";
            foreach (string ing in selectedIngredients)
                result += "- " + ToHebrew(ing) + "\n";

            selectedText.text = result;
        }

        if (pitaBuilder != null)
            pitaBuilder.BuildFromSelection(selectedIngredients);
    }

    private string ToHebrew(string ingredient)
    {
        switch (ingredient.ToLower())
        {
            case "pitta": return "פיתה";
            case "salad": return "סלט";
            case "soy": return "סויה";
            case "chips": return "ציפס";
            case "egg": return "ביצה";
            case "tahini": return "טחינה";
            case "amba": return "עמבה";
            case "eggplant": return "חציל";
            default: return ingredient;
        }
    }

    // Checks if the selected ingredients match the required list.
    public bool IsSelectionMatching(List<string> correctOrder)
    {
        if (selectedIngredients.Count != correctOrder.Count)
            return false;

        foreach (string item in correctOrder)
        {
            if (!selectedIngredients.Contains(item.ToLower()))
                return false;
        }

        return true;
    }

    public List<string> GetSelectedIngredients()
    {
        return new List<string>(selectedIngredients);
    }
}
