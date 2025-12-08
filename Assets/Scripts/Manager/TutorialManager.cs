
using UnityEngine.SceneManagement;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    private enum TutorialPhase
    {
        FryEggplant,
        BuildSandwich,
        Done
    }

    private TutorialPhase phase = TutorialPhase.FryEggplant;
    private int buildStep = 0;

    private readonly string[] buildOrderNames =
    {
        "pitta",
        "egg",
        "salad",
        "eggplant",
        "amba",
        "tahini"
    };

    public static TutorialManager Instance { get; private set; }

    [Header("Items")]
    [SerializeField] private Item eggplantItem;
    [SerializeField] private Item[] otherItems;

    [Header("Arrow")]
    [SerializeField] private GameObject arrowEggplant;

    [Header("Fry Zone")]
    [SerializeField] private FryZoneEggplant fryZoneEggplant;

    [Header("Customer")]
    [SerializeField] private Transform customerTarget;

    [Header("Customer Visuals")]
    [SerializeField] private SpriteRenderer customerRenderer;
    [SerializeField] private Sprite[] customerSprites;
    [SerializeField] private int tutorialCustomersCount = 3;
    private int servedCustomers = 0;

    [Header("Game Flow Manager")]
    [SerializeField] private GameFlowManager gameFlowManager;

    [Header("Customer Logic")]
    [SerializeField] private CustomerMoodTimer customerLogic;


    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        StartCustomerRound();
    }

    public void OnIngredientClicked(Item item, string ingredientName)
    {
        if (item == null)
            return;

        ingredientName = ingredientName.ToLowerInvariant();
        Debug.Log("[TutorialManager] OnIngredientClicked: " + ingredientName);

        switch (phase)
        {
            case TutorialPhase.FryEggplant:
                HandleFryPhaseClick(item, ingredientName);
                break;

            case TutorialPhase.BuildSandwich:
                HandleBuildSandwichClick(item, ingredientName);
                break;

            case TutorialPhase.Done:
                break;
        }
    }

    private void HandleFryPhaseClick(Item item, string ingredientName)
    {
        if (item != eggplantItem && ingredientName != "eggplantrow")
            return;

        Debug.Log("[TutorialManager] Eggplant clicked - starting fry");

        ShowArrow(false);

        if (fryZoneEggplant != null)
        {
            fryZoneEggplant.StartFry();
            SetGamePhase(GamePhase.FryingEggplant);
        }
    }

    public void OnEggplantFried(FryZoneEggplant fryZone)
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

    public void OnEggplantTrayFull()
    {
        Debug.Log("[TutorialManager] Eggplant tray is full");

        phase = TutorialPhase.BuildSandwich;
        buildStep = 0;
        SetGamePhase(GamePhase.AssembleDish);

        if (eggplantItem != null)
            eggplantItem.SetClickable(false);

        SetAllOtherItemsClickable(false);

        if (otherItems != null && otherItems.Length > 0 && otherItems[0] != null)
        {
            otherItems[0].SetClickable(true);
            ShowArrowAbove(otherItems[0].transform, 2f);
        }
    }

    private void HandleBuildSandwichClick(Item item, string ingredientName)
    {
        if (buildStep >= buildOrderNames.Length)
            return;

        string expected = buildOrderNames[buildStep];

        if (ingredientName != expected)
        {
            Debug.Log($"[Tutorial] Expected {expected}, but got {ingredientName}");
            return;
        }

        Debug.Log($"[Tutorial] Correct ingredient: {ingredientName} (step {buildStep})");

        if (otherItems != null &&
            buildStep < otherItems.Length &&
            otherItems[buildStep] != null)
        {
            otherItems[buildStep].SetClickable(false);
        }

        buildStep++;

        if (buildStep >= buildOrderNames.Length)
        {
            Debug.Log("[Tutorial] Sandwich build tutorial DONE");

            phase = TutorialPhase.Done;

            if (customerTarget != null)
            {
                ShowArrowAbove(customerTarget, 2f);
                SetGamePhase(GamePhase.ServeCustomer);
            }
            else
            {
                ShowArrow(false);
            }

            return;
        }

        if (otherItems != null &&
            buildStep < otherItems.Length &&
            otherItems[buildStep] != null)
        {
            Item nextItem = otherItems[buildStep];
            nextItem.SetClickable(true);
            ShowArrowAbove(nextItem.transform, 2f);
        }
    }

    public void CustomerOrderServed()
    {
        Debug.Log("[Tutorial] Customer served successfully");
        ShowArrow(false);

        if (ScoreManager.Instance != null)
            ScoreManager.Instance.AddMoney(30);
        SetGamePhase(GamePhase.NextCustomer);
    }

    private void StartCustomerRound()
    {
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

        UpdateCustomerSprite();

        if (customerLogic != null)
            customerLogic.ResetCustomer();
    }



    private void UpdateCustomerSprite()
    {
        if (customerRenderer == null || customerSprites == null || customerSprites.Length == 0)
            return;

        int index = servedCustomers % customerSprites.Length;
        customerRenderer.sprite = customerSprites[index];
    }

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

    private void ShowArrow(bool visible)
    {
        if (arrowEggplant != null)
            arrowEggplant.SetActive(visible);
    }

    private void ShowArrowAbove(Transform target, float yOffset)
    {
        if (arrowEggplant == null || target == null)
            return;

        arrowEggplant.SetActive(true);
        Vector3 pos = target.position;
        arrowEggplant.transform.position = pos + new Vector3(0f, yOffset, 0f);
    }

    private void SetGamePhase(GamePhase newPhase)
    {
        if (gameFlowManager != null)
            gameFlowManager.SetPhase(newPhase);
    }


    public void OnCustomerLeftScene()
    {
        Debug.Log("[Tutorial] Customer left scene");

        servedCustomers++;
        Debug.Log("[Tutorial] Customers served in tutorial: " + servedCustomers);

        if (servedCustomers < tutorialCustomersCount)
        {
            StartCustomerRound();
        }
        else
        {
            SceneManager.LoadScene("TutorialEndScene");
        }
    }

}
