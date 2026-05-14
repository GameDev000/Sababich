// using TMPro;
// using UnityEngine;
// using UnityEngine.UI;

// /// <summary>
// /// Controls the runtime control panel UI.
// /// Uses the existing PauseButton/PauseManager flow when opening and closing the panel.
// /// </summary>
// public class ControlPanelUI : MonoBehaviour
// {
//     [Header("Root")]
//     [SerializeField] private GameObject controlPanelOverlay;
//     [SerializeField] private CanvasGroup controlPanelCanvasGroup;

//     [Header("Pause")]
//     [SerializeField] private PauseButton pauseButton;

//     [Header("Buttons")]
//     [SerializeField] private Button openPanelButton;
//     [SerializeField] private Button add30SecondsButton;
//     [SerializeField] private Button remove30SecondsButton;
//     [SerializeField] private Button resetButton;
//     [SerializeField] private Button saveButton;

//     [Header("Level Timer")]

//     [SerializeField] private LevelTimerWinLose levelTimer;

//     [Header("Anger Time")]
//     [SerializeField] private Slider angerTimeSlider;
//     [SerializeField] private TextMeshProUGUI angerTimeValueText;

//     [Header("Defaults")]
//     [SerializeField] private int defaultAngerTimeSeconds = 7;

//     private bool panelPausedGame;

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

//         if (angerTimeSlider != null)
//         {
//             angerTimeSlider.minValue = 7;
//             angerTimeSlider.maxValue = 12;
//             angerTimeSlider.wholeNumbers = true;
//             angerTimeSlider.value = defaultAngerTimeSeconds;
//             angerTimeSlider.onValueChanged.AddListener(OnAngerTimeSliderChanged);
//         }

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

//     public void OpenPanel()
//     {
//         PauseGameForPanel();
//         SetPanelVisible(true);
//     }

//     public void SaveAndClose()
//     {
//         // Later we will apply saved gameplay settings here.

//         SetPanelVisible(false);
//         ResumeGameAfterPanel();
//     }

//     public void ResetSettings()
//     {
//         if (angerTimeSlider != null)
//             angerTimeSlider.value = defaultAngerTimeSeconds;

//         UpdateAngerTimeText();

//         // More settings will be reset here later.
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

//     private void OnAngerTimeSliderChanged(float value)
//     {
//         UpdateAngerTimeText();
//     }

//     private void UpdateAngerTimeText()
//     {
//         if (angerTimeValueText == null || angerTimeSlider == null)
//             return;

//         angerTimeValueText.text = $"{Mathf.RoundToInt(angerTimeSlider.value)} שניות";
//     }

//     private void OnAdd30SecondsClicked()
//     {
//         if (levelTimer == null)
//         {
//             Debug.LogWarning("[ControlPanelUI] LevelTimerWinLose is not assigned.");
//             return;
//         }

//         levelTimer.AddTimeSeconds(30f);
//     }

//     private void OnRemove30SecondsClicked()
//     {
//         if (levelTimer == null)
//         {
//             Debug.LogWarning("[ControlPanelUI] LevelTimerWinLose is not assigned.");
//             return;
//         }

//         levelTimer.AddTimeSeconds(-30f);
//     }
// }


using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the runtime control panel UI.
/// Uses the existing PauseButton/PauseManager flow when opening and closing the panel.
/// Applies level time changes immediately and applies anger-time changes on Save.
/// </summary>
public class ControlPanelUI : MonoBehaviour
{
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

    [Header("Defaults")]
    [SerializeField] private int defaultAngerTimeSeconds = 7;

    private const string ADD_TIME_METHOD_NAME = "AddTimeSeconds";

    private bool panelPausedGame;
    private MethodInfo addTimeMethod;

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

    public void OpenPanel()
    {
        // Keep the slider synchronized with the last saved runtime value.
        if (angerTimeSlider != null)
            angerTimeSlider.value = CustomerMoodTimer_levels.RuntimeSecondsPerStage;

        UpdateAngerTimeText();

        PauseGameForPanel();
        SetPanelVisible(true);
    }

    public void SaveAndClose()
    {
        ApplyAngerTimeSetting();

        SetPanelVisible(false);
        ResumeGameAfterPanel();
    }

    public void ResetSettings()
    {
        if (angerTimeSlider != null)
            angerTimeSlider.value = defaultAngerTimeSeconds;

        UpdateAngerTimeText();

        Debug.Log("[ControlPanelUI] Settings reset in panel. Anger time slider returned to 7.");
    }

    private void ApplyAngerTimeSetting()
    {
        if (angerTimeSlider == null)
            return;

        int selectedAngerTime = Mathf.RoundToInt(angerTimeSlider.value);

        CustomerMoodTimer_levels.SetRuntimeSecondsPerStage(selectedAngerTime);

        Debug.Log($"[ControlPanelUI] Anger time saved: {selectedAngerTime} seconds per stage.");
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