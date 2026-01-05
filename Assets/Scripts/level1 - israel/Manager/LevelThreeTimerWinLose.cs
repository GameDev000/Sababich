using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.Core;
using Unity.Services.Authentication;

public class LevelThreeTimerWinLose : MonoBehaviour
{
    [Header("Timer")]
    [SerializeField] private float levelDurationSeconds = 150f;

    [Header("Timer UI")]
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Win Condition")]
    [SerializeField] private int coinsTarget = 300;

    [Header("End Scene")]
    [SerializeField] private string endSceneName = "Level3 - endScene";

    private float timeLeft;
    private bool finished;

    // Save only once when reaching target
    private bool timeSaved = false;

    // Freeze the exact timeLeft at the first moment target is reached
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

            SaveLevel3TimeOnce();
        }
    }

    private void SaveLevel3TimeOnce()
    {
        if (timeSaved) return;
        timeSaved = true;

        float safeFrozen = (frozenTimeLeft < 0f) ? timeLeft : frozenTimeLeft;
        int timeToTargetSeconds = Mathf.RoundToInt(levelDurationSeconds - safeFrozen);

        if (UnityServices.State == ServicesInitializationState.Initialized &&
            AuthenticationService.Instance.IsSignedIn)
        {
            _ = DatabaseManager.SaveData(("level3_timeSeconds", timeToTargetSeconds));
            Debug.Log($"[Level3] Saved timeSeconds={timeToTargetSeconds}");
        }
        else
        {
            Debug.LogWarning("[Level3] Could not save time (services not ready or not signed in).");
        }
    }

    private void EndLevel()
    {
        int coinsEnd = (ScoreManager.Instance != null) ? ScoreManager.Instance.CurrentMoney : 0;
        Debug.Log($"[Level3] coinsEnd={coinsEnd}");

        // Fallback save
        if (!timeSaved && coinsEnd >= coinsTarget)
        {
            if (frozenTimeLeft < 0f) frozenTimeLeft = timeLeft;
            SaveLevel3TimeOnce();
        }

        // Save coins at end
        if (UnityServices.State == ServicesInitializationState.Initialized &&
            AuthenticationService.Instance.IsSignedIn)
        {
            _ = DatabaseManager.SaveData(("level3_coins", coinsEnd));
        }

        bool success = coinsEnd >= coinsTarget;
        LevelThreeState.IsSuccess = success;

        // Save perfect/total served dishes for Level 3 end screen
        int totalServed = LevelThreeState.TotalServedDishes;
        int perfectServed = LevelThreeState.PerfectServedDishes;

        // Save these stats to cloud
        if (UnityServices.State == ServicesInitializationState.Initialized &&
            AuthenticationService.Instance.IsSignedIn)
        {
            _ = DatabaseManager.SaveData(("level3_totalServed", totalServed));
            _ = DatabaseManager.SaveData(("level3_perfectServed", perfectServed));
        }

        // Save passed flag for level 3 (used after relogin / resume)
        if (UnityServices.State == ServicesInitializationState.Initialized &&
            AuthenticationService.Instance.IsSignedIn)
        {
            _ = DatabaseManager.SaveData(("level3_passed", success ? 1 : 0));
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
