using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a customer in the game, managing their appearance, order, mood, and anger bar UI.
/// </summary>
public class Customer : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private GameObject orderBubbleRoot;
    [SerializeField] private Transform iconsParent;
    [SerializeField] private GameObject iconPrefab;
    [SerializeField] private List<IngredientIconInfo> ingredientIcons;
    [SerializeField] private CustomerMoodTimer_levels moodTimer;

    [Header("Anger Bar UI")]
    [SerializeField] private CustomerAngerBar angerBar;

    [Tooltip("Fallback duration only. Used if no mood timer exists.")]
    [SerializeField] private float fallbackAngerBarDurationSeconds = 17f;

    [SerializeField] private bool autoStartAngerBarOnInit = true;

    private List<string> activeRequiredIngredients; // Actual customer order
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

        if (moodTimer == null)
            moodTimer = GetComponent<CustomerMoodTimer_levels>();

        if (angerBar == null)
            angerBar = GetComponentInChildren<CustomerAngerBar>(true);
    }

    /// <summary>
    /// Initializes the customer with the provided data.
    /// maxMissingItems - for subtracting ingredients.
    /// </summary>
    public void Init(CustomerType data, int maxMissingItems = 0)
    {
        Data = data;
        IsLeaving = false;

        var headSetup = GetComponent<CustomerHeadSetup>();

        if (headSetup != null)
        {
            headSetup.Apply(data);
        }
        else
        {
            if (spriteRenderer != null && data != null && data.sprite != null)
                spriteRenderer.sprite = data.sprite;

            if (moodTimer != null && data != null)
            {
                if (data.happyFace == null)
                    Debug.LogWarning($"CustomerType '{data.name}' has no happyFace assigned!");

                if (data.angryFaces == null || data.angryFaces.Length == 0)
                    Debug.LogWarning($"CustomerType '{data.name}' has no angryFaces assigned!");

                moodTimer.Configure(data.happyFace, data.angryFaces);
            }
        }

        BuildActiveOrder(maxMissingItems);
        SetupOrderBubble();

        if (autoStartAngerBarOnInit)
        {
            StartAngerBar();
        }
        else
        {
            ResetAngerBar();
        }
    }

    private void BuildActiveOrder(int maxMissingItems)
    {
        activeRequiredIngredients = new List<string>();

        if (Data == null || Data.requiredIngredients == null)
            return;

        foreach (var r in Data.requiredIngredients)
        {
            if (!string.IsNullOrWhiteSpace(r))
                activeRequiredIngredients.Add(r.ToLower());
        }

        if (maxMissingItems <= 0)
            return;

        const string pitaId = "pitta";

        if (!activeRequiredIngredients.Contains(pitaId))
            activeRequiredIngredients.Insert(0, pitaId);

        List<int> removableIndices = new List<int>();

        for (int i = 0; i < activeRequiredIngredients.Count; i++)
        {
            if (activeRequiredIngredients[i] != pitaId)
                removableIndices.Add(i);
        }

        int maxCanRemove = Mathf.Min(maxMissingItems, removableIndices.Count);
        int toRemove = Random.Range(0, maxCanRemove + 1);

        for (int k = 0; k < toRemove; k++)
        {
            int pick = Random.Range(0, removableIndices.Count);
            int idxToRemove = removableIndices[pick];

            activeRequiredIngredients.RemoveAt(idxToRemove);

            for (int j = 0; j < removableIndices.Count; j++)
            {
                if (removableIndices[j] > idxToRemove)
                    removableIndices[j]--;
            }

            removableIndices.RemoveAt(pick);
        }
    }

    /// <summary>
    /// Marks the customer as leaving and stops the anger bar.
    /// </summary>
    public void MarkLeaving()
    {
        IsLeaving = true;
        StopAngerBar();
    }

    /// <summary>
    /// Starts the visual anger bar using the real duration from the mood timer.
    /// </summary>
    public void StartAngerBar()
    {
        if (angerBar == null)
            return;

        float durationSeconds = fallbackAngerBarDurationSeconds;

        if (moodTimer != null)
        {
            durationSeconds = moodTimer.GetTotalAngerDurationSeconds();
        }

        Debug.Log($"[CustomerAngerBar] Customer={gameObject.name}, Duration={durationSeconds}");

        angerBar.StartBar(durationSeconds);
    }

    /// <summary>
    /// Stops and hides the visual anger bar.
    /// </summary>
    public void StopAngerBar()
    {
        if (angerBar == null)
            return;

        angerBar.StopBar();
    }

    /// <summary>
    /// Resets the anger bar without starting it.
    /// </summary>
    public void ResetAngerBar()
    {
        if (angerBar == null)
            return;

        angerBar.ResetBar();
    }

    /// <summary>
    /// Sets up the order bubble by instantiating ingredient icons based on the customer's order.
    /// </summary>
    private void SetupOrderBubble()
    {
        if (orderBubbleRoot == null || iconsParent == null || iconPrefab == null || Data == null)
            return;

        orderBubbleRoot.SetActive(true);

        for (int i = iconsParent.childCount - 1; i >= 0; i--)
            Destroy(iconsParent.GetChild(i).gameObject);

        var ingredients = activeRequiredIngredients;

        if (ingredients == null || ingredients.Count == 0)
            return;

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
    public bool IsOrderCorrect(List<string> ingredients)
    {
        if (activeRequiredIngredients == null)
            return false;

        if (ingredients == null)
            return false;

        var given = new HashSet<string>();

        foreach (var x in ingredients)
            given.Add(x.ToLower());

        if (given.Count != activeRequiredIngredients.Count)
            return false;

        foreach (var req in activeRequiredIngredients)
        {
            if (!given.Contains(req))
                return false;
        }

        return true;
    }

    public void HideOrderBubble()
    {
        if (orderBubbleRoot != null)
            orderBubbleRoot.SetActive(false);
    }

    /// <summary>
    /// Exposes the actual active order after missing-items removal.
    /// </summary>
    public List<string> GetActiveRequiredIngredients()
    {
        return activeRequiredIngredients;
    }

    private void OnDisable()
    {
        StopAngerBar();
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