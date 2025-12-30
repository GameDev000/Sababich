
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Manages customer spawning, serving, and departure in the game.
/// Handles the lifecycle of customers including their movement and interactions.
/// </summary>
public class CustomerManager : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 3f;

    public static CustomerManager Instance { get; private set; }

    [Header("Prefabs & Positions")]
    [SerializeField] private Customer customerPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform standPoint;
    [SerializeField] private Transform exitPoint;

    [Header("Customer types (Israel)")]
    [SerializeField] private List<CustomerType> customerTypes;

    private Customer currentCustomer;
    private Coroutine moveRoutine;
    private Coroutine leaveRoutine;
    private bool isHandlingLeave;

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
        SpawnNextCustomer();// Start the first customer
    }

    /// <summary>
    /// Spawns the next customer at the spawn point and moves them to the stand point.
    /// </summary>
    public void SpawnNextCustomer()
    {
        CleanupCurrentCustomer();// Clean up any existing customer

        if (customerTypes == null || customerTypes.Count == 0)
        {
            Debug.LogWarning("CustomerManager: no customer types defined!");
            return;
        }

        CustomerType chosen = customerTypes[Random.Range(0, customerTypes.Count)];// Randomly select a customer type

        currentCustomer = Instantiate(customerPrefab, spawnPoint.position, Quaternion.identity);// Instantiate the customer
        currentCustomer.Init(chosen);// Initialize with chosen type

        // Subscribe to mood timer
        if (currentCustomer.MoodTimer != null)
        {
            currentCustomer.MoodTimer.OnCustomerFinished -= OnCustomerFinished;
            currentCustomer.MoodTimer.OnCustomerFinished += OnCustomerFinished;
            //currentCustomer.MoodTimer.ResetTimer();
        }
        else
        {
            Debug.LogWarning("CustomerManager: currentCustomer.MoodTimer is not assigned on the Customer prefab.");
        }

        StartMove(currentCustomer.transform, standPoint.position);// Move to stand point
    }

    /// <summary>
    /// Serves the current customer based on the selected ingredients.
    /// </summary>
    public void ServeCurrentCustomer()
    {
        if (currentCustomer == null)
        {
            Debug.LogWarning("ServeCurrentCustomer called but no current customer.");
            return;
        }
        if (currentCustomer.IsLeaving)
            return;

        if (SelectionList.Instance == null)
        {
            Debug.LogWarning("SelectionList.Instance is null.");
            return;
        }

        List<string> ingredients = SelectionList.Instance.GetSelectedIngredients();// Get selected ingredients
        bool ok = currentCustomer.IsOrderCorrect(ingredients);// Check if order is correct

        if (ok)
        {
            Debug.Log("Correct order!");

            if (ScoreManager.Instance != null)
                ScoreManager.Instance.AddMoney(30);

            // Tell mood timer the customer was served (so he becomes happy and leaves)
            if (currentCustomer.MoodTimer != null)
                currentCustomer.MoodTimer.CustomerServed();

            SelectionList.Instance.ClearIngredients();
            StartLeaveSequence();// Customer leaves happily
            return;
        }

        Debug.Log("Wrong order!");
        SelectionList.Instance.ClearIngredients();

        // Wrong order -> customer leaves immediately (angry)
        StartLeaveSequence();// Customer leaves angrily
    }

    /// <summary>
    /// Called when the customer's mood timer finishes, indicating they are done (either served or time up).
    /// </summary>
    /// <param name="served"></param>
    private void OnCustomerFinished(bool served)
    {
        // served==false -> time up angry
        // served==true  -> served in time happy
        StartLeaveSequence();
    }
    private void StartLeaveSequence()
    {
        if (isHandlingLeave) return;
        isHandlingLeave = true;

        if (currentCustomer != null)
            currentCustomer.MarkLeaving();// Prevent further interactions

        if (leaveRoutine != null)
            StopCoroutine(leaveRoutine);// Stop any existing leave routine

        leaveRoutine = StartCoroutine(CustomerLeaveAndNext());// Start leave coroutine
    }

    /// <summary>
    /// Handles the customer leaving animation and spawns the next customer.
    /// </summary>
    /// <returns></returns>
    private IEnumerator CustomerLeaveAndNext()
    {
        if (currentCustomer != null && exitPoint != null)
        {
            // stop any move-to-stand coroutine first
            if (moveRoutine != null)
            {
                StopCoroutine(moveRoutine);
                moveRoutine = null;
            }

            yield return MoveToPoint(currentCustomer.transform, exitPoint.position);

            CleanupCurrentCustomer();
        }

        isHandlingLeave = false;
        SpawnNextCustomer();
    }

    /// <summary>
    /// Cleans up the current customer by unsubscribing events and destroying the GameObject.
    /// </summary>
    private void CleanupCurrentCustomer()
    {
        if (currentCustomer == null) return;

        if (currentCustomer.MoodTimer != null)
            currentCustomer.MoodTimer.OnCustomerFinished -= OnCustomerFinished;

        Destroy(currentCustomer.gameObject);// Destroy the customer GameObject
        currentCustomer = null;

        if (moveRoutine != null)
        {
            StopCoroutine(moveRoutine);// Stop any existing move routine
            moveRoutine = null;
        }
    }

    private void StartMove(Transform t, Vector3 target)
    {
        if (moveRoutine != null)
            StopCoroutine(moveRoutine);// Stop any existing move routine
        moveRoutine = StartCoroutine(MoveToPoint(t, target));// Start moving to target
    }

    /// <summary>
    /// Moves a transform to a target point over time.
    /// </summary>
    /// <param name="t"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    private IEnumerator MoveToPoint(Transform t, Vector3 target)
    {
        while (t != null && Vector3.Distance(t.position, target) > 0.01f)
        {
            t.position = Vector3.MoveTowards(t.position, target, speed * Time.deltaTime);// Move towards target
            yield return null;
        }

        if (t != null)
            t.position = target;
    }

}
