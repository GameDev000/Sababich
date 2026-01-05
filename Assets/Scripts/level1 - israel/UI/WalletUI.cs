using UnityEngine;
using TMPro; 

public class WalletUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI coinsText; 

    private void Start()
    {
        if (EconomyManager.Instance != null)
        {
            UpdateText(EconomyManager.Instance.TotalCoins);

            EconomyManager.Instance.OnCoinsChanged.AddListener(UpdateText);
        }
        else
        {
            Debug.LogWarning("EconomyManager missing! Make sure game starts from MainMenu or generic loader.");
        }
    }

    private void OnDestroy()
    {
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnCoinsChanged.RemoveListener(UpdateText);
        }
    }

    private void UpdateText(int amount)
    {
        if (coinsText != null)
        {
            coinsText.text = amount.ToString(); 
        }
    }
}