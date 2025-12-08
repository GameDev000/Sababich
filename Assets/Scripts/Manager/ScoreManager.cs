using TMPro;
using UnityEngine;

/// <summary>
/// Manages the player's score (money) and updates the UI accordingly.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; } // Singleton instance

    [SerializeField] private TextMeshProUGUI ScoreText; // Reference to the UI text component
    [SerializeField] private int startMoney = 0; // Starting money amount

    public int CurrentMoney { get; private set; } // Current money amount

    private void Awake()
    {
        Instance = this; // Set the singleton instance
    }

    private void Start()
    {
        CurrentMoney = startMoney;
        UpdateScoreUI(); // Initialize the score UI
    }

    public void AddMoney(int amount)
    {
        CurrentMoney += amount; // Increase money
        UpdateScoreUI(); // Update the score UI
    }

    /// <summary>
    /// Updates the score UI text to reflect the current money amount.
    /// </summary>
    private void UpdateScoreUI()
    {
        if (ScoreText != null)
            ScoreText.text = "Coins: " + CurrentMoney;
    }
}
