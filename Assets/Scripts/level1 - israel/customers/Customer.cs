// using System.Collections.Generic;
// using UnityEngine;

// /// <summary>
// /// Represents a customer in the game, managing their appearance, order, mood, and anger bar UI.
// /// </summary>
// public class Customer : MonoBehaviour
// {
//     [SerializeField] private SpriteRenderer spriteRenderer;
//     [SerializeField] private GameObject orderBubbleRoot;
//     [SerializeField] private Transform iconsParent;
//     [SerializeField] private GameObject iconPrefab;
//     [SerializeField] private List<IngredientIconInfo> ingredientIcons;
//     [SerializeField] private CustomerMoodTimer_levels moodTimer;

//     [Header("Anger Bar UI")]
//     [SerializeField] private CustomerAngerBar angerBar;

//     [Tooltip("Fallback duration only. Used if no mood timer exists.")]
//     [SerializeField] private float fallbackAngerBarDurationSeconds = 17f;

//     [SerializeField] private bool autoStartAngerBarOnInit = true;

//     private List<string> activeRequiredIngredients; // Actual customer order
//     public CustomerMoodTimer_levels MoodTimer => moodTimer;

//     public CustomerType Data { get; private set; }
//     public bool IsLeaving { get; private set; }

//     private Dictionary<string, IngredientIconInfo> iconLookup; // For quick lookup of ingredient icons by ID

//     private void Awake()
//     {
//         iconLookup = new Dictionary<string, IngredientIconInfo>();

//         foreach (var info in ingredientIcons)
//         {
//             if (!string.IsNullOrEmpty(info.id) && info.sprite != null)
//             {
//                 iconLookup[info.id.ToLower()] = info;
//             }
//         }

//         if (moodTimer == null)
//             moodTimer = GetComponent<CustomerMoodTimer_levels>();

//         if (angerBar == null)
//             angerBar = GetComponentInChildren<CustomerAngerBar>(true);
//     }

//     /// <summary>
//     /// Initializes the customer with the provided data.
//     /// maxMissingItems - for subtracting ingredients.
//     /// </summary>
//     public void Init(CustomerType data, int maxMissingItems = 0)
//     {
//         Data = data;
//         IsLeaving = false;

//         var headSetup = GetComponent<CustomerHeadSetup>();

//         if (headSetup != null)
//         {
//             headSetup.Apply(data);
//         }
//         else
//         {
//             if (spriteRenderer != null && data != null && data.sprite != null)
//                 spriteRenderer.sprite = data.sprite;

//             if (moodTimer != null && data != null)
//             {
//                 if (data.happyFace == null)
//                     Debug.LogWarning($"CustomerType '{data.name}' has no happyFace assigned!");

//                 if (data.angryFaces == null || data.angryFaces.Length == 0)
//                     Debug.LogWarning($"CustomerType '{data.name}' has no angryFaces assigned!");

//                 moodTimer.Configure(data.happyFace, data.angryFaces);
//             }
//         }

//         BuildActiveOrder(maxMissingItems);
//         SetupOrderBubble();

//         if (autoStartAngerBarOnInit)
//         {
//             StartAngerBar();
//         }
//         else
//         {
//             ResetAngerBar();
//         }
//     }

//     private void BuildActiveOrder(int maxMissingItems)
//     {
//         activeRequiredIngredients = new List<string>();

//         if (Data == null || Data.requiredIngredients == null)
//             return;

//         foreach (var r in Data.requiredIngredients)
//         {
//             if (!string.IsNullOrWhiteSpace(r))
//                 activeRequiredIngredients.Add(r.ToLower());
//         }

//         if (maxMissingItems <= 0)
//             return;

//         const string pitaId = "pitta";

//         if (!activeRequiredIngredients.Contains(pitaId))
//             activeRequiredIngredients.Insert(0, pitaId);

//         List<int> removableIndices = new List<int>();

//         for (int i = 0; i < activeRequiredIngredients.Count; i++)
//         {
//             if (activeRequiredIngredients[i] != pitaId)
//                 removableIndices.Add(i);
//         }

//         int maxCanRemove = Mathf.Min(maxMissingItems, removableIndices.Count);
//         int toRemove = Random.Range(0, maxCanRemove + 1);

//         for (int k = 0; k < toRemove; k++)
//         {
//             int pick = Random.Range(0, removableIndices.Count);
//             int idxToRemove = removableIndices[pick];

//             activeRequiredIngredients.RemoveAt(idxToRemove);

//             for (int j = 0; j < removableIndices.Count; j++)
//             {
//                 if (removableIndices[j] > idxToRemove)
//                     removableIndices[j]--;
//             }

//             removableIndices.RemoveAt(pick);
//         }
//     }

//     /// <summary>
//     /// Marks the customer as leaving and stops the anger bar.
//     /// </summary>
//     public void MarkLeaving()
//     {
//         IsLeaving = true;
//         StopAngerBar();
//     }

//     /// <summary>
//     /// Starts the visual anger bar using the real duration from the mood timer.
//     /// </summary>
//     public void StartAngerBar()
//     {
//         if (angerBar == null)
//             return;

//         float durationSeconds = fallbackAngerBarDurationSeconds;

//         if (moodTimer != null)
//         {
//             durationSeconds = moodTimer.GetTotalAngerDurationSeconds();
//         }

//         Debug.Log($"[CustomerAngerBar] Customer={gameObject.name}, Duration={durationSeconds}");

//         angerBar.StartBar(durationSeconds);
//     }

//     /// <summary>
//     /// Stops and hides the visual anger bar.
//     /// </summary>
//     public void StopAngerBar()
//     {
//         if (angerBar == null)
//             return;

//         angerBar.StopBar();
//     }

//     /// <summary>
//     /// Resets the anger bar without starting it.
//     /// </summary>
//     public void ResetAngerBar()
//     {
//         if (angerBar == null)
//             return;

//         angerBar.ResetBar();
//     }

//     /// <summary>
//     /// Sets up the order bubble by instantiating ingredient icons based on the customer's order.
//     /// </summary>
//     private void SetupOrderBubble()
//     {
//         if (orderBubbleRoot == null || iconsParent == null || iconPrefab == null || Data == null)
//             return;

//         orderBubbleRoot.SetActive(true);

//         for (int i = iconsParent.childCount - 1; i >= 0; i--)
//             Destroy(iconsParent.GetChild(i).gameObject);

//         var ingredients = activeRequiredIngredients;

//         if (ingredients == null || ingredients.Count == 0)
//             return;

//         foreach (var ing in ingredients)
//         {
//             var id = ing.ToLower();

//             if (!iconLookup.TryGetValue(id, out var info))
//                 continue;

//             var icon = Instantiate(iconPrefab, iconsParent);
//             icon.transform.localPosition = info.localPosition;
//             icon.transform.localScale = info.localScale;

//             var sr = icon.GetComponent<SpriteRenderer>();

//             if (sr != null)
//                 sr.sprite = info.sprite;
//         }
//     }

//     /// <summary>
//     /// Checks if the provided list of ingredients matches the customer's order.
//     /// </summary>
//     public bool IsOrderCorrect(List<string> ingredients)
//     {
//         if (activeRequiredIngredients == null)
//             return false;

//         if (ingredients == null)
//             return false;

//         var given = new HashSet<string>();

//         foreach (var x in ingredients)
//             given.Add(x.ToLower());

//         if (given.Count != activeRequiredIngredients.Count)
//             return false;

//         foreach (var req in activeRequiredIngredients)
//         {
//             if (!given.Contains(req))
//                 return false;
//         }

//         return true;
//     }

//     public void HideOrderBubble()
//     {
//         if (orderBubbleRoot != null)
//             orderBubbleRoot.SetActive(false);
//     }

//     /// <summary>
//     /// Exposes the actual active order after missing-items removal.
//     /// </summary>
//     public List<string> GetActiveRequiredIngredients()
//     {
//         return activeRequiredIngredients;
//     }

//     private void OnDisable()
//     {
//         StopAngerBar();
//     }
// }

// [System.Serializable]
// public class IngredientIconInfo
// {
//     public string id;
//     public Sprite sprite;
//     public Vector3 localPosition;
//     public Vector3 localScale = Vector3.one;
// }




using System.Collections.Generic;
using UnityEngine;
using TMPro;
/// <summary>
/// Represents a customer in the game, managing their appearance, order, mood,
/// anger bar UI, and order ingredient markers.
/// </summary>
public class Customer : MonoBehaviour
{
    private const string ADDED_MARKER_CHILD_NAME = "AddedMarker";

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

    [Header("Ingredient Markers")]
    [Tooltip("Prefab used to show a red X on ingredients that were added but are not part of this customer's order.")]
    [SerializeField] private GameObject wrongIngredientMarkerPrefab;
    [SerializeField] private Vector3 addedMarkerWorldScale = new Vector3(0.25f, 0.25f, 0.25f);
    [SerializeField] private Vector3 wrongMarkerScale = new Vector3(0.35f, 0.35f, 0.35f);

    private List<string> activeRequiredIngredients; // Actual customer order after missing-items removal
    public CustomerMoodTimer_levels MoodTimer => moodTimer;

    public CustomerType Data { get; private set; }
    public bool IsLeaving { get; private set; }

    private Dictionary<string, IngredientIconInfo> iconLookup; // Ingredient id -> icon info
    private Dictionary<string, GameObject> orderIconObjects;   // Ingredient id -> instantiated order icon
    private Dictionary<string, GameObject> addedMarkers;       // Ingredient id -> AddedMarker child
    private readonly List<GameObject> wrongIngredientMarkers = new List<GameObject>();

    private void Awake()
    {
        BuildIconLookup();

        orderIconObjects = new Dictionary<string, GameObject>();
        addedMarkers = new Dictionary<string, GameObject>();

        if (moodTimer == null)
            moodTimer = GetComponent<CustomerMoodTimer_levels>();

        if (angerBar == null)
            angerBar = GetComponentInChildren<CustomerAngerBar>(true);
    }

    /// <summary>
    /// Initializes the customer with the provided data.
    /// maxMissingItems is used for subtracting ingredients from the displayed order.
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

    private void BuildIconLookup()
    {
        iconLookup = new Dictionary<string, IngredientIconInfo>();

        if (ingredientIcons == null)
            return;

        foreach (var info in ingredientIcons)
        {
            if (info == null)
                continue;

            string normalizedId = NormalizeIngredientId(info.id);

            if (!string.IsNullOrEmpty(normalizedId) && info.sprite != null)
                iconLookup[normalizedId] = info;
        }
    }

    private void BuildActiveOrder(int maxMissingItems)
    {
        activeRequiredIngredients = new List<string>();

        if (Data == null || Data.requiredIngredients == null)
            return;

        foreach (var r in Data.requiredIngredients)
        {
            string normalizedIngredient = NormalizeIngredientId(r);

            if (!string.IsNullOrWhiteSpace(normalizedIngredient))
                activeRequiredIngredients.Add(normalizedIngredient);
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
        ClearIngredientMarkers();
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
            durationSeconds = moodTimer.GetTotalAngerDurationSeconds();

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
    /// Sets up the order bubble by instantiating ingredient icons based on the customer's active order.
    /// </summary>
    private void SetupOrderBubble()
    {
        ClearIngredientMarkers();

        orderIconObjects.Clear();
        addedMarkers.Clear();

        if (orderBubbleRoot == null || iconsParent == null || iconPrefab == null || Data == null)
            return;

        orderBubbleRoot.SetActive(true);

        for (int i = iconsParent.childCount - 1; i >= 0; i--)
            Destroy(iconsParent.GetChild(i).gameObject);

        if (activeRequiredIngredients == null || activeRequiredIngredients.Count == 0)
            return;

        foreach (var ing in activeRequiredIngredients)
        {
            string id = NormalizeIngredientId(ing);

            if (!iconLookup.TryGetValue(id, out var info))
                continue;

            GameObject icon = Instantiate(iconPrefab, iconsParent);
            icon.transform.localPosition = info.localPosition;
            icon.transform.localScale = info.localScale;

            SpriteRenderer sr = icon.GetComponent<SpriteRenderer>();

            if (sr != null)
                sr.sprite = info.sprite;

            orderIconObjects[id] = icon;

            // Transform markerTransform = icon.transform.Find(ADDED_MARKER_CHILD_NAME);

            // if (markerTransform != null)
            // {
            //     GameObject markerObject = markerTransform.gameObject;
            //     markerObject.SetActive(false);
            //     addedMarkers[id] = markerObject;
            // }
            Transform markerTransform = icon.transform.Find(ADDED_MARKER_CHILD_NAME);

            if (markerTransform != null)
            {
                GameObject markerObject = markerTransform.gameObject;

                // Keep the V marker visually consistent even when ingredient icons have different scales.
                Vector3 iconScale = icon.transform.localScale;

                markerTransform.localScale = new Vector3(
                    iconScale.x != 0f ? addedMarkerWorldScale.x / iconScale.x : addedMarkerWorldScale.x,
                    iconScale.y != 0f ? addedMarkerWorldScale.y / iconScale.y : addedMarkerWorldScale.y,
                    iconScale.z != 0f ? addedMarkerWorldScale.z / iconScale.z : addedMarkerWorldScale.z
                );

                TextMeshPro text = markerObject.GetComponent<TextMeshPro>();

                if (text != null)
                    text.fontStyle = FontStyles.Bold;

                markerObject.SetActive(false);
                addedMarkers[id] = markerObject;
            }
        }
    }

    /// <summary>
    /// Updates all order-bubble markers according to the ingredients currently inside the pita.
    /// Correct ingredients get a green check marker.
    /// Extra ingredients get a red X at their fixed ingredient position.
    /// </summary>
    public void UpdateIngredientMarkers(List<string> currentPitaIngredients, bool featureEnabled)
    {
        ClearIngredientMarkers();

        if (!featureEnabled)
            return;

        if (currentPitaIngredients == null || currentPitaIngredients.Count == 0)
            return;

        if (activeRequiredIngredients == null)
            return;

        HashSet<string> currentIngredients = BuildNormalizedSet(currentPitaIngredients);
        HashSet<string> requiredIngredients = new HashSet<string>(activeRequiredIngredients);

        foreach (string ingredientId in currentIngredients)
        {
            if (requiredIngredients.Contains(ingredientId))
            {
                ShowAddedMarker(ingredientId);
            }
            else
            {
                ShowWrongIngredientMarker(ingredientId);
            }
        }
    }

    /// <summary>
    /// Clears all green check markers and red X markers from this customer's order bubble.
    /// </summary>
    public void ClearIngredientMarkers()
    {
        foreach (var marker in addedMarkers.Values)
        {
            if (marker != null)
                marker.SetActive(false);
        }

        for (int i = wrongIngredientMarkers.Count - 1; i >= 0; i--)
        {
            if (wrongIngredientMarkers[i] != null)
                Destroy(wrongIngredientMarkers[i]);
        }

        wrongIngredientMarkers.Clear();
    }

    private void ShowAddedMarker(string ingredientId)
    {
        if (addedMarkers.TryGetValue(ingredientId, out GameObject marker) && marker != null)
        {
            marker.SetActive(true);
        }
    }
    private void ShowWrongIngredientMarker(string ingredientId)
    {
        if (wrongIngredientMarkerPrefab == null)
        {
            Debug.LogWarning("[Customer] WrongIngredientMarkerPrefab is not assigned.");
            return;
        }

        if (iconsParent == null)
        {
            Debug.LogWarning("[Customer] IconsParent is not assigned.");
            return;
        }

        if (!iconLookup.TryGetValue(ingredientId, out IngredientIconInfo info))
        {
            Debug.LogWarning($"[Customer] No IngredientIconInfo found for wrong ingredient id: {ingredientId}");
            return;
        }

        GameObject wrongMarker = Instantiate(wrongIngredientMarkerPrefab, iconsParent);

        wrongMarker.transform.localPosition = info.localPosition;
        wrongMarker.transform.localRotation = Quaternion.identity;
        wrongMarker.transform.localScale = wrongMarkerScale;

        TextMeshPro text = wrongMarker.GetComponent<TextMeshPro>();

        if (text != null)
        {
            text.fontStyle = FontStyles.Bold;
            text.color = Color.red;
        }

        wrongMarker.SetActive(true);
        wrongIngredientMarkers.Add(wrongMarker);

        Debug.Log($"[Customer] Wrong ingredient marker shown for: {ingredientId}");
    }

    /// <summary>
    /// Checks if the provided list of ingredients matches the customer's active order.
    /// </summary>
    public bool IsOrderCorrect(List<string> ingredients)
    {
        if (activeRequiredIngredients == null)
            return false;

        if (ingredients == null)
            return false;

        HashSet<string> given = BuildNormalizedSet(ingredients);

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
        ClearIngredientMarkers();

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

    /// <summary>
    /// Refreshes ingredient markers for every customer currently in the scene.
    /// Call this after adding an ingredient to the pita.
    /// </summary>
    public static void RefreshAllCustomerIngredientMarkers(List<string> currentPitaIngredients)
    {
        Customer[] customers = FindObjectsOfType<Customer>(true);

        foreach (Customer customer in customers)
        {
            if (customer == null)
                continue;

            customer.UpdateIngredientMarkers(
                currentPitaIngredients,
                ControlPanelUI.MarkAddedItemsEnabled
            );
        }
    }

    /// <summary>
    /// Clears ingredient markers from every customer currently in the scene.
    /// Call this after serving a dish or throwing it in the trash.
    /// </summary>
    public static void ClearAllCustomerIngredientMarkers()
    {
        Customer[] customers = FindObjectsOfType<Customer>(true);

        foreach (Customer customer in customers)
        {
            if (customer == null)
                continue;

            customer.ClearIngredientMarkers();
        }
    }

    private static HashSet<string> BuildNormalizedSet(List<string> ingredients)
    {
        HashSet<string> normalized = new HashSet<string>();

        foreach (string ingredient in ingredients)
        {
            string normalizedIngredient = NormalizeIngredientId(ingredient);

            if (!string.IsNullOrWhiteSpace(normalizedIngredient))
                normalized.Add(normalizedIngredient);
        }

        return normalized;
    }

    private static string NormalizeIngredientId(string ingredientId)
    {
        if (string.IsNullOrWhiteSpace(ingredientId))
            return string.Empty;

        return ingredientId.Trim().ToLower();
    }

    private void OnDisable()
    {
        StopAngerBar();
        ClearIngredientMarkers();
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