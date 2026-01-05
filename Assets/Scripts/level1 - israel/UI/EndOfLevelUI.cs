using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// Cloud usings (to show coins from cloud after re-login)
using Unity.Services.Core;
using Unity.Services.Authentication;
using System.Threading.Tasks;

public class EndOfLevelUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI coinsText;

    [Header("Perfect Orders")]
    [SerializeField] private TextMeshProUGUI perfectOrdersText;

    [Header("Message")]
    [SerializeField] private string successMessage = "כל הכבוד! עמדת במשימה היעד הבא - יפן!";
    [SerializeField] private string failMessage = "לא נורא.. נסה שוב";

    // Start is async so we can pull coins from cloud after showing local immediately
    private async void Start()
    {
        Scene currentScene = SceneManager.GetActiveScene();

        int localCoins = (ScoreManager.Instance != null) ? ScoreManager.Instance.GetCurrentMoney() : 0;

        // Show something immediately (no waiting)
        if (coinsText != null)
            coinsText.text = $"Coins: {localCoins}";

        // Original behavior for success/fail text (local flags)
        if (titleText != null)
        {
            if (currentScene.name == "Level1 - endScene")
            {
                titleText.text = LevelOneState.IsSuccess ? successMessage : failMessage;
            }
            else if (currentScene.name == "Level2 - endScene")
            {
                titleText.text = LevelTwoState.IsSuccess ? successMessage : failMessage;
            }
            else if (currentScene.name == "Level3 - endScene")
            {
                titleText.text = LevelThreeState.IsSuccess ? successMessage : failMessage;
            }
        }

        // Show local perfect orders immediately
        UpdatePerfectOrdersTextLocal(currentScene.name);

        Debug.Log("ScoreManager Start. Money=" + localCoins);

        // If localCoins is 0 (common after re-login), try cloud override.
        if (localCoins == 0)
        {
            await TryOverrideCoinsFromCloud(currentScene.name, localCoins);
        }

        // Try override perfect orders from cloud as well
        await TryOverridePerfectOrdersFromCloud(currentScene.name);
    }

    // Local display for perfect orders stats
    private void UpdatePerfectOrdersTextLocal(string sceneName)
    {
        if (perfectOrdersText == null) return;

        int total = 0;
        int perfect = 0;

        if (sceneName == "Level1 - endScene")
        {
            total = LevelOneState.TotalServedDishes;
            perfect = LevelOneState.PerfectServedDishes;
        }
        else if (sceneName == "Level2 - endScene")
        {
            total = LevelTwoState.TotalServedDishes;
            perfect = LevelTwoState.PerfectServedDishes;
        }
        else if (sceneName == "Level3 - endScene")
        {
            total = LevelThreeState.TotalServedDishes;
            perfect = LevelThreeState.PerfectServedDishes;
        }

        perfectOrdersText.text = $"הוגשו {perfect} מנות מושלמות מתוך {total}";
    }

    // Pull coins from cloud for the relevant level end scene and update UI.
    // If cloud is not ready / key missing -> keep localCoins.
    private async Task TryOverrideCoinsFromCloud(string sceneName, int localCoins)
    {
        int levelNumber = 0;

        if (sceneName == "Level1 - endScene") levelNumber = 1;
        else if (sceneName == "Level2 - endScene") levelNumber = 2;
        else if (sceneName == "Level3 - endScene") levelNumber = 3;
        else return;

        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            Debug.Log("[EndOfLevelUI] Cloud not initialized -> keep local coins.");
            return;
        }

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            Debug.Log("[EndOfLevelUI] Not signed in -> keep local coins.");
            return;
        }

        string coinsKey = $"level{levelNumber}_coins";
        var data = await DatabaseManager.LoadData(coinsKey);

        if (data == null || !data.ContainsKey(coinsKey))
        {
            Debug.Log($"[EndOfLevelUI] No cloud key '{coinsKey}' -> keep local coins.");
            return;
        }

        int cloudCoins = DatabaseManager.ReadInt(data, coinsKey, localCoins);

        if (coinsText != null)
            coinsText.text = $"Coins: {cloudCoins}";

        Debug.Log($"[EndOfLevelUI] Cloud coins override: {coinsKey}={cloudCoins}");
    }

    // Pull perfect orders stats from cloud for the relevant level end scene and update UI.
    private async Task TryOverridePerfectOrdersFromCloud(string sceneName)
    {
        if (perfectOrdersText == null) return;

        int levelNumber = 0;
        if (sceneName == "Level1 - endScene") levelNumber = 1;
        else if (sceneName == "Level2 - endScene") levelNumber = 2;
        else if (sceneName == "Level3 - endScene") levelNumber = 3;
        else return;

        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            Debug.Log("[EndOfLevelUI] Cloud not initialized -> keep local perfect orders.");
            return;
        }

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            Debug.Log("[EndOfLevelUI] Not signed in -> keep local perfect orders.");
            return;
        }

        string totalKey = $"level{levelNumber}_totalServed";
        string perfectKey = $"level{levelNumber}_perfectServed";

        // Load
        var totalData = await DatabaseManager.LoadData(totalKey);
        var perfectData = await DatabaseManager.LoadData(perfectKey);

        // If keys missing -> keep local
        if (totalData == null || !totalData.ContainsKey(totalKey) ||
            perfectData == null || !perfectData.ContainsKey(perfectKey))
        {
            Debug.Log($"[EndOfLevelUI] Missing cloud stats ({totalKey}/{perfectKey}) -> keep local.");
            return;
        }

        int total = DatabaseManager.ReadInt(totalData, totalKey, 0);
        int perfect = DatabaseManager.ReadInt(perfectData, perfectKey, 0);

        perfectOrdersText.text = $"הוגשו {perfect} מנות מושלמות מתוך {total}";
        Debug.Log($"[EndOfLevelUI] Cloud perfect orders override: perfect={perfect}, total={total}");
    }
}
