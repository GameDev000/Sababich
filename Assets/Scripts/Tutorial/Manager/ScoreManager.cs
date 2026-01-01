using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// Manages the player's score (money) and updates the UI accordingly.

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI ScoreText;
    [SerializeField] private int startMoney = 0;

    [Header("Visual FX")]
    [SerializeField] private ParticleSystem coinSparkles;

    [SerializeField] private int target = 150;

    [SerializeField] private SoundCoins soundCoins;
    [SerializeField] private float coinSoundStartTime = 2f;

    public int CurrentMoney { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        CurrentMoney = startMoney;
        UpdateScoreUI();

    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // reconnect UI text in the new scene
        ScoreText = FindScoreTextInScene();

        // reset only on GAMEPLAY level scenes (not end scenes)
        if (IsGameplayLevelScene(scene.name))
            CurrentMoney = startMoney;

        UpdateScoreUI();
    }

    private bool IsGameplayLevelScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
            return false;

        string s = sceneName.Trim().ToLowerInvariant();

        // must start with "level"
        if (!s.StartsWith("level"))
            return false;

        // do NOT reset on end scenes
        if (s.Contains("endscene") || s.Contains("end scene"))
            return false;

        return true;
    }

    private TextMeshProUGUI FindScoreTextInScene()
    {
        var allTexts = FindObjectsOfType<TextMeshProUGUI>(true);

        foreach (var t in allTexts)
        {
            if (t.CompareTag("ScoreText"))
                return t;
        }

        return null;
    }

    public void AddMoney(int amount)
    {
        CurrentMoney += amount;
         if (amount > 0 && soundCoins != null)
        {
            soundCoins.PlayFromSecond(coinSoundStartTime);
        }
        UpdateScoreUI();

        // Notify active level timers so they can freeze timeLeft the first moment target is reached.
        var l1 = FindObjectOfType<LevelTimerWinLose>();
        if (l1 != null) l1.NotifyMoneyChanged(CurrentMoney);

        var l2 = FindObjectOfType<LevelTwoTimerWinLose>();
        if (l2 != null) l2.NotifyMoneyChanged(CurrentMoney);

        var l3 = FindObjectOfType<LevelThreeTimerWinLose>();
        if (l3 != null) l3.NotifyMoneyChanged(CurrentMoney);

        if (coinSparkles != null)
            coinSparkles.Emit(30);
        else
            Debug.LogWarning("coinSparkles is not assigned on ScoreManager!");
    }
    public void SetTarget(int newTarget)
    {
        target = newTarget;
        UpdateScoreUI();
    }   


    private void UpdateScoreUI()
    {
        if (ScoreText != null)
            ScoreText.text = "" + CurrentMoney + "/" + target;
    }

    public int GetCurrentMoney() => CurrentMoney;

    public void ResetMoney()
    {
        CurrentMoney = startMoney;
        UpdateScoreUI();
    }
}
