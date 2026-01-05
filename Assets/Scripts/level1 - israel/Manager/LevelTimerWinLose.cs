using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.Core;
using Unity.Services.Authentication;

/// <summary>
/// Runs the level countdown timer and ends the level when time is over.
/// Tracks if the player reached the coin target, and saves results (time + coins + passed) to the cloud.
/// </summary>
public class LevelTimerWinLose : MonoBehaviour
{
    [Header("Timer")]
    [SerializeField] private float levelDurationSeconds = 90f;
    [Header("Timer UI")]
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Win Condition")]
    [SerializeField] private int coinsTarget = 150;

    [Header("End Scene")]
    [SerializeField] private string endSceneName = "Level1 - endScene";

    // Current timer state
    private float timeLeft;
    private bool finished;

    // We want to save time to reach target only once
    private bool timeSaved = false;
    private int timeToTargetSeconds = -1;
    // Freeze the exact remaining time
    private float frozenTimeLeft = -1f;

    private void Start()
    {
        timeLeft = levelDurationSeconds;
        UpdateTimerUI(timeLeft);
    }

    private void Update()
    {
        if (finished) return;

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
        if (timeSaved) return;

        if (moneyNow >= coinsTarget)
        {
            if (frozenTimeLeft < 0f)
                frozenTimeLeft = timeLeft;

            SaveLevel1TimeOnce();
        }
    }

    private void SaveLevel1TimeOnce()
    {
        if (timeSaved) return;
        timeSaved = true;

        float safeFrozen = (frozenTimeLeft < 0f) ? timeLeft : frozenTimeLeft;
        timeToTargetSeconds = Mathf.RoundToInt(levelDurationSeconds - safeFrozen);

        if (UnityServices.State == ServicesInitializationState.Initialized &&
            AuthenticationService.Instance.IsSignedIn)
        {
            _ = DatabaseManager.SaveData(("level1_timeSeconds", timeToTargetSeconds)); //1
            Debug.Log($"[Level1] Saved timeSeconds={timeToTargetSeconds}");
        }
        else
        {
            Debug.LogWarning("[Level1] Could not save time (services not ready or not signed in).");
        }
    }

    private void EndLevel()
    {
        int coinsEnd = (ScoreManager.Instance != null) ? ScoreManager.Instance.CurrentMoney : 0;

        if (!timeSaved && coinsEnd >= coinsTarget)
        {
            if (frozenTimeLeft < 0f) frozenTimeLeft = timeLeft;
            SaveLevel1TimeOnce();
        }

        SaveLevel1CoinsToCloud(coinsEnd);

        bool success = coinsEnd >= coinsTarget;
        LevelOneState.IsSuccess = success;

        // Read served dishes statistics for Level 1
        int totalServed = LevelOneState.TotalServedDishes;
        int perfectServed = LevelOneState.PerfectServedDishes;

        // Save served dishes statistics
        if (UnityServices.State == ServicesInitializationState.Initialized &&
            AuthenticationService.Instance.IsSignedIn)
        {
            _ = DatabaseManager.SaveData(("level1_totalServed", totalServed));
            _ = DatabaseManager.SaveData(("level1_perfectServed", perfectServed));
        }

        if (UnityServices.State == ServicesInitializationState.Initialized &&
            AuthenticationService.Instance.IsSignedIn)
        {
            _ = DatabaseManager.SaveData(("level1_passed", success ? 1 : 0)); // 2
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(endSceneName);
    }

    private void SaveLevel1CoinsToCloud(int coins)
    {
        if (UnityServices.State != ServicesInitializationState.Initialized) return;
        if (!AuthenticationService.Instance.IsSignedIn) return;

        _ = DatabaseManager.SaveData(("level1_coins", coins));// 3
    }

    private void UpdateTimerUI(float secondsLeft)
    {
        if (timerText == null) return;

        int totalSeconds = Mathf.CeilToInt(secondsLeft);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        timerText.text = $"{minutes:00}:{seconds:00}";
    }
}
