using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Authentication;

public class LevelOneTwoTimerWinLose : MonoBehaviour
{
    [Header("Timer")]
    [SerializeField] private float levelDurationSeconds = 90f;

    [Header("Timer UI")]
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Win Condition")]
    [SerializeField] private int coinsTarget = 10;

    [Header("End Scene")]
    [SerializeField] private string endSceneName = "Level1.2 - endScene";

    private float timeLeft;
    private bool finished;

    // Save only once when reaching target
    private bool timeSaved = false;

    // Freeze the exact timeLeft at the first moment target is reached
    private float frozenTimeLeft = -1f;
    private int timeToTargetSeconds = -1;

    private void Start()
    {
        // Clear stats from any previous attempt so retries don't accumulate
        LevelOneTwoState.Reset();

        timeLeft = levelDurationSeconds;
        UpdateTimerUI(timeLeft);
    }

    private void Update()
    {
        if (finished)
            return;

        timeLeft -= Time.deltaTime;

        TryFreezeTimeWhenReachedTarget();

        if (timeLeft <= 0f)
        {
            timeLeft = 0f;
            UpdateTimerUI(timeLeft);

            finished = true;
            EndLevel();
            return;
        }

        UpdateTimerUI(timeLeft);
    }

    /// <summary>
    /// Adds or removes seconds from the current remaining level time.
    /// The timer cannot go below zero.
    /// This is used by the runtime control panel.
    /// </summary>
    public void AddTimeSeconds(float secondsToAdd)
    {
        if (finished)
            return;

        float elapsedTime = Mathf.Max(0f, levelDurationSeconds - timeLeft);

        timeLeft = Mathf.Max(0f, timeLeft + secondsToAdd);

        // Keep levelDurationSeconds aligned so timeToTargetSeconds remains logical
        // even if time was added or removed during the level.
        levelDurationSeconds = elapsedTime + timeLeft;

        UpdateTimerUI(timeLeft);

        Debug.Log($"[LevelOneTwoTimerWinLose] Time changed by {secondsToAdd}. New timeLeft={timeLeft}");
    }

    public float GetTimeLeft()
    {
        return timeLeft;
    }

    public void NotifyMoneyChanged(int newMoney)
    {
        FreezeTimeIfNeeded(newMoney);
    }

    private void TryFreezeTimeWhenReachedTarget()
    {
        int moneyNow = (ScoreManager.Instance != null) ? ScoreManager.Instance.CurrentMoney : 0;
        FreezeTimeIfNeeded(moneyNow);
    }

    private void FreezeTimeIfNeeded(int moneyNow)
    {
        if (timeSaved)
            return;

        if (moneyNow >= coinsTarget)
        {
            if (frozenTimeLeft < 0f)
                frozenTimeLeft = timeLeft;

            SaveLevel2TimeOnce();
        }
    }

    private void SaveLevel2TimeOnce()
    {
        if (timeSaved)
            return;

        timeSaved = true;

        float safeFrozen = (frozenTimeLeft < 0f) ? timeLeft : frozenTimeLeft;
        timeToTargetSeconds = Mathf.RoundToInt(levelDurationSeconds - safeFrozen);

        if (UnityServices.State == ServicesInitializationState.Initialized &&
            AuthenticationService.Instance.IsSignedIn)
        {
            _ = DatabaseManager.SaveData((CloudSaveKeys.Level1_2TimeSeconds, timeToTargetSeconds));
            Debug.Log($"[Level1.2] Saved timeSeconds={timeToTargetSeconds}");
        }
        else
        {
            Debug.LogWarning("[Level1.2] Could not save time (services not ready or not signed in).");
        }
    }

    private async void EndLevel()
    {
        int coinsEnd = (ScoreManager.Instance != null) ? ScoreManager.Instance.CurrentMoney : 0;

        if (!timeSaved && coinsEnd >= coinsTarget)
        {
            if (frozenTimeLeft < 0f)
                frozenTimeLeft = timeLeft;

            SaveLevel2TimeOnce();
        }

        if (!timeSaved)
            timeToTargetSeconds = Mathf.RoundToInt(levelDurationSeconds);

        if (UnityServices.State == ServicesInitializationState.Initialized &&
            AuthenticationService.Instance.IsSignedIn)
        {
            await DatabaseManager.SaveData((CloudSaveKeys.Level1_2Coins, coinsEnd));
        }

        bool success = coinsEnd >= coinsTarget;
        LevelOneTwoState.IsSuccess = success;

        // Read served dishes statistics for Level 1.2
        int totalServed = LevelOneTwoState.TotalServedDishes;
        int perfectServed = LevelOneTwoState.PerfectServedDishes;

        // Save served dishes statistics
        if (UnityServices.State == ServicesInitializationState.Initialized &&
            AuthenticationService.Instance.IsSignedIn)
        {
            await DatabaseManager.SaveData((CloudSaveKeys.Level1_2TotalServed, totalServed));
            await DatabaseManager.SaveData((CloudSaveKeys.Level1_2PerfectServed, perfectServed));
        }

        // Save passed flag
        if (UnityServices.State == ServicesInitializationState.Initialized &&
            AuthenticationService.Instance.IsSignedIn)
        {
            await DatabaseManager.SaveData((CloudSaveKeys.Level1_2Passed, success ? 1 : 0));
        }

        if (UnityServices.State == ServicesInitializationState.Initialized &&
            AuthenticationService.Instance.IsSignedIn)
        {
            await DatabaseManager.SaveData((CloudSaveKeys.DuplicateClicksKey("1.2"), LevelOneTwoState.DuplicateIngredientClicks));
            await DatabaseManager.SaveData((CloudSaveKeys.GlutenChildAppearedKey("1.2"), LevelOneTwoState.GlutenChildAppeared));
            await DatabaseManager.SaveData((CloudSaveKeys.GlutenChildServedKey("1.2"), LevelOneTwoState.GlutenChildServed));
            await DatabaseManager.SaveData((CloudSaveKeys.CustomersArrivedKey("1.2"), LevelOneTwoState.CustomersArrived));
        }

        // Record this level's attempt for dashboard export before leaving the scene
        SessionDataCollector.RecordLevelAttempt(12, timeToTargetSeconds);

        Time.timeScale = 1f;
        SceneManager.LoadScene(endSceneName);
    }

    private void UpdateTimerUI(float secondsLeft)
    {
        if (timerText == null)
            return;

        int totalSeconds = Mathf.CeilToInt(secondsLeft);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        timerText.text = $"{minutes:00}:{seconds:00}";
    }
}