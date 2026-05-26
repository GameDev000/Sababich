using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Authentication;

/// <summary>
/// Manages the timer, win/lose condition, cloud save, and transition to the end scene
/// for Level 2.2 USA.
/// </summary>
public class LevelTwoTwoTimerWinLose : MonoBehaviour
{
    [Header("Timer")]
    [SerializeField] private float levelDurationSeconds = 150f;

    [Header("Timer UI")]
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Win Condition")]
    [SerializeField] private int coinsTarget = 300;

    [Header("End Scene")]
    [SerializeField] private string endSceneName = "Level2.2 - endScene";

    private float timeLeft;
    private bool finished;

    private bool timeSaved = false;
    private float frozenTimeLeft = -1f;
    private int timeToTargetSeconds = -1;

    private float originalDurationSeconds;

    private const string LevelTwoTwoTimeSecondsKey = "Level2_2TimeSeconds";
    private const string LevelTwoTwoCoinsKey = "Level2_2Coins";
    private const string LevelTwoTwoTotalServedKey = "Level2_2TotalServed";
    private const string LevelTwoTwoPerfectServedKey = "Level2_2PerfectServed";
    private const string LevelTwoTwoPassedKey = "Level2_2Passed";

    private void Start()
    {
        LevelTwoTwoState.Reset();

        originalDurationSeconds = levelDurationSeconds;
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

        Debug.Log($"[LevelTwoTwoTimerWinLose] Time changed by {secondsToAdd}. New timeLeft={timeLeft}");
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

            SaveLevelTwoTwoTimeOnce();
        }
    }

    private void SaveLevelTwoTwoTimeOnce()
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
            _ = DatabaseManager.SaveData((LevelTwoTwoTimeSecondsKey, timeToTargetSeconds));
            Debug.Log($"[Level2.2] Saved timeSeconds={timeToTargetSeconds}");
        }
        else
        {
            Debug.LogWarning("[Level2.2] Could not save time. Services are not ready or user is not signed in.");
        }
    }

    private async void EndLevel()
    {
        int coinsEnd = (ScoreManager.Instance != null) ? ScoreManager.Instance.CurrentMoney : 0;

        Debug.Log($"[Level2.2] coinsEnd={coinsEnd}");

        if (!timeSaved && coinsEnd >= coinsTarget)
        {
            if (frozenTimeLeft < 0f)
            {
                frozenTimeLeft = timeLeft;
            }

            SaveLevelTwoTwoTimeOnce();
        }

        if (!timeSaved)
        {
            timeToTargetSeconds = Mathf.RoundToInt(levelDurationSeconds);
        }

        bool servicesReady =
            UnityServices.State == ServicesInitializationState.Initialized &&
            AuthenticationService.Instance.IsSignedIn;

        bool success = coinsEnd >= coinsTarget;
        LevelTwoTwoState.IsSuccess = success;

        int totalServed = LevelTwoTwoState.TotalServedDishes;
        int perfectServed = LevelTwoTwoState.PerfectServedDishes;

        if (servicesReady)
        {
            await DatabaseManager.SaveData((LevelTwoTwoCoinsKey, coinsEnd));
            await DatabaseManager.SaveData((LevelTwoTwoTotalServedKey, totalServed));
            await DatabaseManager.SaveData((LevelTwoTwoPerfectServedKey, perfectServed));
            await DatabaseManager.SaveData((LevelTwoTwoPassedKey, success ? 1 : 0));

            await DatabaseManager.SaveData((CloudSaveKeys.DuplicateClicksKey(22), LevelTwoTwoState.DuplicateIngredientClicks));
            await DatabaseManager.SaveData((CloudSaveKeys.GlutenChildAppearedKey(22), LevelTwoTwoState.GlutenChildAppeared));
            await DatabaseManager.SaveData((CloudSaveKeys.GlutenChildServedKey(22), LevelTwoTwoState.GlutenChildServed));
            await DatabaseManager.SaveData((CloudSaveKeys.CustomersArrivedKey(22), LevelTwoTwoState.CustomersArrived));
        }

        SessionDataCollector.RecordLevelAttempt(22, timeToTargetSeconds, Mathf.RoundToInt(levelDurationSeconds), Mathf.RoundToInt(originalDurationSeconds));

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
