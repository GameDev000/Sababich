using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls which ingredients are available in the current level.
/// It can hide/show counter objects and exposes the active ingredient list
/// so customers can avoid showing unavailable ingredients in their order bubbles.
/// </summary>
public class LevelIngredientAvailabilityManager : MonoBehaviour
{
    [Header("Frying Reset")]
    [SerializeField] private FryZoneIngredient eggplantFryerToReset;
    [SerializeField] private FryZoneIngredient chipsFryerToReset;
    [SerializeField] private FriedTrayState eggplantTrayToReset;
    [SerializeField] private FriedTrayState chipsTrayToReset;
    public static LevelIngredientAvailabilityManager Instance { get; private set; }

    [System.Serializable]
    public class IngredientObjectsGroup
    {
        [Tooltip("Logical ingredient id, for example: tahini, amba, eggplant, soy, chips")]
        public string ingredientId;

        [Tooltip("All scene objects that should be hidden when this ingredient is disabled.")]
        public GameObject[] objectsToToggle;
    }

    [Header("Ingredient Count")]
    [SerializeField] private int maxIngredientCount = 6;
    [SerializeField] private int maxRemovableIngredients = 3;

    [Header("Removal Order")]
    [Tooltip("Ingredients will be removed in this exact order when lowering the ingredient count.")]
    [SerializeField] private string[] removalOrder;

    [Header("Scene Objects By Ingredient")]
    [SerializeField] private IngredientObjectsGroup[] ingredientObjectsGroups;

    [Header("Clear Selected Pita")]
    [Tooltip("Use the same SelectionList reference used by TrashCan_0.")]
    [SerializeField] private SelectionList selectionListManager;

    [Header("Behavior")]
    [SerializeField] private bool clearPitaWhenIngredientCountChanges = true;
    [SerializeField] private bool refreshCustomerBubblesWhenIngredientCountChanges = true;

    private int currentIngredientCount;
    private readonly HashSet<string> disabledIngredients = new HashSet<string>();

    public int MaxIngredientCount => maxIngredientCount;
    public int MinIngredientCount => Mathf.Max(1, maxIngredientCount - maxRemovableIngredients);
    public int CurrentIngredientCount => currentIngredientCount;


    [Header("Pita Visual Clear")]
    [SerializeField] private PitaBuilder pitaBuilderToClear;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        ResetToDefault(false);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void ApplyIngredientCount(int requestedIngredientCount, bool clearPitaIfChanged = true)
    {
        Debug.Log($"[LevelIngredientAvailabilityManager] ApplyIngredientCount CALLED. requested={requestedIngredientCount}, clearPitaIfChanged={clearPitaIfChanged}");

        int clampedCount = Mathf.Clamp(requestedIngredientCount, MinIngredientCount, MaxIngredientCount);
        bool changed = clampedCount != currentIngredientCount;

        Debug.Log($"[LevelIngredientAvailabilityManager] current={currentIngredientCount}, clamped={clampedCount}, changed={changed}");

        // Important:
        // Clear the pita BEFORE changing disabledIngredients and BEFORE hiding objects.
        if (changed && clearPitaWhenIngredientCountChanges && clearPitaIfChanged)
        {
            Debug.Log("[LevelIngredientAvailabilityManager] Ingredient count changed. Clearing pita BEFORE disabling ingredients.");
            ClearCurrentPitaLikeTrashCan();
        }
        else
        {
            Debug.Log($"[LevelIngredientAvailabilityManager] Pita clear skipped. changed={changed}, clearPitaWhenIngredientCountChanges={clearPitaWhenIngredientCountChanges}, clearPitaIfChanged={clearPitaIfChanged}");
        }

        currentIngredientCount = clampedCount;

        Debug.Log("[LevelIngredientAvailabilityManager] Rebuilding disabled ingredients...");
        RebuildDisabledIngredients();

        Debug.Log($"[LevelIngredientAvailabilityManager] Disabled ingredients after rebuild: {string.Join(", ", disabledIngredients)}");
        ResetFryingIfNeeded();
        Debug.Log("[LevelIngredientAvailabilityManager] Applying scene object visibility...");
        ApplySceneObjectsVisibility();

        if (changed && refreshCustomerBubblesWhenIngredientCountChanges)
        {
            Debug.Log("[LevelIngredientAvailabilityManager] Refreshing all customer order bubbles...");
            RefreshAllCustomerOrderBubbles();
        }

        Debug.Log($"[LevelIngredientAvailabilityManager] Ingredient count applied: {currentIngredientCount}/{maxIngredientCount}");
    }

    /// <summary>
    /// Restores all level ingredients to the default max count.
    /// </summary>
    public void ResetToDefault(bool clearPitaIfChanged = true)
    {
        ApplyIngredientCount(maxIngredientCount, clearPitaIfChanged);
    }

    /// <summary>
    /// Returns true if this ingredient is currently allowed in the level.
    /// </summary>
    public bool IsIngredientAvailable(string ingredientId)
    {
        string id = NormalizeIngredientId(ingredientId);

        if (string.IsNullOrWhiteSpace(id))
            return true;

        return !disabledIngredients.Contains(id);
    }

    /// <summary>
    /// Static helper for any class that needs to check ingredient availability.
    /// If no manager exists in the scene, all ingredients are treated as available.
    /// </summary>
    public static bool IsIngredientCurrentlyAvailable(string ingredientId)
    {
        if (Instance == null)
            return true;

        return Instance.IsIngredientAvailable(ingredientId);
    }

    /// <summary>
    /// Returns a copy of the disabled ingredient ids.
    /// </summary>
    public List<string> GetDisabledIngredients()
    {
        return new List<string>(disabledIngredients);
    }

    public int GetRemovedIngredientAmountForCount(int ingredientCount)
    {
        int clampedCount = Mathf.Clamp(ingredientCount, MinIngredientCount, MaxIngredientCount);
        return Mathf.Clamp(maxIngredientCount - clampedCount, 0, maxRemovableIngredients);
    }

    private void RebuildDisabledIngredients()
    {
        disabledIngredients.Clear();

        if (removalOrder == null || removalOrder.Length == 0)
            return;

        int amountToRemove = Mathf.Clamp(maxIngredientCount - currentIngredientCount, 0, maxRemovableIngredients);

        for (int i = 0; i < amountToRemove && i < removalOrder.Length; i++)
        {
            string id = NormalizeIngredientId(removalOrder[i]);

            if (!string.IsNullOrWhiteSpace(id))
                disabledIngredients.Add(id);
        }
    }

    private void ApplySceneObjectsVisibility()
    {
        if (ingredientObjectsGroups == null)
            return;

        foreach (IngredientObjectsGroup group in ingredientObjectsGroups)
        {
            if (group == null)
                continue;

            string id = NormalizeIngredientId(group.ingredientId);
            bool shouldBeActive = !disabledIngredients.Contains(id);

            if (group.objectsToToggle == null)
                continue;

            foreach (GameObject obj in group.objectsToToggle)
            {
                if (obj == null)
                    continue;

                obj.SetActive(shouldBeActive);
            }
        }
    }

    /// <summary>
    /// Clears the selected pita exactly like the trash can does,
    /// then forces the pita visuals to disappear completely through PitaBuilder.
    /// </summary>
    private void ClearCurrentPitaLikeTrashCan()
    {
        Debug.Log("[LevelIngredientAvailabilityManager] ClearCurrentPitaLikeTrashCan CALLED.");

        if (selectionListManager != null)
        {
            Debug.Log($"[LevelIngredientAvailabilityManager] Using assigned SelectionList: {selectionListManager.name}");
            selectionListManager.ClearIngredients();
            Debug.Log("[LevelIngredientAvailabilityManager] selectionListManager.ClearIngredients() FINISHED.");
        }
        else if (SelectionList.Instance != null)
        {
            Debug.LogWarning("[LevelIngredientAvailabilityManager] selectionListManager is NULL. Using SelectionList.Instance.");
            SelectionList.Instance.ClearIngredients();
            Debug.Log("[LevelIngredientAvailabilityManager] SelectionList.Instance.ClearIngredients() FINISHED.");
        }
        else
        {
            Debug.LogError("[LevelIngredientAvailabilityManager] No SelectionList found! Cannot clear pita.");
        }
    }
    private void RefreshAllCustomerOrderBubbles()
    {
        Customer.RefreshAllCustomerOrderBubblesForIngredientAvailability();
    }

    private static string NormalizeIngredientId(string ingredientId)
    {
        if (string.IsNullOrWhiteSpace(ingredientId))
            return string.Empty;

        return ingredientId.Trim().ToLowerInvariant();
    }

    /// <summary>
    /// Forces the pita visual object to clear all layers.
    /// This is used as an extra safety layer after SelectionList.ClearIngredients().
    /// </summary>
    private void ClearPitaVisualsDirectly()
    {
        if (pitaBuilderToClear != null)
        {
            pitaBuilderToClear.ClearPita();
            return;
        }

        PitaBuilder foundPitaBuilder = FindObjectOfType<PitaBuilder>(true);

        if (foundPitaBuilder != null)
        {
            foundPitaBuilder.ClearPita();
        }
        else
        {
            Debug.LogWarning("[LevelIngredientAvailabilityManager] No PitaBuilder found to clear pita visuals.");
        }
    }

    private void ResetFryingIfNeeded()
    {
        if (disabledIngredients.Contains("eggplant"))
        {
            Debug.Log("[LevelIngredientAvailabilityManager] Eggplant disabled -> resetting eggplant frying process.");

            if (eggplantFryerToReset != null)
                eggplantFryerToReset.ResetFryProcess();

            if (eggplantTrayToReset != null)
                eggplantTrayToReset.ResetTray();
        }

        if (disabledIngredients.Contains("chips"))
        {
            Debug.Log("[LevelIngredientAvailabilityManager] Chips disabled -> resetting chips frying process.");

            if (chipsFryerToReset != null)
                chipsFryerToReset.ResetFryProcess();

            if (chipsTrayToReset != null)
                chipsTrayToReset.ResetTray();
        }
    }
}