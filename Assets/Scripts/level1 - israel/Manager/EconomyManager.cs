using UnityEngine;
using UnityEngine.Events;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance { get; private set; }

    public int TotalCoins { get; private set; }

    public UnityEvent<int> OnCoinsChanged;

    private const string COINS_SAVE_KEY = "PlayerTotalCoins";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadMoney();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddMoney(int amount)
    {
        TotalCoins += amount;
        SaveMoney();

        OnCoinsChanged?.Invoke(TotalCoins);
        Debug.Log("Money Added! New Total: " + TotalCoins);
    }

    private void SaveMoney()
    {
        PlayerPrefs.SetInt(COINS_SAVE_KEY, TotalCoins);
        PlayerPrefs.Save();
    }

    private void LoadMoney()
    {
        TotalCoins = PlayerPrefs.GetInt(COINS_SAVE_KEY, 0);
    }
}