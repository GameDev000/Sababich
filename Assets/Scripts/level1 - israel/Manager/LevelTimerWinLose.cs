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
        // Initialize timer at the start of the level
        timeLeft = levelDurationSeconds;
        UpdateTimerUI(timeLeft);
    }

    private void Update()
    {
        if (finished) return;

        // Countdown
        timeLeft -= Time.deltaTime;

        // Fallback check: in case money changes without calling NotifyMoneyChanged
        TryFreezeTimeWhenReachedTarget();

        // If time is over -> end the level
        if (timeLeft <= 0f)
        {
            timeLeft = 0f;
            UpdateTimerUI(timeLeft);

            finished = true;
            EndLevel();
            return;
        }

        // Update UI every frame
        UpdateTimerUI(timeLeft);
    }

    /// <summary>
    /// Called directly from ScoreManager.AddMoney (best/most accurate moment).
    /// This lets us freeze and save the exact time when the player reaches the target.
    /// </summary>
    public void NotifyMoneyChanged(int newMoney)
    {
        FreezeTimeIfNeeded(newMoney);
    }

    /// <summary>
    /// Extra safety: if money was changed somewhere else (not via AddMoney),
    /// we still detect reaching the target.
    /// </summary>
    private void TryFreezeTimeWhenReachedTarget()
    {
        // Always read money from the singleton ScoreManager
        int moneyNow = (ScoreManager.Instance != null) ? ScoreManager.Instance.CurrentMoney : 0;
        FreezeTimeIfNeeded(moneyNow);
    }

    /// <summary>
    /// If player reached coin target: freeze time once and save once.
    /// </summary>
    private void FreezeTimeIfNeeded(int moneyNow)
    {
        if (timeSaved) return;

        if (moneyNow >= coinsTarget)
        {
            // Freeze remaining time at the first moment we cross the target
            if (frozenTimeLeft < 0f)
                frozenTimeLeft = timeLeft;

            // Save "time to target" (only once)
            SaveLevel1TimeOnce();
        }
    }

    /// Saves the time it took to reach the target (level1_timeSeconds) exactly once.
    private void SaveLevel1TimeOnce()
    {
        if (timeSaved) return;
        timeSaved = true;

        // Convert "remaining time" into "time spent to reach target"
        // timeToTarget = total duration - frozen remaining time
        float safeFrozen = (frozenTimeLeft < 0f) ? timeLeft : frozenTimeLeft;
        timeToTargetSeconds = Mathf.RoundToInt(levelDurationSeconds - safeFrozen);

        // Save to cloud only if services are ready AND user is signed in
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

    /// <summary>
    /// Called once when time is over.
    /// Calculates success, saves coins/results to cloud, and loads the end scene.
    /// </summary>
    private void EndLevel()
    {
        // Read final coins from ScoreManager singleton
        int coinsEnd = (ScoreManager.Instance != null) ? ScoreManager.Instance.CurrentMoney : 0;

        // If player reached target but time wasn't saved yet, save it now (still correct)
        if (!timeSaved && coinsEnd >= coinsTarget)
        {
            if (frozenTimeLeft < 0f) frozenTimeLeft = timeLeft;
            SaveLevel1TimeOnce();
        }

        // Save final coins to cloud
        SaveLevel1CoinsToCloud(coinsEnd);

        // Determine win/lose and store it globally
        bool success = coinsEnd >= coinsTarget;
        LevelOneState.IsSuccess = success;

        // Save "passed" flag to cloud (used for resume/relogin)
        if (UnityServices.State == ServicesInitializationState.Initialized &&
            AuthenticationService.Instance.IsSignedIn)
        {
            _ = DatabaseManager.SaveData(("level1_passed", success ? 1 : 0)); // 2
        }

        // Make sure timeScale is normal and move to end scene
        Time.timeScale = 1f;
        SceneManager.LoadScene(endSceneName);
    }

    // Saves level1_coins at the end of the level (if cloud is ready).
    private void SaveLevel1CoinsToCloud(int coins)
    {
        if (UnityServices.State != ServicesInitializationState.Initialized) return;
        if (!AuthenticationService.Instance.IsSignedIn) return;

        _ = DatabaseManager.SaveData(("level1_coins", coins));// 3
    }

    // Updates timer UI as MM:SS.
    private void UpdateTimerUI(float secondsLeft)
    {
        if (timerText == null) return;

        int totalSeconds = Mathf.CeilToInt(secondsLeft);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        timerText.text = $"{minutes:00}:{seconds:00}";
    }
}
