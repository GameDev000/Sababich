using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI ScoreText;
    [SerializeField] private int startMoney = 0;

    public int CurrentMoney { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        CurrentMoney = startMoney;
        UpdateScoreUI();
    }

    public void AddMoney(int amount)
    {
        CurrentMoney += amount;
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (ScoreText != null)
            ScoreText.text = "Coins: " + CurrentMoney;
    }
}
