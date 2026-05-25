using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Authentication;

/// <summary>
/// Manages the timer, win/lose condition, cloud save, and transition to the end scene
/// for Level 2.1 USA.
/// </summary>
public class LevelTwoOneTimerWinLose : MonoBehaviour
{
    [Header("Timer")]
    [SerializeField] private float levelDurationSeconds = 150f;

    [Header("Timer UI")]
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Win Condition")]
    [SerializeField] private int coinsTarget = 300;

    [Header("End Scene")]
    [SerializeField] private string endSceneName = "Level2.1 - endScene";

    private float timeLeft;
    private bool finished;

    private bool timeSaved = false;
    private float frozenTimeLeft = -1f;
    private int timeToTargetSeconds = -1;

    private const string LevelTwoOneTimeSecondsKey = "Level2_1TimeSeconds";
    private const string LevelTwoOneCoinsKey = "Level2_1Coins";
    private const string LevelTwoOneTotalServedKey = "Level2_1TotalServed";
    private const string LevelTwoOnePerfectServedKey = "Level2_1PerfectServed";
    private const string LevelTwoOnePassedKey = "Level2_1Passed";

    private void Start()
    {
        LevelTwoOneState.Reset();

        timeLeft = levelDurationSeconds;
        UpdateTimerUI(timeLeft);
    }

    private void Update()
    {
        if (finished)
        {
            return;
        }

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
    /// </summary>
    public void AddTimeSeconds(float secondsToAdd)
    {
        if (finished)
        {
            return;
        }

        float elapsedTime = Mathf.Max(0f, levelDurationSeconds - timeLeft);

        timeLeft = Mathf.Max(0f, timeLeft + secondsToAdd);

        levelDurationSeconds = elapsedTime + timeLeft;

        UpdateTimerUI(timeLeft);

        Debug.Log($"[LevelTwoOneTimerWinLose] Time changed by {secondsToAdd}. New timeLeft={timeLeft}");
    }

    public float GetTimeLeft()
    {
        return timeLeft;
    }

    /// <summary>
    /// Called from ScoreManager.AddMoney.
    /// </summary>
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
        {
            return;
        }

        if (moneyNow >= coinsTarget)
        {
            if (frozenTimeLeft < 0f)
            {
                frozenTimeLeft = timeLeft;
            }

            SaveLevelTwoOneTimeOnce();
        }
    }

    private void SaveLevelTwoOneTimeOnce()
    {
        if (timeSaved)
        {
            return;
        }

        timeSaved = true;

        float safeFrozen = (frozenTimeLeft < 0f) ? timeLeft : frozenTimeLeft;
        timeToTargetSeconds = Mathf.RoundToInt(levelDurationSeconds - safeFrozen);

        if (UnityServices.State == ServicesInitializationState.Initialized &&
            AuthenticationService.Instance.IsSignedIn)
        {
            _ = DatabaseManager.SaveData((LevelTwoOneTimeSecondsKey, timeToTargetSeconds));
            Debug.Log($"[Level2.1] Saved timeSeconds={timeToTargetSeconds}");
        }
        else
        {
            Debug.LogWarning("[Level2.1] Could not save time. Services are not ready or user is not signed in.");
        }
    }

    private async void EndLevel()
    {
        int coinsEnd = (ScoreManager.Instance != null) ? ScoreManager.Instance.CurrentMoney : 0;

        Debug.Log($"[Level2.1] coinsEnd={coinsEnd}");

        if (!timeSaved && coinsEnd >= coinsTarget)
        {
            if (frozenTimeLeft < 0f)
            {
                frozenTimeLeft = timeLeft;
            }

            SaveLevelTwoOneTimeOnce();
        }

        if (!timeSaved)
        {
            timeToTargetSeconds = Mathf.RoundToInt(levelDurationSeconds);
        }

        bool servicesReady =
            UnityServices.State == ServicesInitializationState.Initialized &&
            AuthenticationService.Instance.IsSignedIn;

        bool success = coinsEnd >= coinsTarget;
        LevelTwoOneState.IsSuccess = success;

        int totalServed = LevelTwoOneState.TotalServedDishes;
        int perfectServed = LevelTwoOneState.PerfectServedDishes;

        if (servicesReady)
        {
            await DatabaseManager.SaveData((LevelTwoOneCoinsKey, coinsEnd));
            await DatabaseManager.SaveData((LevelTwoOneTotalServedKey, totalServed));
            await DatabaseManager.SaveData((LevelTwoOnePerfectServedKey, perfectServed));
            await DatabaseManager.SaveData((LevelTwoOnePassedKey, success ? 1 : 0));

            await DatabaseManager.SaveData((CloudSaveKeys.DuplicateClicksKey(21), LevelTwoOneState.DuplicateIngredientClicks));
            await DatabaseManager.SaveData((CloudSaveKeys.GlutenChildAppearedKey(21), LevelTwoOneState.GlutenChildAppeared));
            await DatabaseManager.SaveData((CloudSaveKeys.GlutenChildServedKey(21), LevelTwoOneState.GlutenChildServed));
        }

        SessionDataCollector.RecordLevelAttempt(21, timeToTargetSeconds);

        Time.timeScale = 1f;
        SceneManager.LoadScene(endSceneName);
    }

    private void UpdateTimerUI(float secondsLeft)
    {
        if (timerText == null)
        {
            return;
        }

        int totalSeconds = Mathf.CeilToInt(secondsLeft);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        timerText.text = $"{minutes:00}:{seconds:00}";
    }
}