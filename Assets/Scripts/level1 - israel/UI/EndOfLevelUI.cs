// using TMPro;
// using UnityEngine;
// using UnityEngine.SceneManagement;

// // Cloud usings (to show coins from cloud after re-login)
// using Unity.Services.Core;
// using Unity.Services.Authentication;
// using System.Threading.Tasks;

// public class EndOfLevelUI : MonoBehaviour
// {
//     private const string LEVEL_1_END_SCENE = "Level1 - endScene";
//     private const string LEVEL_1_1_END_SCENE = "Level1.1 - endScene";
//     private const string LEVEL_1_2_END_SCENE = "Level1.2 - endScene";
//     private const string LEVEL_2_END_SCENE = "Level2 - endScene";
//     private const string LEVEL_3_1_END_SCENE = "Level3.1 - endScene";
//     private const string LEVEL_3_END_SCENE = "Level3 - endScene";

//     // Cloud-save keys for Level 3.1.
//     // These must match the keys used by LevelThreeOneTimerWinLose.
//     private const string LEVEL_3_1_COINS_KEY = "Level3_1Coins";
//     private const string LEVEL_3_1_TOTAL_SERVED_KEY = "Level3_1TotalServed";
//     private const string LEVEL_3_1_PERFECT_SERVED_KEY = "Level3_1PerfectServed";

//     [Header("UI")]
//     [SerializeField] private TextMeshProUGUI titleText;
//     [SerializeField] private TextMeshProUGUI coinsText;

//     [Header("Perfect Orders")]
//     [SerializeField] private TextMeshProUGUI perfectOrdersText;

//     [Header("Success Messages")]
//     [SerializeField] private string successMessage_level1 = "כל הכבוד! עמדת במשימה. היעד הבא - ישראל 2!";
//     [SerializeField] private string successMessage_level1_1 = "כל הכבוד! עמדת במשימה. היעד הבא - סין 1!";
//     [SerializeField] private string successMessage_level1_2 = "כל הכבוד! עמדת במשימה. היעד הבא - סין 2!";
//     [SerializeField] private string successMessage_level2 = "כל הכבוד! עמדת במשימה. היעד הבא - ארצות הברית 1!";
//     [SerializeField] private string successMessage_level3_1 = "כל הכבוד! עמדת במשימה. היעד הבא - ארצות הברית 2!";
//     [SerializeField] private string successMessage_level3 = "כל הכבוד! סיימת את כל השלבים!";

//     [Header("Fail Messages")]
//     [SerializeField] private string failMessage_level1 = "לא נורא.. נסה שוב";
//     [SerializeField] private string failMessage_level1_1 = "לא נורא.. נסה שוב";
//     [SerializeField] private string failMessage_level1_2 = "לא נורא.. נסה שוב";
//     [SerializeField] private string failMessage_level2 = "לא נורא.. נסה שוב";
//     [SerializeField] private string failMessage_level3_1 = "לא נורא.. נסה שוב";
//     [SerializeField] private string failMessage_level3 = "לא נורא.. נסה שוב";

//     [Header("End Level Audio - Level 1")]
//     [SerializeField] private AudioClip successClip_level1;
//     [SerializeField] private AudioClip failClip_level1;

//     [Header("End Level Audio - Level 1.1")]
//     [SerializeField] private AudioClip successClip_level1_1;
//     [SerializeField] private AudioClip failClip_level1_1;

//     [Header("End Level Audio - Level 1.2")]
//     [SerializeField] private AudioClip successClip_level1_2;
//     [SerializeField] private AudioClip failClip_level1_2;

//     [Header("End Level Audio - Level 2")]
//     [SerializeField] private AudioClip successClip_level2;
//     [SerializeField] private AudioClip failClip_level2;

//     [Header("End Level Audio - Level 3.1")]
//     [SerializeField] private AudioClip successClip_level3_1;
//     [SerializeField] private AudioClip failClip_level3_1;

//     [Header("End Level Audio - Level 3")]
//     [SerializeField] private AudioClip successClip_level3;
//     [SerializeField] private AudioClip failClip_level3;

//     [Header("End Level Audio Settings")]
//     [SerializeField] private AudioSource endLevelAudioSource;
//     [SerializeField, Range(0f, 1f)] private float endLevelAudioVolume = 1f;

//     // Start is async so we can pull coins from cloud after showing local data immediately.
//     private async void Start()
//     {
//         string currentSceneName = SceneManager.GetActiveScene().name;

//         int localCoins = (ScoreManager.Instance != null)
//             ? ScoreManager.Instance.GetCurrentMoney()
//             : 0;

//         if (coinsText != null)
//             coinsText.text = $"{localCoins}";

//         UpdateTitleAndAudio(currentSceneName);
//         UpdatePerfectOrdersTextLocal(currentSceneName);

//         Debug.Log("[EndOfLevelUI] Start. Scene=" + currentSceneName + ", Money=" + localCoins);

//         // If localCoins is 0, try cloud override.
//         // This is useful after re-login or when ScoreManager was recreated.
//         if (localCoins == 0)
//         {
//             await TryOverrideCoinsFromCloud(currentSceneName, localCoins);
//         }

//         await TryOverridePerfectOrdersFromCloud(currentSceneName);
//     }

//     private void UpdateTitleAndAudio(string sceneName)
//     {
//         if (titleText == null)
//             return;

//         bool isSuccess = GetIsSuccessByScene(sceneName);

//         titleText.text = isSuccess
//             ? GetSuccessMessageByScene(sceneName)
//             : GetFailMessageByScene(sceneName);

//         AudioClip clip = GetEndLevelAudioClip(sceneName, isSuccess);
//         PlayEndLevelAudio(clip);
//     }

//     private bool GetIsSuccessByScene(string sceneName)
//     {
//         if (sceneName == LEVEL_1_END_SCENE)
//             return LevelOneState.IsSuccess;

//         if (sceneName == LEVEL_1_1_END_SCENE)
//             return LevelOneOneState.IsSuccess;

//         if (sceneName == LEVEL_1_2_END_SCENE)
//             return LevelOneTwoState.IsSuccess;

//         if (sceneName == LEVEL_2_END_SCENE)
//             return LevelTwoState.IsSuccess;

//         if (sceneName == LEVEL_3_1_END_SCENE)
//             return LevelThreeOneState.IsSuccess;

//         if (sceneName == LEVEL_3_END_SCENE)
//             return LevelThreeState.IsSuccess;

//         Debug.LogWarning("[EndOfLevelUI] No success state configured for scene: " + sceneName);
//         return false;
//     }

//     private string GetSuccessMessageByScene(string sceneName)
//     {
//         if (sceneName == LEVEL_1_END_SCENE)
//             return successMessage_level1;

//         if (sceneName == LEVEL_1_1_END_SCENE)
//             return successMessage_level1_1;

//         if (sceneName == LEVEL_1_2_END_SCENE)
//             return successMessage_level1_2;

//         if (sceneName == LEVEL_2_END_SCENE)
//             return successMessage_level2;

//         if (sceneName == LEVEL_3_1_END_SCENE)
//             return successMessage_level3_1;

//         if (sceneName == LEVEL_3_END_SCENE)
//             return successMessage_level3;

//         return "כל הכבוד! עמדת במשימה";
//     }

//     private string GetFailMessageByScene(string sceneName)
//     {
//         if (sceneName == LEVEL_1_END_SCENE)
//             return failMessage_level1;

//         if (sceneName == LEVEL_1_1_END_SCENE)
//             return failMessage_level1_1;

//         if (sceneName == LEVEL_1_2_END_SCENE)
//             return failMessage_level1_2;

//         if (sceneName == LEVEL_2_END_SCENE)
//             return failMessage_level2;

//         if (sceneName == LEVEL_3_1_END_SCENE)
//             return failMessage_level3_1;

//         if (sceneName == LEVEL_3_END_SCENE)
//             return failMessage_level3;

//         return "לא נורא.. נסה שוב";
//     }

//     private AudioClip GetEndLevelAudioClip(string sceneName, bool isSuccess)
//     {
//         if (sceneName == LEVEL_1_END_SCENE)
//             return isSuccess ? successClip_level1 : failClip_level1;

//         if (sceneName == LEVEL_1_1_END_SCENE)
//             return isSuccess ? successClip_level1_1 : failClip_level1_1;

//         if (sceneName == LEVEL_1_2_END_SCENE)
//             return isSuccess ? successClip_level1_2 : failClip_level1_2;

//         if (sceneName == LEVEL_2_END_SCENE)
//             return isSuccess ? successClip_level2 : failClip_level2;

//         if (sceneName == LEVEL_3_1_END_SCENE)
//         {
//             // If no specific audio was assigned for Level 3.1, reuse Level 3 audio.
//             AudioClip levelThreeOneClip = isSuccess ? successClip_level3_1 : failClip_level3_1;
//             AudioClip fallbackLevelThreeClip = isSuccess ? successClip_level3 : failClip_level3;

//             return levelThreeOneClip != null ? levelThreeOneClip : fallbackLevelThreeClip;
//         }

//         if (sceneName == LEVEL_3_END_SCENE)
//             return isSuccess ? successClip_level3 : failClip_level3;

//         Debug.LogWarning("[EndOfLevelUI] No audio clip configured for scene: " + sceneName);
//         return null;
//     }

//     private void PlayEndLevelAudio(AudioClip clip)
//     {
//         if (endLevelAudioSource == null)
//             return;

//         if (clip == null)
//             return;

//         endLevelAudioSource.PlayOneShot(clip, endLevelAudioVolume);
//     }

//     private void UpdatePerfectOrdersTextLocal(string sceneName)
//     {
//         if (perfectOrdersText == null)
//             return;

//         int total = 0;
//         int perfect = 0;

//         if (sceneName == LEVEL_1_END_SCENE)
//         {
//             total = LevelOneState.TotalServedDishes;
//             perfect = LevelOneState.PerfectServedDishes;
//         }
//         else if (sceneName == LEVEL_1_1_END_SCENE)
//         {
//             total = LevelOneOneState.TotalServedDishes;
//             perfect = LevelOneOneState.PerfectServedDishes;
//         }
//         else if (sceneName == LEVEL_1_2_END_SCENE)
//         {
//             total = LevelOneTwoState.TotalServedDishes;
//             perfect = LevelOneTwoState.PerfectServedDishes;
//         }
//         else if (sceneName == LEVEL_2_END_SCENE)
//         {
//             total = LevelTwoState.TotalServedDishes;
//             perfect = LevelTwoState.PerfectServedDishes;
//         }
//         else if (sceneName == LEVEL_3_1_END_SCENE)
//         {
//             total = LevelThreeOneState.TotalServedDishes;
//             perfect = LevelThreeOneState.PerfectServedDishes;
//         }
//         else if (sceneName == LEVEL_3_END_SCENE)
//         {
//             total = LevelThreeState.TotalServedDishes;
//             perfect = LevelThreeState.PerfectServedDishes;
//         }
//         else
//         {
//             Debug.LogWarning("[EndOfLevelUI] No perfect orders state configured for scene: " + sceneName);
//         }

//         perfectOrdersText.text = $"\u200E{perfect} / {total}\u200E";
//     }

//     private async Task TryOverrideCoinsFromCloud(string sceneName, int localCoins)
//     {
//         string coinsKey = GetCloudCoinsKey(sceneName);

//         if (string.IsNullOrEmpty(coinsKey))
//             return;

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

//     private async Task TryOverridePerfectOrdersFromCloud(string sceneName)
//     {
//         if (perfectOrdersText == null)
//             return;

//         string totalKey = GetCloudTotalServedKey(sceneName);
//         string perfectKey = GetCloudPerfectServedKey(sceneName);

//         if (string.IsNullOrEmpty(totalKey) || string.IsNullOrEmpty(perfectKey))
//             return;

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

//         var totalData = await DatabaseManager.LoadData(totalKey);
//         var perfectData = await DatabaseManager.LoadData(perfectKey);

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

//     private string GetCloudCoinsKey(string sceneName)
//     {
//         if (sceneName == LEVEL_3_1_END_SCENE)
//             return LEVEL_3_1_COINS_KEY;

//         int levelNumber = GetCloudLevelNumber(sceneName);

//         if (levelNumber == 0)
//             return string.Empty;

//         return CloudSaveKeys.CoinsKey(levelNumber);
//     }

//     private string GetCloudTotalServedKey(string sceneName)
//     {
//         if (sceneName == LEVEL_3_1_END_SCENE)
//             return LEVEL_3_1_TOTAL_SERVED_KEY;

//         int levelNumber = GetCloudLevelNumber(sceneName);

//         if (levelNumber == 0)
//             return string.Empty;

//         return CloudSaveKeys.TotalServedKey(levelNumber);
//     }

//     private string GetCloudPerfectServedKey(string sceneName)
//     {
//         if (sceneName == LEVEL_3_1_END_SCENE)
//             return LEVEL_3_1_PERFECT_SERVED_KEY;

//         int levelNumber = GetCloudLevelNumber(sceneName);

//         if (levelNumber == 0)
//             return string.Empty;

//         return CloudSaveKeys.PerfectServedKey(levelNumber);
//     }

//     private int GetCloudLevelNumber(string sceneName)
//     {
//         if (sceneName == LEVEL_1_END_SCENE)
//             return 1;

//         if (sceneName == LEVEL_1_1_END_SCENE)
//             return 11;

//         if (sceneName == LEVEL_1_2_END_SCENE)
//             return 12;

//         if (sceneName == LEVEL_2_END_SCENE)
//             return 2;

//         if (sceneName == LEVEL_3_1_END_SCENE)
//             return 31;

//         if (sceneName == LEVEL_3_END_SCENE)
//             return 3;

//         Debug.LogWarning("[EndOfLevelUI] No cloud level number configured for scene: " + sceneName);
//         return 0;
//     }
// }
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Authentication;

public class EndOfLevelUI : MonoBehaviour
{
    private const string LEVEL_1_END_SCENE = "Level1 - endScene";
    private const string LEVEL_1_1_END_SCENE = "Level1.1 - endScene";
    private const string LEVEL_1_2_END_SCENE = "Level1.2 - endScene";
    private const string LEVEL_2_END_SCENE = "Level2 - endScene";
    private const string LEVEL_2_1_END_SCENE = "Level2.1 - endScene";
    private const string LEVEL_2_2_END_SCENE = "Level2.2 - endScene";
    private const string LEVEL_3_END_SCENE = "Level3 - endScene";

    private const string LEVEL_2_1_COINS_KEY = "Level2_1Coins";
    private const string LEVEL_2_1_TOTAL_SERVED_KEY = "Level2_1TotalServed";
    private const string LEVEL_2_1_PERFECT_SERVED_KEY = "Level2_1PerfectServed";

    private const string LEVEL_2_2_COINS_KEY = "Level2_2Coins";
    private const string LEVEL_2_2_TOTAL_SERVED_KEY = "Level2_2TotalServed";
    private const string LEVEL_2_2_PERFECT_SERVED_KEY = "Level2_2PerfectServed";

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI coinsText;

    [Header("Perfect Orders")]
    [SerializeField] private TextMeshProUGUI perfectOrdersText;

    [Header("Success Messages")]
    [SerializeField] private string successMessage_level1 = "כל הכבוד! עמדת במשימה. היעד הבא - ישראל 2!";
    [SerializeField] private string successMessage_level1_1 = "כל הכבוד! עמדת במשימה. היעד הבא - סין 1!";
    [SerializeField] private string successMessage_level1_2 = "כל הכבוד! עמדת במשימה. היעד הבא - סין 2!";
    [SerializeField] private string successMessage_level2 = "כל הכבוד! עמדת במשימה. היעד הבא - ארצות הברית 1!";
    [SerializeField] private string successMessage_level2_1 = "כל הכבוד! עמדת במשימה. היעד הבא - ארצות הברית 2!";
    [SerializeField] private string successMessage_level2_2 = "כל הכבוד! עמדת במשימה. היעד הבא - ארצות הברית 3!";
    [SerializeField] private string successMessage_level3 = "כל הכבוד! סיימת את כל השלבים!";

    [Header("Fail Messages")]
    [SerializeField] private string failMessage_level1 = "לא נורא.. נסה שוב";
    [SerializeField] private string failMessage_level1_1 = "לא נורא.. נסה שוב";
    [SerializeField] private string failMessage_level1_2 = "לא נורא.. נסה שוב";
    [SerializeField] private string failMessage_level2 = "לא נורא.. נסה שוב";
    [SerializeField] private string failMessage_level2_1 = "לא נורא.. נסה שוב";
    [SerializeField] private string failMessage_level2_2 = "לא נורא.. נסה שוב";
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

    [Header("End Level Audio - Level 2.1")]
    [SerializeField] private AudioClip successClip_level2_1;
    [SerializeField] private AudioClip failClip_level2_1;

    [Header("End Level Audio - Level 2.2")]
    [SerializeField] private AudioClip successClip_level2_2;
    [SerializeField] private AudioClip failClip_level2_2;

    [Header("End Level Audio - Level 3")]
    [SerializeField] private AudioClip successClip_level3;
    [SerializeField] private AudioClip failClip_level3;

    [Header("End Level Audio Settings")]
    [SerializeField] private AudioSource endLevelAudioSource;
    [SerializeField, Range(0f, 1f)] private float endLevelAudioVolume = 1f;

    private async void Start()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;

        int localCoins = (ScoreManager.Instance != null)
            ? ScoreManager.Instance.GetCurrentMoney()
            : 0;

        if (coinsText != null)
        {
            coinsText.text = $"{localCoins}";
        }

        UpdateTitleAndAudio(currentSceneName);
        UpdatePerfectOrdersTextLocal(currentSceneName);

        Debug.Log("[EndOfLevelUI] Start. Scene=" + currentSceneName + ", Money=" + localCoins);

        if (localCoins == 0)
        {
            await TryOverrideCoinsFromCloud(currentSceneName, localCoins);
        }

        await TryOverridePerfectOrdersFromCloud(currentSceneName);
    }

    private void UpdateTitleAndAudio(string sceneName)
    {
        if (titleText == null)
        {
            return;
        }

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
        {
            return LevelOneState.IsSuccess;
        }

        if (sceneName == LEVEL_1_1_END_SCENE)
        {
            return LevelOneOneState.IsSuccess;
        }

        if (sceneName == LEVEL_1_2_END_SCENE)
        {
            return LevelOneTwoState.IsSuccess;
        }

        if (sceneName == LEVEL_2_END_SCENE)
        {
            return LevelTwoState.IsSuccess;
        }

        if (sceneName == LEVEL_2_1_END_SCENE)
        {
            return LevelTwoOneState.IsSuccess;
        }

        if (sceneName == LEVEL_2_2_END_SCENE)
        {
            return LevelTwoTwoState.IsSuccess;
        }

        if (sceneName == LEVEL_3_END_SCENE)
        {
            return LevelThreeState.IsSuccess;
        }

        Debug.LogWarning("[EndOfLevelUI] No success state configured for scene: " + sceneName);
        return false;
    }

    private string GetSuccessMessageByScene(string sceneName)
    {
        if (sceneName == LEVEL_1_END_SCENE)
        {
            return successMessage_level1;
        }

        if (sceneName == LEVEL_1_1_END_SCENE)
        {
            return successMessage_level1_1;
        }

        if (sceneName == LEVEL_1_2_END_SCENE)
        {
            return successMessage_level1_2;
        }

        if (sceneName == LEVEL_2_END_SCENE)
        {
            return successMessage_level2;
        }

        if (sceneName == LEVEL_2_1_END_SCENE)
        {
            return successMessage_level2_1;
        }

        if (sceneName == LEVEL_2_2_END_SCENE)
        {
            return successMessage_level2_2;
        }

        if (sceneName == LEVEL_3_END_SCENE)
        {
            return successMessage_level3;
        }

        return "כל הכבוד! עמדת במשימה";
    }

    private string GetFailMessageByScene(string sceneName)
    {
        if (sceneName == LEVEL_1_END_SCENE)
        {
            return failMessage_level1;
        }

        if (sceneName == LEVEL_1_1_END_SCENE)
        {
            return failMessage_level1_1;
        }

        if (sceneName == LEVEL_1_2_END_SCENE)
        {
            return failMessage_level1_2;
        }

        if (sceneName == LEVEL_2_END_SCENE)
        {
            return failMessage_level2;
        }

        if (sceneName == LEVEL_2_1_END_SCENE)
        {
            return failMessage_level2_1;
        }

        if (sceneName == LEVEL_2_2_END_SCENE)
        {
            return failMessage_level2_2;
        }

        if (sceneName == LEVEL_3_END_SCENE)
        {
            return failMessage_level3;
        }

        return "לא נורא.. נסה שוב";
    }

    private AudioClip GetEndLevelAudioClip(string sceneName, bool isSuccess)
    {
        if (sceneName == LEVEL_1_END_SCENE)
        {
            return isSuccess ? successClip_level1 : failClip_level1;
        }

        if (sceneName == LEVEL_1_1_END_SCENE)
        {
            return isSuccess ? successClip_level1_1 : failClip_level1_1;
        }

        if (sceneName == LEVEL_1_2_END_SCENE)
        {
            return isSuccess ? successClip_level1_2 : failClip_level1_2;
        }

        if (sceneName == LEVEL_2_END_SCENE)
        {
            return isSuccess ? successClip_level2 : failClip_level2;
        }

        if (sceneName == LEVEL_2_1_END_SCENE)
        {
            AudioClip selectedClip = isSuccess ? successClip_level2_1 : failClip_level2_1;
            AudioClip fallbackClip = isSuccess ? successClip_level2 : failClip_level2;

            return selectedClip != null ? selectedClip : fallbackClip;
        }

        if (sceneName == LEVEL_2_2_END_SCENE)
        {
            AudioClip selectedClip = isSuccess ? successClip_level2_2 : failClip_level2_2;
            AudioClip fallbackClip = isSuccess ? successClip_level2 : failClip_level2;

            return selectedClip != null ? selectedClip : fallbackClip;
        }

        if (sceneName == LEVEL_3_END_SCENE)
        {
            return isSuccess ? successClip_level3 : failClip_level3;
        }

        Debug.LogWarning("[EndOfLevelUI] No audio clip configured for scene: " + sceneName);
        return null;
    }

    private void PlayEndLevelAudio(AudioClip clip)
    {
        if (endLevelAudioSource == null)
        {
            return;
        }

        if (clip == null)
        {
            return;
        }

        endLevelAudioSource.PlayOneShot(clip, endLevelAudioVolume);
    }

    private void UpdatePerfectOrdersTextLocal(string sceneName)
    {
        if (perfectOrdersText == null)
        {
            return;
        }

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
        else if (sceneName == LEVEL_2_1_END_SCENE)
        {
            total = LevelTwoOneState.TotalServedDishes;
            perfect = LevelTwoOneState.PerfectServedDishes;
        }
        else if (sceneName == LEVEL_2_2_END_SCENE)
        {
            total = LevelTwoTwoState.TotalServedDishes;
            perfect = LevelTwoTwoState.PerfectServedDishes;
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
        string coinsKey = GetCloudCoinsKey(sceneName);

        if (string.IsNullOrEmpty(coinsKey))
        {
            return;
        }

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

        var data = await DatabaseManager.LoadData(coinsKey);

        if (data == null || !data.ContainsKey(coinsKey))
        {
            Debug.Log($"[EndOfLevelUI] No cloud key '{coinsKey}' -> keep local coins.");
            return;
        }

        int cloudCoins = DatabaseManager.ReadInt(data, coinsKey, localCoins);

        if (coinsText != null)
        {
            coinsText.text = $"{cloudCoins}";
        }

        Debug.Log($"[EndOfLevelUI] Cloud coins override: {coinsKey}={cloudCoins}");
    }

    private async Task TryOverridePerfectOrdersFromCloud(string sceneName)
    {
        if (perfectOrdersText == null)
        {
            return;
        }

        string totalKey = GetCloudTotalServedKey(sceneName);
        string perfectKey = GetCloudPerfectServedKey(sceneName);

        if (string.IsNullOrEmpty(totalKey) || string.IsNullOrEmpty(perfectKey))
        {
            return;
        }

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

    private string GetCloudCoinsKey(string sceneName)
    {
        if (sceneName == LEVEL_2_1_END_SCENE)
        {
            return LEVEL_2_1_COINS_KEY;
        }

        if (sceneName == LEVEL_2_2_END_SCENE)
        {
            return LEVEL_2_2_COINS_KEY;
        }

        int levelNumber = GetCloudLevelNumber(sceneName);

        if (levelNumber == 0)
        {
            return string.Empty;
        }

        return CloudSaveKeys.CoinsKey(levelNumber);
    }

    private string GetCloudTotalServedKey(string sceneName)
    {
        if (sceneName == LEVEL_2_1_END_SCENE)
        {
            return LEVEL_2_1_TOTAL_SERVED_KEY;
        }

        if (sceneName == LEVEL_2_2_END_SCENE)
        {
            return LEVEL_2_2_TOTAL_SERVED_KEY;
        }

        int levelNumber = GetCloudLevelNumber(sceneName);

        if (levelNumber == 0)
        {
            return string.Empty;
        }

        return CloudSaveKeys.TotalServedKey(levelNumber);
    }

    private string GetCloudPerfectServedKey(string sceneName)
    {
        if (sceneName == LEVEL_2_1_END_SCENE)
        {
            return LEVEL_2_1_PERFECT_SERVED_KEY;
        }

        if (sceneName == LEVEL_2_2_END_SCENE)
        {
            return LEVEL_2_2_PERFECT_SERVED_KEY;
        }

        int levelNumber = GetCloudLevelNumber(sceneName);

        if (levelNumber == 0)
        {
            return string.Empty;
        }

        return CloudSaveKeys.PerfectServedKey(levelNumber);
    }

    private int GetCloudLevelNumber(string sceneName)
    {
        if (sceneName == LEVEL_1_END_SCENE)
        {
            return 1;
        }

        if (sceneName == LEVEL_1_1_END_SCENE)
        {
            return 11;
        }

        if (sceneName == LEVEL_1_2_END_SCENE)
        {
            return 12;
        }

        if (sceneName == LEVEL_2_END_SCENE)
        {
            return 2;
        }

        if (sceneName == LEVEL_2_1_END_SCENE)
        {
            return 21;
        }

        if (sceneName == LEVEL_2_2_END_SCENE)
        {
            return 22;
        }

        if (sceneName == LEVEL_3_END_SCENE)
        {
            return 3;
        }

        Debug.LogWarning("[EndOfLevelUI] No cloud level number configured for scene: " + sceneName);
        return 0;
    }
}
