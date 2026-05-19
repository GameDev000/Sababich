using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Authentication;

/// <summary>
/// Manages the timer, win/lose condition, cloud save, and transition to the end scene
/// for Level 3.1 USA.
/// </summary>
public class LevelThreeOneTimerWinLose : MonoBehaviour
{
    [Header("Timer")]
    [SerializeField] private float levelDurationSeconds = 150f;

    [Header("Timer UI")]
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Win Condition")]
    [SerializeField] private int coinsTarget = 300;

    [Header("End Scene")]
    [SerializeField] private string endSceneName = "Level3.1 - endScene";

    private float timeLeft;
    private bool finished;

    // Save only once when reaching target
    private bool timeSaved = false;

    // Freeze the exact timeLeft at the first moment target is reached
    private float frozenTimeLeft = -1f;
    private int timeToTargetSeconds = -1;

    // Separate cloud keys for the pre-level, so Level 3.1 will not overwrite Level 3 data.
    private const string LevelThreeOneTimeSecondsKey = "Level3_1TimeSeconds";
    private const string LevelThreeOneCoinsKey = "Level3_1Coins";
    private const string LevelThreeOneTotalServedKey = "Level3_1TotalServed";
    private const string LevelThreeOnePerfectServedKey = "Level3_1PerfectServed";
    private const string LevelThreeOnePassedKey = "Level3_1Passed";

    private void Start()
    {
        // Clear stats from any previous attempt so retries don't accumulate
        LevelThreeOneState.Reset();

        timeLeft = levelDurationSeconds;
        UpdateTimerUI(timeLeft);
    }

    private void Update()
    {
        if (finished)
            return;

        timeLeft -= Time.deltaTime;

        // Fallback polling
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

        Debug.Log($"[LevelThreeOneTimerWinLose] Time changed by {secondsToAdd}. New timeLeft={timeLeft}");
    }

    public float GetTimeLeft()
    {
        return timeLeft;
    }

    // Called from ScoreManager.AddMoney
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

            SaveLevelThreeOneTimeOnce();
        }
    }

    private void SaveLevelThreeOneTimeOnce()
    {
        if (timeSaved)
            return;

        timeSaved = true;

        float safeFrozen = (frozenTimeLeft < 0f) ? timeLeft : frozenTimeLeft;
        timeToTargetSeconds = Mathf.RoundToInt(levelDurationSeconds - safeFrozen);

        if (UnityServices.State == ServicesInitializationState.Initialized &&
            AuthenticationService.Instance.IsSignedIn)
        {
            _ = DatabaseManager.SaveData((LevelThreeOneTimeSecondsKey, timeToTargetSeconds));
            Debug.Log($"[Level3.1] Saved timeSeconds={timeToTargetSeconds}");
        }
        else
        {
            Debug.LogWarning("[Level3.1] Could not save time (services not ready or not signed in).");
        }
    }

    private async void EndLevel()
    {
        int coinsEnd = (ScoreManager.Instance != null) ? ScoreManager.Instance.CurrentMoney : 0;
        Debug.Log($"[Level3.1] coinsEnd={coinsEnd}");

        // Fallback save
        if (!timeSaved && coinsEnd >= coinsTarget)
        {
            if (frozenTimeLeft < 0f)
                frozenTimeLeft = timeLeft;

            SaveLevelThreeOneTimeOnce();
        }

        if (!timeSaved)
            timeToTargetSeconds = Mathf.RoundToInt(levelDurationSeconds);

        bool servicesReady =
            UnityServices.State == ServicesInitializationState.Initialized &&
            AuthenticationService.Instance.IsSignedIn;

        // Save coins at end
        if (servicesReady)
        {
            await DatabaseManager.SaveData((LevelThreeOneCoinsKey, coinsEnd));
        }

        bool success = coinsEnd >= coinsTarget;
        LevelThreeOneState.IsSuccess = success;

        // Save perfect/total served dishes for Level 3.1 end screen
        int totalServed = LevelThreeOneState.TotalServedDishes;
        int perfectServed = LevelThreeOneState.PerfectServedDishes;

        // Save these stats to cloud
        if (servicesReady)
        {
            await DatabaseManager.SaveData((LevelThreeOneTotalServedKey, totalServed));
            await DatabaseManager.SaveData((LevelThreeOnePerfectServedKey, perfectServed));
        }

        // Save passed flag for level 3.1
        if (servicesReady)
        {
            await DatabaseManager.SaveData((LevelThreeOnePassedKey, success ? 1 : 0));
        }

        if (servicesReady)
        {
            // Use 31 to separate Level 3.1 statistics from regular Level 3 statistics.
            await DatabaseManager.SaveData((CloudSaveKeys.DuplicateClicksKey(31), LevelThreeOneState.DuplicateIngredientClicks));
            await DatabaseManager.SaveData((CloudSaveKeys.GlutenChildAppearedKey(31), LevelThreeOneState.GlutenChildAppeared));
            await DatabaseManager.SaveData((CloudSaveKeys.GlutenChildServedKey(31), LevelThreeOneState.GlutenChildServed));
        }

        // Record this level's attempt for dashboard export before leaving the scene.
        // 31 represents Level 3.1 and prevents overwriting Level 3 attempt data.
        SessionDataCollector.RecordLevelAttempt(31, timeToTargetSeconds);

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
