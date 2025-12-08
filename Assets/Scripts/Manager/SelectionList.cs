using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SelectionList : MonoBehaviour
{
    public static SelectionList Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI selectedText;

    private List<string> selectedIngredients = new List<string>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        UpdateText();
    }

    public bool TryAddIngredient(string ingredientName)
    {
        string lower = ingredientName.ToLower();


        if (selectedIngredients.Count == 0 && lower != "pitta")
        {
            Debug.Log("First ingredient must be Pitta. ");
            return false;
        }

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

    private void UpdateText()
    {
        if (selectedText == null) return;

        string result = "Selected items:\n";
        foreach (string ing in selectedIngredients)
        {
            result += "- " + ing + "\n";
        }

        selectedText.text = result;
    }

    public bool IsSelectionMatching(List<string> correctOrder)
    {
        if (selectedIngredients.Count != correctOrder.Count)
            return false;

        foreach (string item in correctOrder)
        {
            Debug.Log("Check item: " + item);


            if (!selectedIngredients.Contains(item.ToLower()))
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
