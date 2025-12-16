
// /// <summary>
// /// Manages the player's score (money) and updates the UI accordingly.
// /// </summary>
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI ScoreText;
    [SerializeField] private int startMoney = 0;

    [Header("Visual FX")]
    [SerializeField] private ParticleSystem coinSparkles;

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
    }

    // private void Start()
    // {
    //    // CurrentMoney = startMoney;
    //     UpdateScoreUI();
    // }

    public void AddMoney(int amount)
    {
        CurrentMoney += amount;
        UpdateScoreUI();

        if (coinSparkles != null)
        {
            coinSparkles.Emit(30);
        }
        else
        {
            Debug.LogWarning("coinSparkles is not assigned on ScoreManager!");
        }
    }

    private void UpdateScoreUI()
    {
        if (ScoreText != null)
            ScoreText.text = "Coins: " + CurrentMoney;
    }

    public int GetCurrentMoney()
    {
        return CurrentMoney;
    }

    public void ResetMoney()
    {
        CurrentMoney = startMoney;
        UpdateScoreUI();
    }
}
