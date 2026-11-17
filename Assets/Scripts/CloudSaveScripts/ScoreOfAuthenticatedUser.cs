using TMPro;
using Unity.Services.Authentication;
using UnityEngine;
using System.Threading.Tasks;
using Unity.Services.CloudSave.Models;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using System.Security.Cryptography;
using System.Text;
using System;
using Unity.Services.CloudCode;
using System.Collections.Generic;



public class ScoreOfAuthenticatedUser : MonoBehaviour
{
    /* ================= UI REFERENCES ================= */

    [SerializeField] private TMP_InputField usernameInputField;
    [SerializeField] private TMP_InputField passwordInputField;

    // Status messages: only "Registering..." / "Logging in..." / "Signing in as guest..."
    [SerializeField] private TextMeshProUGUI statusField;

    [SerializeField] private GameObject signInPanel;
    [SerializeField] private GameObject gamePanel;

    // Kept for scene safety (not used)
    [SerializeField] private Button continueButton;
    [SerializeField] private TextMeshProUGUI continueButtonText;

    /* ================= AUTHENTICATION ================= */

    [SerializeField] private AuthenticationManagerWithPassword authManager;

    /* ================= SABABICH DATA ================= */

    private string resumeScene = "";

    private const string FIXED_PASSWORD = "SababichFixedPassword2025!";

    [SerializeField] private string mainMenuSceneName = "MainMenu";
    private const string LOGIN_SCENE_NAME = "Login";

    [SerializeField] private CloudProgressTracker progressTracker;
    [SerializeField] private CloudStateSync cloudStateSync;

    [SerializeField] private string introVideoSceneName = "IntroVideoScene";

    /* ================== Hebrew display name + internal username ================== */

    private string displayName = "";
    private string internalUsername = "";

    // Guest flag (guest does NOT save to cloud)
    private bool isGuest = false;

    void Awake()
    {
        Debug.Log("ScoreOfAuthenticatedUser Awake");

        // Ensure tracker doesn't try to save before sign-in (extra safety)
        if (progressTracker != null)
            progressTracker.enabled = false;
    }

    void Start()
    {
        if (signInPanel != null) signInPanel.SetActive(true);

        // gamePanel/continue are no longer used, keep hidden
        if (gamePanel != null) gamePanel.SetActive(false);

        if (usernameInputField != null) usernameInputField.readOnly = false;
        if (passwordInputField != null) passwordInputField.readOnly = false;

        if (statusField != null)
        {
            statusField.text = "";
            statusField.isRightToLeftText = true;
            statusField.alignment = TMPro.TextAlignmentOptions.Right;
        }

        if (continueButton != null) continueButton.interactable = false;
        if (continueButtonText != null) continueButtonText.text = "";
    }

    // Hash any displayName (Hebrew/English) to an ASCII-only key
    private string HashToHex(string input)
    {
        using (var sha = SHA256.Create())
        {
            byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }
    }

    // Deterministic internal username from displayName (Hebrew/English OK)
    private string InternalFromDisplayName(string disp)
    {
        if (string.IsNullOrWhiteSpace(disp)) return "";

        string normalized = disp.Trim().Normalize(NormalizationForm.FormC).ToLowerInvariant();
        string hex = HashToHex(normalized);

        return "user_" + hex.Substring(0, 10);
    }

    // Cloud key used to map displayName -> internalUsername
    private string NameMapKey(string disp)
    {
        string safe = HashToHex(disp.Trim().Normalize(NormalizationForm.FormC));
        return $"user_of_{safe}";
    }

    private bool AllowCloudSave()
    {
        if (isGuest) return false;
        if (!AuthenticationService.Instance.IsSignedIn) return false;
        return true;
    }

    // Runs after successful auth for register/login
    // private async Task PostSignInCommon()
    // {
    //     // Save username right after sign-in (registered/logged-in users only)
    //     Debug.Log($"[PostSignIn] displayName='{displayName}' internalUsername='{internalUsername}' isGuest={isGuest} IsSignedIn={AuthenticationService.Instance.IsSignedIn}");

    //     if (!string.IsNullOrEmpty(displayName) && !string.IsNullOrEmpty(internalUsername))
    //     {
    //         if (AllowCloudSave())
    //         {
    //             try
    //             {
    //                 await DatabaseManager.SaveData(
    //                     (CloudSaveKeys.DisplayName, displayName),
    //                     (NameMapKey(displayName), internalUsername),
    //                     (CloudSaveKeys.Username, internalUsername)
    //                 );
    //                 Debug.Log($"[PostSignIn] Username saved to cloud: displayName='{displayName}'");
    //             }
    //             catch (System.Exception e)
    //             {
    //                 Debug.LogError($"[PostSignIn] Failed to save username to cloud: {e.Message}");
    //             }
    //         }
    //         else
    //         {
    //             Debug.LogWarning($"[PostSignIn] Cloud save skipped — AllowCloudSave()=false (isGuest={isGuest}, IsSignedIn={AuthenticationService.Instance.IsSignedIn})");
    //         }
    //     }
    //     else
    //     {
    //         Debug.LogWarning($"[PostSignIn] Cloud save skipped — displayName or internalUsername is empty");
    //     }

    //     // Enable tracker after sign-in (so resumeScene can be saved automatically)
    //     if (progressTracker == null)
    //         progressTracker = FindObjectOfType<CloudProgressTracker>();

    //     if (progressTracker != null)
    //     {
    //         if (!isGuest)
    //             progressTracker.EnableAfterSignIn();
    //         else
    //             progressTracker.enabled = false;
    //     }
    //     else
    //     {
    //         Debug.LogWarning("ScoreOfAuthenticatedUser: CloudProgressTracker not found.");
    //     }
    // }


    // private async Task PostSignInCommon()
    // {
    //     // Save player data after successful registration/login
    //     Debug.Log(
    //         $"[PostSignIn] displayName='{displayName}' " +
    //         $"internalUsername='{internalUsername}' " +
    //         $"isGuest={isGuest} " +
    //         $"IsSignedIn={AuthenticationService.Instance.IsSignedIn}"
    //     );

    //     if (!string.IsNullOrWhiteSpace(displayName) &&
    //         !string.IsNullOrWhiteSpace(internalUsername))
    //     {
    //         if (AllowCloudSave())
    //         {
    //             try
    //             {
    //                 // Step 1: Validate displayName on the server
    //                 // and save it to Protected Player Data
    //                 var result = await CloudCodeService.Instance
    //                     .CallEndpointAsync<SetDisplayNameResult>(
    //                         "SetDisplayName",
    //                         new Dictionary<string, object>
    //                         {
    //                             { "displayName", displayName }
    //                         }
    //                     );

    //                 if (result == null ||
    //                     !result.success ||
    //                     string.IsNullOrWhiteSpace(result.displayName))
    //                 {
    //                     throw new System.Exception(
    //                         "Server rejected displayName"
    //                     );
    //                 }

    //                 // Step 2: Use the validated server response
    //                 displayName = result.displayName;

    //                 Debug.Log(
    //                     $"[PostSignIn] Protected displayName saved: '{displayName}'"
    //                 );

    //                 // Step 3: Save other player data as before
    //                 // displayName is NOT saved to Default Cloud Save
    //                 await DatabaseManager.SaveData(
    //                     (NameMapKey(displayName), internalUsername),
    //                     (CloudSaveKeys.Username, internalUsername)
    //                 );

    //                 Debug.Log(
    //                     "[PostSignIn] Player mapping data saved successfully"
    //                 );
    //             }
    //             catch (System.Exception e)
    //             {
    //                 Debug.LogError(
    //                     $"[PostSignIn] Cloud Code validation/save failed: {e.Message}"
    //                 );

    //                 // Do not enable cloud progress tracking
    //                 // when server-side validation fails
    //                 return;
    //             }
    //         }
    //         else
    //         {
    //             Debug.LogWarning(
    //                 $"[PostSignIn] Cloud save skipped — " +
    //                 $"AllowCloudSave()=false " +
    //                 $"(isGuest={isGuest}, " +
    //                 $"IsSignedIn={AuthenticationService.Instance.IsSignedIn})"
    //             );
    //         }
    //     }
    //     else
    //     {
    //         Debug.LogWarning(
    //             "[PostSignIn] Cloud save skipped — " +
    //             "displayName or internalUsername is empty"
    //         );
    //     }

    //     // Enable tracker after successful sign-in
    //     if (progressTracker == null)
    //         progressTracker = FindObjectOfType<CloudProgressTracker>();

    //     if (progressTracker != null)
    //     {
    //         if (!isGuest)
    //             progressTracker.EnableAfterSignIn();
    //         else
    //             progressTracker.enabled = false;
    //     }
    //     else
    //     {
    //         Debug.LogWarning(
    //             "ScoreOfAuthenticatedUser: CloudProgressTracker not found."
    //         );
    //     }
    // }


    private async Task<bool> PostSignInCommon()
    {
        Debug.Log(
            $"[PostSignIn] displayName='{displayName}' " +
            $"internalUsername='{internalUsername}' " +
            $"isGuest={isGuest} " +
            $"IsSignedIn={AuthenticationService.Instance.IsSignedIn}"
        );

        // Step 1: Validate local input before contacting the server
        if (string.IsNullOrWhiteSpace(displayName) ||
            string.IsNullOrWhiteSpace(internalUsername))
        {
            Debug.LogWarning(
                "[PostSignIn] Invalid displayName or internalUsername"
            );

            return false;
        }

        // Step 2: Handle authenticated players
        if (!isGuest)
        {
            if (!AllowCloudSave())
            {
                Debug.LogError(
                    "[PostSignIn] Cloud services unavailable or player not signed in"
                );

                return false;
            }

            try
            {
                // Step 3: Server-side validation and Protected Data save
                var result = await CloudCodeService.Instance
                    .CallEndpointAsync<SetDisplayNameResult>(
                        "SetDisplayName",
                        new Dictionary<string, object>
                        {
                            { "displayName", displayName }
                           //{ "displayName", "   " }
                        }
                    );

                // Step 4: Check the server response
                if (result == null ||
                    !result.success ||
                    string.IsNullOrWhiteSpace(result.displayName))
                {
                    throw new System.Exception(
                        "Server rejected displayName"
                    );
                }

                // Step 5: Use the server-validated display name
                displayName = result.displayName;

                Debug.Log(
                    $"[PostSignIn] Protected displayName saved: '{displayName}'"
                );

                // Step 6: Save remaining player mapping data
                // displayName is NOT saved to Default Cloud Save
                await DatabaseManager.SaveData(
                    (NameMapKey(displayName), internalUsername),
                    (CloudSaveKeys.Username, internalUsername)
                );

                Debug.Log(
                    "[PostSignIn] Player mapping data saved successfully"
                );
            }
            catch (System.Exception e)
            {
                Debug.LogError(
                    $"[PostSignIn] Server validation/save failed: {e.Message}"
                );

                // Stop the flow when server-side validation fails
                return false;
            }
        }
        else
        {
            // Guests do not save player data to Cloud Save
            Debug.Log(
                "[PostSignIn] Guest login - Cloud Save skipped"
            );
        }

        // Step 7: Enable progress tracking only after success
        if (progressTracker == null)
        {
            progressTracker = FindObjectOfType<CloudProgressTracker>();
        }

        if (progressTracker != null)
        {
            if (!isGuest)
            {
                progressTracker.EnableAfterSignIn();
            }
            else
            {
                progressTracker.enabled = false;
            }
        }
        else
        {
            Debug.LogWarning(
                "[PostSignIn] CloudProgressTracker not found"
            );
        }

        // Step 8: Return success
        return true;
    }



    /* ===================================================
     * START GAME IMMEDIATELY AFTER AUTH (NO SUCCESS/FAIL UI)
     * =================================================== */

    private async Task StartAfterRegisteredLoginAsync(ButtonType type)
    {
        if (usernameInputField != null) usernameInputField.readOnly = true;
        if (passwordInputField != null) passwordInputField.readOnly = true;

        if (signInPanel != null) signInPanel.SetActive(false);
        if (gamePanel != null) gamePanel.SetActive(false);

        // Register -> Intro video
        if (type == ButtonType.REGISTER)
        {
            SceneManager.LoadScene(introVideoSceneName);
            return;
        }

        // Login -> Sync cloud state + resumeScene
        if (cloudStateSync == null)
            cloudStateSync = FindObjectOfType<CloudStateSync>();

        if (cloudStateSync != null)
            await cloudStateSync.SyncStatesFromCloud();
        else
            Debug.LogWarning("[ScoreOfAuthenticatedUser] CloudStateSync not found -> keeping local Level states.");

        var data = await DatabaseManager.LoadData(CloudSaveKeys.ResumeScene);
        resumeScene = DatabaseManager.ReadString(data, CloudSaveKeys.ResumeScene, "");

        GoToResumeOrMainMenu();
    }

    private void StartAfterGuestLogin()
    {
        if (signInPanel != null) signInPanel.SetActive(false);
        if (gamePanel != null) gamePanel.SetActive(false);

        // Guest always starts fresh (no cloud)
        SceneManager.LoadScene(mainMenuSceneName);
    }

    /* ===================================================
     * AUTH BUTTON HANDLING
     * =================================================== */

    enum ButtonType { REGISTER, LOGIN }

    async Task OnButtonClicked(ButtonType type)
    {
        isGuest = false;

        displayName = usernameInputField != null ? usernameInputField.text.Trim() : "";
        string password = FIXED_PASSWORD;

        if (string.IsNullOrEmpty(displayName))
        {
            if (statusField != null) statusField.text = "אנא הכנס שם";
            return;
        }

        internalUsername = InternalFromDisplayName(displayName);

        string message;

        if (type == ButtonType.REGISTER)
        {
            if (statusField != null) statusField.text = "נרשם...";
            message = await authManager.RegisterWithUsernameAndPassword(internalUsername, password);
        }
        else
        {
            if (statusField != null) statusField.text = "מתחבר...";
            message = await authManager.LoginWithUsernameAndPassword(internalUsername, password);
        }

        if (message != null && message.ToLower().Contains("success"))
        {
            if (AuthenticationService.Instance.IsSignedIn)
                await HandlePostAuth(type);
            else
                AuthenticationService.Instance.SignedIn += async () => await HandlePostAuth(type);
        }
        else
        {
            if (statusField != null) statusField.text = "שגיאה — נסה שוב";
        }
    }

    public async void OnSignUpButtonClicked()
    {
        await OnButtonClicked(ButtonType.REGISTER);
    }

    public async void OnSignInButtonClicked()
    {
        await OnButtonClicked(ButtonType.LOGIN);
    }

    public async void OnGuestButtonClicked()
    {
        if (statusField != null) statusField.text = "כניסה כאורח...";

        isGuest = true;

        displayName = "Guest";
        internalUsername = "guest_" + Guid.NewGuid().ToString("N").Substring(0, 8);

        // Populate SessionIdentity for guest session — cloud write will be skipped,
        // but local dashboard export still uses this identity.
        SessionIdentity.SetFromLogin("Guest", internalUsername, isGuest: true);

        string message = await authManager.LoginAnonymously();

        // Do NOT show message (no PlayerID / no extra text)

        if (message != null && message.ToLower().Contains("success"))
        {
            if (AuthenticationService.Instance.IsSignedIn)
            {
                await PostSignInCommon(); // will not save because isGuest == true
                StartAfterGuestLogin();
            }
            else
            {
                AuthenticationService.Instance.SignedIn += async () =>
                {
                    await PostSignInCommon();
                    StartAfterGuestLogin();
                };
            }
        }
    }

    /* ===================================================
     * CLOUD SAVE API (called by tracker / levels)
     * =================================================== */

    public async void SaveResumeScene(string sceneName)
    {
        if (!enabled) return;

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            Debug.LogWarning("SaveResumeScene: not signed in -> ignoring.");
            return;
        }

        if (isGuest) return;

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("SaveResumeScene: sceneName is empty - ignoring.");
            return;
        }

        if (sceneName == LOGIN_SCENE_NAME)
        {
            Debug.Log("SaveResumeScene: ignoring Login scene.");
            return;
        }

        resumeScene = sceneName;

        await DatabaseManager.SaveData((CloudSaveKeys.ResumeScene, resumeScene));
    }

    public async void ClearResumeToMainMenu()
    {
        if (!enabled) return;

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            Debug.LogWarning("ClearResumeToMainMenu: not signed in -> ignoring.");
            return;
        }

        if (isGuest) return;

        resumeScene = mainMenuSceneName;

        await DatabaseManager.SaveData((CloudSaveKeys.ResumeScene, resumeScene));
    }

    private void GoToResumeOrMainMenu()
    {
        string target = string.IsNullOrEmpty(resumeScene) ? mainMenuSceneName : resumeScene;

        if (!IsSceneInBuildSettings(target))
        {
            Debug.LogWarning($"Saved scene '{target}' is not in Build Settings. Falling back to '{mainMenuSceneName}'.");
            target = mainMenuSceneName;
        }

        SceneManager.LoadScene(target);
    }

    private bool IsSceneInBuildSettings(string sceneName)
    {
        int count = SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < count; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            if (name == sceneName) return true;
        }
        return false;
    }

    // private async Task HandlePostAuth(ButtonType type)
    // {
    //     // Populate SessionIdentity so dashboard backend scripts know who is logged in
    //     // without needing to access private fields of this class.
    //     SessionIdentity.SetFromLogin(displayName, internalUsername, isGuest: false);

    //     await PostSignInCommon();
    //     enabled = true;

    //     await StartAfterRegisteredLoginAsync(type);
    // }

    private async Task HandlePostAuth(ButtonType type)
    {
        // Validate and save player data on the server
        bool validationSucceeded = await PostSignInCommon();

        // Stop login flow if server validation failed
        if (!validationSucceeded)
        {
            Debug.LogError(
                "[HandlePostAuth] Server validation failed. Login flow stopped."
            );

            if (statusField != null)
            {
                statusField.text = "אימות פרטי המשתמש נכשל. נסה שוב.";
            }

            return;
        }

        // Only validated users continue to the game
        SessionIdentity.SetFromLogin(
            displayName,
            internalUsername,
            isGuest: false
        );

        enabled = true;

        await StartAfterRegisteredLoginAsync(type);
    }


    [Serializable]
    private class SetDisplayNameResult
    {
        public bool success;
        public string displayName;
    }
}
