using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.Core;
using Unity.Services.Authentication;

public class LevelTwoTimerWinLose : MonoBehaviour
{
    [Header("Timer")]
    [SerializeField] private float levelDurationSeconds = 90f;

    [Header("Timer UI")]
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Win Condition")]
    [SerializeField] private int coinsTarget = 10;

    [Header("End Scene")]
    [SerializeField] private string endSceneName = "Level2 - endScene";

    // [Header("Coins Source")]
    // [SerializeField] private ScoreManager playerCoins;
    
    private float timeLeft;
    private bool finished;

    // Save only once when reaching target
    private bool timeSaved = false;

    // Freeze the exact timeLeft at the first moment target is reached
    private float frozenTimeLeft = -1f;

    private void Start()
    {
        timeLeft = levelDurationSeconds;

        // Do NOT cache coins manager from FindObjectOfType; duplicates can exist across scenes.
        // We rely only on ScoreManager.Instance.
        UpdateTimerUI(timeLeft);
    }

    private void Update()
    {
        if (finished) return;

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

    // Called from ScoreManager.AddMoney
    public void NotifyMoneyChanged(int newMoney)
    {
        FreezeTimeIfNeeded(newMoney);
    }

    private void TryFreezeTimeWhenReachedTarget()
    {
        // Always read money from ScoreManager.Instance
        int moneyNow = (ScoreManager.Instance != null) ? ScoreManager.Instance.CurrentMoney : 0;
        FreezeTimeIfNeeded(moneyNow);
    }

    // Freeze once, compute once, save once
    private void FreezeTimeIfNeeded(int moneyNow)
    {
        if (timeSaved) return;

        if (moneyNow >= coinsTarget)
        {
            if (frozenTimeLeft < 0f)
                frozenTimeLeft = timeLeft;

            SaveLevel2TimeOnce();
        }
    }

    // Saves level2_timeSeconds once
    private void SaveLevel2TimeOnce()
    {
        if (timeSaved) return;
        timeSaved = true;

        // Stable time even if save happens later
        float safeFrozen = (frozenTimeLeft < 0f) ? timeLeft : frozenTimeLeft;
        int timeToTargetSeconds = Mathf.RoundToInt(levelDurationSeconds - safeFrozen);

        if (UnityServices.State == ServicesInitializationState.Initialized &&
            AuthenticationService.Instance.IsSignedIn)
        {
            _ = DatabaseManager.SaveData(("level2_timeSeconds", timeToTargetSeconds));
            Debug.Log($"[Level2] Saved timeSeconds={timeToTargetSeconds}");
        }
        else
        {
            Debug.LogWarning("[Level2] Could not save time (services not ready or not signed in).");
        }
    }

    private void EndLevel()
    {
        // Always read coins from ScoreManager.Instance
        int coinsEnd = (ScoreManager.Instance != null) ? ScoreManager.Instance.CurrentMoney : 0;

        // Fallback save
        if (!timeSaved && coinsEnd >= coinsTarget)
        {
            if (frozenTimeLeft < 0f) frozenTimeLeft = timeLeft;
            SaveLevel2TimeOnce();
        }

        // Save coins at end
        if (UnityServices.State == ServicesInitializationState.Initialized &&
            AuthenticationService.Instance.IsSignedIn)
        {
            _ = DatabaseManager.SaveData(("level2_coins", coinsEnd));
        }

        bool success = coinsEnd >= coinsTarget;
        LevelTwoState.IsSuccess = success;

        // Save passed flag for level 2 (used after relogin / resume)
        if (UnityServices.State == ServicesInitializationState.Initialized &&
            AuthenticationService.Instance.IsSignedIn)
        {
            _ = DatabaseManager.SaveData(("level2_passed", success ? 1 : 0));
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(endSceneName);
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
