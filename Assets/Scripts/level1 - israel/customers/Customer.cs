using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a customer in the game, managing their appearance, order, and mood.
/// </summary>
public class Customer : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private GameObject orderBubbleRoot;
    [SerializeField] private Transform iconsParent;
    [SerializeField] private GameObject iconPrefab;
    [SerializeField] private List<IngredientIconInfo> ingredientIcons;
    [SerializeField] private CustomerMoodTimer_levels moodTimer;
    public CustomerMoodTimer_levels MoodTimer => moodTimer;

    public CustomerType Data { get; private set; }
    public bool IsLeaving { get; private set; }

    private Dictionary<string, IngredientIconInfo> iconLookup; // For quick lookup of ingredient icons by ID

    private void Awake()
    {
        iconLookup = new Dictionary<string, IngredientIconInfo>();
        foreach (var info in ingredientIcons)
        {
            if (!string.IsNullOrEmpty(info.id) && info.sprite != null)
            {
                iconLookup[info.id.ToLower()] = info;
            }
        }
    }

    /// <summary>
    /// Initializes the customer with the provided data.
    /// </summary>
    /// <param name="data"></param>
    public void Init(CustomerType data)
    {
        Data = data;

        if (spriteRenderer != null && data != null && data.sprite != null)
            spriteRenderer.sprite = data.sprite;

        SetupOrderBubble(); // Setup the order bubble with icons

        if (moodTimer != null && data != null)
        {
            if (data.happyFace == null)
                Debug.LogWarning($"CustomerType '{data.name}' has no happyFace assigned!");

            if (data.angryFaces == null || data.angryFaces.Length == 0)
                Debug.LogWarning($"CustomerType '{data.name}' has no angryFaces assigned!");

            moodTimer.Configure(data.happyFace, data.angryFaces); // Configure mood timer with faces
        }
    }

    public void MarkLeaving()
    {
        IsLeaving = true; // Prevent further interactions
    }

    /// <summary>
    /// Sets up the order bubble by instantiating ingredient icons based on the customer's order.
    /// </summary>
    private void SetupOrderBubble()
    {
        if (orderBubbleRoot == null || iconsParent == null || iconPrefab == null || Data == null)
            return;

        orderBubbleRoot.SetActive(true); // Show the order bubble

        for (int i = iconsParent.childCount - 1; i >= 0; i--) // Clear existing icons
            Destroy(iconsParent.GetChild(i).gameObject); // Destroy existing icons

        var ingredients = Data.requiredIngredients; // Get required ingredients
        if (ingredients == null || ingredients.Count == 0)
            return;

        // Instantiate icons for each required ingredient
        foreach (var ing in ingredients)
        {
            var id = ing.ToLower();
            if (!iconLookup.TryGetValue(id, out var info))
                continue;

            var icon = Instantiate(iconPrefab, iconsParent);
            icon.transform.localPosition = info.localPosition;
            icon.transform.localScale = info.localScale;

            var sr = icon.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.sprite = info.sprite;
        }
    }
    /// <summary>
    /// Checks if the provided list of ingredients matches the customer's order.
    /// </summary>
    /// <param name="ingredients"></param>
    /// <returns></returns>
    public bool IsOrderCorrect(List<string> ingredients)
    {
        if (Data == null || Data.requiredIngredients == null)
            return false;

        if (ingredients.Count != Data.requiredIngredients.Count)
            return false;

        foreach (var req in Data.requiredIngredients)
            if (!ingredients.Contains(req.ToLower()))
                return false;

        return true;
    }

    public void HideOrderBubble()
    {
        if (orderBubbleRoot != null)
            orderBubbleRoot.SetActive(false); // Hide the order bubble
    }
}

[System.Serializable]
public class IngredientIconInfo
{
    public string id;
    public Sprite sprite;
    public Vector3 localPosition;
    public Vector3 localScale = Vector3.one;
}
