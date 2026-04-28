using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
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

            SaveLevel2TimeOnce();
        }
    }

    private void SaveLevel2TimeOnce()
    {
        if (timeSaved) return;
        timeSaved = true;

        float safeFrozen = (frozenTimeLeft < 0f) ? timeLeft : frozenTimeLeft;
        int timeToTargetSeconds = Mathf.RoundToInt(levelDurationSeconds - safeFrozen);

        if (UnityServices.State == ServicesInitializationState.Initialized &&
            AuthenticationService.Instance.IsSignedIn)
        {
            _ = DatabaseManager.SaveData((CloudSaveKeys.Level2TimeSeconds, timeToTargetSeconds));
            Debug.Log($"[Level2] Saved timeSeconds={timeToTargetSeconds}");
        }
        else
        {
            Debug.LogWarning("[Level2] Could not save time (services not ready or not signed in).");
        }
    }

    private async void EndLevel()
    {
        int coinsEnd = (ScoreManager.Instance != null) ? ScoreManager.Instance.CurrentMoney : 0;

        if (!timeSaved && coinsEnd >= coinsTarget)
        {
            if (frozenTimeLeft < 0f) frozenTimeLeft = timeLeft;
            SaveLevel2TimeOnce();
        }

        if (UnityServices.State == ServicesInitializationState.Initialized &&
            AuthenticationService.Instance.IsSignedIn)
        {
            await DatabaseManager.SaveData((CloudSaveKeys.Level2Coins, coinsEnd));
        }

        bool success = coinsEnd >= coinsTarget;
        LevelTwoState.IsSuccess = success;

        // Read served dishes statistics for Level 2
        int totalServed = LevelTwoState.TotalServedDishes;
        int perfectServed = LevelTwoState.PerfectServedDishes;

        // Save served dishes statistics
        if (UnityServices.State == ServicesInitializationState.Initialized &&
            AuthenticationService.Instance.IsSignedIn)
        {
            await DatabaseManager.SaveData((CloudSaveKeys.Level2TotalServed, totalServed));
            await DatabaseManager.SaveData((CloudSaveKeys.Level2PerfectServed, perfectServed));
        }

        // Save passed flag for level 2
        if (UnityServices.State == ServicesInitializationState.Initialized &&
            AuthenticationService.Instance.IsSignedIn)
        {
            await DatabaseManager.SaveData((CloudSaveKeys.Level2Passed, success ? 1 : 0));
        }

        if (UnityServices.State == ServicesInitializationState.Initialized &&
            AuthenticationService.Instance.IsSignedIn)
        {
            await DatabaseManager.SaveData((CloudSaveKeys.DuplicateClicksKey(2),     LevelTwoState.DuplicateIngredientClicks));
            await DatabaseManager.SaveData((CloudSaveKeys.GlutenChildAppearedKey(2), LevelTwoState.GlutenChildAppeared));
            await DatabaseManager.SaveData((CloudSaveKeys.GlutenChildServedKey(2),   LevelTwoState.GlutenChildServed));
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
