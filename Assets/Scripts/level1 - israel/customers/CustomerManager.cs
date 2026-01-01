using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Modular manager that supports N simultaneous customers (N = standPoints.Count).
/// Each stand point represents a "slot".
/// - Each slot can have a first spawn delay (firstSpawnDelays[i]).
/// - Each slot can have a respawn delay after the customer leaves (respawnDelays[i]).
/// Clicking a customer serves THAT specific customer.
/// </summary>
public class CustomerManager : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 3f;

    public static CustomerManager Instance { get; private set; }

    [Header("Prefabs & Positions")]
    [SerializeField] private Customer customerPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform exitPoint;

    [Header("Stand points = number of simultaneous customers")]
    [Tooltip("Add 1 stand point for 1 customer, 2 for stage 2, 3 for future, etc.")]
    [SerializeField] private List<Transform> standPoints = new List<Transform>();

    [Header("Delays per slot (seconds)")]
    [Tooltip("Delay BEFORE the first customer appears in each slot. If missing, uses defaultFirstSpawnDelay.")]
    [SerializeField] private List<float> firstSpawnDelays = new List<float>();

    [Tooltip("Delay AFTER a customer leaves, BEFORE spawning the next customer in the same slot. If missing, uses defaultRespawnDelay.")]
    [SerializeField] private List<float> respawnDelays = new List<float>();

    [SerializeField] private float defaultFirstSpawnDelay = 0f;
    [SerializeField] private float defaultRespawnDelay = 0f;

    [Header("Customer types (Israel)")]
    [SerializeField] private List<CustomerType> customerTypes;

    [Header("Visual FX - Coins Animation")]
    [SerializeField] private CoinFlyVFX coinFlyVFX; // reference to flying coins VFX


    /// <summary>
    /// Holds per-slot state so we don't duplicate variables (no slot0/slot1/slot2 code).
    /// </summary>
    private class SlotState
    {
        public Transform standPoint;                  // Where the customer stands in this slot
        public Customer customer;                      // Current customer in this slot (null if empty)
        public Coroutine moveRoutine;                  // Move coroutine for this slot
        public Coroutine leaveRoutine;                 // Leave coroutine for this slot
        public bool isHandlingLeave;                   // Prevent double leave logic in this slot

        public float firstDelay;                       // First spawn delay for this slot
        public float respawnDelay;                     // Respawn delay for this slot (after leaving)

        public Action<bool> moodHandler;               // Stored handler so we can unsubscribe safely
    }

    private readonly List<SlotState> slots = new List<SlotState>();

    // Fast routing: when you click a customer, we instantly know which slot he belongs to
    private readonly Dictionary<Customer, int> customerToSlot = new Dictionary<Customer, int>();

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
        BuildSlotsFromInspector(); // Read standPoints + delays, create slots list
        StartAllSlots();           // Spawn customers in each slot according to delays
    }

    /// <summary>
    /// Builds the slots list based on Inspector configuration.
    /// </summary>
    private void BuildSlotsFromInspector()
    {
        slots.Clear();
        customerToSlot.Clear();

        if (standPoints == null || standPoints.Count == 0)
        {
            Debug.LogWarning("CustomerManager: No standPoints set. Add at least 1 stand point.");
            return;
        }

        for (int i = 0; i < standPoints.Count; i++)
        {
            Transform sp = standPoints[i];
            if (sp == null)
            {
                Debug.LogWarning($"CustomerManager: standPoints[{i}] is null. This slot will be skipped.");
                continue;
            }

            float first = (firstSpawnDelays != null && i < firstSpawnDelays.Count) ? firstSpawnDelays[i] : defaultFirstSpawnDelay;
            float resp = (respawnDelays != null && i < respawnDelays.Count) ? respawnDelays[i] : defaultRespawnDelay;

            slots.Add(new SlotState
            {
                standPoint = sp,
                firstDelay = Mathf.Max(0f, first),
                respawnDelay = Mathf.Max(0f, resp)
            });
        }
    }

    /// <summary>
    /// Starts spawning in all slots. Each slot spawns after its own firstDelay.
    /// </summary>
    private void StartAllSlots()
    {
        if (slots.Count == 0) return;

        for (int i = 0; i < slots.Count; i++)
        {
            // Spawn each slot independently (parallel behavior)
            StartCoroutine(SpawnInSlotAfterDelay(i, slots[i].firstDelay));
        }
    }

    /// <summary>
    /// Spawns a customer in a slot after a delay.
    /// </summary>
    private IEnumerator SpawnInSlotAfterDelay(int slotIndex, float delay)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        SpawnCustomerInSlot(slotIndex);
    }

    /// <summary>
    /// Spawns a new customer into the given slot and moves him to that slot's stand point.
    /// </summary>
    private void SpawnCustomerInSlot(int slotIndex)
    {
        if (!IsValidSlot(slotIndex)) return;

        // Validate required references
        if (customerPrefab == null || spawnPoint == null || exitPoint == null)
        {
            Debug.LogWarning("CustomerManager: Missing customerPrefab/spawnPoint/exitPoint reference.");
            return;
        }

        // Validate customer types
        if (customerTypes == null || customerTypes.Count == 0)
        {
            Debug.LogWarning("CustomerManager: no customer types defined!");
            return;
        }

        SlotState slot = slots[slotIndex];

        // If something remains in slot, clean it up first
        CleanupSlot(slotIndex);

        // Choose random customer type
        CustomerType chosen = customerTypes[UnityEngine.Random.Range(0, customerTypes.Count)];

        // Instantiate + init customer
        slot.customer = Instantiate(customerPrefab, spawnPoint.position, Quaternion.identity);
        slot.customer.Init(chosen);

        // Register mapping (customer -> slotIndex) so clicks can route quickly
        customerToSlot[slot.customer] = slotIndex;

        // Subscribe to mood timer (slot-specific handler)
        if (slot.customer.MoodTimer != null)
        {
            // Create a unique handler for THIS slot so we know exactly which slot finished
            slot.moodHandler = (served) => OnCustomerFinishedInSlot(slotIndex, served);

            // Safety: ensure we don't double subscribe
            slot.customer.MoodTimer.OnCustomerFinished -= slot.moodHandler;
            slot.customer.MoodTimer.OnCustomerFinished += slot.moodHandler;
        }
        else
        {
            Debug.LogWarning("CustomerManager: Customer.MoodTimer is not assigned on the Customer prefab.");
        }

        // Move to stand point
        StartMove(slotIndex, slot.customer.transform, slot.standPoint.position);
    }

    /// <summary>
    /// Called when a customer's mood timer finishes (served or time-up), for a SPECIFIC slot.
    /// </summary>
    private void OnCustomerFinishedInSlot(int slotIndex, bool served)
    {
        // Customer is done -> leave sequence
        StartLeaveSequence(slotIndex);
    }

    /// <summary>
    /// Serves the customer that was clicked.
    /// This replaces the old "currentCustomer" approach and supports any number of customers.
    /// </summary>
    public void ServeCustomer(Customer target)
    {
        if (target == null)
        {
            Debug.LogWarning("ServeCustomer called with null target.");
            return;
        }

        // Ignore if customer is already leaving
        if (target.IsLeaving)
            return;

        // Selection list must exist
        if (SelectionList.Instance == null)
        {
            Debug.LogWarning("SelectionList.Instance is null.");
            return;
        }

        // Route target to its slot
        if (!customerToSlot.TryGetValue(target, out int slotIndex))
        {
            Debug.LogWarning("ServeCustomer: target customer is not tracked by manager.");
            return;
        }

        List<string> ingredients = SelectionList.Instance.GetSelectedIngredients();
        bool ok = target.IsOrderCorrect(ingredients);

        if (ok)
        {
            Debug.Log("Correct order!");

            // Add reward
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddMoney(30);
                coinFlyVFX.PlayCoinsFromWorld(target.transform);

            }


            // Tell THIS customer's timer he was served in time
            if (target.MoodTimer != null)
                target.MoodTimer.CustomerServed();

            // Clear selection after serving
            SelectionList.Instance.ClearIngredients();

            // Start leaving for this slot
            StartLeaveSequence(slotIndex);
            return;
        }

        Debug.Log("Wrong order!");

        // Clear selection on wrong order
        SelectionList.Instance.ClearIngredients();

        // Wrong order -> leave immediately
        StartLeaveSequence(slotIndex);
    }

    /// <summary>
    /// Starts the leave sequence for a specific slot.
    /// </summary>
    private void StartLeaveSequence(int slotIndex)
    {
        if (!IsValidSlot(slotIndex)) return;

        SlotState slot = slots[slotIndex];

        // Prevent double starts
        if (slot.isHandlingLeave) return;
        slot.isHandlingLeave = true;

        // Mark leaving to prevent further interactions
        if (slot.customer != null)
            slot.customer.MarkLeaving();

        // Stop existing leave routine (if any)
        if (slot.leaveRoutine != null)
            StopCoroutine(slot.leaveRoutine);

        slot.leaveRoutine = StartCoroutine(CustomerLeaveAndRespawn(slotIndex));
    }

    /// <summary>
    /// Moves the slot customer to exit, cleans him up, then respawns after delay.
    /// </summary>
    private IEnumerator CustomerLeaveAndRespawn(int slotIndex)
    {
        if (!IsValidSlot(slotIndex)) yield break;
        SlotState slot = slots[slotIndex];

        // Stop movement to stand if still running
        if (slot.moveRoutine != null)
        {
            StopCoroutine(slot.moveRoutine);
            slot.moveRoutine = null;
        }

        // Move customer to exit
        if (slot.customer != null && exitPoint != null)
        {
            yield return MoveToPoint(slot.customer.transform, exitPoint.position);
        }

        // Cleanup customer and routines
        CleanupSlot(slotIndex);

        // Release leave lock
        slot.isHandlingLeave = false;

        // Delay before spawning next customer in same slot
        if (slot.respawnDelay > 0f)
            yield return new WaitForSeconds(slot.respawnDelay);

        // Spawn next customer in same slot
        SpawnCustomerInSlot(slotIndex);
    }

    /// <summary>
    /// Cleans up a slot: unsubscribe events, destroy customer, stop routines.
    /// </summary>
    private void CleanupSlot(int slotIndex)
    {
        if (!IsValidSlot(slotIndex)) return;

        SlotState slot = slots[slotIndex];

        // Unsubscribe mood timer events + destroy customer
        if (slot.customer != null)
        {
            if (slot.customer.MoodTimer != null && slot.moodHandler != null)
                slot.customer.MoodTimer.OnCustomerFinished -= slot.moodHandler;

            // Remove mapping
            customerToSlot.Remove(slot.customer);

            Destroy(slot.customer.gameObject);
            slot.customer = null;
        }

        // Stop movement routine
        if (slot.moveRoutine != null)
        {
            StopCoroutine(slot.moveRoutine);
            slot.moveRoutine = null;
        }

        // Stop leave routine
        if (slot.leaveRoutine != null)
        {
            StopCoroutine(slot.leaveRoutine);
            slot.leaveRoutine = null;
        }

        // Reset flags/handlers
        slot.isHandlingLeave = false;
        slot.moodHandler = null;
    }

    /// <summary>
    /// Starts movement for a slot. Each slot has its own move routine.
    /// </summary>
    private void StartMove(int slotIndex, Transform t, Vector3 target)
    {
        if (!IsValidSlot(slotIndex)) return;

        SlotState slot = slots[slotIndex];

        if (slot.moveRoutine != null)
            StopCoroutine(slot.moveRoutine);

        slot.moveRoutine = StartCoroutine(MoveToPoint(t, target));
    }

    /// <summary>
    /// Moves a transform to a target point over time.
    /// </summary>
    private IEnumerator MoveToPoint(Transform t, Vector3 target)
    {
        while (t != null && Vector3.Distance(t.position, target) > 0.01f)
        {
            t.position = Vector3.MoveTowards(t.position, target, speed * Time.deltaTime);
            yield return null;
        }

        if (t != null)
            t.position = target;
    }

    /// <summary>
    /// Returns true if slotIndex is inside slots list.
    /// </summary>
    private bool IsValidSlot(int slotIndex)
    {
        return slotIndex >= 0 && slotIndex < slots.Count;
    }
}
