using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.Core;
using Unity.Services.Authentication;

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

    // CHANGE: Removed serialized coin source reference - always use ScoreManager.Instance as single source of truth
    // [Header("Coins Source")]
    // [SerializeField] private ScoreManager playerCoins;

    private float timeLeft;
    private bool finished;

    // Save only once when player reaches the coins target
    private bool timeSaved = false;
    private int timeToTargetSeconds = -1;

    // Freeze the exact timeLeft at the first moment target is reached
    private float frozenTimeLeft = -1f;

    private void Start()
    {
        timeLeft = levelDurationSeconds;

        // CHANGE: Do NOT cache coins manager from FindObjectOfType; duplicates can exist across scenes.
        // We rely only on ScoreManager.Instance.
        UpdateTimerUI(timeLeft);
    }

    private void Update()
    {
        if (finished) return;

        timeLeft -= Time.deltaTime;

        // Keep as fallback (in case money changes without AddMoney)
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

    // Called from ScoreManager.AddMoney (most accurate moment)
    public void NotifyMoneyChanged(int newMoney)
    {
        FreezeTimeIfNeeded(newMoney);
    }

    // In case money is modified elsewhere
    private void TryFreezeTimeWhenReachedTarget()
    {
        // CHANGE: Always read money from ScoreManager.Instance
        int moneyNow = (ScoreManager.Instance != null) ? ScoreManager.Instance.CurrentMoney : 0;
        FreezeTimeIfNeeded(moneyNow);
    }

    // Freeze once, compute once, save once
    private void FreezeTimeIfNeeded(int moneyNow)
    {
        if (timeSaved) return;

        if (moneyNow >= coinsTarget)
        {
            // Freeze the remaining time exactly at the first moment we cross the target
            if (frozenTimeLeft < 0f)
                frozenTimeLeft = timeLeft;

            SaveLevel1TimeOnce();
        }
    }

    // Saves level1_timeSeconds exactly once
    private void SaveLevel1TimeOnce()
    {
        if (timeSaved) return;
        timeSaved = true;

        // timeToTarget = duration - frozenTimeLeft (stable even if saved later)
        float safeFrozen = (frozenTimeLeft < 0f) ? timeLeft : frozenTimeLeft;
        timeToTargetSeconds = Mathf.RoundToInt(levelDurationSeconds - safeFrozen);

        if (UnityServices.State == ServicesInitializationState.Initialized &&
            AuthenticationService.Instance.IsSignedIn)
        {
            _ = DatabaseManager.SaveData(("level1_timeSeconds", timeToTargetSeconds));
            Debug.Log($"[Level1] Saved timeSeconds={timeToTargetSeconds}");
        }
        else
        {
            Debug.LogWarning("[Level1] Could not save time (services not ready or not signed in).");
        }
    }

    private void EndLevel()
    {
        // CHANGE: Always read coins from ScoreManager.Instance
        int coinsEnd = (ScoreManager.Instance != null) ? ScoreManager.Instance.CurrentMoney : 0;

        // If reached target but time not saved yet, freeze+save now (still stable)
        if (!timeSaved && coinsEnd >= coinsTarget)
        {
            if (frozenTimeLeft < 0f) frozenTimeLeft = timeLeft;
            SaveLevel1TimeOnce();
        }

        // Save coins at end of level
        SaveLevel1CoinsToCloud(coinsEnd);

        bool success = coinsEnd >= coinsTarget;
        LevelOneState.IsSuccess = success;
        // Save passed flag for level 1 (used after relogin / resume)
        if (UnityServices.State == ServicesInitializationState.Initialized &&
            AuthenticationService.Instance.IsSignedIn)
        {
            _ = DatabaseManager.SaveData(("level1_passed", success ? 1 : 0));
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(endSceneName);
    }

    // Saves level1_coins at the end of the level
    private void SaveLevel1CoinsToCloud(int coins)
    {
        if (UnityServices.State != ServicesInitializationState.Initialized) return;
        if (!AuthenticationService.Instance.IsSignedIn) return;

        _ = DatabaseManager.SaveData(("level1_coins", coins));
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
