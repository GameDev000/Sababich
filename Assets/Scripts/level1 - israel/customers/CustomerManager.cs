
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

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
            SpawnNextCustomer();
        }

        public void SpawnNextCustomer()
        {
            CleanupCurrentCustomer();

            if (customerTypes == null || customerTypes.Count == 0)
            {
                Debug.LogWarning("CustomerManager: no customer types defined!");
                return;
            }

            CustomerType chosen = customerTypes[Random.Range(0, customerTypes.Count)];

            currentCustomer = Instantiate(customerPrefab, spawnPoint.position, Quaternion.identity);
            currentCustomer.Init(chosen);

            // Subscribe to mood timer (no GetComponent)
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

            StartMove(currentCustomer.transform, standPoint.position);
        }

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

            List<string> ingredients = SelectionList.Instance.GetSelectedIngredients();
            bool ok = currentCustomer.IsOrderCorrect(ingredients);

            if (ok)
            {
                Debug.Log("Correct order!");

                if (ScoreManager.Instance != null)
                    ScoreManager.Instance.AddMoney(30);

                // Tell mood timer the customer was served (so he becomes happy and leaves)
                if (currentCustomer.MoodTimer != null)
                    currentCustomer.MoodTimer.CustomerServed();

                SelectionList.Instance.ClearIngredients();
                return;
            }

            Debug.Log("Wrong order!");
            SelectionList.Instance.ClearIngredients();

            // Wrong order -> customer leaves immediately (angry)
            StartLeaveSequence();
        }

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
                currentCustomer.MarkLeaving();

            if (leaveRoutine != null)
                StopCoroutine(leaveRoutine);

            leaveRoutine = StartCoroutine(CustomerLeaveAndNext());
        }


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

        private void CleanupCurrentCustomer()
        {
            if (currentCustomer == null) return;

            if (currentCustomer.MoodTimer != null)
                currentCustomer.MoodTimer.OnCustomerFinished -= OnCustomerFinished;

            Destroy(currentCustomer.gameObject);
            currentCustomer = null;

            if (moveRoutine != null)
            {
                StopCoroutine(moveRoutine);
                moveRoutine = null;
            }
        }

        private void StartMove(Transform t, Vector3 target)
        {
            if (moveRoutine != null)
                StopCoroutine(moveRoutine);

            moveRoutine = StartCoroutine(MoveToPoint(t, target));
        }

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

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F5))
                ServeCurrentCustomer();
        }
    }
