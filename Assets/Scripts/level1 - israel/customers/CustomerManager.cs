using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// מנהל את תור הלקוחות בשלב ישראל:
/// יצירה רנדומלית של לקוח, בדיקת ההזמנה, תשלום, והבאת לקוח חדש.
/// </summary>
public class CustomerManager : MonoBehaviour
{
    
    [Header("Movement Settings")]
    [SerializeField] float speed = 3f;
    public static CustomerManager Instance { get; private set; }

    [Header("Prefabs & Positions")]
    [SerializeField] private Customer customerPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform standPoint;
    [SerializeField] private Transform exitPoint;

    [Header("Customer types (Israel)")]
    [SerializeField] private List<CustomerType> customerTypes;

    private Customer currentCustomer;

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
        SpawnNextCustomer();
    }

    /// <summary>
    /// Creates and spawns the next customer at the spawn point, moving them to the stand point.
    /// </summary>
    public void SpawnNextCustomer()
    {
        if (currentCustomer != null)
        {
            Destroy(currentCustomer.gameObject);
            currentCustomer = null;
        }

        if (customerTypes == null || customerTypes.Count == 0)
        {
            Debug.LogWarning("CustomerManager: no customer types defined!");
            return;
        }

        CustomerType chosen =customerTypes[Random.Range(0, customerTypes.Count)];

        currentCustomer = Instantiate(
            customerPrefab,
            spawnPoint.position,
            Quaternion.identity
        );

        currentCustomer.Init(chosen);

        StartCoroutine(MoveToPoint(currentCustomer.transform, standPoint.position));
    }

    /// <summary>
    /// Called to serve the current customer: checks the order, gives money if correct,
    /// and moves the customer out before spawning the next one.
    /// </summary>
    public void ServeCurrentCustomer()
    {
        if (currentCustomer == null)
        {
            Debug.LogWarning("ServeCurrentCustomer called but no current customer.");
            return;
        }

        if (SelectionList.Instance == null)
        {
            Debug.LogWarning("SelectionList.Instance is null.");
            return;
        }

        List<string> ingredients = SelectionList.Instance.GetSelectedIngredients();
        bool ok = currentCustomer.IsOrderCorrect(ingredients);

        if (ok)
        {
            Debug.Log("Correct order!");
            if (ScoreManager.Instance != null)
                ScoreManager.Instance.AddMoney(30);
        }
        else
        {
            Debug.Log("Wrong order!");
        }

        SelectionList.Instance.ClearIngredients();

        StartCoroutine(CustomerLeaveAndNext());
    }

    private IEnumerator CustomerLeaveAndNext()
    {
        if (currentCustomer != null && exitPoint != null)
        {
            yield return MoveToPoint(currentCustomer.transform, exitPoint.position);
            Destroy(currentCustomer.gameObject);
            currentCustomer = null;
        }

        SpawnNextCustomer();
    }

private IEnumerator MoveToPoint(Transform t, Vector3 target)
{

    while (Vector3.Distance(t.position, target) > 0.01f)
    {
        t.position = Vector3.MoveTowards(
            t.position,
            target,
            speed * Time.deltaTime
        );
        yield return null;
    }

    t.position = target;
}


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            ServeCurrentCustomer();
        }
    }

}
