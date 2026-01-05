using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections;

/// <summary>
/// Manages the tutorial flow, guiding the player through frying eggplant and building a sandwich.
/// </summary>
public class TutorialManager : MonoBehaviour
{
    private enum TutorialPhase // Different phases of the tutorial for eggplant frying and sandwich building
    {
        FryEggplant,
        BuildSandwich,
        GoWashInKitchen,
        ReturnToStand,
        Done
    }

    private TutorialPhase phase = TutorialPhase.FryEggplant; // Current phase of the tutorial
    private int buildStep = 0;

    // The correct order of ingredients for building the sandwich
    private readonly string[] buildOrderNames =
    {
        "pitta",
        "egg",
        "salad",
        "eggplant",
        "amba",
        "tahini"
    };

    public static TutorialManager Instance { get; private set; } // Singleton instance

    [Header("Items")]
    [SerializeField] private Item eggplantItem; // Reference to the eggplant item (row)
    [SerializeField] private Item[] otherItems; // References to other items used in sandwich building

    [Header("Arrow")]
    [SerializeField] private GameObject arrowEggplant; // Arrow indicator for guiding the player to the eggplant first and then to other items

    [Header("Fry Zone")]
    [SerializeField] private FryZoneIngredient fryZoneIngredient; // Reference to the fry zone for eggplant

    [Header("Customer")]
    [SerializeField] private Transform customerTarget; // Target position for the customer

    [Header("Customer Visuals")]
    [SerializeField] private SpriteRenderer customerRenderer; // Sprite renderer for the customer
    [SerializeField] private Sprite[] customerSprites; // Array of customer sprites for different moods or appearances
    [SerializeField] private int tutorialCustomersCount = 3; // Number of customers in the tutorial
    private int servedCustomers = 0; // Counter for served customers

    [Header("Game Flow Manager")]
    [SerializeField] private GameFlowManager gameFlowManager; // Reference to the GameFlowManager to update game phases

    [Header("Customer Logic")]
    [SerializeField] private CustomerMoodTimer customerLogic; // Reference to the CustomerMoodTimer for managing customer behavior


    [Header("Dirt Tutorial")]
    [SerializeField] private GameObject dirtOverlay;
    [SerializeField] private Transform kitchenArrowTarget;
    [SerializeField] private float kitchenArrowYOffset = 2f;
    [SerializeField] private bool lockGameplayWhileDirty = true;
    [Header("Dirt Tutorial Targets")]
    [SerializeField] private Transform faucetTarget;
    [SerializeField] private float faucetArrowYOffset = 2f;
    [SerializeField] private Transform backArrowTarget;
    [SerializeField] private float backArrowYOffset = 2f;

    [Header("Tutorial Forbidden Customer")]
    [SerializeField] private int forbiddenCustomerIndex = 2;
    [SerializeField] private int rewardAmount = 30;
    [SerializeField] private float forbiddenWaitSeconds = 5f;
    [SerializeField] private bool currentCustomerWasServed = false;
    private bool forbiddenFlowActive = false;

    private Coroutine forbiddenRoutine;



    private int savedBuildStep = 0;
    private bool hasSavedBuildStep = false;
    private TutorialPhase savedPhase = TutorialPhase.BuildSandwich;

    private void Awake()
    {
        Instance = this; // Set the singleton instance
    }

    private void Start()
    {
        StartCustomerRound(); // Start the first customer round
    }


    /// <summary>
    /// Handles ingredient clicks during the tutorial based on the current phase.
    /// </summary>
    /// <param name="item">The item that was clicked.</param>
    /// <param name="ingredientName">The name of the ingredient associated with the item.</param>
    /// <remarks> This method routes the click event to the appropriate handler based on the tutorial phase. </remarks>
    /// </summary>
    public void OnIngredientClicked(Item item, string ingredientName)
    {
        if (item == null)
            return;

        ingredientName = ingredientName.ToLowerInvariant();
        Debug.Log("[TutorialManager] OnIngredientClicked: " + ingredientName);

        switch (phase)
        {
            case TutorialPhase.FryEggplant: // Handle eggplant frying phase
                HandleFryPhaseClick(item, ingredientName);
                break;

            case TutorialPhase.BuildSandwich: // Handle sandwich building phase
                HandleBuildSandwichClick(item, ingredientName);
                break;

            case TutorialPhase.Done:
                break;
        }
    }

    /// <summary>
    /// Handles clicks during the eggplant frying phase of the tutorial.
    /// </summary>
    /// <param name="item">The item that was clicked.</param>
    /// <param name="ingredientName">The name of the ingredient associated with the item.</param>
    /// </summary>
    private void HandleFryPhaseClick(Item item, string ingredientName)
    {
        if (item != eggplantItem && ingredientName != "eggplantrow")
            return;

        Debug.Log("[TutorialManager] Eggplant clicked - starting fry");

        ShowArrow(false);

        if (fryZoneIngredient != null)
        {
            fryZoneIngredient.StartFry(); // Start frying the eggplant
            SetGamePhase(GamePhase.FryingEggplant); // Update game phase to frying eggplant
        }
    }

    /// <summary>
    /// Called when the eggplant has finished frying.
    /// </summary>
    /// <param name="fryZone">The fry zone where the eggplant was fried.</param>
    /// </summary>
    public void OnEggplantFried(FryZoneIngredient fryZone)
    {
        Debug.Log("[TutorialManager] Eggplant is fried - show arrow on pan");
        if (fryZone != null)
            ShowArrowAbove(fryZone.transform, 2f);
    }

    public void OnEggplantTakenFromPan()
    {
        Debug.Log("[TutorialManager] Eggplant taken from pan");
        ShowArrow(false);
    }

    /// <summary>
    /// Called when the eggplant tray is full and ready for sandwich building.
    /// </summary>
    public void OnEggplantTrayFull()
    {
        Debug.Log("[TutorialManager] Eggplant tray is full");

        phase = TutorialPhase.BuildSandwich; // Switch to sandwich building phase
        buildStep = 0;
        SetGamePhase(GamePhase.AssembleDish); // Update game phase to assemble dish

        if (eggplantItem != null)
            eggplantItem.SetClickable(false);

        SetAllOtherItemsClickable(false); // Disable all other items initially

        if (otherItems != null && otherItems.Length > 0 && otherItems[0] != null)
        {
            otherItems[0].SetClickable(true); // Enable the first other item for sandwich building
            ShowArrowAbove(otherItems[0].transform, 2f); // Show arrow above the first item
        }
    }

    /// <summary>
    /// Handles clicks during the sandwich building phase of the tutorial.
    /// </summary>
    /// <param name="item"></param>
    /// <param name="ingredientName"></param>
    private void HandleBuildSandwichClick(Item item, string ingredientName)
    {
        if (buildStep >= buildOrderNames.Length)
            return;

        string expected = buildOrderNames[buildStep]; // Get the expected ingredient for the current build step

        if (ingredientName != expected) // Incorrect ingredient clicked just in the tutorial
        {
            Debug.Log($"[Tutorial] Expected {expected}, but got {ingredientName}");
            return;
        }

        Debug.Log($"[Tutorial] Correct ingredient: {ingredientName} (step {buildStep})");

        if (otherItems != null &&
            buildStep < otherItems.Length &&
            otherItems[buildStep] != null)
        {
            otherItems[buildStep].SetClickable(false);// Disable the current item after it's been used
        }

        buildStep++; // Move to the next step

        if (buildStep >= buildOrderNames.Length) // Completed all steps
        {
            Debug.Log("[Tutorial] Sandwich build tutorial DONE");

            phase = TutorialPhase.Done; // Mark tutorial as done

            if (customerTarget != null)
            {
                ShowArrowAbove(customerTarget, 2f); // Show arrow above the customer
                SetGamePhase(GamePhase.ServeCustomer); // Update game phase to serve customer
            }
            else
            {
                ShowArrow(false);
            }

            return;
        }
        // Enable the next item in the build order
        if (otherItems != null &&
            buildStep < otherItems.Length &&
            otherItems[buildStep] != null)
        {
            Item nextItem = otherItems[buildStep]; // Get the next item
            nextItem.SetClickable(true); // Enable the next item
            ShowArrowAbove(nextItem.transform, 2f); // Show arrow above the next item
        }
    }

    /// <summary>
    /// Called when the customer has been served successfully.
    /// </summary>
    public void CustomerOrderServed()
    {
        if (forbiddenFlowActive && servedCustomers == forbiddenCustomerIndex)
        {
            ShowArrow(false);
            SetGamePhase(GamePhase.ForbiddenCustomerWarning);
            return;
        }

        Debug.Log("[Tutorial] Customer served successfully");
        ShowArrow(false);

        if (ScoreManager.Instance != null)
            ScoreManager.Instance.AddMoney(30);

        if (customerLogic != null)
            customerLogic.CustomerServed();

        SetGamePhase(GamePhase.NextCustomer);
    }



    /// <summary>
    /// Starts a new customer round in the tutorial.
    /// </summary>
    private void StartCustomerRound()
    {
        UpdateCustomerSprite();

        if (customerLogic != null)
            customerLogic.ResetCustomer();

        currentCustomerWasServed = false;

        if (servedCustomers == forbiddenCustomerIndex)
        {
            forbiddenFlowActive = true;

            if (eggplantItem != null) eggplantItem.SetClickable(false);
            SetAllOtherItemsClickable(false);
            ShowArrow(false);

            SetGamePhase(GamePhase.ForbiddenCustomerWarning);

            if (forbiddenRoutine != null) StopCoroutine(forbiddenRoutine);
            forbiddenRoutine = StartCoroutine(ForbiddenCustomerAutoLeaveRoutine());
            return;
        }

        if (servedCustomers == 0)
        {
            phase = TutorialPhase.FryEggplant;
            buildStep = 0;

            if (eggplantItem != null)
                eggplantItem.SetClickable(true);

            SetAllOtherItemsClickable(false);
            ShowArrowAbove(eggplantItem ? eggplantItem.transform : null, 2.5f);
            SetGamePhase(GamePhase.AddRowEggplant);
        }
        else
        {
            phase = TutorialPhase.BuildSandwich;
            buildStep = 0;

            if (eggplantItem != null)
                eggplantItem.SetClickable(false);

            SetAllOtherItemsClickable(false);

            if (otherItems != null && otherItems.Length > 0 && otherItems[0] != null)
            {
                otherItems[0].SetClickable(true);
                ShowArrowAbove(otherItems[0].transform, 2f);
            }

            SetGamePhase(GamePhase.AssembleDish);
        }
    }


    // /// <summary>
    // /// Holds the ORIGINAL round logic exactly as before (no warning logic here).
    // /// </summary>

    private IEnumerator ForbiddenCustomerAutoLeaveRoutine()
    {
        yield return new WaitForSeconds(forbiddenWaitSeconds);

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddMoney(rewardAmount);

        }

        yield return new WaitForSeconds(1);
        // yield return StartCoroutine(WalkCustomerOutRoutine());

        forbiddenFlowActive = false;

        OnCustomerLeftScene();
    }



    /// <summary>
    /// Updates the customer sprite based on the number of served customers.
    /// </summary> 
    private void UpdateCustomerSprite()
    {
        if (customerRenderer == null || customerSprites == null || customerSprites.Length == 0)
            return;

        int index = servedCustomers % customerSprites.Length;
        customerRenderer.sprite = customerSprites[index];
    }

    /// <summary>
    /// Enables or disables all other items except the currently active one.
    /// </summary>
    private void SetAllOtherItemsClickable(bool clickable)
    {
        if (otherItems == null)
            return;

        foreach (var item in otherItems)
        {
            if (item != null)
                item.SetClickable(clickable);
        }
    }

    /// <summary>
    /// Shows or hides the arrow indicator.
    /// </summary>
    private void ShowArrow(bool visible)
    {
        if (arrowEggplant != null)
            arrowEggplant.SetActive(visible);
    }

    /// <summary>
    /// Shows the arrow indicator above a specified target transform.
    /// </summary>
    private void ShowArrowAbove(Transform target, float yOffset)
    {
        if (arrowEggplant == null || target == null)
            return;

        arrowEggplant.SetActive(true);
        Vector3 pos = target.position;
        arrowEggplant.transform.position = pos + new Vector3(0f, yOffset, 0f);
    }

    /// <summary>
    /// Sets the current game phase in the GameFlowManager.
    /// </summary>
    private void SetGamePhase(GamePhase newPhase)
    {
        if (gameFlowManager != null)
            gameFlowManager.SetPhase(newPhase);
    }

    /// <summary>
    /// Called when the customer has left the scene.
    /// </summary>

    public void OnCustomerLeftScene()
    {
        Debug.Log("[Tutorial] Customer left scene");

        if (servedCustomers == forbiddenCustomerIndex && !currentCustomerWasServed)
        {
            if (ScoreManager.Instance != null)
                ScoreManager.Instance.AddMoney(rewardAmount);
        }

        servedCustomers++;

        if (servedCustomers < tutorialCustomersCount)
        {
            StartCustomerRound();
        }
        else
        {
            SceneManager.LoadScene("TutorialEndScene");
        }
    }


    public void OnChipsFried(FryZoneIngredient zone)
    {
        // Tutorial focuses on eggplant. Keep empty.
    }

    public void OnChipsTrayFull()
    {
        // Keep empty (or call OnEggplantTrayFull() if you want chips to advance tutorial).
    }

    public void TriggerDirtTutorial()
    {
        if (phase == TutorialPhase.GoWashInKitchen) return;

        if (phase == TutorialPhase.BuildSandwich)
        {
            savedPhase = phase;
            savedBuildStep = buildStep;
            hasSavedBuildStep = true;
        }

        phase = TutorialPhase.GoWashInKitchen;

        if (lockGameplayWhileDirty)
        {
            if (eggplantItem != null) eggplantItem.SetClickable(false);
            SetAllOtherItemsClickable(false);
        }
        ShowArrowAbove(kitchenArrowTarget, kitchenArrowYOffset);

        SetGamePhase(GamePhase.GoWashInKitchen);
    }


    public void OnArrowClicked(ArrowMoveCamera arrow)
    {

        if (phase == TutorialPhase.GoWashInKitchen)
        {
            if (arrow.Type != ArrowMoveCamera.ArrowType.ToKitchen) return;

            SetGamePhase(GamePhase.WashHands);
            ShowArrowAbove(faucetTarget, faucetArrowYOffset);
            return;
        }

        if (phase == TutorialPhase.ReturnToStand)
        {
            if (arrow.Type != ArrowMoveCamera.ArrowType.ToMain) return;

            ResumeBuildAfterCleaning();
            return;
        }
    }

    public void OnHandsWashed()
    {
        if (phase != TutorialPhase.GoWashInKitchen) return;
        phase = TutorialPhase.ReturnToStand;

        ShowArrowAbove(backArrowTarget, backArrowYOffset);
        SetGamePhase(GamePhase.GoBackToStand);

    }

    private void ResumeBuildAfterCleaning()
    {
        phase = TutorialPhase.BuildSandwich;

        if (hasSavedBuildStep)
            buildStep = savedBuildStep;

        if (lockGameplayWhileDirty)
        {
            SetAllOtherItemsClickable(false);

            if (otherItems != null &&
                buildStep < otherItems.Length &&
                otherItems[buildStep] != null)
            {
                otherItems[buildStep].SetClickable(true);
                ShowArrowAbove(otherItems[buildStep].transform, 2f);
            }
            else
            {
                ShowArrow(false);
            }
        }

        SetGamePhase(GamePhase.AssembleDish);
    }

    private void GoNextCustomerFromWarning()
    {
        SetGamePhase(GamePhase.NextCustomer);
    }





}
