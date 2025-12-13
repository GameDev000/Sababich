// using System.Collections.Generic;
// using UnityEngine;
// using TMPro;

// public class SelectionList : MonoBehaviour
// {
//     public static SelectionList Instance { get; private set; }

//     [SerializeField] private TextMeshProUGUI selectedText;
//     [SerializeField] private PitaBuilder pitaBuilder;

//     private readonly List<string> selectedIngredients = new List<string>();

//     private void Awake()
//     {
//         if (Instance != null && Instance != this)
//         {
//             Destroy(gameObject);
//             return;
//         }
//         Instance = this;
//     }

//     private void Start()
//     {
//         UpdateText();
//     }

//     public bool TryAddIngredient(string ingredientName)
//     {
//         string lower = ingredientName.ToLower();

//         if (selectedIngredients.Count == 0 && lower != "pitta")
//         {
//             Debug.Log("First ingredient must be Pitta.");
//             return false;
//         }

//         Debug.Log("Adding ingredient: " + lower);
//         selectedIngredients.Add(lower);
//         UpdateText();
//         return true;
//     }

//     public void ClearIngredients()
//     {
//         selectedIngredients.Clear();
//         UpdateText();
//     }

//     private void UpdateText()
//     {
//         if (selectedText != null)
//         {
//             string result = "פריטים נבחרים:\n";
//             foreach (string ing in selectedIngredients)
//                 result += "- " + ToHebrew(ing) + "\n";

//             selectedText.text = result;
//         }

//         if (pitaBuilder != null)
//             pitaBuilder.BuildFromSelection(selectedIngredients);
//     }

//     private string ToHebrew(string ingredient)
//     {
//         switch (ingredient.ToLower())
//         {
//             case "pitta":     return "פיתה";
//             case "salad":     return "סלט";
//             case "egg":       return "ביצה";
//             case "tahini":    return "טחינה";
//             case "amba":      return "עמבה";
//             case "eggplant":  return "חציל";
//             default:          return ingredient;
//         }
//     }

//     public bool IsSelectionMatching(List<string> correctOrder)
//     {
//         if (selectedIngredients.Count != correctOrder.Count)
//             return false;

//         foreach (string item in correctOrder)
//         {
//             if (!selectedIngredients.Contains(item.ToLower()))
//                 return false;
//         }

//         return true;
//     }

//     public List<string> GetSelectedIngredients()
//     {
//         return new List<string>(selectedIngredients);
//     }
// }
