using TMPro;
using UnityEngine;

public class EndOfLevelUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI coinsText;

    [Header("Message")]
   // [SerializeField] private bool isSuccess = true;
    [TextArea] [SerializeField] private string successMessage = "כל הכבוד! עמדת במשימה היעד הבא - יפן!";
    [TextArea] [SerializeField] private string failMessage = "לא נורא.. נסה שוב";

    private void Start()
    {
        int coins = (ScoreManager.Instance != null) ? ScoreManager.Instance.GetCurrentMoney() : 0;

        if (titleText != null)
            titleText.text = LevelOneState.IsSuccess ? successMessage : failMessage;

        if (coinsText != null)
            coinsText.text = $"Coins: {coins}";

        Debug.Log("ScoreManager Start. Money=" + coins);

    }

}
