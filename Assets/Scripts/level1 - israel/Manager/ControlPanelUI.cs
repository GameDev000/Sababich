// using System.Collections.Generic;
// using System.Reflection;
// using TMPro;
// using UnityEngine;
// using UnityEngine.SceneManagement;
// using UnityEngine.UI;

// /// <summary>
// /// Controls the runtime control panel UI.
// /// Uses the existing PauseButton/PauseManager flow when opening and closing the panel.
// /// Applies level time changes immediately and applies saved settings on Save.
// /// Also allows manual level navigation through a dropdown and confirmation dialog.
// /// </summary>
// public class ControlPanelUI : MonoBehaviour
// {
//     public static bool MarkAddedItemsEnabled { get; private set; }
//     public static bool DirtEnabled { get; private set; } = true;

//     [Header("Root")]
//     [SerializeField] private GameObject controlPanelOverlay;
//     [SerializeField] private CanvasGroup controlPanelCanvasGroup;

//     [Header("Pause")]
//     [SerializeField] private PauseButton pauseButton;

//     [Header("Level Timer")]
//     [Tooltip("Drag here the object that has the current level timer script. The script must contain AddTimeSeconds(float).")]
//     [SerializeField] private MonoBehaviour levelTimerBehaviour;

//     [Header("Buttons")]
//     [SerializeField] private Button openPanelButton;
//     [SerializeField] private Button add30SecondsButton;
//     [SerializeField] private Button remove30SecondsButton;
//     [SerializeField] private Button resetButton;
//     [SerializeField] private Button saveButton;

//     [Header("Anger Time")]
//     [SerializeField] private Slider angerTimeSlider;
//     [SerializeField] private TextMeshProUGUI angerTimeValueText;
//     [Header("Anger Time Range")]
//     [SerializeField] private int minAngerTimeSeconds = 7;
//     [SerializeField] private int maxAngerTimeSeconds = 20;

//     [Header("Added Items Marking")]
//     [SerializeField] private Toggle markAddedItemsToggle;

//     [Header("Dirt Settings")]
//     [SerializeField] private Toggle showDirtToggle;

//     [Header("Concurrent Customers")]
//     [SerializeField] private Button customerLimit1Button;
//     [SerializeField] private Button customerLimit2Button;
//     [SerializeField] private Button customerLimit3Button;

//     [SerializeField] private Color selectedCustomerLimitColor = new Color(0.36f, 0.62f, 0.30f);
//     [SerializeField] private Color normalCustomerLimitColor = new Color(0.54f, 0.52f, 0.42f);
//     [SerializeField] private Color disabledCustomerLimitColor = new Color(0.55f, 0.55f, 0.55f);
//     [SerializeField] private Color enabledCustomerLimitTextColor = Color.white;
//     [SerializeField] private Color disabledCustomerLimitTextColor = new Color(0.85f, 0.85f, 0.85f);

//     [Header("Ingredient Count")]
//     [SerializeField] private Slider ingredientCountSlider;
//     [SerializeField] private TextMeshProUGUI ingredientCountValueText;

//     [Header("World Visuals Hidden While Panel Is Open")]
//     [Tooltip("Drag here world objects such as pitta_in_hands or world timer objects. All Renderers under these roots will be hidden while the control panel is open.")]
//     [SerializeField] private Transform[] worldVisualRootsToHideWhilePanelOpen;

//     [Header("Defaults")]
//     [SerializeField] private int defaultAngerTimeSeconds = 7;
//     [SerializeField] private bool defaultMarkAddedItemsEnabled = false;
//     [SerializeField] private bool defaultDirtEnabled = true;

//     [Header("Level Navigation")]
//     [SerializeField] private TMP_Dropdown levelSelectDropdown;
//     [SerializeField] private Button goToLevelButton;
//     [SerializeField] private MainMenu sceneNavigationManager;

//     [Header("Level Navigation Confirmation")]
//     [SerializeField] private GameObject confirmLevelChangePanel;
//     [SerializeField] private TextMeshProUGUI confirmLevelChangeMessageText;
//     [SerializeField] private Button confirmGoToLevelButton;
//     [SerializeField] private Button cancelGoToLevelButton;

//     private const string ADD_TIME_METHOD_NAME = "AddTimeSeconds";

//     private class LevelNavigationTarget
//     {
//         public string DisplayName;
//         public string SceneName;

//         public LevelNavigationTarget(string displayName, string sceneName)
//         {
//             DisplayName = displayName;
//             SceneName = sceneName;
//         }
//     }

//     private readonly List<LevelNavigationTarget> levelNavigationTargets = new List<LevelNavigationTarget>
//     {
//         new LevelNavigationTarget("שלב 1 - ישראל", "level1 - israel"),
//         new LevelNavigationTarget("שלב 1.1 - ישראל", "level1.1 - israel"),
//         new LevelNavigationTarget("שלב 1.2 - סין", "level1.2 - china"),
//         new LevelNavigationTarget("שלב 2 - סין", "level2 - china"),
//         new LevelNavigationTarget("שלב 2.1 - ארה\"ב", "level2.1 - USA"),
//         new LevelNavigationTarget("שלב 2.2 - ארה\"ב", "level2.2 - USA"),
//         new LevelNavigationTarget("שלב 3 - ארה\"ב", "level3 - USA")
//     };

//     private string pendingLevelSceneName = string.Empty;
//     private string pendingLevelDisplayName = string.Empty;

//     private bool panelPausedGame;
//     private MethodInfo addTimeMethod;

//     private int selectedCustomerLimit = 1;
//     private int supportedCustomerLimit = 1;

//     private int selectedIngredientCount = 1;
//     private int minIngredientCount = 1;
//     private int maxIngredientCount = 1;

//     private readonly List<Renderer> hiddenWorldRenderers = new List<Renderer>();
//     private readonly List<bool> previousWorldRendererStates = new List<bool>();

//     private void Awake()
//     {
//         SetupCanvasGroup();
//         SetPanelVisible(false);

//         if (openPanelButton != null)
//             openPanelButton.onClick.AddListener(OpenPanel);

//         if (resetButton != null)
//             resetButton.onClick.AddListener(ResetSettings);

//         if (saveButton != null)
//             saveButton.onClick.AddListener(SaveAndClose);

//         if (add30SecondsButton != null)
//             add30SecondsButton.onClick.AddListener(OnAdd30SecondsClicked);

//         if (remove30SecondsButton != null)
//             remove30SecondsButton.onClick.AddListener(OnRemove30SecondsClicked);

//         if (customerLimit1Button != null)
//             customerLimit1Button.onClick.AddListener(OnCustomerLimit1Clicked);

//         if (customerLimit2Button != null)
//             customerLimit2Button.onClick.AddListener(OnCustomerLimit2Clicked);

//         if (customerLimit3Button != null)
//             customerLimit3Button.onClick.AddListener(OnCustomerLimit3Clicked);

//         SetupAngerTimeSlider();
//         SetupMarkAddedItemsToggle();
//         SetupDirtToggle();
//         SetupIngredientCountSlider();
//         SetupLevelNavigation();

//         ResolveLevelTimerIfNeeded();
//         SyncCustomerLimitButtonsFromManager();
//         SyncIngredientCountSliderFromManager();
//         UpdateAngerTimeText();
//     }

//     private void Start()
//     {
//         SyncCustomerLimitButtonsFromManager();
//         SyncIngredientCountSliderFromManager();
//         SelectCurrentSceneInDropdown();
//     }

//     private void OnDestroy()
//     {
//         if (openPanelButton != null)
//             openPanelButton.onClick.RemoveListener(OpenPanel);

//         if (resetButton != null)
//             resetButton.onClick.RemoveListener(ResetSettings);

//         if (saveButton != null)
//             saveButton.onClick.RemoveListener(SaveAndClose);

//         if (add30SecondsButton != null)
//             add30SecondsButton.onClick.RemoveListener(OnAdd30SecondsClicked);

//         if (remove30SecondsButton != null)
//             remove30SecondsButton.onClick.RemoveListener(OnRemove30SecondsClicked);

//         if (customerLimit1Button != null)
//             customerLimit1Button.onClick.RemoveListener(OnCustomerLimit1Clicked);

//         if (customerLimit2Button != null)
//             customerLimit2Button.onClick.RemoveListener(OnCustomerLimit2Clicked);

//         if (customerLimit3Button != null)
//             customerLimit3Button.onClick.RemoveListener(OnCustomerLimit3Clicked);

//         if (goToLevelButton != null)
//             goToLevelButton.onClick.RemoveListener(OnGoToLevelClicked);

//         if (confirmGoToLevelButton != null)
//             confirmGoToLevelButton.onClick.RemoveListener(OnConfirmGoToLevelClicked);

//         if (cancelGoToLevelButton != null)
//             cancelGoToLevelButton.onClick.RemoveListener(OnCancelGoToLevelClicked);

//         if (angerTimeSlider != null)
//             angerTimeSlider.onValueChanged.RemoveListener(OnAngerTimeSliderChanged);

//         if (ingredientCountSlider != null)
//             ingredientCountSlider.onValueChanged.RemoveListener(OnIngredientCountSliderChanged);

//         RestoreWorldVisualsAfterPanel();
//     }

//     private void SetupLevelNavigation()
//     {
//         PopulateLevelDropdown();
//         SelectCurrentSceneInDropdown();
//         SetConfirmLevelChangeVisible(false);

//         if (goToLevelButton != null)
//             goToLevelButton.onClick.AddListener(OnGoToLevelClicked);

//         if (confirmGoToLevelButton != null)
//             confirmGoToLevelButton.onClick.AddListener(OnConfirmGoToLevelClicked);

//         if (cancelGoToLevelButton != null)
//             cancelGoToLevelButton.onClick.AddListener(OnCancelGoToLevelClicked);

//         if (sceneNavigationManager == null)
//             sceneNavigationManager = FindObjectOfType<MainMenu>(true);
//     }

//     private void PopulateLevelDropdown()
//     {
//         if (levelSelectDropdown == null)
//             return;

//         levelSelectDropdown.ClearOptions();

//         List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();

//         foreach (LevelNavigationTarget target in levelNavigationTargets)
//         {
//             options.Add(new TMP_Dropdown.OptionData(target.DisplayName));
//         }

//         levelSelectDropdown.AddOptions(options);
//         levelSelectDropdown.RefreshShownValue();
//     }

//     private void SelectCurrentSceneInDropdown()
//     {
//         if (levelSelectDropdown == null)
//             return;

//         string currentSceneName = SceneManager.GetActiveScene().name;

//         for (int i = 0; i < levelNavigationTargets.Count; i++)
//         {
//             if (levelNavigationTargets[i].SceneName == currentSceneName)
//             {
//                 levelSelectDropdown.SetValueWithoutNotify(i);
//                 levelSelectDropdown.RefreshShownValue();
//                 return;
//             }
//         }

//         levelSelectDropdown.SetValueWithoutNotify(0);
//         levelSelectDropdown.RefreshShownValue();
//     }

//     private void OnGoToLevelClicked()
//     {
//         LevelNavigationTarget target = GetSelectedLevelNavigationTarget();

//         if (target == null)
//         {
//             Debug.LogWarning("[ControlPanelUI] No level was selected.");
//             return;
//         }

//         pendingLevelSceneName = target.SceneName;
//         pendingLevelDisplayName = target.DisplayName;

//         if (confirmLevelChangeMessageText != null)
//         {
//             confirmLevelChangeMessageText.text =
//                 $"האם לעבור אל {pendingLevelDisplayName}?\nההתקדמות הנוכחית בשלב תאבד.";
//         }

//         SetConfirmLevelChangeVisible(true);
//     }

//     private void OnCancelGoToLevelClicked()
//     {
//         pendingLevelSceneName = string.Empty;
//         pendingLevelDisplayName = string.Empty;

//         SetConfirmLevelChangeVisible(false);
//     }

//     private void OnConfirmGoToLevelClicked()
//     {
//         if (string.IsNullOrEmpty(pendingLevelSceneName))
//         {
//             Debug.LogWarning("[ControlPanelUI] Confirm was clicked, but no target scene was selected.");
//             SetConfirmLevelChangeVisible(false);
//             return;
//         }

//         string sceneToLoad = pendingLevelSceneName;

//         SetConfirmLevelChangeVisible(false);
//         RestoreWorldVisualsAfterPanel();
//         SetPanelVisible(false);
//         ForceResumeBeforeSceneChange();

//         if (sceneNavigationManager == null)
//             sceneNavigationManager = FindObjectOfType<MainMenu>(true);

//         if (sceneNavigationManager != null)
//         {
//             sceneNavigationManager.LoadSceneFromControlPanel(sceneToLoad);
//         }
//         else
//         {
//             Debug.LogWarning("[ControlPanelUI] MainMenu navigation manager was not found. Loading scene directly.");
//             Time.timeScale = 1f;
//             SceneManager.LoadScene(sceneToLoad);
//         }
//     }

//     private void ForceResumeBeforeSceneChange()
//     {
//         if (PauseManager.Instance != null && PauseManager.Instance.IsPaused)
//         {
//             if (pauseButton != null)
//                 pauseButton.Toggle();
//             else
//                 PauseManager.Instance.TogglePause();
//         }
//         else
//         {
//             if (pauseButton != null)
//                 pauseButton.UpdateIcon();
//         }

//         panelPausedGame = false;
//         Time.timeScale = 1f;
//     }

//     private LevelNavigationTarget GetSelectedLevelNavigationTarget()
//     {
//         if (levelSelectDropdown == null)
//             return null;

//         int index = levelSelectDropdown.value;

//         if (index < 0 || index >= levelNavigationTargets.Count)
//             return null;

//         return levelNavigationTargets[index];
//     }

//     private void SetConfirmLevelChangeVisible(bool visible)
//     {
//         if (confirmLevelChangePanel != null)
//             confirmLevelChangePanel.SetActive(visible);
//     }

//     private void SetupCanvasGroup()
//     {
//         if (controlPanelOverlay == null)
//             return;

//         if (controlPanelCanvasGroup == null)
//             controlPanelCanvasGroup = controlPanelOverlay.GetComponent<CanvasGroup>();

//         if (controlPanelCanvasGroup == null)
//             controlPanelCanvasGroup = controlPanelOverlay.AddComponent<CanvasGroup>();
//     }

//    private void SetupAngerTimeSlider()
//     {
//         if (angerTimeSlider == null)
//             return;

//         angerTimeSlider.minValue = minAngerTimeSeconds;
//         angerTimeSlider.maxValue = maxAngerTimeSeconds;
//         angerTimeSlider.wholeNumbers = true;

//         angerTimeSlider.value = Mathf.Clamp(
//             CustomerMoodTimer_levels.RuntimeSecondsPerStage,
//             angerTimeSlider.minValue,
//             angerTimeSlider.maxValue
//         );

//         angerTimeSlider.onValueChanged.AddListener(OnAngerTimeSliderChanged);
//     }

//     private void SetupMarkAddedItemsToggle()
//     {
//         if (markAddedItemsToggle == null)
//             return;

//         markAddedItemsToggle.isOn = MarkAddedItemsEnabled;
//     }

//     private void SetupDirtToggle()
//     {
//         if (showDirtToggle == null)
//             return;

//         showDirtToggle.isOn = DirtEnabled;
//     }

//     private void SetupIngredientCountSlider()
//     {
//         if (ingredientCountSlider == null)
//             return;

//         ingredientCountSlider.wholeNumbers = true;
//         ingredientCountSlider.onValueChanged.AddListener(OnIngredientCountSliderChanged);
//     }

//     public void OpenPanel()
//     {
//         transform.SetAsLastSibling();

//         if (angerTimeSlider != null)
//             angerTimeSlider.value = CustomerMoodTimer_levels.RuntimeSecondsPerStage;

//         if (markAddedItemsToggle != null)
//             markAddedItemsToggle.isOn = MarkAddedItemsEnabled;

//         if (showDirtToggle != null)
//             showDirtToggle.isOn = DirtEnabled;

//         SyncCustomerLimitButtonsFromManager();
//         SyncIngredientCountSliderFromManager();
//         SelectCurrentSceneInDropdown();

//         UpdateAngerTimeText();

//         PauseGameForPanel();
//         HideWorldVisualsForPanel();
//         SetPanelVisible(true);
//     }

//     public void SaveAndClose()
//     {
//         ApplyAngerTimeSetting();
//         ApplyMarkAddedItemsSetting();
//         ApplyDirtSetting();
//         ApplyCustomerLimitSetting();
//         ApplyIngredientCountSetting();

//         SetConfirmLevelChangeVisible(false);
//         SetPanelVisible(false);
//         RestoreWorldVisualsAfterPanel();
//         ResumeGameAfterPanel();
//     }

//     public void ResetSettings()
//     {
//         if (angerTimeSlider != null)
//             angerTimeSlider.value = defaultAngerTimeSeconds;

//         if (markAddedItemsToggle != null)
//             markAddedItemsToggle.isOn = defaultMarkAddedItemsEnabled;

//         if (showDirtToggle != null)
//             showDirtToggle.isOn = defaultDirtEnabled;

//         MarkAddedItemsEnabled = defaultMarkAddedItemsEnabled;
//         DirtEnabled = defaultDirtEnabled;

//         ResetCustomerLimitToLevelDefault();
//         ResetIngredientCountToLevelDefault();

//         if (!MarkAddedItemsEnabled)
//             Customer.ClearAllCustomerIngredientMarkers();

//         if (!DirtEnabled && DirtStateManager.Instance != null)
//             DirtStateManager.Instance.Clean();

//         UpdateAngerTimeText();

//         Debug.Log("[ControlPanelUI] Settings reset to defaults.");
//     }

//     private void ApplyAngerTimeSetting()
//     {
//         if (angerTimeSlider == null)
//             return;

//         int selectedAngerTime = Mathf.RoundToInt(angerTimeSlider.value);
//         CustomerMoodTimer_levels.SetRuntimeSecondsPerStage(selectedAngerTime);

//         Debug.Log($"[ControlPanelUI] Anger time saved: {selectedAngerTime} seconds per stage.");
//     }

//     private void ApplyMarkAddedItemsSetting()
//     {
//         if (markAddedItemsToggle == null)
//         {
//             MarkAddedItemsEnabled = false;
//             Customer.ClearAllCustomerIngredientMarkers();
//             Debug.LogWarning("[ControlPanelUI] Mark Added Items Toggle is not assigned.");
//             return;
//         }

//         MarkAddedItemsEnabled = markAddedItemsToggle.isOn;

//         if (!MarkAddedItemsEnabled)
//             Customer.ClearAllCustomerIngredientMarkers();

//         Debug.Log($"[ControlPanelUI] Mark added items saved: {MarkAddedItemsEnabled}");
//     }

//     private void ApplyDirtSetting()
//     {
//         if (showDirtToggle == null)
//         {
//             DirtEnabled = true;
//             Debug.LogWarning("[ControlPanelUI] Show Dirt Toggle is not assigned. Dirt remains enabled.");
//             return;
//         }

//         DirtEnabled = showDirtToggle.isOn;

//         if (!DirtEnabled && DirtStateManager.Instance != null)
//             DirtStateManager.Instance.Clean();

//         Debug.Log($"[ControlPanelUI] Dirt enabled saved: {DirtEnabled}");
//     }

//     private void ApplyCustomerLimitSetting()
//     {
//         CustomerManager manager = CustomerManager.Instance;

//         if (manager == null)
//         {
//             Debug.LogWarning("[ControlPanelUI] CustomerManager.Instance was not found.");
//             return;
//         }

//         int supported = manager.GetMaxSupportedConcurrentCustomers();
//         selectedCustomerLimit = Mathf.Clamp(selectedCustomerLimit, 1, supported);

//         manager.SetMaxConcurrentCustomers(selectedCustomerLimit);
//         SyncCustomerLimitButtonsFromManager();

//         Debug.Log($"[ControlPanelUI] Customer limit saved: {selectedCustomerLimit}");
//     }

//     private void ApplyIngredientCountSetting()
//     {
//         LevelIngredientAvailabilityManager manager = LevelIngredientAvailabilityManager.Instance;

//         if (manager == null)
//         {
//             Debug.LogWarning("[ControlPanelUI] LevelIngredientAvailabilityManager.Instance was not found.");
//             return;
//         }

//         selectedIngredientCount = Mathf.Clamp(
//             selectedIngredientCount,
//             manager.MinIngredientCount,
//             manager.MaxIngredientCount
//         );

//         manager.ApplyIngredientCount(selectedIngredientCount, true);
//         SyncIngredientCountSliderFromManager();

//         Debug.Log($"[ControlPanelUI] Ingredient count saved: {selectedIngredientCount}/{manager.MaxIngredientCount}");
//     }

//     private void SyncCustomerLimitButtonsFromManager()
//     {
//         CustomerManager manager = CustomerManager.Instance;

//         if (manager == null)
//         {
//             supportedCustomerLimit = 1;
//             selectedCustomerLimit = 1;
//             UpdateCustomerLimitButtonsVisualState();
//             return;
//         }

//         supportedCustomerLimit = manager.GetMaxSupportedConcurrentCustomers();
//         selectedCustomerLimit = manager.GetCurrentMaxConcurrentCustomers();
//         selectedCustomerLimit = Mathf.Clamp(selectedCustomerLimit, 1, supportedCustomerLimit);

//         UpdateCustomerLimitButtonsVisualState();
//     }

//     private void ResetCustomerLimitToLevelDefault()
//     {
//         CustomerManager manager = CustomerManager.Instance;

//         if (manager == null)
//         {
//             supportedCustomerLimit = 1;
//             selectedCustomerLimit = 1;
//         }
//         else
//         {
//             supportedCustomerLimit = manager.GetMaxSupportedConcurrentCustomers();
//             selectedCustomerLimit = supportedCustomerLimit;
//         }

//         UpdateCustomerLimitButtonsVisualState();
//     }

//     private void SyncIngredientCountSliderFromManager()
//     {
//         LevelIngredientAvailabilityManager manager = LevelIngredientAvailabilityManager.Instance;

//         if (manager == null)
//         {
//             minIngredientCount = 1;
//             maxIngredientCount = 1;
//             selectedIngredientCount = 1;

//             if (ingredientCountSlider != null)
//             {
//                 ingredientCountSlider.interactable = false;
//                 ingredientCountSlider.minValue = 1;
//                 ingredientCountSlider.maxValue = 1;
//                 ingredientCountSlider.SetValueWithoutNotify(1);
//             }

//             UpdateIngredientCountText();
//             return;
//         }

//         minIngredientCount = manager.MinIngredientCount;
//         maxIngredientCount = manager.MaxIngredientCount;
//         selectedIngredientCount = Mathf.Clamp(
//             manager.CurrentIngredientCount,
//             minIngredientCount,
//             maxIngredientCount
//         );

//         if (ingredientCountSlider != null)
//         {
//             ingredientCountSlider.interactable = true;
//             ingredientCountSlider.wholeNumbers = true;
//             ingredientCountSlider.minValue = minIngredientCount;
//             ingredientCountSlider.maxValue = maxIngredientCount;
//             ingredientCountSlider.SetValueWithoutNotify(selectedIngredientCount);
//         }

//         UpdateIngredientCountText();
//     }

//     private void ResetIngredientCountToLevelDefault()
//     {
//         LevelIngredientAvailabilityManager manager = LevelIngredientAvailabilityManager.Instance;

//         if (manager == null)
//         {
//             minIngredientCount = 1;
//             maxIngredientCount = 1;
//             selectedIngredientCount = 1;
//         }
//         else
//         {
//             minIngredientCount = manager.MinIngredientCount;
//             maxIngredientCount = manager.MaxIngredientCount;
//             selectedIngredientCount = manager.MaxIngredientCount;
//         }

//         if (ingredientCountSlider != null)
//         {
//             ingredientCountSlider.interactable = true;
//             ingredientCountSlider.minValue = minIngredientCount;
//             ingredientCountSlider.maxValue = maxIngredientCount;
//             ingredientCountSlider.SetValueWithoutNotify(selectedIngredientCount);
//         }

//         UpdateIngredientCountText();
//     }

//     private void OnCustomerLimit1Clicked()
//     {
//         SelectCustomerLimit(1);
//     }

//     private void OnCustomerLimit2Clicked()
//     {
//         SelectCustomerLimit(2);
//     }

//     private void OnCustomerLimit3Clicked()
//     {
//         SelectCustomerLimit(3);
//     }

//     private void SelectCustomerLimit(int amount)
//     {
//         if (amount < 1 || amount > supportedCustomerLimit)
//             return;

//         selectedCustomerLimit = amount;
//         UpdateCustomerLimitButtonsVisualState();

//         Debug.Log($"[ControlPanelUI] Customer limit selected in panel: {selectedCustomerLimit}");
//     }

//     private void UpdateCustomerLimitButtonsVisualState()
//     {
//         UpdateCustomerLimitButton(customerLimit1Button, 1);
//         UpdateCustomerLimitButton(customerLimit2Button, 2);
//         UpdateCustomerLimitButton(customerLimit3Button, 3);
//     }

//     private void UpdateCustomerLimitButton(Button button, int value)
//     {
//         if (button == null)
//             return;

//         bool supported = value <= supportedCustomerLimit;
//         bool selected = value == selectedCustomerLimit;

//         button.interactable = supported;

//         Color targetColor;

//         if (!supported)
//             targetColor = disabledCustomerLimitColor;
//         else if (selected)
//             targetColor = selectedCustomerLimitColor;
//         else
//             targetColor = normalCustomerLimitColor;

//         Graphic graphic = button.targetGraphic;

//         if (graphic != null)
//             graphic.color = targetColor;

//         ColorBlock colors = button.colors;
//         colors.normalColor = targetColor;
//         colors.highlightedColor = supported ? targetColor * 1.08f : disabledCustomerLimitColor;
//         colors.pressedColor = supported ? selectedCustomerLimitColor * 0.9f : disabledCustomerLimitColor;
//         colors.selectedColor = targetColor;
//         colors.disabledColor = disabledCustomerLimitColor;
//         button.colors = colors;

//         TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>(true);

//         if (label != null)
//             label.color = supported ? enabledCustomerLimitTextColor : disabledCustomerLimitTextColor;
//     }

//     private void OnIngredientCountSliderChanged(float value)
//     {
//         selectedIngredientCount = Mathf.RoundToInt(value);
//         selectedIngredientCount = Mathf.Clamp(selectedIngredientCount, minIngredientCount, maxIngredientCount);

//         UpdateIngredientCountText();

//         Debug.Log($"[ControlPanelUI] Ingredient count selected in panel: {selectedIngredientCount}/{maxIngredientCount}");
//     }

//     private void UpdateIngredientCountText()
//     {
//         if (ingredientCountValueText == null)
//             return;

//         ingredientCountValueText.text = $"{selectedIngredientCount}/{maxIngredientCount}";
//     }

//     private void PauseGameForPanel()
//     {
//         panelPausedGame = false;

//         if (PauseManager.Instance == null)
//             return;

//         if (!PauseManager.Instance.IsPaused)
//         {
//             panelPausedGame = true;

//             if (pauseButton != null)
//                 pauseButton.Toggle();
//             else
//                 PauseManager.Instance.TogglePause();
//         }
//         else
//         {
//             if (pauseButton != null)
//                 pauseButton.UpdateIcon();
//         }
//     }

//     private void ResumeGameAfterPanel()
//     {
//         if (PauseManager.Instance == null)
//             return;

//         if (panelPausedGame && PauseManager.Instance.IsPaused)
//         {
//             if (pauseButton != null)
//                 pauseButton.Toggle();
//             else
//                 PauseManager.Instance.TogglePause();
//         }
//         else
//         {
//             if (pauseButton != null)
//                 pauseButton.UpdateIcon();
//         }

//         panelPausedGame = false;
//     }

//     private void SetPanelVisible(bool visible)
//     {
//         if (controlPanelOverlay != null)
//             controlPanelOverlay.SetActive(visible);

//         if (controlPanelCanvasGroup != null)
//         {
//             controlPanelCanvasGroup.alpha = visible ? 1f : 0f;
//             controlPanelCanvasGroup.interactable = visible;
//             controlPanelCanvasGroup.blocksRaycasts = visible;
//         }
//     }

//     private void HideWorldVisualsForPanel()
//     {
//         hiddenWorldRenderers.Clear();
//         previousWorldRendererStates.Clear();

//         if (worldVisualRootsToHideWhilePanelOpen == null)
//             return;

//         foreach (Transform root in worldVisualRootsToHideWhilePanelOpen)
//         {
//             if (root == null)
//                 continue;

//             Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);

//             foreach (Renderer rendererToHide in renderers)
//             {
//                 if (rendererToHide == null)
//                     continue;

//                 hiddenWorldRenderers.Add(rendererToHide);
//                 previousWorldRendererStates.Add(rendererToHide.enabled);

//                 rendererToHide.enabled = false;
//             }
//         }
//     }

//     private void RestoreWorldVisualsAfterPanel()
//     {
//         int count = Mathf.Min(hiddenWorldRenderers.Count, previousWorldRendererStates.Count);

//         for (int i = 0; i < count; i++)
//         {
//             Renderer rendererToRestore = hiddenWorldRenderers[i];

//             if (rendererToRestore == null)
//                 continue;

//             rendererToRestore.enabled = previousWorldRendererStates[i];
//         }

//         hiddenWorldRenderers.Clear();
//         previousWorldRendererStates.Clear();
//     }

//     private void OnAngerTimeSliderChanged(float value)
//     {
//         UpdateAngerTimeText();
//     }

//     private void UpdateAngerTimeText()
//     {
//         if (angerTimeValueText == null || angerTimeSlider == null)
//             return;

//         angerTimeValueText.text = $"{Mathf.RoundToInt(angerTimeSlider.value)} sec";
//     }

//     private void OnAdd30SecondsClicked()
//     {
//         AddTimeToCurrentLevel(30f);
//     }

//     private void OnRemove30SecondsClicked()
//     {
//         AddTimeToCurrentLevel(-30f);
//     }

//     private void AddTimeToCurrentLevel(float seconds)
//     {
//         ResolveLevelTimerIfNeeded();

//         if (levelTimerBehaviour == null || addTimeMethod == null)
//         {
//             Debug.LogWarning("[ControlPanelUI] No level timer with AddTimeSeconds(float) was found or assigned.");
//             return;
//         }

//         addTimeMethod.Invoke(levelTimerBehaviour, new object[] { seconds });
//     }

//     private void ResolveLevelTimerIfNeeded()
//     {
//         if (levelTimerBehaviour != null && TryCacheAddTimeMethod(levelTimerBehaviour))
//             return;

//         MonoBehaviour[] behaviours = FindObjectsOfType<MonoBehaviour>(true);

//         foreach (MonoBehaviour behaviour in behaviours)
//         {
//             if (behaviour == null)
//                 continue;

//             if (TryCacheAddTimeMethod(behaviour))
//             {
//                 levelTimerBehaviour = behaviour;
//                 Debug.Log($"[ControlPanelUI] Found level timer: {behaviour.GetType().Name}");
//                 return;
//             }
//         }

//         addTimeMethod = null;
//     }

//     private bool TryCacheAddTimeMethod(MonoBehaviour behaviour)
//     {
//         if (behaviour == null)
//             return false;

//         MethodInfo method = behaviour.GetType().GetMethod(
//             ADD_TIME_METHOD_NAME,
//             BindingFlags.Instance | BindingFlags.Public,
//             null,
//             new[] { typeof(float) },
//             null
//         );

//         if (method == null)
//             return false;

//         addTimeMethod = method;
//         return true;
//     }
// }
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Controls the runtime control panel UI.
/// Uses the existing PauseButton/PauseManager flow when opening and closing the panel.
/// Applies level time changes immediately and applies saved settings on Save.
/// Also allows manual level navigation through a dropdown and confirmation dialog.
/// </summary>
public class ControlPanelUI : MonoBehaviour
{
    public static bool MarkAddedItemsEnabled { get; private set; }
    public static bool DirtEnabled { get; private set; } = true;
    public static bool CustomerPatienceTimerEnabled { get; private set; } = true;
    public static bool GlutenChildEnabled { get; private set; } = true;
    public static bool ColorfulBackgroundEnabled { get; private set; } = true;

    [Header("Root")]
    [SerializeField] private GameObject controlPanelOverlay;
    [SerializeField] private CanvasGroup controlPanelCanvasGroup;

    [Header("Pause")]
    [SerializeField] private PauseButton pauseButton;

    [Header("Level Timer")]
    [Tooltip("Drag here the object that has the current level timer script. The script must contain AddTimeSeconds(float).")]
    [SerializeField] private MonoBehaviour levelTimerBehaviour;

    [Header("Buttons")]
    [SerializeField] private Button openPanelButton;
    [SerializeField] private Button add30SecondsButton;
    [SerializeField] private Button remove30SecondsButton;
    [SerializeField] private Button resetButton;
    [SerializeField] private Button saveButton;

    [Header("Anger Time")]
    [SerializeField] private Slider angerTimeSlider;
    [SerializeField] private TextMeshProUGUI angerTimeValueText;
    [SerializeField] private Toggle customerPatienceTimerToggle;

    [Header("Anger Time Range")]
    [SerializeField] private int minAngerTimeSeconds = 7;
    [SerializeField] private int maxAngerTimeSeconds = 20;
    [SerializeField] private Color disabledAngerTimeTextColor = new Color(0.65f, 0.65f, 0.65f, 1f);

    [Header("Added Items Marking")]
    [SerializeField] private Toggle markAddedItemsToggle;

    [Header("Dirt Settings")]
    [SerializeField] private Toggle showDirtToggle;

    [Header("Gluten Child")]
    [SerializeField] private Toggle glutenChildToggle;

    [Header("Background Mode")]
    [SerializeField] private Toggle colorfulBackgroundToggle;
    [SerializeField] private SpriteRenderer backgroundRenderer;
    [SerializeField] private Sprite colorfulBackgroundSprite;
    [SerializeField] private Sprite plainWhiteBackgroundSprite;

    [Header("Concurrent Customers")]
    [SerializeField] private Button customerLimit1Button;
    [SerializeField] private Button customerLimit2Button;
    [SerializeField] private Button customerLimit3Button;

    [SerializeField] private Color selectedCustomerLimitColor = new Color(0.36f, 0.62f, 0.30f);
    [SerializeField] private Color normalCustomerLimitColor = new Color(0.54f, 0.52f, 0.42f);
    [SerializeField] private Color disabledCustomerLimitColor = new Color(0.55f, 0.55f, 0.55f);
    [SerializeField] private Color enabledCustomerLimitTextColor = Color.white;
    [SerializeField] private Color disabledCustomerLimitTextColor = new Color(0.85f, 0.85f, 0.85f);

    [Header("Ingredient Count")]
    [SerializeField] private Slider ingredientCountSlider;
    [SerializeField] private TextMeshProUGUI ingredientCountValueText;

    [Header("World Visuals Hidden While Panel Is Open")]
    [Tooltip("Drag here world objects such as pitta_in_hands or world timer objects. All Renderers under these roots will be hidden while the control panel is open.")]
    [SerializeField] private Transform[] worldVisualRootsToHideWhilePanelOpen;

    [Header("Defaults")]
    [SerializeField] private int defaultAngerTimeSeconds = 7;
    [SerializeField] private bool defaultMarkAddedItemsEnabled = false;
    [SerializeField] private bool defaultDirtEnabled = true;
    [SerializeField] private bool defaultCustomerPatienceTimerEnabled = true;
    [SerializeField] private bool defaultGlutenChildEnabled = true;
    [SerializeField] private bool defaultColorfulBackgroundEnabled = true;

    [Header("Level Navigation")]
    [SerializeField] private TMP_Dropdown levelSelectDropdown;
    [SerializeField] private Button goToLevelButton;
    [SerializeField] private MainMenu sceneNavigationManager;

    [Header("Level Navigation Confirmation")]
    [SerializeField] private GameObject confirmLevelChangePanel;
    [SerializeField] private TextMeshProUGUI confirmLevelChangeMessageText;
    [SerializeField] private Button confirmGoToLevelButton;
    [SerializeField] private Button cancelGoToLevelButton;

    private const string ADD_TIME_METHOD_NAME = "AddTimeSeconds";

    private class LevelNavigationTarget
    {
        public string DisplayName;
        public string SceneName;

        public LevelNavigationTarget(string displayName, string sceneName)
        {
            DisplayName = displayName;
            SceneName = sceneName;
        }
    }

    private readonly List<LevelNavigationTarget> levelNavigationTargets = new List<LevelNavigationTarget>
    {
        new LevelNavigationTarget("שלב 1 - ישראל", "level1 - israel"),
        new LevelNavigationTarget("שלב 1.1 - ישראל", "level1.1 - israel"),
        new LevelNavigationTarget("שלב 1.2 - סין", "level1.2 - china"),
        new LevelNavigationTarget("שלב 2 - סין", "level2 - china"),
        new LevelNavigationTarget("שלב 2.1 - ארה\"ב", "level2.1 - USA"),
        new LevelNavigationTarget("שלב 2.2 - ארה\"ב", "level2.2 - USA"),
        new LevelNavigationTarget("שלב 3 - ארה\"ב", "level3 - USA")
    };

    private string pendingLevelSceneName = string.Empty;
    private string pendingLevelDisplayName = string.Empty;

    private bool panelPausedGame;
    private MethodInfo addTimeMethod;

    private Color angerTimeValueTextOriginalColor;
    private bool hasAngerTimeValueTextOriginalColor;

    private Sprite cachedOriginalBackgroundSprite;

    private int selectedCustomerLimit = 1;
    private int supportedCustomerLimit = 1;

    private int selectedIngredientCount = 1;
    private int minIngredientCount = 1;
    private int maxIngredientCount = 1;

    private readonly List<Renderer> hiddenWorldRenderers = new List<Renderer>();
    private readonly List<bool> previousWorldRendererStates = new List<bool>();

    private void Awake()
    {
        SetupCanvasGroup();
        SetPanelVisible(false);

        if (openPanelButton != null)
            openPanelButton.onClick.AddListener(OpenPanel);

        if (resetButton != null)
            resetButton.onClick.AddListener(ResetSettings);

        if (saveButton != null)
            saveButton.onClick.AddListener(SaveAndClose);

        if (add30SecondsButton != null)
            add30SecondsButton.onClick.AddListener(OnAdd30SecondsClicked);

        if (remove30SecondsButton != null)
            remove30SecondsButton.onClick.AddListener(OnRemove30SecondsClicked);

        if (customerLimit1Button != null)
            customerLimit1Button.onClick.AddListener(OnCustomerLimit1Clicked);

        if (customerLimit2Button != null)
            customerLimit2Button.onClick.AddListener(OnCustomerLimit2Clicked);

        if (customerLimit3Button != null)
            customerLimit3Button.onClick.AddListener(OnCustomerLimit3Clicked);

        SetupAngerTimeSlider();
        SetupCustomerPatienceTimerToggle();
        SetupMarkAddedItemsToggle();
        SetupDirtToggle();
        SetupGlutenChildToggle();
        SetupBackgroundModeToggle();
        SetupIngredientCountSlider();
        SetupLevelNavigation();

        ResolveLevelTimerIfNeeded();
        SyncCustomerLimitButtonsFromManager();
        SyncIngredientCountSliderFromManager();
        UpdateAngerTimeText();
    }

    private void Start()
    {
        SyncCustomerLimitButtonsFromManager();
        SyncIngredientCountSliderFromManager();
        SelectCurrentSceneInDropdown();
    }

    private void OnDestroy()
    {
        if (openPanelButton != null)
            openPanelButton.onClick.RemoveListener(OpenPanel);

        if (resetButton != null)
            resetButton.onClick.RemoveListener(ResetSettings);

        if (saveButton != null)
            saveButton.onClick.RemoveListener(SaveAndClose);

        if (add30SecondsButton != null)
            add30SecondsButton.onClick.RemoveListener(OnAdd30SecondsClicked);

        if (remove30SecondsButton != null)
            remove30SecondsButton.onClick.RemoveListener(OnRemove30SecondsClicked);

        if (customerLimit1Button != null)
            customerLimit1Button.onClick.RemoveListener(OnCustomerLimit1Clicked);

        if (customerLimit2Button != null)
            customerLimit2Button.onClick.RemoveListener(OnCustomerLimit2Clicked);

        if (customerLimit3Button != null)
            customerLimit3Button.onClick.RemoveListener(OnCustomerLimit3Clicked);

        if (goToLevelButton != null)
            goToLevelButton.onClick.RemoveListener(OnGoToLevelClicked);

        if (confirmGoToLevelButton != null)
            confirmGoToLevelButton.onClick.RemoveListener(OnConfirmGoToLevelClicked);

        if (cancelGoToLevelButton != null)
            cancelGoToLevelButton.onClick.RemoveListener(OnCancelGoToLevelClicked);

        if (angerTimeSlider != null)
            angerTimeSlider.onValueChanged.RemoveListener(OnAngerTimeSliderChanged);

        if (customerPatienceTimerToggle != null)
            customerPatienceTimerToggle.onValueChanged.RemoveListener(OnCustomerPatienceTimerToggleChanged);

        if (glutenChildToggle != null)
            glutenChildToggle.onValueChanged.RemoveListener(OnGlutenChildToggleChanged);

        if (colorfulBackgroundToggle != null)
            colorfulBackgroundToggle.onValueChanged.RemoveListener(OnColorfulBackgroundToggleChanged);

        if (ingredientCountSlider != null)
            ingredientCountSlider.onValueChanged.RemoveListener(OnIngredientCountSliderChanged);

        RestoreWorldVisualsAfterPanel();
    }

    private void SetupLevelNavigation()
    {
        PopulateLevelDropdown();
        SelectCurrentSceneInDropdown();
        SetConfirmLevelChangeVisible(false);

        if (goToLevelButton != null)
            goToLevelButton.onClick.AddListener(OnGoToLevelClicked);

        if (confirmGoToLevelButton != null)
            confirmGoToLevelButton.onClick.AddListener(OnConfirmGoToLevelClicked);

        if (cancelGoToLevelButton != null)
            cancelGoToLevelButton.onClick.AddListener(OnCancelGoToLevelClicked);

        if (sceneNavigationManager == null)
            sceneNavigationManager = FindObjectOfType<MainMenu>(true);
    }

    private void PopulateLevelDropdown()
    {
        if (levelSelectDropdown == null)
            return;

        levelSelectDropdown.ClearOptions();

        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();

        foreach (LevelNavigationTarget target in levelNavigationTargets)
        {
            options.Add(new TMP_Dropdown.OptionData(target.DisplayName));
        }

        levelSelectDropdown.AddOptions(options);
        levelSelectDropdown.RefreshShownValue();
    }

    private void SelectCurrentSceneInDropdown()
    {
        if (levelSelectDropdown == null)
            return;

        string currentSceneName = SceneManager.GetActiveScene().name;

        for (int i = 0; i < levelNavigationTargets.Count; i++)
        {
            if (levelNavigationTargets[i].SceneName == currentSceneName)
            {
                levelSelectDropdown.SetValueWithoutNotify(i);
                levelSelectDropdown.RefreshShownValue();
                return;
            }
        }

        levelSelectDropdown.SetValueWithoutNotify(0);
        levelSelectDropdown.RefreshShownValue();
    }

    private void OnGoToLevelClicked()
    {
        LevelNavigationTarget target = GetSelectedLevelNavigationTarget();

        if (target == null)
        {
            Debug.LogWarning("[ControlPanelUI] No level was selected.");
            return;
        }

        pendingLevelSceneName = target.SceneName;
        pendingLevelDisplayName = target.DisplayName;

        if (confirmLevelChangeMessageText != null)
        {
            confirmLevelChangeMessageText.text =
                $"האם לעבור אל {pendingLevelDisplayName}?\nההתקדמות הנוכחית בשלב תאבד.";
        }

        SetConfirmLevelChangeVisible(true);
    }

    private void OnCancelGoToLevelClicked()
    {
        pendingLevelSceneName = string.Empty;
        pendingLevelDisplayName = string.Empty;

        SetConfirmLevelChangeVisible(false);
    }

    private void OnConfirmGoToLevelClicked()
    {
        if (string.IsNullOrEmpty(pendingLevelSceneName))
        {
            Debug.LogWarning("[ControlPanelUI] Confirm was clicked, but no target scene was selected.");
            SetConfirmLevelChangeVisible(false);
            return;
        }

        string sceneToLoad = pendingLevelSceneName;

        SetConfirmLevelChangeVisible(false);
        RestoreWorldVisualsAfterPanel();
        SetPanelVisible(false);
        ForceResumeBeforeSceneChange();

        if (sceneNavigationManager == null)
            sceneNavigationManager = FindObjectOfType<MainMenu>(true);

        if (sceneNavigationManager != null)
        {
            sceneNavigationManager.LoadSceneFromControlPanel(sceneToLoad);
        }
        else
        {
            Debug.LogWarning("[ControlPanelUI] MainMenu navigation manager was not found. Loading scene directly.");
            Time.timeScale = 1f;
            SceneManager.LoadScene(sceneToLoad);
        }
    }

    private void ForceResumeBeforeSceneChange()
    {
        if (PauseManager.Instance != null && PauseManager.Instance.IsPaused)
        {
            if (pauseButton != null)
                pauseButton.Toggle();
            else
                PauseManager.Instance.TogglePause();
        }
        else
        {
            if (pauseButton != null)
                pauseButton.UpdateIcon();
        }

        panelPausedGame = false;
        Time.timeScale = 1f;
    }

    private LevelNavigationTarget GetSelectedLevelNavigationTarget()
    {
        if (levelSelectDropdown == null)
            return null;

        int index = levelSelectDropdown.value;

        if (index < 0 || index >= levelNavigationTargets.Count)
            return null;

        return levelNavigationTargets[index];
    }

    private void SetConfirmLevelChangeVisible(bool visible)
    {
        if (confirmLevelChangePanel != null)
            confirmLevelChangePanel.SetActive(visible);
    }

    private void SetupCanvasGroup()
    {
        if (controlPanelOverlay == null)
            return;

        if (controlPanelCanvasGroup == null)
            controlPanelCanvasGroup = controlPanelOverlay.GetComponent<CanvasGroup>();

        if (controlPanelCanvasGroup == null)
            controlPanelCanvasGroup = controlPanelOverlay.AddComponent<CanvasGroup>();
    }

    private void SetupAngerTimeSlider()
    {
        if (angerTimeSlider == null)
            return;

        angerTimeSlider.minValue = minAngerTimeSeconds;
        angerTimeSlider.maxValue = maxAngerTimeSeconds;
        angerTimeSlider.wholeNumbers = true;

        angerTimeSlider.value = Mathf.Clamp(
            CustomerMoodTimer_levels.RuntimeSecondsPerStage,
            angerTimeSlider.minValue,
            angerTimeSlider.maxValue
        );

        angerTimeSlider.onValueChanged.AddListener(OnAngerTimeSliderChanged);
    }

    private void SetupCustomerPatienceTimerToggle()
    {
        if (angerTimeValueText != null && !hasAngerTimeValueTextOriginalColor)
        {
            angerTimeValueTextOriginalColor = angerTimeValueText.color;
            hasAngerTimeValueTextOriginalColor = true;
        }

        if (customerPatienceTimerToggle == null)
        {
            CustomerPatienceTimerEnabled = true;
            UpdateAngerTimeSliderAvailability();
            return;
        }

        customerPatienceTimerToggle.SetIsOnWithoutNotify(CustomerPatienceTimerEnabled);
        customerPatienceTimerToggle.onValueChanged.AddListener(OnCustomerPatienceTimerToggleChanged);

        UpdateAngerTimeSliderAvailability();
    }

    private void SetupMarkAddedItemsToggle()
    {
        if (markAddedItemsToggle == null)
            return;

        markAddedItemsToggle.isOn = MarkAddedItemsEnabled;
    }

    private void SetupDirtToggle()
    {
        if (showDirtToggle == null)
            return;

        showDirtToggle.isOn = DirtEnabled;
    }

    private void SetupGlutenChildToggle()
    {
        if (glutenChildToggle == null)
        {
            GlutenChildEnabled = true;
            return;
        }

        glutenChildToggle.SetIsOnWithoutNotify(GlutenChildEnabled);
        glutenChildToggle.onValueChanged.AddListener(OnGlutenChildToggleChanged);
    }

    private void SetupBackgroundModeToggle()
    {
        CacheOriginalBackgroundSpriteIfNeeded();

        if (colorfulBackgroundToggle == null)
        {
            ColorfulBackgroundEnabled = true;
            UpdateBackgroundSprite();
            return;
        }

        colorfulBackgroundToggle.SetIsOnWithoutNotify(ColorfulBackgroundEnabled);
        colorfulBackgroundToggle.onValueChanged.AddListener(OnColorfulBackgroundToggleChanged);

        UpdateBackgroundSprite();
    }

    private void SetupIngredientCountSlider()
    {
        if (ingredientCountSlider == null)
            return;

        ingredientCountSlider.wholeNumbers = true;
        ingredientCountSlider.onValueChanged.AddListener(OnIngredientCountSliderChanged);
    }

    public void OpenPanel()
    {
        transform.SetAsLastSibling();

        if (angerTimeSlider != null)
        {
            angerTimeSlider.value = Mathf.Clamp(
                CustomerMoodTimer_levels.RuntimeSecondsPerStage,
                angerTimeSlider.minValue,
                angerTimeSlider.maxValue
            );
        }

        if (customerPatienceTimerToggle != null)
            customerPatienceTimerToggle.SetIsOnWithoutNotify(CustomerPatienceTimerEnabled);

        UpdateAngerTimeSliderAvailability();

        if (markAddedItemsToggle != null)
            markAddedItemsToggle.isOn = MarkAddedItemsEnabled;

        if (showDirtToggle != null)
            showDirtToggle.isOn = DirtEnabled;

        if (glutenChildToggle != null)
            glutenChildToggle.SetIsOnWithoutNotify(GlutenChildEnabled);

        if (colorfulBackgroundToggle != null)
            colorfulBackgroundToggle.SetIsOnWithoutNotify(ColorfulBackgroundEnabled);

        SyncCustomerLimitButtonsFromManager();
        SyncIngredientCountSliderFromManager();
        SelectCurrentSceneInDropdown();

        UpdateAngerTimeText();

        PauseGameForPanel();
        HideWorldVisualsForPanel();
        SetPanelVisible(true);
    }

    public void SaveAndClose()
    {
        ApplyAngerTimeSetting();
        ApplyCustomerPatienceTimerSetting();
        ApplyMarkAddedItemsSetting();
        ApplyDirtSetting();
        ApplyGlutenChildSetting();
        ApplyBackgroundModeSetting();
        ApplyCustomerLimitSetting();
        ApplyIngredientCountSetting();

        SetConfirmLevelChangeVisible(false);
        SetPanelVisible(false);
        RestoreWorldVisualsAfterPanel();
        ResumeGameAfterPanel();
    }

    public void ResetSettings()
    {
        if (angerTimeSlider != null)
            angerTimeSlider.value = defaultAngerTimeSeconds;

        if (markAddedItemsToggle != null)
            markAddedItemsToggle.isOn = defaultMarkAddedItemsEnabled;

        if (showDirtToggle != null)
            showDirtToggle.isOn = defaultDirtEnabled;

        if (glutenChildToggle != null)
            glutenChildToggle.SetIsOnWithoutNotify(defaultGlutenChildEnabled);

        if (colorfulBackgroundToggle != null)
            colorfulBackgroundToggle.SetIsOnWithoutNotify(defaultColorfulBackgroundEnabled);

        if (customerPatienceTimerToggle != null)
            customerPatienceTimerToggle.SetIsOnWithoutNotify(defaultCustomerPatienceTimerEnabled);

        MarkAddedItemsEnabled = defaultMarkAddedItemsEnabled;
        DirtEnabled = defaultDirtEnabled;
        GlutenChildEnabled = defaultGlutenChildEnabled;
        ColorfulBackgroundEnabled = defaultColorfulBackgroundEnabled;
        CustomerPatienceTimerEnabled = defaultCustomerPatienceTimerEnabled;

        UpdateAngerTimeSliderAvailability();
        UpdateBackgroundSprite();

        ResetCustomerLimitToLevelDefault();
        ResetIngredientCountToLevelDefault();

        if (!MarkAddedItemsEnabled)
            Customer.ClearAllCustomerIngredientMarkers();

        if (!DirtEnabled && DirtStateManager.Instance != null)
            DirtStateManager.Instance.Clean();

        UpdateAngerTimeText();

        Debug.Log("[ControlPanelUI] Settings reset to defaults.");
    }

    private void ApplyAngerTimeSetting()
    {
        if (angerTimeSlider == null)
            return;

        int selectedAngerTime = Mathf.RoundToInt(angerTimeSlider.value);
        CustomerMoodTimer_levels.SetRuntimeSecondsPerStage(selectedAngerTime);

        Debug.Log($"[ControlPanelUI] Anger time saved: {selectedAngerTime} seconds per stage.");
    }

    private void ApplyCustomerPatienceTimerSetting()
    {
        if (customerPatienceTimerToggle == null)
        {
            CustomerPatienceTimerEnabled = true;
            Debug.LogWarning("[ControlPanelUI] Customer Patience Timer Toggle is not assigned. Timer remains enabled.");
            return;
        }

        CustomerPatienceTimerEnabled = customerPatienceTimerToggle.isOn;
        UpdateAngerTimeSliderAvailability();

        Debug.Log($"[ControlPanelUI] Customer patience timer enabled: {CustomerPatienceTimerEnabled}");
    }

    private void ApplyMarkAddedItemsSetting()
    {
        if (markAddedItemsToggle == null)
        {
            MarkAddedItemsEnabled = false;
            Customer.ClearAllCustomerIngredientMarkers();
            Debug.LogWarning("[ControlPanelUI] Mark Added Items Toggle is not assigned.");
            return;
        }

        MarkAddedItemsEnabled = markAddedItemsToggle.isOn;

        if (!MarkAddedItemsEnabled)
            Customer.ClearAllCustomerIngredientMarkers();

        Debug.Log($"[ControlPanelUI] Mark added items saved: {MarkAddedItemsEnabled}");
    }

    private void ApplyDirtSetting()
    {
        if (showDirtToggle == null)
        {
            DirtEnabled = true;
            Debug.LogWarning("[ControlPanelUI] Show Dirt Toggle is not assigned. Dirt remains enabled.");
            return;
        }

        DirtEnabled = showDirtToggle.isOn;

        if (!DirtEnabled && DirtStateManager.Instance != null)
            DirtStateManager.Instance.Clean();

        Debug.Log($"[ControlPanelUI] Dirt enabled saved: {DirtEnabled}");
    }

    private void ApplyGlutenChildSetting()
    {
        if (glutenChildToggle == null)
        {
            GlutenChildEnabled = true;
            Debug.LogWarning("[ControlPanelUI] Gluten Child Toggle is not assigned. Gluten child remains enabled.");
            return;
        }

        GlutenChildEnabled = glutenChildToggle.isOn;

        Debug.Log($"[ControlPanelUI] Gluten child enabled: {GlutenChildEnabled}");
    }

    private void ApplyBackgroundModeSetting()
    {
        if (colorfulBackgroundToggle == null)
        {
            ColorfulBackgroundEnabled = true;
            Debug.LogWarning("[ControlPanelUI] Colorful Background Toggle is not assigned. Colorful background remains enabled.");
        }
        else
        {
            ColorfulBackgroundEnabled = colorfulBackgroundToggle.isOn;
        }

        UpdateBackgroundSprite();

        Debug.Log($"[ControlPanelUI] Colorful background enabled: {ColorfulBackgroundEnabled}");
    }

    private void ApplyCustomerLimitSetting()
    {
        CustomerManager manager = CustomerManager.Instance;

        if (manager == null)
        {
            Debug.LogWarning("[ControlPanelUI] CustomerManager.Instance was not found.");
            return;
        }

        int supported = manager.GetMaxSupportedConcurrentCustomers();
        selectedCustomerLimit = Mathf.Clamp(selectedCustomerLimit, 1, supported);

        manager.SetMaxConcurrentCustomers(selectedCustomerLimit);
        SyncCustomerLimitButtonsFromManager();

        Debug.Log($"[ControlPanelUI] Customer limit saved: {selectedCustomerLimit}");
    }

    private void ApplyIngredientCountSetting()
    {
        LevelIngredientAvailabilityManager manager = LevelIngredientAvailabilityManager.Instance;

        if (manager == null)
        {
            Debug.LogWarning("[ControlPanelUI] LevelIngredientAvailabilityManager.Instance was not found.");
            return;
        }

        selectedIngredientCount = Mathf.Clamp(
            selectedIngredientCount,
            manager.MinIngredientCount,
            manager.MaxIngredientCount
        );

        manager.ApplyIngredientCount(selectedIngredientCount, true);
        SyncIngredientCountSliderFromManager();

        Debug.Log($"[ControlPanelUI] Ingredient count saved: {selectedIngredientCount}/{manager.MaxIngredientCount}");
    }

    private void SyncCustomerLimitButtonsFromManager()
    {
        CustomerManager manager = CustomerManager.Instance;

        if (manager == null)
        {
            supportedCustomerLimit = 1;
            selectedCustomerLimit = 1;
            UpdateCustomerLimitButtonsVisualState();
            return;
        }

        supportedCustomerLimit = manager.GetMaxSupportedConcurrentCustomers();
        selectedCustomerLimit = manager.GetCurrentMaxConcurrentCustomers();
        selectedCustomerLimit = Mathf.Clamp(selectedCustomerLimit, 1, supportedCustomerLimit);

        UpdateCustomerLimitButtonsVisualState();
    }

    private void ResetCustomerLimitToLevelDefault()
    {
        CustomerManager manager = CustomerManager.Instance;

        if (manager == null)
        {
            supportedCustomerLimit = 1;
            selectedCustomerLimit = 1;
        }
        else
        {
            supportedCustomerLimit = manager.GetMaxSupportedConcurrentCustomers();
            selectedCustomerLimit = supportedCustomerLimit;
        }

        UpdateCustomerLimitButtonsVisualState();
    }

    private void SyncIngredientCountSliderFromManager()
    {
        LevelIngredientAvailabilityManager manager = LevelIngredientAvailabilityManager.Instance;

        if (manager == null)
        {
            minIngredientCount = 1;
            maxIngredientCount = 1;
            selectedIngredientCount = 1;

            if (ingredientCountSlider != null)
            {
                ingredientCountSlider.interactable = false;
                ingredientCountSlider.minValue = 1;
                ingredientCountSlider.maxValue = 1;
                ingredientCountSlider.SetValueWithoutNotify(1);
            }

            UpdateIngredientCountText();
            return;
        }

        minIngredientCount = manager.MinIngredientCount;
        maxIngredientCount = manager.MaxIngredientCount;
        selectedIngredientCount = Mathf.Clamp(
            manager.CurrentIngredientCount,
            minIngredientCount,
            maxIngredientCount
        );

        if (ingredientCountSlider != null)
        {
            ingredientCountSlider.interactable = true;
            ingredientCountSlider.wholeNumbers = true;
            ingredientCountSlider.minValue = minIngredientCount;
            ingredientCountSlider.maxValue = maxIngredientCount;
            ingredientCountSlider.SetValueWithoutNotify(selectedIngredientCount);
        }

        UpdateIngredientCountText();
    }

    private void ResetIngredientCountToLevelDefault()
    {
        LevelIngredientAvailabilityManager manager = LevelIngredientAvailabilityManager.Instance;

        if (manager == null)
        {
            minIngredientCount = 1;
            maxIngredientCount = 1;
            selectedIngredientCount = 1;
        }
        else
        {
            minIngredientCount = manager.MinIngredientCount;
            maxIngredientCount = manager.MaxIngredientCount;
            selectedIngredientCount = manager.MaxIngredientCount;
        }

        if (ingredientCountSlider != null)
        {
            ingredientCountSlider.interactable = true;
            ingredientCountSlider.minValue = minIngredientCount;
            ingredientCountSlider.maxValue = maxIngredientCount;
            ingredientCountSlider.SetValueWithoutNotify(selectedIngredientCount);
        }

        UpdateIngredientCountText();
    }

    private void OnCustomerLimit1Clicked()
    {
        SelectCustomerLimit(1);
    }

    private void OnCustomerLimit2Clicked()
    {
        SelectCustomerLimit(2);
    }

    private void OnCustomerLimit3Clicked()
    {
        SelectCustomerLimit(3);
    }

    private void SelectCustomerLimit(int amount)
    {
        if (amount < 1 || amount > supportedCustomerLimit)
            return;

        selectedCustomerLimit = amount;
        UpdateCustomerLimitButtonsVisualState();

        Debug.Log($"[ControlPanelUI] Customer limit selected in panel: {selectedCustomerLimit}");
    }

    private void UpdateCustomerLimitButtonsVisualState()
    {
        UpdateCustomerLimitButton(customerLimit1Button, 1);
        UpdateCustomerLimitButton(customerLimit2Button, 2);
        UpdateCustomerLimitButton(customerLimit3Button, 3);
    }

    private void UpdateCustomerLimitButton(Button button, int value)
    {
        if (button == null)
            return;

        bool supported = value <= supportedCustomerLimit;
        bool selected = value == selectedCustomerLimit;

        button.interactable = supported;

        Color targetColor;

        if (!supported)
            targetColor = disabledCustomerLimitColor;
        else if (selected)
            targetColor = selectedCustomerLimitColor;
        else
            targetColor = normalCustomerLimitColor;

        Graphic graphic = button.targetGraphic;

        if (graphic != null)
            graphic.color = targetColor;

        ColorBlock colors = button.colors;
        colors.normalColor = targetColor;
        colors.highlightedColor = supported ? targetColor * 1.08f : disabledCustomerLimitColor;
        colors.pressedColor = supported ? selectedCustomerLimitColor * 0.9f : disabledCustomerLimitColor;
        colors.selectedColor = targetColor;
        colors.disabledColor = disabledCustomerLimitColor;
        button.colors = colors;

        TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>(true);

        if (label != null)
            label.color = supported ? enabledCustomerLimitTextColor : disabledCustomerLimitTextColor;
    }

    private void OnIngredientCountSliderChanged(float value)
    {
        selectedIngredientCount = Mathf.RoundToInt(value);
        selectedIngredientCount = Mathf.Clamp(selectedIngredientCount, minIngredientCount, maxIngredientCount);

        UpdateIngredientCountText();

        Debug.Log($"[ControlPanelUI] Ingredient count selected in panel: {selectedIngredientCount}/{maxIngredientCount}");
    }

    private void UpdateIngredientCountText()
    {
        if (ingredientCountValueText == null)
            return;

        ingredientCountValueText.text = $"{selectedIngredientCount}/{maxIngredientCount}";
    }

    private void PauseGameForPanel()
    {
        panelPausedGame = false;

        if (PauseManager.Instance == null)
            return;

        if (!PauseManager.Instance.IsPaused)
        {
            panelPausedGame = true;

            if (pauseButton != null)
                pauseButton.Toggle();
            else
                PauseManager.Instance.TogglePause();
        }
        else
        {
            if (pauseButton != null)
                pauseButton.UpdateIcon();
        }
    }

    private void ResumeGameAfterPanel()
    {
        if (PauseManager.Instance == null)
            return;

        if (panelPausedGame && PauseManager.Instance.IsPaused)
        {
            if (pauseButton != null)
                pauseButton.Toggle();
            else
                PauseManager.Instance.TogglePause();
        }
        else
        {
            if (pauseButton != null)
                pauseButton.UpdateIcon();
        }

        panelPausedGame = false;
    }

    private void SetPanelVisible(bool visible)
    {
        if (controlPanelOverlay != null)
            controlPanelOverlay.SetActive(visible);

        if (controlPanelCanvasGroup != null)
        {
            controlPanelCanvasGroup.alpha = visible ? 1f : 0f;
            controlPanelCanvasGroup.interactable = visible;
            controlPanelCanvasGroup.blocksRaycasts = visible;
        }
    }

    private void HideWorldVisualsForPanel()
    {
        hiddenWorldRenderers.Clear();
        previousWorldRendererStates.Clear();

        if (worldVisualRootsToHideWhilePanelOpen == null)
            return;

        foreach (Transform root in worldVisualRootsToHideWhilePanelOpen)
        {
            if (root == null)
                continue;

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);

            foreach (Renderer rendererToHide in renderers)
            {
                if (rendererToHide == null)
                    continue;

                hiddenWorldRenderers.Add(rendererToHide);
                previousWorldRendererStates.Add(rendererToHide.enabled);

                rendererToHide.enabled = false;
            }
        }
    }

    private void RestoreWorldVisualsAfterPanel()
    {
        int count = Mathf.Min(hiddenWorldRenderers.Count, previousWorldRendererStates.Count);

        for (int i = 0; i < count; i++)
        {
            Renderer rendererToRestore = hiddenWorldRenderers[i];

            if (rendererToRestore == null)
                continue;

            rendererToRestore.enabled = previousWorldRendererStates[i];
        }

        hiddenWorldRenderers.Clear();
        previousWorldRendererStates.Clear();
    }

    private void OnAngerTimeSliderChanged(float value)
    {
        UpdateAngerTimeText();
    }

    private void OnCustomerPatienceTimerToggleChanged(bool isOn)
    {
        CustomerPatienceTimerEnabled = isOn;
        UpdateAngerTimeSliderAvailability();

        Debug.Log($"[ControlPanelUI] Customer patience timer changed immediately: {CustomerPatienceTimerEnabled}");
    }

    private void OnGlutenChildToggleChanged(bool isOn)
    {
        GlutenChildEnabled = isOn;
        Debug.Log($"[ControlPanelUI] Gluten child changed immediately: {GlutenChildEnabled}");
    }

    private void OnColorfulBackgroundToggleChanged(bool isOn)
    {
        ColorfulBackgroundEnabled = isOn;
        UpdateBackgroundSprite();

        Debug.Log($"[ControlPanelUI] Colorful background changed immediately: {ColorfulBackgroundEnabled}");
    }

    private void CacheOriginalBackgroundSpriteIfNeeded()
    {
        if (backgroundRenderer == null)
            return;

        if (cachedOriginalBackgroundSprite == null)
            cachedOriginalBackgroundSprite = backgroundRenderer.sprite;

        if (colorfulBackgroundSprite == null)
            colorfulBackgroundSprite = cachedOriginalBackgroundSprite;
    }

    private void UpdateBackgroundSprite()
    {
        CacheOriginalBackgroundSpriteIfNeeded();

        if (backgroundRenderer == null)
        {
            Debug.LogWarning("[ControlPanelUI] Background Renderer is not assigned.");
            return;
        }

        if (ColorfulBackgroundEnabled)
        {
            if (colorfulBackgroundSprite != null)
            {
                backgroundRenderer.sprite = colorfulBackgroundSprite;
            }

            return;
        }

        if (plainWhiteBackgroundSprite != null)
        {
            backgroundRenderer.sprite = plainWhiteBackgroundSprite;
        }
        else
        {
            Debug.LogWarning("[ControlPanelUI] Plain White Background Sprite is not assigned.");
        }
    }

    private void UpdateAngerTimeSliderAvailability()
    {
        bool isTimerEnabledInPanel = IsCustomerPatienceTimerEnabledInPanel();

        if (angerTimeSlider != null)
            angerTimeSlider.interactable = isTimerEnabledInPanel;

        UpdateAngerTimeText();

        if (angerTimeValueText != null)
        {
            if (!hasAngerTimeValueTextOriginalColor)
            {
                angerTimeValueTextOriginalColor = angerTimeValueText.color;
                hasAngerTimeValueTextOriginalColor = true;
            }

            angerTimeValueText.color = isTimerEnabledInPanel
                ? angerTimeValueTextOriginalColor
                : disabledAngerTimeTextColor;
        }
    }

    private bool IsCustomerPatienceTimerEnabledInPanel()
    {
        return customerPatienceTimerToggle == null || customerPatienceTimerToggle.isOn;
    }

    private void UpdateAngerTimeText()
    {
        if (angerTimeValueText == null || angerTimeSlider == null)
            return;

        if (!IsCustomerPatienceTimerEnabledInPanel())
        {
            angerTimeValueText.text = "יובכ";
            return;
        }

        angerTimeValueText.text = $"{Mathf.RoundToInt(angerTimeSlider.value)} sec";
    }

    private void OnAdd30SecondsClicked()
    {
        AddTimeToCurrentLevel(30f);
    }

    private void OnRemove30SecondsClicked()
    {
        AddTimeToCurrentLevel(-30f);
    }

    private void AddTimeToCurrentLevel(float seconds)
    {
        ResolveLevelTimerIfNeeded();

        if (levelTimerBehaviour == null || addTimeMethod == null)
        {
            Debug.LogWarning("[ControlPanelUI] No level timer with AddTimeSeconds(float) was found or assigned.");
            return;
        }

        addTimeMethod.Invoke(levelTimerBehaviour, new object[] { seconds });
    }

    private void ResolveLevelTimerIfNeeded()
    {
        if (levelTimerBehaviour != null && TryCacheAddTimeMethod(levelTimerBehaviour))
            return;

        MonoBehaviour[] behaviours = FindObjectsOfType<MonoBehaviour>(true);

        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour == null)
                continue;

            if (TryCacheAddTimeMethod(behaviour))
            {
                levelTimerBehaviour = behaviour;
                Debug.Log($"[ControlPanelUI] Found level timer: {behaviour.GetType().Name}");
                return;
            }
        }

        addTimeMethod = null;
    }

    private bool TryCacheAddTimeMethod(MonoBehaviour behaviour)
    {
        if (behaviour == null)
            return false;

        MethodInfo method = behaviour.GetType().GetMethod(
            ADD_TIME_METHOD_NAME,
            BindingFlags.Instance | BindingFlags.Public,
            null,
            new[] { typeof(float) },
            null
        );

        if (method == null)
            return false;

        addTimeMethod = method;
        return true;
    }
}