// using TMPro;
// using UnityEngine;
// using UnityEngine.SceneManagement;

// // Cloud usings (to show coins from cloud after re-login)
// using Unity.Services.Core;
// using Unity.Services.Authentication;
// using System.Threading.Tasks;

// public class EndOfLevelUI : MonoBehaviour
// {
//     [Header("UI")]
//     [SerializeField] private TextMeshProUGUI titleText;
//     [SerializeField] private TextMeshProUGUI coinsText;

//     [Header("Perfect Orders")]
//     [SerializeField] private TextMeshProUGUI perfectOrdersText;

//     [Header("Message")]
//     [SerializeField] private string successMessage = "כל הכבוד! עמדת במשימה היעד הבא - יפן!";
//     [SerializeField] private string failMessage = "לא נורא.. נסה שוב";

//     [Header("End Level Audio")]
//     [SerializeField] private AudioSource endLevelAudioSource;
//     [SerializeField] private AudioClip successClip_level1;
//     [SerializeField] private AudioClip failClip_level1;
//     [SerializeField] private AudioClip successClip_level2;
//     [SerializeField] private AudioClip failClip_level2;
//     [SerializeField] private AudioClip successClip_level3;
//     [SerializeField] private AudioClip failClip_level3;
//     [SerializeField, Range(0f, 1f)] private float endLevelAudioVolume = 1f;



//     // Start is async so we can pull coins from cloud after showing local immediately
//     private async void Start()
//     {
//         Scene currentScene = SceneManager.GetActiveScene();

//         int localCoins = (ScoreManager.Instance != null) ? ScoreManager.Instance.GetCurrentMoney() : 0;

//         // Show something immediately (no waiting)
//         if (coinsText != null)
//             coinsText.text = $"{localCoins}";

//         // Original behavior for success/fail text (local flags)
//         if (titleText != null)
//         {
//             if (currentScene.name == "Level1 - endScene")
//             {
//                 titleText.text = LevelOneState.IsSuccess ? successMessage : failMessage;
//                 if (LevelOneState.IsSuccess)
//                     PlayEndLevelAudio(successClip_level1);
//                 else
//                     PlayEndLevelAudio(failClip_level1);
//             }
//             else if (currentScene.name == "Level2 - endScene")
//             {
//                 titleText.text = LevelTwoState.IsSuccess ? successMessage : failMessage;
//                 if (LevelTwoState.IsSuccess)
//                     PlayEndLevelAudio(successClip_level2);
//                 else
//                     PlayEndLevelAudio(failClip_level2);
//             }
//             else if (currentScene.name == "Level3 - endScene")
//             {
//                 titleText.text = LevelThreeState.IsSuccess ? successMessage : failMessage;
//                 if (LevelThreeState.IsSuccess)
//                     PlayEndLevelAudio(successClip_level3);
//                 else
//                     PlayEndLevelAudio(failClip_level3);
//             }
//         }

//         // Show local perfect orders immediately
//         UpdatePerfectOrdersTextLocal(currentScene.name);

//         Debug.Log("ScoreManager Start. Money=" + localCoins);

//         // If localCoins is 0 (common after re-login), try cloud override.
//         if (localCoins == 0)
//         {
//             await TryOverrideCoinsFromCloud(currentScene.name, localCoins);
//         }

//         // Try override perfect orders from cloud as well
//         await TryOverridePerfectOrdersFromCloud(currentScene.name);
//     }


//     private void PlayEndLevelAudio(AudioClip Clip)
//     {

//         if (endLevelAudioSource == null)
//             return;

//         if (Clip == null)
//             return;

//         endLevelAudioSource.PlayOneShot(Clip, endLevelAudioVolume);
//     }

//     // Local display for perfect orders stats
//     private void UpdatePerfectOrdersTextLocal(string sceneName)
//     {
//         if (perfectOrdersText == null) return;

//         int total = 0;
//         int perfect = 0;

//         if (sceneName == "Level1 - endScene")
//         {
//             total = LevelOneState.TotalServedDishes;
//             perfect = LevelOneState.PerfectServedDishes;
//         }
//         else if (sceneName == "Level2 - endScene")
//         {
//             total = LevelTwoState.TotalServedDishes;
//             perfect = LevelTwoState.PerfectServedDishes;
//         }
//         else if (sceneName == "Level3 - endScene")
//         {
//             total = LevelThreeState.TotalServedDishes;
//             perfect = LevelThreeState.PerfectServedDishes;
//         }

//         perfectOrdersText.text = $"\u200E{perfect} / {total}\u200E";
//     }

//     // Pull coins from cloud for the relevant level end scene and update UI.
//     // If cloud is not ready / key missing -> keep localCoins.
//     private async Task TryOverrideCoinsFromCloud(string sceneName, int localCoins)
//     {
//         int levelNumber = 0;

//         if (sceneName == "Level1 - endScene") levelNumber = 1;
//         else if (sceneName == "Level2 - endScene") levelNumber = 2;
//         else if (sceneName == "Level3 - endScene") levelNumber = 3;
//         else return;

//         if (UnityServices.State != ServicesInitializationState.Initialized)
//         {
//             Debug.Log("[EndOfLevelUI] Cloud not initialized -> keep local coins.");
//             return;
//         }

//         if (!AuthenticationService.Instance.IsSignedIn)
//         {
//             Debug.Log("[EndOfLevelUI] Not signed in -> keep local coins.");
//             return;
//         }

//         string coinsKey = CloudSaveKeys.CoinsKey(levelNumber);
//         var data = await DatabaseManager.LoadData(coinsKey);

//         if (data == null || !data.ContainsKey(coinsKey))
//         {
//             Debug.Log($"[EndOfLevelUI] No cloud key '{coinsKey}' -> keep local coins.");
//             return;
//         }

//         int cloudCoins = DatabaseManager.ReadInt(data, coinsKey, localCoins);

//         if (coinsText != null)
//             coinsText.text = $"{cloudCoins}";

//         Debug.Log($"[EndOfLevelUI] Cloud coins override: {coinsKey}={cloudCoins}");
//     }

//     // Pull perfect orders stats from cloud for the relevant level end scene and update UI.
//     private async Task TryOverridePerfectOrdersFromCloud(string sceneName)
//     {
//         if (perfectOrdersText == null) return;

//         int levelNumber = 0;
//         if (sceneName == "Level1 - endScene") levelNumber = 1;
//         else if (sceneName == "Level2 - endScene") levelNumber = 2;
//         else if (sceneName == "Level3 - endScene") levelNumber = 3;
//         else return;

//         if (UnityServices.State != ServicesInitializationState.Initialized)
//         {
//             Debug.Log("[EndOfLevelUI] Cloud not initialized -> keep local perfect orders.");
//             return;
//         }

//         if (!AuthenticationService.Instance.IsSignedIn)
//         {
//             Debug.Log("[EndOfLevelUI] Not signed in -> keep local perfect orders.");
//             return;
//         }

//         string totalKey = CloudSaveKeys.TotalServedKey(levelNumber);
//         string perfectKey = CloudSaveKeys.PerfectServedKey(levelNumber);

//         // Load
//         var totalData = await DatabaseManager.LoadData(totalKey);
//         var perfectData = await DatabaseManager.LoadData(perfectKey);

//         // If keys missing -> keep local
//         if (totalData == null || !totalData.ContainsKey(totalKey) ||
//             perfectData == null || !perfectData.ContainsKey(perfectKey))
//         {
//             Debug.Log($"[EndOfLevelUI] Missing cloud stats ({totalKey}/{perfectKey}) -> keep local.");
//             return;
//         }

//         int total = DatabaseManager.ReadInt(totalData, totalKey, 0);
//         int perfect = DatabaseManager.ReadInt(perfectData, perfectKey, 0);

//         perfectOrdersText.text = $"\u200E{perfect} / {total}\u200E";
//         Debug.Log($"[EndOfLevelUI] Cloud perfect orders override: perfect={perfect}, total={total}");
//     }
// }



using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// Cloud usings (to show coins from cloud after re-login)
using Unity.Services.Core;
using Unity.Services.Authentication;
using System.Threading.Tasks;

public class EndOfLevelUI : MonoBehaviour
{
    private const string LEVEL_1_END_SCENE = "Level1 - endScene";
    private const string LEVEL_1_1_END_SCENE = "Level1.1 - endScene";
    private const string LEVEL_1_2_END_SCENE = "Level1.2 - endScene";
    private const string LEVEL_2_END_SCENE = "Level2 - endScene";
    private const string LEVEL_3_END_SCENE = "Level3 - endScene";

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI coinsText;

    [Header("Perfect Orders")]
    [SerializeField] private TextMeshProUGUI perfectOrdersText;

    [Header("Success Messages")]
    [SerializeField] private string successMessage_level1 = "כל הכבוד! עמדת במשימה. היעד הבא - ישראל 2!";
    [SerializeField] private string successMessage_level1_1 = "כל הכבוד! עמדת במשימה. היעד הבא - סין 1!";
    [SerializeField] private string successMessage_level1_2 = "כל הכבוד! עמדת במשימה. היעד הבא - סין 2!";
    [SerializeField] private string successMessage_level2 = "כל הכבוד! עמדת במשימה. היעד הבא - ארצות הברית!";
    [SerializeField] private string successMessage_level3 = "כל הכבוד! סיימת את כל השלבים!";

    [Header("Fail Messages")]
    [SerializeField] private string failMessage_level1 = "לא נורא.. נסה שוב";
    [SerializeField] private string failMessage_level1_1 = "לא נורא.. נסה שוב";
    [SerializeField] private string failMessage_level1_2 = "לא נורא.. נסה שוב";
    [SerializeField] private string failMessage_level2 = "לא נורא.. נסה שוב";
    [SerializeField] private string failMessage_level3 = "לא נורא.. נסה שוב";

    [Header("End Level Audio - Level 1")]
    [SerializeField] private AudioClip successClip_level1;
    [SerializeField] private AudioClip failClip_level1;

    [Header("End Level Audio - Level 1.1")]
    [SerializeField] private AudioClip successClip_level1_1;
    [SerializeField] private AudioClip failClip_level1_1;

    [Header("End Level Audio - Level 1.2")]
    [SerializeField] private AudioClip successClip_level1_2;
    [SerializeField] private AudioClip failClip_level1_2;

    [Header("End Level Audio - Level 2")]
    [SerializeField] private AudioClip successClip_level2;
    [SerializeField] private AudioClip failClip_level2;

    [Header("End Level Audio - Level 3")]
    [SerializeField] private AudioClip successClip_level3;
    [SerializeField] private AudioClip failClip_level3;

    [Header("End Level Audio Settings")]
    [SerializeField] private AudioSource endLevelAudioSource;
    [SerializeField, Range(0f, 1f)] private float endLevelAudioVolume = 1f;

    // Start is async so we can pull coins from cloud after showing local data immediately.
    private async void Start()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;

        int localCoins = (ScoreManager.Instance != null)
            ? ScoreManager.Instance.GetCurrentMoney()
            : 0;

        if (coinsText != null)
            coinsText.text = $"{localCoins}";

        UpdateTitleAndAudio(currentSceneName);
        UpdatePerfectOrdersTextLocal(currentSceneName);

        Debug.Log("[EndOfLevelUI] Start. Scene=" + currentSceneName + ", Money=" + localCoins);

        // If localCoins is 0, try cloud override.
        // This is useful after re-login or when ScoreManager was recreated.
        if (localCoins == 0)
        {
            await TryOverrideCoinsFromCloud(currentSceneName, localCoins);
        }

        await TryOverridePerfectOrdersFromCloud(currentSceneName);
    }

    private void UpdateTitleAndAudio(string sceneName)
    {
        if (titleText == null)
            return;

        bool isSuccess = GetIsSuccessByScene(sceneName);

        titleText.text = isSuccess
            ? GetSuccessMessageByScene(sceneName)
            : GetFailMessageByScene(sceneName);

        AudioClip clip = GetEndLevelAudioClip(sceneName, isSuccess);
        PlayEndLevelAudio(clip);
    }

    private bool GetIsSuccessByScene(string sceneName)
    {
        if (sceneName == LEVEL_1_END_SCENE)
            return LevelOneState.IsSuccess;

        if (sceneName == LEVEL_1_1_END_SCENE)
            return LevelOneOneState.IsSuccess;

        if (sceneName == LEVEL_1_2_END_SCENE)
            return LevelOneTwoState.IsSuccess;

        if (sceneName == LEVEL_2_END_SCENE)
            return LevelTwoState.IsSuccess;

        if (sceneName == LEVEL_3_END_SCENE)
            return LevelThreeState.IsSuccess;

        Debug.LogWarning("[EndOfLevelUI] No success state configured for scene: " + sceneName);
        return false;
    }

    private string GetSuccessMessageByScene(string sceneName)
    {
        if (sceneName == LEVEL_1_END_SCENE)
            return successMessage_level1;

        if (sceneName == LEVEL_1_1_END_SCENE)
            return successMessage_level1_1;

        if (sceneName == LEVEL_1_2_END_SCENE)
            return successMessage_level1_2;

        if (sceneName == LEVEL_2_END_SCENE)
            return successMessage_level2;

        if (sceneName == LEVEL_3_END_SCENE)
            return successMessage_level3;

        return "כל הכבוד! עמדת במשימה";
    }

    private string GetFailMessageByScene(string sceneName)
    {
        if (sceneName == LEVEL_1_END_SCENE)
            return failMessage_level1;

        if (sceneName == LEVEL_1_1_END_SCENE)
            return failMessage_level1_1;

        if (sceneName == LEVEL_1_2_END_SCENE)
            return failMessage_level1_2;

        if (sceneName == LEVEL_2_END_SCENE)
            return failMessage_level2;

        if (sceneName == LEVEL_3_END_SCENE)
            return failMessage_level3;

        return "לא נורא.. נסה שוב";
    }

    private AudioClip GetEndLevelAudioClip(string sceneName, bool isSuccess)
    {
        if (sceneName == LEVEL_1_END_SCENE)
            return isSuccess ? successClip_level1 : failClip_level1;

        if (sceneName == LEVEL_1_1_END_SCENE)
            return isSuccess ? successClip_level1_1 : failClip_level1_1;

        if (sceneName == LEVEL_1_2_END_SCENE)
            return isSuccess ? successClip_level1_2 : failClip_level1_2;

        if (sceneName == LEVEL_2_END_SCENE)
            return isSuccess ? successClip_level2 : failClip_level2;

        if (sceneName == LEVEL_3_END_SCENE)
            return isSuccess ? successClip_level3 : failClip_level3;

        Debug.LogWarning("[EndOfLevelUI] No audio clip configured for scene: " + sceneName);
        return null;
    }

    private void PlayEndLevelAudio(AudioClip clip)
    {
        if (endLevelAudioSource == null)
            return;

        if (clip == null)
            return;

        endLevelAudioSource.PlayOneShot(clip, endLevelAudioVolume);
    }

    private void UpdatePerfectOrdersTextLocal(string sceneName)
    {
        if (perfectOrdersText == null)
            return;

        int total = 0;
        int perfect = 0;

        if (sceneName == LEVEL_1_END_SCENE)
        {
            total = LevelOneState.TotalServedDishes;
            perfect = LevelOneState.PerfectServedDishes;
        }
        else if (sceneName == LEVEL_1_1_END_SCENE)
        {
            total = LevelOneOneState.TotalServedDishes;
            perfect = LevelOneOneState.PerfectServedDishes;
        }
        else if (sceneName == LEVEL_1_2_END_SCENE)
        {
            total = LevelOneTwoState.TotalServedDishes;
            perfect = LevelOneTwoState.PerfectServedDishes;
        }
        else if (sceneName == LEVEL_2_END_SCENE)
        {
            total = LevelTwoState.TotalServedDishes;
            perfect = LevelTwoState.PerfectServedDishes;
        }
        else if (sceneName == LEVEL_3_END_SCENE)
        {
            total = LevelThreeState.TotalServedDishes;
            perfect = LevelThreeState.PerfectServedDishes;
        }
        else
        {
            Debug.LogWarning("[EndOfLevelUI] No perfect orders state configured for scene: " + sceneName);
        }

        perfectOrdersText.text = $"\u200E{perfect} / {total}\u200E";
    }

    private async Task TryOverrideCoinsFromCloud(string sceneName, int localCoins)
    {
        int levelNumber = GetCloudLevelNumber(sceneName);

        if (levelNumber == 0)
            return;

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

    private async Task TryOverridePerfectOrdersFromCloud(string sceneName)
    {
        if (perfectOrdersText == null)
            return;

        int levelNumber = GetCloudLevelNumber(sceneName);

        if (levelNumber == 0)
            return;

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

        var totalData = await DatabaseManager.LoadData(totalKey);
        var perfectData = await DatabaseManager.LoadData(perfectKey);

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

    private int GetCloudLevelNumber(string sceneName)
    {
        if (sceneName == LEVEL_1_END_SCENE)
            return 1;

        if (sceneName == LEVEL_1_1_END_SCENE)
            return 11;

        if (sceneName == LEVEL_1_2_END_SCENE)
            return 12;

        if (sceneName == LEVEL_2_END_SCENE)
            return 2;

        if (sceneName == LEVEL_3_END_SCENE)
            return 3;

        Debug.LogWarning("[EndOfLevelUI] No cloud level number configured for scene: " + sceneName);
        return 0;
    }
}