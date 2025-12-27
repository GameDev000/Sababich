using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndOfLevelUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI coinsText;

    [Header("Message")]
    // [SerializeField] private bool isSuccess = true;
    [TextArea][SerializeField] private string successMessage = "כל הכבוד! עמדת במשימה היעד הבא - יפן!";
    [TextArea][SerializeField] private string failMessage = "לא נורא.. נסה שוב";

    private void Start()
    {
        Scene currentScene = SceneManager.GetActiveScene();

        int coins = (ScoreManager.Instance != null) ? ScoreManager.Instance.GetCurrentMoney() : 0;

        if (titleText != null)
        {
            if (currentScene.name == "Level1 - endScene")
                titleText.text = LevelOneState.IsSuccess ? successMessage : failMessage;
            else if (currentScene.name == "Level2 - endScene")
                titleText.text = LevelTwoState.IsSuccess ? successMessage : failMessage;
            else if (currentScene.name == "Level3 - endScene")
                titleText.text = LevelThreeState.IsSuccess ? successMessage : failMessage;


        }

        if (coinsText != null)
            coinsText.text = $"Coins: {coins}";

        Debug.Log("ScoreManager Start. Money=" + coins);

    }

}
