// using System.Collections.Generic;
// using System.Reflection;
// using TMPro;
// using UnityEngine;
// using UnityEngine.UI;

// /// <summary>
// /// Controls the runtime control panel UI.
// /// Uses the existing PauseButton/PauseManager flow when opening and closing the panel.
// /// Applies level time changes immediately and applies saved settings on Save.
// /// </summary>
// public class ControlPanelUI : MonoBehaviour
// {
//     public static bool MarkAddedItemsEnabled { get; private set; }

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

//     [Header("Added Items Marking")]
//     [SerializeField] private Toggle markAddedItemsToggle;

//     [Header("World Visuals Hidden While Panel Is Open")]
//     [Tooltip("Drag here world objects such as pitta_in_hands. All SpriteRenderers under these roots will be hidden while the control panel is open.")]
//     [SerializeField] private Transform[] worldVisualRootsToHideWhilePanelOpen;

//     [Header("Defaults")]
//     [SerializeField] private int defaultAngerTimeSeconds = 7;
//     [SerializeField] private bool defaultMarkAddedItemsEnabled = false;

//     private const string ADD_TIME_METHOD_NAME = "AddTimeSeconds";

//     private bool panelPausedGame;
//     private MethodInfo addTimeMethod;

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

//         SetupAngerTimeSlider();
//         SetupMarkAddedItemsToggle();

//         ResolveLevelTimerIfNeeded();
//         UpdateAngerTimeText();
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

//         if (angerTimeSlider != null)
//             angerTimeSlider.onValueChanged.RemoveListener(OnAngerTimeSliderChanged);

//         RestoreWorldVisualsAfterPanel();
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

//     private void SetupAngerTimeSlider()
//     {
//         if (angerTimeSlider == null)
//             return;

//         angerTimeSlider.minValue = 7;
//         angerTimeSlider.maxValue = 12;
//         angerTimeSlider.wholeNumbers = true;
//         angerTimeSlider.value = CustomerMoodTimer_levels.RuntimeSecondsPerStage;
//         angerTimeSlider.onValueChanged.AddListener(OnAngerTimeSliderChanged);
//     }

//     private void SetupMarkAddedItemsToggle()
//     {
//         if (markAddedItemsToggle == null)
//             return;

//         markAddedItemsToggle.isOn = MarkAddedItemsEnabled;
//     }

//     public void OpenPanel()
//     {
//         transform.SetAsLastSibling();

//         if (angerTimeSlider != null)
//             angerTimeSlider.value = CustomerMoodTimer_levels.RuntimeSecondsPerStage;

//         if (markAddedItemsToggle != null)
//             markAddedItemsToggle.isOn = MarkAddedItemsEnabled;

//         UpdateAngerTimeText();

//         PauseGameForPanel();
//         HideWorldVisualsForPanel();
//         SetPanelVisible(true);
//     }

//     public void SaveAndClose()
//     {
//         ApplyAngerTimeSetting();
//         ApplyMarkAddedItemsSetting();

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

//         MarkAddedItemsEnabled = defaultMarkAddedItemsEnabled;

//         if (!MarkAddedItemsEnabled)
//             Customer.ClearAllCustomerIngredientMarkers();

//         UpdateAngerTimeText();

//         Debug.Log("[ControlPanelUI] Settings reset. Anger time returned to 7 and added-items marking disabled.");
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
//    private void HideWorldVisualsForPanel()
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
using UnityEngine.UI;

/// <summary>
/// Controls the runtime control panel UI.
/// Uses the existing PauseButton/PauseManager flow when opening and closing the panel.
/// Applies level time changes immediately and applies saved settings on Save.
/// </summary>
public class ControlPanelUI : MonoBehaviour
{
    public static bool MarkAddedItemsEnabled { get; private set; }
    public static bool DirtEnabled { get; private set; } = true;

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

    [Header("Added Items Marking")]
    [SerializeField] private Toggle markAddedItemsToggle;

    [Header("Dirt Settings")]
    [SerializeField] private Toggle showDirtToggle;

    [Header("World Visuals Hidden While Panel Is Open")]
    [Tooltip("Drag here world objects such as pitta_in_hands or world timer objects. All Renderers under these roots will be hidden while the control panel is open.")]
    [SerializeField] private Transform[] worldVisualRootsToHideWhilePanelOpen;

    [Header("Defaults")]
    [SerializeField] private int defaultAngerTimeSeconds = 7;
    [SerializeField] private bool defaultMarkAddedItemsEnabled = false;
    [SerializeField] private bool defaultDirtEnabled = true;

    private const string ADD_TIME_METHOD_NAME = "AddTimeSeconds";

    private bool panelPausedGame;
    private MethodInfo addTimeMethod;

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

        SetupAngerTimeSlider();
        SetupMarkAddedItemsToggle();
        SetupDirtToggle();

        ResolveLevelTimerIfNeeded();
        UpdateAngerTimeText();
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

        if (angerTimeSlider != null)
            angerTimeSlider.onValueChanged.RemoveListener(OnAngerTimeSliderChanged);

        RestoreWorldVisualsAfterPanel();
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

        angerTimeSlider.minValue = 7;
        angerTimeSlider.maxValue = 12;
        angerTimeSlider.wholeNumbers = true;
        angerTimeSlider.value = CustomerMoodTimer_levels.RuntimeSecondsPerStage;
        angerTimeSlider.onValueChanged.AddListener(OnAngerTimeSliderChanged);
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

    public void OpenPanel()
    {
        transform.SetAsLastSibling();

        if (angerTimeSlider != null)
            angerTimeSlider.value = CustomerMoodTimer_levels.RuntimeSecondsPerStage;

        if (markAddedItemsToggle != null)
            markAddedItemsToggle.isOn = MarkAddedItemsEnabled;

        if (showDirtToggle != null)
            showDirtToggle.isOn = DirtEnabled;

        UpdateAngerTimeText();

        PauseGameForPanel();
        HideWorldVisualsForPanel();
        SetPanelVisible(true);
    }

    public void SaveAndClose()
    {
        ApplyAngerTimeSetting();
        ApplyMarkAddedItemsSetting();
        ApplyDirtSetting();

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

        MarkAddedItemsEnabled = defaultMarkAddedItemsEnabled;
        DirtEnabled = defaultDirtEnabled;

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
        {
            DirtStateManager.Instance.Clean();
        }

        Debug.Log($"[ControlPanelUI] Dirt enabled saved: {DirtEnabled}");
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

    private void UpdateAngerTimeText()
    {
        if (angerTimeValueText == null || angerTimeSlider == null)
            return;

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