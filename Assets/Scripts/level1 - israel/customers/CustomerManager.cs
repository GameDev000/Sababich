// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// /// <summary>
// /// Modular manager that supports N simultaneous customers.
// /// Each stand point represents one customer slot.
// /// This manager is used by all levels, so level-specific statistics are routed by levelNumber.
// /// </summary>
// public class CustomerManager : MonoBehaviour
// {
//     [Header("Movement Settings")]
//     [SerializeField] private float speed = 3f;

//     [Header("Exit Speed After Service")]
//     [SerializeField] private float goodServiceExitSpeed = 3f;
//     [SerializeField] private float badServiceExitSpeed = 1.2f;

//     public static CustomerManager Instance { get; private set; }

//     [Header("Prefabs & Positions")]
//     [SerializeField] private CustomerType playerBaseBodyType;
//     [SerializeField] private Customer customerPrefabNormal;
//     [SerializeField] private Customer customerPrefabPlayerHead;
//     [SerializeField] private Transform spawnPoint;
//     [SerializeField] private Transform exitPoint;

//     [Header("Stand points = number of simultaneous customers")]
//     [Tooltip("Add 1 stand point for 1 customer, 2 for 2 customers, 3 for 3 customers.")]
//     [SerializeField] private List<Transform> standPoints = new List<Transform>();

//     [Header("Delays per slot (seconds)")]
//     [Tooltip("Delay before the first customer appears in each slot.")]
//     [SerializeField] private List<float> firstSpawnDelays = new List<float>();

//     [Tooltip("Delay after a customer leaves, before spawning the next customer in the same slot.")]
//     [SerializeField] private List<float> respawnDelays = new List<float>();

//     [SerializeField] private float defaultFirstSpawnDelay = 0f;
//     [SerializeField] private float defaultRespawnDelay = 0f;

//     [Header("Customer Types")]
//     [SerializeField] private List<CustomerType> customerTypes = new List<CustomerType>();

//     [Header("Visual FX - Coins Animation")]
//     [SerializeField] private CoinFlyVFX coinFlyVFX;

//     [Header("Removing Ingredients")]
//     [SerializeField] private int maxMissingItems = 0;

//     [Header("Scoring Per Order")]
//     [SerializeField] private int baseOrderReward = 30;
//     [SerializeField] private int wrongDishPenalty = -5;
//     [SerializeField] private int alergicServePenalty = -10;
//     [SerializeField] private int alergicOrderReward = 20;

//     [Header("Level Number")]
//     [Tooltip("Level1=1, Level1.1=11, Level1.2=12, Level2=2, Level2.1=21, Level2.2=22, Level3=3")]
//     [SerializeField] private int levelNumber = 1;

//     [Header("Instructions UI")]
//     [SerializeField] private FeatureHintsSequence instructionManager;
//     [SerializeField] private bool shouldRunInstructions_level2 = true;
//     [SerializeField] private bool shouldRunInstructions_level3 = true;

//     [Header("Customer Voice Feedback")]
//     [SerializeField] private AudioSource customerVoiceAudioSource;
//     [SerializeField, Range(0f, 1f)] private float customerVoiceVolume = 1f;


//     [Header("Reminders")]
//     [SerializeField] private AudioSource reminderAudioSource;
//     [SerializeField, Range(0f, 1f)] private float reminderVolume = 1f;
//     private bool reminderActive = true;

//     private CustomerType lastSpawnedType = null;
//     private readonly CustomerType[] slotTypes = new CustomerType[3];

//     private int currentMaxConcurrentCustomers = 1;

//     private class SlotState
//     {
//         public Transform standPoint;
//         public Customer customer;

//         public Coroutine spawnRoutine;
//         public Coroutine moveRoutine;
//         public Coroutine leaveRoutine;

//         public bool isHandlingLeave;

//         public float firstDelay;
//         public float respawnDelay;

//         public float nextLeaveSpeedOverride = -1f;
//         public float currentLeaveSpeed = -1f;

//         public Action<bool> moodHandler;
//     }

//     private readonly List<SlotState> slots = new List<SlotState>();
//     private readonly Dictionary<Customer, int> customerToSlot = new Dictionary<Customer, int>();

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
//         if (PlayerFaceStore.HasAll)
//         {
//             RegisterPlayerCustomer(PlayerFaceStore.Happy, PlayerFaceStore.Angry, PlayerFaceStore.Furious);
//         }

//         BuildSlotsFromInspector();

//         currentMaxConcurrentCustomers = GetMaxSupportedConcurrentCustomers();

//         StartAllSlots();

//         shouldRunInstructions_level2 = false;
//         shouldRunInstructions_level3 = false;
//     }

//     public int GetMaxSupportedConcurrentCustomers()
//     {
//         int supported = slots.Count;

//         if (supported <= 0 && standPoints != null)
//         {
//             supported = standPoints.Count;
//         }

//         return Mathf.Clamp(supported, 1, 3);
//     }

//     public int GetCurrentMaxConcurrentCustomers()
//     {
//         int supported = GetMaxSupportedConcurrentCustomers();

//         if (currentMaxConcurrentCustomers <= 0)
//         {
//             currentMaxConcurrentCustomers = supported;
//         }

//         return Mathf.Clamp(currentMaxConcurrentCustomers, 1, supported);
//     }

//     public void SetMaxConcurrentCustomers(int amount)
//     {
//         int supported = GetMaxSupportedConcurrentCustomers();
//         int newLimit = Mathf.Clamp(amount, 1, supported);
//         int previousLimit = GetCurrentMaxConcurrentCustomers();

//         currentMaxConcurrentCustomers = newLimit;

//         Debug.Log($"[CustomerManager] Max concurrent customers changed: {previousLimit} -> {newLimit} / supported={supported}");

//         if (newLimit > previousLimit)
//         {
//             for (int i = previousLimit; i < newLimit && i < slots.Count; i++)
//             {
//                 if (CanStartSpawnForSlot(i))
//                 {
//                     StartSlotSpawnCoroutine(i, 0f);
//                 }
//             }
//         }
//     }

//     private void BuildSlotsFromInspector()
//     {
//         slots.Clear();
//         customerToSlot.Clear();

//         if (standPoints == null || standPoints.Count == 0)
//         {
//             Debug.LogWarning("CustomerManager: No standPoints set. Add at least 1 stand point.");
//             return;
//         }

//         for (int i = 0; i < standPoints.Count; i++)
//         {
//             Transform sp = standPoints[i];

//             if (sp == null)
//             {
//                 Debug.LogWarning($"CustomerManager: standPoints[{i}] is null. This slot will be skipped.");
//                 continue;
//             }

//             float first = (firstSpawnDelays != null && i < firstSpawnDelays.Count)
//                 ? firstSpawnDelays[i]
//                 : defaultFirstSpawnDelay;

//             float resp = (respawnDelays != null && i < respawnDelays.Count)
//                 ? respawnDelays[i]
//                 : defaultRespawnDelay;

//             slots.Add(new SlotState
//             {
//                 standPoint = sp,
//                 firstDelay = Mathf.Max(0f, first),
//                 respawnDelay = Mathf.Max(0f, resp)
//             });
//         }
//     }

//     private void StartAllSlots()
//     {
//         if (slots.Count == 0)
//         {
//             return;
//         }

//         for (int i = 0; i < slots.Count; i++)
//         {
//             if (!IsSlotAllowedByRuntimeLimit(i))
//             {
//                 continue;
//             }

//             StartSlotSpawnCoroutine(i, slots[i].firstDelay);
//         }
//     }

//     private void StartSlotSpawnCoroutine(int slotIndex, float delay)
//     {
//         if (!IsValidSlot(slotIndex))
//         {
//             return;
//         }

//         if (!IsSlotAllowedByRuntimeLimit(slotIndex))
//         {
//             return;
//         }

//         SlotState slot = slots[slotIndex];

//         if (slot.customer != null || slot.isHandlingLeave)
//         {
//             return;
//         }

//         if (slot.spawnRoutine != null)
//         {
//             StopCoroutine(slot.spawnRoutine);
//         }

//         slot.spawnRoutine = StartCoroutine(SpawnInSlotAfterDelay(slotIndex, delay));
//     }

//     private IEnumerator SpawnInSlotAfterDelay(int slotIndex, float delay)
//     {
//         if (delay > 0f)
//         {
//             yield return new WaitForSeconds(delay);
//         }

//         if (!IsValidSlot(slotIndex))
//         {
//             yield break;
//         }

//         SlotState slot = slots[slotIndex];
//         slot.spawnRoutine = null;

//         if (!IsSlotAllowedByRuntimeLimit(slotIndex))
//         {
//             yield break;
//         }

//         SpawnCustomerInSlot(slotIndex);
//     }

//     private void SpawnCustomerInSlot(int slotIndex)
//     {
//         if (!IsValidSlot(slotIndex))
//         {
//             return;
//         }

//         if (!IsSlotAllowedByRuntimeLimit(slotIndex))
//         {
//             return;
//         }

//         if (customerPrefabNormal == null || spawnPoint == null || exitPoint == null)
//         {
//             Debug.LogWarning("CustomerManager: Missing customerPrefabNormal/spawnPoint/exitPoint reference.");
//             return;
//         }

//         if (customerTypes == null || customerTypes.Count == 0)
//         {
//             Debug.LogWarning("CustomerManager: no customer types defined!");
//             return;
//         }

//         SlotState slot = slots[slotIndex];

//         CleanupSlot(slotIndex);

//         CustomerType type0 = (slots.Count > 0 && slotTypes.Length > 0) ? slotTypes[0] : null;
//         CustomerType type1 = (slots.Count > 1 && slotTypes.Length > 1) ? slotTypes[1] : null;
//         CustomerType type2 = (slots.Count > 2 && slotTypes.Length > 2) ? slotTypes[2] : null;

//         CustomerType chosen = null;

//         int attempts = 0;
//         int maxAttempts = 30;

//         do
//         {
//             chosen = customerTypes[UnityEngine.Random.Range(0, customerTypes.Count)];
//             attempts++;

//             if (attempts >= maxAttempts)
//             {
//                 if (chosen != type0 && chosen != type1 && chosen != type2)
//                 {
//                     break;
//                 }

//                 chosen = customerTypes[UnityEngine.Random.Range(0, customerTypes.Count)];
//                 break;
//             }
//         }
//         while (
//             chosen == lastSpawnedType ||
//             (slotIndex != 0 && chosen == type0) ||
//             (slotIndex != 1 && chosen == type1) ||
//             (slotIndex != 2 && chosen == type2)
//         );

//         lastSpawnedType = chosen;

//         if (slotIndex >= 0 && slotIndex < slotTypes.Length)
//         {
//             slotTypes[slotIndex] = chosen;
//         }

//         bool isPlayerCustomer = chosen != null && chosen.name == "player_customer";
//         Customer prefabToSpawn = customerPrefabNormal;

//         if (isPlayerCustomer)
//         {
//             if (customerPrefabPlayerHead != null)
//             {
//                 prefabToSpawn = customerPrefabPlayerHead;
//             }
//             else
//             {
//                 Debug.LogWarning("CustomerManager: player_customer chosen but customerPrefabPlayerHead is not assigned. Falling back to normal prefab.");
//             }
//         }

//         slot.customer = Instantiate(prefabToSpawn, spawnPoint.position, Quaternion.identity);
//         slot.customer.Init(chosen, maxMissingItems);

//         RegisterCustomerArrived();

//         if (chosen != null && chosen.scoreIfNotServed)
//         {
//             RegisterGlutenChildAppeared();
//         }

//         customerToSlot[slot.customer] = slotIndex;

//         if (slot.customer.MoodTimer != null)
//         {
//             slot.moodHandler = (served) => OnCustomerFinishedInSlot(slotIndex, served);

//             slot.customer.MoodTimer.OnCustomerFinished -= slot.moodHandler;
//             slot.customer.MoodTimer.OnCustomerFinished += slot.moodHandler;
//         }
//         else
//         {
//             Debug.LogWarning("CustomerManager: Customer.MoodTimer is not assigned on the Customer prefab.");
//         }

//         StartMove(slotIndex, slot.customer.transform, slot.standPoint.position);

//         if (instructionManager != null)
//         {
//             instructionManager.OnCustomerSpawned();
//         }
//     }

//     private void OnCustomerFinishedInSlot(int slotIndex, bool served)
//     {
//         if (!IsValidSlot(slotIndex))
//         {
//             return;
//         }

//         SlotState slot = slots[slotIndex];
//         Customer c = slot.customer;

//         if (!served && c != null && c.Data.scoreIfNotServed)
//         {
//             Debug.Log("Special customer: NOT served → reward!");

//             if (ScoreManager.Instance != null)
//             {
//                 ScoreManager.Instance.AddMoney(alergicOrderReward);

//                 if (coinFlyVFX != null)
//                 {
//                     coinFlyVFX.PlayCoinsFromWorld(c.transform, alergicOrderReward, true);
//                 }
//             }
//         }

//         StartLeaveSequence(slotIndex);
//     }

//     private void RegisterServedDish(bool isPerfect)
//     {
//         if (levelNumber == 1)
//         {
//             LevelOneState.TotalServedDishes++;

//             if (isPerfect)
//             {
//                 LevelOneState.PerfectServedDishes++;
//             }
//         }
//         else if (levelNumber == 11)
//         {
//             LevelOneOneState.TotalServedDishes++;

//             if (isPerfect)
//             {
//                 LevelOneOneState.PerfectServedDishes++;
//             }
//         }
//         else if (levelNumber == 12)
//         {
//             LevelOneTwoState.TotalServedDishes++;

//             if (isPerfect)
//             {
//                 LevelOneTwoState.PerfectServedDishes++;
//             }
//         }
//         else if (levelNumber == 2)
//         {
//             LevelTwoState.TotalServedDishes++;

//             if (isPerfect)
//             {
//                 LevelTwoState.PerfectServedDishes++;
//             }
//         }
//         else if (levelNumber == 21)
//         {
//             LevelTwoOneState.TotalServedDishes++;

//             if (isPerfect)
//             {
//                 LevelTwoOneState.PerfectServedDishes++;
//             }
//         }
//         else if (levelNumber == 22)
//         {
//             LevelTwoTwoState.TotalServedDishes++;

//             if (isPerfect)
//             {
//                 LevelTwoTwoState.PerfectServedDishes++;
//             }
//         }
//         else if (levelNumber == 3)
//         {
//             LevelThreeState.TotalServedDishes++;

//             if (isPerfect)
//             {
//                 LevelThreeState.PerfectServedDishes++;
//             }
//         }
//         else
//         {
//             Debug.LogWarning("[CustomerManager] RegisterServedDish: unsupported levelNumber=" + levelNumber);
//         }
//     }

//     public void RegisterDuplicateIngredientClick()
//     {
//         if (levelNumber == 1)
//         {
//             LevelOneState.DuplicateIngredientClicks++;
//         }
//         else if (levelNumber == 11)
//         {
//             LevelOneOneState.DuplicateIngredientClicks++;
//         }
//         else if (levelNumber == 12)
//         {
//             LevelOneTwoState.DuplicateIngredientClicks++;
//         }
//         else if (levelNumber == 2)
//         {
//             LevelTwoState.DuplicateIngredientClicks++;
//         }
//         else if (levelNumber == 21)
//         {
//             LevelTwoOneState.DuplicateIngredientClicks++;
//         }
//         else if (levelNumber == 22)
//         {
//             LevelTwoTwoState.DuplicateIngredientClicks++;
//         }
//         else if (levelNumber == 3)
//         {
//             LevelThreeState.DuplicateIngredientClicks++;
//         }
//         else
//         {
//             Debug.LogWarning("[CustomerManager] RegisterDuplicateIngredientClick: unsupported levelNumber=" + levelNumber);
//         }
//     }

//     public void ServeCustomer(Customer target)
//     {
//         if (target == null)
//         {
//             Debug.LogWarning("ServeCustomer called with null target.");
//             return;
//         }

//         if (target.IsLeaving)
//         {
//             return;
//         }

//         if (SelectionList.Instance == null)
//         {
//             Debug.LogWarning("SelectionList.Instance is null.");
//             return;
//         }

//         if (!customerToSlot.TryGetValue(target, out int slotIndex))
//         {
//             Debug.LogWarning("ServeCustomer: target customer is not tracked by manager.");
//             return;
//         }

//         List<string> ingredients = SelectionList.Instance.GetSelectedIngredients();

//         if (target.Data != null && target.Data.scoreIfNotServed)
//         {
//             Debug.Log("Special customer: served -> NO score.");
//             PlayCustomerVoice(target, false);

//             RegisterGlutenChildServed();

//             if (ScoreManager.Instance != null)
//             {
//                 ScoreManager.Instance.FlashPenaltyUI();
//                 ScoreManager.Instance.AddMoney(alergicServePenalty);

//                 if (coinFlyVFX != null)
//                 {
//                     coinFlyVFX.PlayPenaltyFromWorld(target.transform, Mathf.Abs(alergicServePenalty), true);
//                 }
//             }

//             if (ControlPanelUI.MarkAddedItemsEnabled)
//             {
//                 target.PreserveIngredientMarkersUntilDestroyed();
//             }

//             SelectionList.Instance.ClearIngredients();

//             StartCoroutine(LeaveAfterWrongFeedback(slotIndex, target, 0.4f));
//             return;
//         }

//         int mistakes = CountOrderMistakes(target, ingredients);
//         bool ok = mistakes == 0;

//         RegisterServedDish(ok);

//         if (ok)
//         {
//             Debug.Log("Correct order!");
//             PlayCustomerVoice(target, true);

//             if (ScoreManager.Instance != null)
//             {
//                 ScoreManager.Instance.AddMoney(baseOrderReward);

//                 if (baseOrderReward > 0 && coinFlyVFX != null)
//                 {
//                     coinFlyVFX.PlayCoinsFromWorld(target.transform);
//                 }
//             }

//             SetNextLeaveSpeed(slotIndex, goodServiceExitSpeed);

//             if (target.MoodTimer != null)
//             {
//                 target.MoodTimer.CustomerServed();
//             }

//             if (ControlPanelUI.MarkAddedItemsEnabled)
//             {
//                 target.PreserveIngredientMarkersUntilDestroyed();
//             }

//             SelectionList.Instance.ClearIngredients();

//             StartLeaveSequence(slotIndex);
//             return;
//         }

//         Debug.Log($"Wrong order! Mistakes={mistakes}");
//         PlayCustomerVoice(target, false);

//         if (ScoreManager.Instance != null)
//         {
//             ScoreManager.Instance.FlashPenaltyUI();
//             ScoreManager.Instance.AddMoney(wrongDishPenalty);

//             if (coinFlyVFX != null)
//             {
//                 coinFlyVFX.PlayPenaltyFromWorld(target.transform, Mathf.Abs(wrongDishPenalty));
//             }
//         }

//         if (ControlPanelUI.MarkAddedItemsEnabled)
//         {
//             target.PreserveIngredientMarkersUntilDestroyed();
//         }

//         SelectionList.Instance.ClearIngredients();

//         StartCoroutine(LeaveAfterWrongFeedback(slotIndex, target, 0.4f));
//     }

//     private int CountOrderMistakes(Customer target, List<string> givenIngredients)
//     {
//         if (target == null)
//         {
//             return 0;
//         }

//         List<string> activeOrder = target.GetActiveRequiredIngredients();

//         if (activeOrder == null)
//         {
//             return 0;
//         }

//         List<string> required = new List<string>();

//         foreach (string r in activeOrder)
//         {
//             required.Add(r.ToLower());
//         }

//         List<string> given = new List<string>();

//         if (givenIngredients != null)
//         {
//             foreach (string g in givenIngredients)
//             {
//                 given.Add(g.ToLower());
//             }
//         }

//         int mistakes = 0;

//         foreach (string r in required)
//         {
//             if (!given.Contains(r))
//             {
//                 mistakes++;
//             }
//         }

//         foreach (string g in given)
//         {
//             if (!required.Contains(g))
//             {
//                 mistakes++;
//             }
//         }

//         return mistakes;
//     }

//     private void SetNextLeaveSpeed(int slotIndex, float leaveSpeed)
//     {
//         if (!IsValidSlot(slotIndex))
//         {
//             return;
//         }

//         slots[slotIndex].nextLeaveSpeedOverride = Mathf.Max(0.01f, leaveSpeed);
//     }

//     private void StartLeaveSequence(int slotIndex)
//     {
//         if (!IsValidSlot(slotIndex))
//         {
//             return;
//         }

//         SlotState slot = slots[slotIndex];

//         if (slot.isHandlingLeave)
//         {
//             return;
//         }

//         slot.isHandlingLeave = true;

//         slot.currentLeaveSpeed = slot.nextLeaveSpeedOverride > 0f
//             ? slot.nextLeaveSpeedOverride
//             : speed;

//         slot.nextLeaveSpeedOverride = -1f;

//         Debug.Log($"[CustomerManager] Customer leaving from slot {slotIndex} with speed {slot.currentLeaveSpeed}");

//         if (slot.customer != null)
//         {
//             slot.customer.MarkLeaving();
//         }

//         if (slot.leaveRoutine != null)
//         {
//             StopCoroutine(slot.leaveRoutine);
//         }

//         slot.leaveRoutine = StartCoroutine(CustomerLeaveAndRespawn(slotIndex));
//     }

//     private IEnumerator CustomerLeaveAndRespawn(int slotIndex)
//     {
//         if (!IsValidSlot(slotIndex))
//         {
//             yield break;
//         }

//         SlotState slot = slots[slotIndex];

//         if (slot.moveRoutine != null)
//         {
//             StopCoroutine(slot.moveRoutine);
//             slot.moveRoutine = null;
//         }

//         if (slot.customer != null && exitPoint != null)
//         {
//             float leaveSpeed = slot.currentLeaveSpeed > 0f ? slot.currentLeaveSpeed : speed;
//             yield return MoveToPoint(slot.customer.transform, exitPoint.position, leaveSpeed);
//         }

//         CleanupSlot(slotIndex);

//         slot.isHandlingLeave = false;

//         if (!IsSlotAllowedByRuntimeLimit(slotIndex))
//         {
//             yield break;
//         }

//         if (slot.respawnDelay > 0f)
//         {
//             yield return new WaitForSeconds(slot.respawnDelay);
//         }

//         if (!IsSlotAllowedByRuntimeLimit(slotIndex))
//         {
//             yield break;
//         }

//         SpawnCustomerInSlot(slotIndex);
//     }

//     private void CleanupSlot(int slotIndex)
//     {
//         if (!IsValidSlot(slotIndex))
//         {
//             return;
//         }

//         SlotState slot = slots[slotIndex];

//         if (slot.customer != null)
//         {
//             if (slot.customer.MoodTimer != null && slot.moodHandler != null)
//             {
//                 slot.customer.MoodTimer.OnCustomerFinished -= slot.moodHandler;
//             }

//             customerToSlot.Remove(slot.customer);

//             Destroy(slot.customer.gameObject);
//             slot.customer = null;
//         }

//         if (slot.spawnRoutine != null)
//         {
//             StopCoroutine(slot.spawnRoutine);
//             slot.spawnRoutine = null;
//         }

//         if (slot.moveRoutine != null)
//         {
//             StopCoroutine(slot.moveRoutine);
//             slot.moveRoutine = null;
//         }

//         if (slot.leaveRoutine != null)
//         {
//             StopCoroutine(slot.leaveRoutine);
//             slot.leaveRoutine = null;
//         }

//         slot.isHandlingLeave = false;
//         slot.moodHandler = null;
//         slot.nextLeaveSpeedOverride = -1f;
//         slot.currentLeaveSpeed = -1f;

//         if (slotIndex >= 0 && slotIndex < slotTypes.Length)
//         {
//             slotTypes[slotIndex] = null;
//         }
//     }

//     private void StartMove(int slotIndex, Transform t, Vector3 target)
//     {
//         if (!IsValidSlot(slotIndex))
//         {
//             return;
//         }

//         SlotState slot = slots[slotIndex];

//         if (slot.moveRoutine != null)
//         {
//             StopCoroutine(slot.moveRoutine);
//         }

//         slot.moveRoutine = StartCoroutine(MoveToPoint(t, target, speed));
//     }

//     private IEnumerator MoveToPoint(Transform t, Vector3 target, float moveSpeed)
//     {
//         moveSpeed = Mathf.Max(0.01f, moveSpeed);

//         while (t != null && Vector3.Distance(t.position, target) > 0.01f)
//         {
//             t.position = Vector3.MoveTowards(t.position, target, moveSpeed * Time.deltaTime);
//             yield return null;
//         }

//         if (t != null)
//         {
//             t.position = target;
//         }
//     }

//     private bool IsValidSlot(int slotIndex)
//     {
//         return slotIndex >= 0 && slotIndex < slots.Count;
//     }

//     private bool IsSlotAllowedByRuntimeLimit(int slotIndex)
//     {
//         return slotIndex >= 0 && slotIndex < GetCurrentMaxConcurrentCustomers();
//     }

//     private bool CanStartSpawnForSlot(int slotIndex)
//     {
//         if (!IsValidSlot(slotIndex))
//         {
//             return false;
//         }

//         if (!IsSlotAllowedByRuntimeLimit(slotIndex))
//         {
//             return false;
//         }

//         SlotState slot = slots[slotIndex];

//         return slot.customer == null && !slot.isHandlingLeave && slot.spawnRoutine == null;
//     }

//     private IEnumerator LeaveAfterWrongFeedback(int slotIndex, Customer target, float delay)
//     {
//         if (target != null)
//         {
//             target.MarkLeaving();
//         }

//         if (target != null && target.MoodTimer != null)
//         {
//             target.MoodTimer.ShowAngryNow(target);
//         }

//         yield return new WaitForSeconds(delay);

//         SetNextLeaveSpeed(slotIndex, badServiceExitSpeed);
//         StartLeaveSequence(slotIndex);
//     }

//     public void AddCustomerType(CustomerType type)
//     {
//         if (type == null)
//         {
//             return;
//         }

//         if (customerTypes == null)
//         {
//             customerTypes = new List<CustomerType>();
//         }

//         if (!customerTypes.Contains(type))
//         {
//             customerTypes.Add(type);
//         }
//     }

//     public void RegisterPlayerCustomer(Sprite happy, Sprite angry, Sprite furious)
//     {
//         CustomerType baseType = playerBaseBodyType;

//         if (baseType == null)
//         {
//             Debug.LogWarning("RegisterPlayerCustomer: playerBaseBodyType is not assigned.");
//             return;
//         }

//         CustomerType playerType =
//             RuntimeCustomerFactory.CreateFromBase(baseType, happy, angry, furious, "player_customer");

//         AddCustomerType(playerType);
//     }

//     private void PlayCustomerVoice(Customer target, bool success)
//     {
//         if (target == null || target.Data == null)
//         {
//             return;
//         }

//         AudioClip clip = success
//             ? target.Data.successVoiceClip
//             : target.Data.failureVoiceClip;

//         if (clip == null)
//         {
//             return;
//         }

//         if (customerVoiceAudioSource != null)
//         {
//             customerVoiceAudioSource.PlayOneShot(clip, customerVoiceVolume);
//         }
//     }

//     private void RegisterCustomerArrived()
//     {
//         if (levelNumber == 1)
//         {
//             LevelOneState.CustomersArrived++;
//         }
//         else if (levelNumber == 11)
//         {
//             LevelOneOneState.CustomersArrived++;
//         }
//         else if (levelNumber == 12)
//         {
//             LevelOneTwoState.CustomersArrived++;
//         }
//         else if (levelNumber == 2)
//         {
//             LevelTwoState.CustomersArrived++;
//         }
//         else if (levelNumber == 21)
//         {
//             LevelTwoOneState.CustomersArrived++;
//         }
//         else if (levelNumber == 22)
//         {
//             LevelTwoTwoState.CustomersArrived++;
//         }
//         else if (levelNumber == 3)
//         {
//             LevelThreeState.CustomersArrived++;
//         }
//         else
//         {
//             Debug.LogWarning("[CustomerManager] RegisterCustomerArrived: unsupported levelNumber=" + levelNumber);
//         }
//     }

//     private void RegisterGlutenChildAppeared()
//     {
//         if (levelNumber == 1)
//         {
//             LevelOneState.GlutenChildAppeared++;
//         }
//         else if (levelNumber == 11)
//         {
//             LevelOneOneState.GlutenChildAppeared++;
//         }
//         else if (levelNumber == 12)
//         {
//             LevelOneTwoState.GlutenChildAppeared++;
//         }
//         else if (levelNumber == 2)
//         {
//             LevelTwoState.GlutenChildAppeared++;
//         }
//         else if (levelNumber == 21)
//         {
//             LevelTwoOneState.GlutenChildAppeared++;
//         }
//         else if (levelNumber == 22)
//         {
//             LevelTwoTwoState.GlutenChildAppeared++;
//         }
//         else if (levelNumber == 3)
//         {
//             LevelThreeState.GlutenChildAppeared++;
//         }
//         else
//         {
//             Debug.LogWarning("[CustomerManager] RegisterGlutenChildAppeared: unsupported levelNumber=" + levelNumber);
//         }
//     }

//     private void RegisterGlutenChildServed()
//     {
//         if (levelNumber == 1)
//         {
//             LevelOneState.GlutenChildServed++;
//         }
//         else if (levelNumber == 11)
//         {
//             LevelOneOneState.GlutenChildServed++;
//         }
//         else if (levelNumber == 12)
//         {
//             LevelOneTwoState.GlutenChildServed++;
//         }
//         else if (levelNumber == 2)
//         {
//             LevelTwoState.GlutenChildServed++;
//         }
//         else if (levelNumber == 21)
//         {
//             LevelTwoOneState.GlutenChildServed++;
//         }
//         else if (levelNumber == 22)
//         {
//             LevelTwoTwoState.GlutenChildServed++;
//         }
//         else if (levelNumber == 3)
//         {
//             LevelThreeState.GlutenChildServed++;
//         }
//         else
//         {
//             Debug.LogWarning("[CustomerManager] RegisterGlutenChildServed: unsupported levelNumber=" + levelNumber);
//         }
//     }
// }

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Modular manager that supports N simultaneous customers.
/// Each stand point represents one customer slot.
/// This manager is used by all levels, so level-specific statistics are routed by levelNumber.
/// </summary>
public class CustomerManager : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 3f;

    [Header("Exit Speed After Service")]
    [SerializeField] private float goodServiceExitSpeed = 3f;
    [SerializeField] private float badServiceExitSpeed = 1.2f;

    public static CustomerManager Instance { get; private set; }

    [Header("Prefabs & Positions")]
    [SerializeField] private CustomerType playerBaseBodyType;
    [SerializeField] private Customer customerPrefabNormal;
    [SerializeField] private Customer customerPrefabPlayerHead;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform exitPoint;

    [Header("Stand points = number of simultaneous customers")]
    [Tooltip("Add 1 stand point for 1 customer, 2 for 2 customers, 3 for 3 customers.")]
    [SerializeField] private List<Transform> standPoints = new List<Transform>();

    [Header("Delays per slot (seconds)")]
    [Tooltip("Delay before the first customer appears in each slot.")]
    [SerializeField] private List<float> firstSpawnDelays = new List<float>();

    [Tooltip("Delay after a customer leaves, before spawning the next customer in the same slot.")]
    [SerializeField] private List<float> respawnDelays = new List<float>();

    [SerializeField] private float defaultFirstSpawnDelay = 0f;
    [SerializeField] private float defaultRespawnDelay = 0f;

    [Header("Customer Types")]
    [SerializeField] private List<CustomerType> customerTypes = new List<CustomerType>();

    [Header("Special Customer Spawn Rules")]
    [SerializeField] private bool preventGlutenCustomerAsFirstSpawn = true;

    [Tooltip("How many customers must appear before the gluten-sensitive customer is allowed to appear.")]
    [SerializeField, Min(0)] private int minCustomersBeforeGlutenCustomer = 1;

    private int spawnedCustomersThisLevel = 0;

    [Header("Visual FX - Coins Animation")]
    [SerializeField] private CoinFlyVFX coinFlyVFX;

    [Header("Removing Ingredients")]
    [SerializeField] private int maxMissingItems = 0;

    [Header("Scoring Per Order")]
    [SerializeField] private int baseOrderReward = 30;
    [SerializeField] private int wrongDishPenalty = -5;
    [SerializeField] private int alergicServePenalty = -10;
    [SerializeField] private int alergicOrderReward = 20;

    [Header("Level Number")]
    [Tooltip("Level1=1, Level1.1=11, Level1.2=12, Level2=2, Level2.1=21, Level2.2=22, Level3=3")]
    [SerializeField] private int levelNumber = 1;

    [Header("Instructions UI")]
    [SerializeField] private FeatureHintsSequence instructionManager;
    [SerializeField] private bool shouldRunInstructions_level2 = true;
    [SerializeField] private bool shouldRunInstructions_level3 = true;

    [Header("Customer Voice Feedback")]
    [SerializeField] private AudioSource customerVoiceAudioSource;
    [SerializeField, Range(0f, 1f)] private float customerVoiceVolume = 1f;

    [Header("Reminders")]
    [SerializeField] private AudioSource reminderAudioSource;

    [Tooltip("Optional. If empty, the manager will try to use the clip already assigned on the Reminder Audio Source.")]
    [SerializeField] private AudioClip glutenReminderClip;

    [SerializeField, Range(0f, 1f)] private float reminderVolume = 1f;

    private bool reminderActive = true;

    private CustomerType lastSpawnedType = null;
    private readonly CustomerType[] slotTypes = new CustomerType[3];

    private int currentMaxConcurrentCustomers = 1;

    private class SlotState
    {
        public Transform standPoint;
        public Customer customer;

        public Coroutine spawnRoutine;
        public Coroutine moveRoutine;
        public Coroutine leaveRoutine;

        public bool isHandlingLeave;

        public float firstDelay;
        public float respawnDelay;

        public float nextLeaveSpeedOverride = -1f;
        public float currentLeaveSpeed = -1f;

        public Action<bool> moodHandler;
    }

    private readonly List<SlotState> slots = new List<SlotState>();
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
        reminderActive = true;
        spawnedCustomersThisLevel = 0;

        if (PlayerFaceStore.HasAll)
        {
            RegisterPlayerCustomer(PlayerFaceStore.Happy, PlayerFaceStore.Angry, PlayerFaceStore.Furious);
        }

        BuildSlotsFromInspector();

        currentMaxConcurrentCustomers = GetMaxSupportedConcurrentCustomers();

        StartAllSlots();

        shouldRunInstructions_level2 = false;
        shouldRunInstructions_level3 = false;
    }

    public int GetMaxSupportedConcurrentCustomers()
    {
        int supported = slots.Count;

        if (supported <= 0 && standPoints != null)
        {
            supported = standPoints.Count;
        }

        return Mathf.Clamp(supported, 1, 3);
    }

    public int GetCurrentMaxConcurrentCustomers()
    {
        int supported = GetMaxSupportedConcurrentCustomers();

        if (currentMaxConcurrentCustomers <= 0)
        {
            currentMaxConcurrentCustomers = supported;
        }

        return Mathf.Clamp(currentMaxConcurrentCustomers, 1, supported);
    }

    public void SetMaxConcurrentCustomers(int amount)
    {
        int supported = GetMaxSupportedConcurrentCustomers();
        int newLimit = Mathf.Clamp(amount, 1, supported);
        int previousLimit = GetCurrentMaxConcurrentCustomers();

        currentMaxConcurrentCustomers = newLimit;

        Debug.Log($"[CustomerManager] Max concurrent customers changed: {previousLimit} -> {newLimit} / supported={supported}");

        if (newLimit > previousLimit)
        {
            for (int i = previousLimit; i < newLimit && i < slots.Count; i++)
            {
                if (CanStartSpawnForSlot(i))
                {
                    StartSlotSpawnCoroutine(i, 0f);
                }
            }
        }
    }

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

            float first = (firstSpawnDelays != null && i < firstSpawnDelays.Count)
                ? firstSpawnDelays[i]
                : defaultFirstSpawnDelay;

            float resp = (respawnDelays != null && i < respawnDelays.Count)
                ? respawnDelays[i]
                : defaultRespawnDelay;

            slots.Add(new SlotState
            {
                standPoint = sp,
                firstDelay = Mathf.Max(0f, first),
                respawnDelay = Mathf.Max(0f, resp)
            });
        }
    }

    private void StartAllSlots()
    {
        if (slots.Count == 0)
        {
            return;
        }

        for (int i = 0; i < slots.Count; i++)
        {
            if (!IsSlotAllowedByRuntimeLimit(i))
            {
                continue;
            }

            StartSlotSpawnCoroutine(i, slots[i].firstDelay);
        }
    }

    private void StartSlotSpawnCoroutine(int slotIndex, float delay)
    {
        if (!IsValidSlot(slotIndex))
        {
            return;
        }

        if (!IsSlotAllowedByRuntimeLimit(slotIndex))
        {
            return;
        }

        SlotState slot = slots[slotIndex];

        if (slot.customer != null || slot.isHandlingLeave)
        {
            return;
        }

        if (slot.spawnRoutine != null)
        {
            StopCoroutine(slot.spawnRoutine);
        }

        slot.spawnRoutine = StartCoroutine(SpawnInSlotAfterDelay(slotIndex, delay));
    }

    private IEnumerator SpawnInSlotAfterDelay(int slotIndex, float delay)
    {
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        if (!IsValidSlot(slotIndex))
        {
            yield break;
        }

        SlotState slot = slots[slotIndex];
        slot.spawnRoutine = null;

        if (!IsSlotAllowedByRuntimeLimit(slotIndex))
        {
            yield break;
        }

        SpawnCustomerInSlot(slotIndex);
    }

    private void SpawnCustomerInSlot(int slotIndex)
    {
        if (!IsValidSlot(slotIndex))
        {
            return;
        }

        if (!IsSlotAllowedByRuntimeLimit(slotIndex))
        {
            return;
        }

        if (customerPrefabNormal == null || spawnPoint == null || exitPoint == null)
        {
            Debug.LogWarning("CustomerManager: Missing customerPrefabNormal/spawnPoint/exitPoint reference.");
            return;
        }

        if (customerTypes == null || customerTypes.Count == 0)
        {
            Debug.LogWarning("CustomerManager: no customer types defined!");
            return;
        }

        SlotState slot = slots[slotIndex];

        CleanupSlot(slotIndex);

        CustomerType chosen = ChooseCustomerTypeForSlot(slotIndex);

        if (chosen == null)
        {
            Debug.LogWarning("CustomerManager: Could not choose a customer type.");
            return;
        }

        lastSpawnedType = chosen;

        if (slotIndex >= 0 && slotIndex < slotTypes.Length)
        {
            slotTypes[slotIndex] = chosen;
        }

        bool isPlayerCustomer = chosen != null && chosen.name == "player_customer";
        Customer prefabToSpawn = customerPrefabNormal;

        if (isPlayerCustomer)
        {
            if (customerPrefabPlayerHead != null)
            {
                prefabToSpawn = customerPrefabPlayerHead;
            }
            else
            {
                Debug.LogWarning("CustomerManager: player_customer chosen but customerPrefabPlayerHead is not assigned. Falling back to normal prefab.");
            }
        }

        slot.customer = Instantiate(prefabToSpawn, spawnPoint.position, Quaternion.identity);
        slot.customer.Init(chosen, maxMissingItems);

        spawnedCustomersThisLevel++;

        RegisterCustomerArrived();

        if (chosen != null && chosen.scoreIfNotServed)
        {
            RegisterGlutenChildAppeared();
            PlayGlutenCustomerReminderOnce();
        }

        customerToSlot[slot.customer] = slotIndex;

        if (slot.customer.MoodTimer != null)
        {
            slot.moodHandler = (served) => OnCustomerFinishedInSlot(slotIndex, served);

            slot.customer.MoodTimer.OnCustomerFinished -= slot.moodHandler;
            slot.customer.MoodTimer.OnCustomerFinished += slot.moodHandler;
        }
        else
        {
            Debug.LogWarning("CustomerManager: Customer.MoodTimer is not assigned on the Customer prefab.");
        }

        StartMove(slotIndex, slot.customer.transform, slot.standPoint.position);

        if (instructionManager != null)
        {
            instructionManager.OnCustomerSpawned();
        }
    }

    private CustomerType ChooseCustomerTypeForSlot(int slotIndex)
    {
        List<CustomerType> strictCandidates = BuildCustomerCandidates(
            slotIndex,
            blockLastSpawnedType: true,
            blockEarlyGlutenCustomer: true,
            blockSameTypeInOtherSlots: true
        );

        if (strictCandidates.Count > 0)
        {
            return GetRandomCustomerType(strictCandidates);
        }

        List<CustomerType> allowLastSpawnedCandidates = BuildCustomerCandidates(
            slotIndex,
            blockLastSpawnedType: false,
            blockEarlyGlutenCustomer: true,
            blockSameTypeInOtherSlots: true
        );

        if (allowLastSpawnedCandidates.Count > 0)
        {
            return GetRandomCustomerType(allowLastSpawnedCandidates);
        }

        List<CustomerType> allowEarlyGlutenCandidates = BuildCustomerCandidates(
            slotIndex,
            blockLastSpawnedType: false,
            blockEarlyGlutenCustomer: false,
            blockSameTypeInOtherSlots: true
        );

        if (allowEarlyGlutenCandidates.Count > 0)
        {
            if (preventGlutenCustomerAsFirstSpawn)
            {
                Debug.LogWarning("CustomerManager: No valid non-gluten customer found for early spawn. Allowing gluten customer to avoid blocking spawning.");
            }

            return GetRandomCustomerType(allowEarlyGlutenCandidates);
        }

        List<CustomerType> fallbackCandidates = BuildCustomerCandidates(
            slotIndex,
            blockLastSpawnedType: false,
            blockEarlyGlutenCustomer: false,
            blockSameTypeInOtherSlots: false
        );

        if (fallbackCandidates.Count > 0)
        {
            return GetRandomCustomerType(fallbackCandidates);
        }

        return null;
    }

    private List<CustomerType> BuildCustomerCandidates(
        int slotIndex,
        bool blockLastSpawnedType,
        bool blockEarlyGlutenCustomer,
        bool blockSameTypeInOtherSlots)
    {
        List<CustomerType> candidates = new List<CustomerType>();

        if (customerTypes == null)
        {
            return candidates;
        }

        foreach (CustomerType type in customerTypes)
        {
            if (type == null)
            {
                continue;
            }
            if (!ControlPanelUI.GlutenChildEnabled && type.scoreIfNotServed)
            {
                continue;
            }

            if (blockLastSpawnedType && type == lastSpawnedType)
            {
                continue;
            }

            if (blockSameTypeInOtherSlots && IsSameCustomerTypeActiveInAnotherSlot(slotIndex, type))
            {
                continue;
            }

            if (blockEarlyGlutenCustomer && ShouldBlockGlutenCustomerAsEarlySpawn(type))
            {
                continue;
            }

            candidates.Add(type);
        }

        return candidates;
    }

    private CustomerType GetRandomCustomerType(List<CustomerType> candidates)
    {
        if (candidates == null || candidates.Count == 0)
        {
            return null;
        }

        return candidates[UnityEngine.Random.Range(0, candidates.Count)];
    }

    private bool IsSameCustomerTypeActiveInAnotherSlot(int slotIndex, CustomerType type)
    {
        if (type == null)
        {
            return false;
        }

        for (int i = 0; i < slotTypes.Length; i++)
        {
            if (i == slotIndex)
            {
                continue;
            }

            if (slotTypes[i] == type)
            {
                return true;
            }
        }

        return false;
    }

    private bool ShouldBlockGlutenCustomerAsEarlySpawn(CustomerType type)
    {
        if (!preventGlutenCustomerAsFirstSpawn)
        {
            return false;
        }

        if (type == null)
        {
            return false;
        }

        if (!type.scoreIfNotServed)
        {
            return false;
        }

        return spawnedCustomersThisLevel < minCustomersBeforeGlutenCustomer;
    }

    private void PlayGlutenCustomerReminderOnce()
    {
        if (!reminderActive)
        {
            return;
        }

        reminderActive = false;

        if (reminderAudioSource == null)
        {
            return;
        }

        AudioClip clipToPlay = glutenReminderClip;

        if (clipToPlay == null)
        {
            clipToPlay = reminderAudioSource.clip;
        }

        if (clipToPlay == null)
        {
            return;
        }

        reminderAudioSource.PlayOneShot(clipToPlay, reminderVolume);
    }

    private void OnCustomerFinishedInSlot(int slotIndex, bool served)
    {
        if (!IsValidSlot(slotIndex))
        {
            return;
        }

        SlotState slot = slots[slotIndex];
        Customer c = slot.customer;

        if (!served && c != null && c.Data != null && c.Data.scoreIfNotServed)
        {
            Debug.Log("Special customer: NOT served → reward!");

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddMoney(alergicOrderReward);

                if (coinFlyVFX != null)
                {
                    coinFlyVFX.PlayCoinsFromWorld(c.transform, alergicOrderReward, true);
                }
            }
        }

        StartLeaveSequence(slotIndex);
    }

    private void RegisterServedDish(bool isPerfect)
    {
        if (levelNumber == 1)
        {
            LevelOneState.TotalServedDishes++;

            if (isPerfect)
            {
                LevelOneState.PerfectServedDishes++;
            }
        }
        else if (levelNumber == 11)
        {
            LevelOneOneState.TotalServedDishes++;

            if (isPerfect)
            {
                LevelOneOneState.PerfectServedDishes++;
            }
        }
        else if (levelNumber == 12)
        {
            LevelOneTwoState.TotalServedDishes++;

            if (isPerfect)
            {
                LevelOneTwoState.PerfectServedDishes++;
            }
        }
        else if (levelNumber == 2)
        {
            LevelTwoState.TotalServedDishes++;

            if (isPerfect)
            {
                LevelTwoState.PerfectServedDishes++;
            }
        }
        else if (levelNumber == 21)
        {
            LevelTwoOneState.TotalServedDishes++;

            if (isPerfect)
            {
                LevelTwoOneState.PerfectServedDishes++;
            }
        }
        else if (levelNumber == 22)
        {
            LevelTwoTwoState.TotalServedDishes++;

            if (isPerfect)
            {
                LevelTwoTwoState.PerfectServedDishes++;
            }
        }
        else if (levelNumber == 3)
        {
            LevelThreeState.TotalServedDishes++;

            if (isPerfect)
            {
                LevelThreeState.PerfectServedDishes++;
            }
        }
        else
        {
            Debug.LogWarning("[CustomerManager] RegisterServedDish: unsupported levelNumber=" + levelNumber);
        }
    }

    public void RegisterDuplicateIngredientClick()
    {
        if (levelNumber == 1)
        {
            LevelOneState.DuplicateIngredientClicks++;
        }
        else if (levelNumber == 11)
        {
            LevelOneOneState.DuplicateIngredientClicks++;
        }
        else if (levelNumber == 12)
        {
            LevelOneTwoState.DuplicateIngredientClicks++;
        }
        else if (levelNumber == 2)
        {
            LevelTwoState.DuplicateIngredientClicks++;
        }
        else if (levelNumber == 21)
        {
            LevelTwoOneState.DuplicateIngredientClicks++;
        }
        else if (levelNumber == 22)
        {
            LevelTwoTwoState.DuplicateIngredientClicks++;
        }
        else if (levelNumber == 3)
        {
            LevelThreeState.DuplicateIngredientClicks++;
        }
        else
        {
            Debug.LogWarning("[CustomerManager] RegisterDuplicateIngredientClick: unsupported levelNumber=" + levelNumber);
        }
    }

    public void ServeCustomer(Customer target)
    {
        if (target == null)
        {
            Debug.LogWarning("ServeCustomer called with null target.");
            return;
        }

        if (target.IsLeaving)
        {
            return;
        }

        if (SelectionList.Instance == null)
        {
            Debug.LogWarning("SelectionList.Instance is null.");
            return;
        }

        if (!customerToSlot.TryGetValue(target, out int slotIndex))
        {
            Debug.LogWarning("ServeCustomer: target customer is not tracked by manager.");
            return;
        }

        List<string> ingredients = SelectionList.Instance.GetSelectedIngredients();

        if (target.Data != null && target.Data.scoreIfNotServed)
        {
            Debug.Log("Special customer: served -> NO score.");
            PlayCustomerVoice(target, false);

            RegisterGlutenChildServed();

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.FlashPenaltyUI();
                ScoreManager.Instance.AddMoney(alergicServePenalty);

                if (coinFlyVFX != null)
                {
                    coinFlyVFX.PlayPenaltyFromWorld(target.transform, Mathf.Abs(alergicServePenalty), true);
                }
            }

            if (ControlPanelUI.MarkAddedItemsEnabled)
            {
                target.PreserveIngredientMarkersUntilDestroyed();
            }

            SelectionList.Instance.ClearIngredients();

            StartCoroutine(LeaveAfterWrongFeedback(slotIndex, target, 0.4f));
            return;
        }

        int mistakes = CountOrderMistakes(target, ingredients);
        bool ok = mistakes == 0;

        RegisterServedDish(ok);

        if (ok)
        {
            Debug.Log("Correct order!");
            PlayCustomerVoice(target, true);

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddMoney(baseOrderReward);

                if (baseOrderReward > 0 && coinFlyVFX != null)
                {
                    coinFlyVFX.PlayCoinsFromWorld(target.transform);
                }
            }

            SetNextLeaveSpeed(slotIndex, goodServiceExitSpeed);

            if (target.MoodTimer != null)
            {
                target.MoodTimer.CustomerServed();
            }

            if (ControlPanelUI.MarkAddedItemsEnabled)
            {
                target.PreserveIngredientMarkersUntilDestroyed();
            }

            SelectionList.Instance.ClearIngredients();

            StartLeaveSequence(slotIndex);
            return;
        }

        Debug.Log($"Wrong order! Mistakes={mistakes}");
        PlayCustomerVoice(target, false);

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.FlashPenaltyUI();
            ScoreManager.Instance.AddMoney(wrongDishPenalty);

            if (coinFlyVFX != null)
            {
                coinFlyVFX.PlayPenaltyFromWorld(target.transform, Mathf.Abs(wrongDishPenalty));
            }
        }

        if (ControlPanelUI.MarkAddedItemsEnabled)
        {
            target.PreserveIngredientMarkersUntilDestroyed();
        }

        SelectionList.Instance.ClearIngredients();

        StartCoroutine(LeaveAfterWrongFeedback(slotIndex, target, 0.4f));
    }

    private int CountOrderMistakes(Customer target, List<string> givenIngredients)
    {
        if (target == null)
        {
            return 0;
        }

        List<string> activeOrder = target.GetActiveRequiredIngredients();

        if (activeOrder == null)
        {
            return 0;
        }

        List<string> required = new List<string>();

        foreach (string r in activeOrder)
        {
            required.Add(r.ToLower());
        }

        List<string> given = new List<string>();

        if (givenIngredients != null)
        {
            foreach (string g in givenIngredients)
            {
                given.Add(g.ToLower());
            }
        }

        int mistakes = 0;

        foreach (string r in required)
        {
            if (!given.Contains(r))
            {
                mistakes++;
            }
        }

        foreach (string g in given)
        {
            if (!required.Contains(g))
            {
                mistakes++;
            }
        }

        return mistakes;
    }

    private void SetNextLeaveSpeed(int slotIndex, float leaveSpeed)
    {
        if (!IsValidSlot(slotIndex))
        {
            return;
        }

        slots[slotIndex].nextLeaveSpeedOverride = Mathf.Max(0.01f, leaveSpeed);
    }

    private void StartLeaveSequence(int slotIndex)
    {
        if (!IsValidSlot(slotIndex))
        {
            return;
        }

        SlotState slot = slots[slotIndex];

        if (slot.isHandlingLeave)
        {
            return;
        }

        slot.isHandlingLeave = true;

        slot.currentLeaveSpeed = slot.nextLeaveSpeedOverride > 0f
            ? slot.nextLeaveSpeedOverride
            : speed;

        slot.nextLeaveSpeedOverride = -1f;

        Debug.Log($"[CustomerManager] Customer leaving from slot {slotIndex} with speed {slot.currentLeaveSpeed}");

        if (slot.customer != null)
        {
            slot.customer.MarkLeaving();
        }

        if (slot.leaveRoutine != null)
        {
            StopCoroutine(slot.leaveRoutine);
        }

        slot.leaveRoutine = StartCoroutine(CustomerLeaveAndRespawn(slotIndex));
    }

    private IEnumerator CustomerLeaveAndRespawn(int slotIndex)
    {
        if (!IsValidSlot(slotIndex))
        {
            yield break;
        }

        SlotState slot = slots[slotIndex];

        if (slot.moveRoutine != null)
        {
            StopCoroutine(slot.moveRoutine);
            slot.moveRoutine = null;
        }

        if (slot.customer != null && exitPoint != null)
        {
            float leaveSpeed = slot.currentLeaveSpeed > 0f ? slot.currentLeaveSpeed : speed;
            yield return MoveToPoint(slot.customer.transform, exitPoint.position, leaveSpeed);
        }

        CleanupSlot(slotIndex);

        slot.isHandlingLeave = false;

        if (!IsSlotAllowedByRuntimeLimit(slotIndex))
        {
            yield break;
        }

        if (slot.respawnDelay > 0f)
        {
            yield return new WaitForSeconds(slot.respawnDelay);
        }

        if (!IsSlotAllowedByRuntimeLimit(slotIndex))
        {
            yield break;
        }

        SpawnCustomerInSlot(slotIndex);
    }

    private void CleanupSlot(int slotIndex)
    {
        if (!IsValidSlot(slotIndex))
        {
            return;
        }

        SlotState slot = slots[slotIndex];

        if (slot.customer != null)
        {
            if (slot.customer.MoodTimer != null && slot.moodHandler != null)
            {
                slot.customer.MoodTimer.OnCustomerFinished -= slot.moodHandler;
            }

            customerToSlot.Remove(slot.customer);

            Destroy(slot.customer.gameObject);
            slot.customer = null;
        }

        if (slot.spawnRoutine != null)
        {
            StopCoroutine(slot.spawnRoutine);
            slot.spawnRoutine = null;
        }

        if (slot.moveRoutine != null)
        {
            StopCoroutine(slot.moveRoutine);
            slot.moveRoutine = null;
        }

        if (slot.leaveRoutine != null)
        {
            StopCoroutine(slot.leaveRoutine);
            slot.leaveRoutine = null;
        }

        slot.isHandlingLeave = false;
        slot.moodHandler = null;
        slot.nextLeaveSpeedOverride = -1f;
        slot.currentLeaveSpeed = -1f;

        if (slotIndex >= 0 && slotIndex < slotTypes.Length)
        {
            slotTypes[slotIndex] = null;
        }
    }

    private void StartMove(int slotIndex, Transform t, Vector3 target)
    {
        if (!IsValidSlot(slotIndex))
        {
            return;
        }

        SlotState slot = slots[slotIndex];

        if (slot.moveRoutine != null)
        {
            StopCoroutine(slot.moveRoutine);
        }

        slot.moveRoutine = StartCoroutine(MoveToPoint(t, target, speed));
    }

    private IEnumerator MoveToPoint(Transform t, Vector3 target, float moveSpeed)
    {
        moveSpeed = Mathf.Max(0.01f, moveSpeed);

        while (t != null && Vector3.Distance(t.position, target) > 0.01f)
        {
            t.position = Vector3.MoveTowards(t.position, target, moveSpeed * Time.deltaTime);
            yield return null;
        }

        if (t != null)
        {
            t.position = target;
        }
    }

    private bool IsValidSlot(int slotIndex)
    {
        return slotIndex >= 0 && slotIndex < slots.Count;
    }

    private bool IsSlotAllowedByRuntimeLimit(int slotIndex)
    {
        return slotIndex >= 0 && slotIndex < GetCurrentMaxConcurrentCustomers();
    }

    private bool CanStartSpawnForSlot(int slotIndex)
    {
        if (!IsValidSlot(slotIndex))
        {
            return false;
        }

        if (!IsSlotAllowedByRuntimeLimit(slotIndex))
        {
            return false;
        }

        SlotState slot = slots[slotIndex];

        return slot.customer == null && !slot.isHandlingLeave && slot.spawnRoutine == null;
    }

    private IEnumerator LeaveAfterWrongFeedback(int slotIndex, Customer target, float delay)
    {
        if (target != null)
        {
            target.MarkLeaving();
        }

        if (target != null && target.MoodTimer != null)
        {
            target.MoodTimer.ShowAngryNow(target);
        }

        yield return new WaitForSeconds(delay);

        SetNextLeaveSpeed(slotIndex, badServiceExitSpeed);
        StartLeaveSequence(slotIndex);
    }

    public void AddCustomerType(CustomerType type)
    {
        if (type == null)
        {
            return;
        }

        if (customerTypes == null)
        {
            customerTypes = new List<CustomerType>();
        }

        if (!customerTypes.Contains(type))
        {
            customerTypes.Add(type);
        }
    }

    public void RegisterPlayerCustomer(Sprite happy, Sprite angry, Sprite furious)
    {
        CustomerType baseType = playerBaseBodyType;

        if (baseType == null)
        {
            Debug.LogWarning("RegisterPlayerCustomer: playerBaseBodyType is not assigned.");
            return;
        }

        CustomerType playerType =
            RuntimeCustomerFactory.CreateFromBase(baseType, happy, angry, furious, "player_customer");

        AddCustomerType(playerType);
    }

    private void PlayCustomerVoice(Customer target, bool success)
    {
        if (target == null || target.Data == null)
        {
            return;
        }

        AudioClip clip = success
            ? target.Data.successVoiceClip
            : target.Data.failureVoiceClip;

        if (clip == null)
        {
            return;
        }

        if (customerVoiceAudioSource != null)
        {
            customerVoiceAudioSource.PlayOneShot(clip, customerVoiceVolume);
        }
    }

    private void RegisterCustomerArrived()
    {
        if (levelNumber == 1)
        {
            LevelOneState.CustomersArrived++;
        }
        else if (levelNumber == 11)
        {
            LevelOneOneState.CustomersArrived++;
        }
        else if (levelNumber == 12)
        {
            LevelOneTwoState.CustomersArrived++;
        }
        else if (levelNumber == 2)
        {
            LevelTwoState.CustomersArrived++;
        }
        else if (levelNumber == 21)
        {
            LevelTwoOneState.CustomersArrived++;
        }
        else if (levelNumber == 22)
        {
            LevelTwoTwoState.CustomersArrived++;
        }
        else if (levelNumber == 3)
        {
            LevelThreeState.CustomersArrived++;
        }
        else
        {
            Debug.LogWarning("[CustomerManager] RegisterCustomerArrived: unsupported levelNumber=" + levelNumber);
        }
    }

    private void RegisterGlutenChildAppeared()
    {
        if (levelNumber == 1)
        {
            LevelOneState.GlutenChildAppeared++;
        }
        else if (levelNumber == 11)
        {
            LevelOneOneState.GlutenChildAppeared++;
        }
        else if (levelNumber == 12)
        {
            LevelOneTwoState.GlutenChildAppeared++;
        }
        else if (levelNumber == 2)
        {
            LevelTwoState.GlutenChildAppeared++;
        }
        else if (levelNumber == 21)
        {
            LevelTwoOneState.GlutenChildAppeared++;
        }
        else if (levelNumber == 22)
        {
            LevelTwoTwoState.GlutenChildAppeared++;
        }
        else if (levelNumber == 3)
        {
            LevelThreeState.GlutenChildAppeared++;
        }
        else
        {
            Debug.LogWarning("[CustomerManager] RegisterGlutenChildAppeared: unsupported levelNumber=" + levelNumber);
        }
    }

    private void RegisterGlutenChildServed()
    {
        if (levelNumber == 1)
        {
            LevelOneState.GlutenChildServed++;
        }
        else if (levelNumber == 11)
        {
            LevelOneOneState.GlutenChildServed++;
        }
        else if (levelNumber == 12)
        {
            LevelOneTwoState.GlutenChildServed++;
        }
        else if (levelNumber == 2)
        {
            LevelTwoState.GlutenChildServed++;
        }
        else if (levelNumber == 21)
        {
            LevelTwoOneState.GlutenChildServed++;
        }
        else if (levelNumber == 22)
        {
            LevelTwoTwoState.GlutenChildServed++;
        }
        else if (levelNumber == 3)
        {
            LevelThreeState.GlutenChildServed++;
        }
        else
        {
            Debug.LogWarning("[CustomerManager] RegisterGlutenChildServed: unsupported levelNumber=" + levelNumber);
        }
    }
}