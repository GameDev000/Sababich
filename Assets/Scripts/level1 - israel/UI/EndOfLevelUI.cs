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

    [Header("End Level Audio")]
    [SerializeField] private AudioSource endLevelAudioSource;
    [SerializeField] private AudioClip successClip_level1;
    [SerializeField] private AudioClip failClip_level1;
    [SerializeField] private AudioClip successClip_level2;
    [SerializeField] private AudioClip failClip_level2;
    [SerializeField] private AudioClip successClip_level3;
    [SerializeField] private AudioClip failClip_level3;
    [SerializeField, Range(0f, 1f)] private float endLevelAudioVolume = 1f;



    // Start is async so we can pull coins from cloud after showing local immediately
    private async void Start()
    {
        Scene currentScene = SceneManager.GetActiveScene();

        int localCoins = (ScoreManager.Instance != null) ? ScoreManager.Instance.GetCurrentMoney() : 0;

        // Show something immediately (no waiting)
        if (coinsText != null)
            coinsText.text = $"{localCoins}";

        // Original behavior for success/fail text (local flags)
        if (titleText != null)
        {
            if (currentScene.name == "Level1 - endScene")
            {
                titleText.text = LevelOneState.IsSuccess ? successMessage : failMessage;
                if (LevelOneState.IsSuccess)
                    PlayEndLevelAudio(successClip_level1);
                else
                    PlayEndLevelAudio(failClip_level1);
            }
            else if (currentScene.name == "Level2 - endScene")
            {
                titleText.text = LevelTwoState.IsSuccess ? successMessage : failMessage;
                if (LevelTwoState.IsSuccess)
                    PlayEndLevelAudio(successClip_level2);
                else
                    PlayEndLevelAudio(failClip_level2);
            }
            else if (currentScene.name == "Level3 - endScene")
            {
                titleText.text = LevelThreeState.IsSuccess ? successMessage : failMessage;
                if (LevelThreeState.IsSuccess)
                    PlayEndLevelAudio(successClip_level3);
                else
                    PlayEndLevelAudio(failClip_level3);
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


    private void PlayEndLevelAudio(AudioClip Clip)
    {

        if (endLevelAudioSource == null)
            return;

        if (Clip == null)
            return;

        endLevelAudioSource.PlayOneShot(Clip, endLevelAudioVolume);
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

        perfectOrdersText.text = $"\u200E{perfect} / {total}\u200E";
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

        string coinsKey = CloudSaveKeys.CoinsKey(levelNumber);
        var data = await DatabaseManager.LoadData(coinsKey);

        if (data == null || !data.ContainsKey(coinsKey))
        {
            Debug.Log($"[EndOfLevelUI] No cloud key '{coinsKey}' -> keep local coins.");
            return;
        }

        int cloudCoins = DatabaseManager.ReadInt(data, coinsKey, localCoins);

        if (coinsText != null)
            coinsText.text = $"{cloudCoins}";

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

        string totalKey = CloudSaveKeys.TotalServedKey(levelNumber);
        string perfectKey = CloudSaveKeys.PerfectServedKey(levelNumber);

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

        perfectOrdersText.text = $"\u200E{perfect} / {total}\u200E";
        Debug.Log($"[EndOfLevelUI] Cloud perfect orders override: perfect={perfect}, total={total}");
    }
}
