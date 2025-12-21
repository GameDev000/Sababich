using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelTwoTimerWinLose : MonoBehaviour
{
    [Header("Timer")]
    [SerializeField] private float levelDurationSeconds = 90f;

    [Header("Timer UI")]
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Win Condition")]
    [SerializeField] private int coinsTarget = 150;

    [Header("End Scene")]
    [SerializeField] private string endSceneName = "Level2 - endScene";


    [Header("Coins Source")]
    [SerializeField] private ScoreManager playerCoins;

    private float timeLeft;
    private bool finished;

    private void Start()
    {
        timeLeft = levelDurationSeconds;

        if (playerCoins == null)
            playerCoins = ScoreManager.Instance != null
                ? ScoreManager.Instance
                : FindObjectOfType<ScoreManager>();

        UpdateTimerUI(timeLeft);
    }

    private void Update()
    {
        if (finished) return;

        timeLeft -= Time.deltaTime;

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

    private void UpdateTimerUI(float secondsLeft)
    {
        if (timerText == null) return;

        int totalSeconds = Mathf.CeilToInt(secondsLeft);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    private void EndLevel()
    {
        int coins = (playerCoins != null) ? playerCoins.GetCurrentMoney() : 0;

        LevelTwoState.IsSuccess = coins >= coinsTarget;
        Time.timeScale = 1f;
        SceneManager.LoadScene(endSceneName);
    }
}
