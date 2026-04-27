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

        if (statusField != null) statusField.text = "";

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
    private async Task PostSignInCommon()
    {
        // Save mapping right after sign-in (registered/logged-in users only)
        if (!string.IsNullOrEmpty(displayName) && !string.IsNullOrEmpty(internalUsername))
        {
            if (AllowCloudSave())
            {
                await DatabaseManager.SaveData((CloudSaveKeys.DisplayName, displayName));
                await DatabaseManager.SaveData((NameMapKey(displayName), internalUsername));
                await DatabaseManager.SaveData((CloudSaveKeys.Username, internalUsername));
            }
        }

        // Enable tracker after sign-in (so resumeScene can be saved automatically)
        if (progressTracker == null)
            progressTracker = FindObjectOfType<CloudProgressTracker>();

        if (progressTracker != null)
        {
            if (!isGuest)
                progressTracker.EnableAfterSignIn();
            else
                progressTracker.enabled = false;
        }
        else
        {
            Debug.LogWarning("ScoreOfAuthenticatedUser: CloudProgressTracker not found.");
        }
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
            if (statusField != null) statusField.text = "Please enter username";
            return;
        }

        internalUsername = InternalFromDisplayName(displayName);

        string message;

        if (type == ButtonType.REGISTER)
        {
            if (statusField != null) statusField.text = "Registering...";
            message = await authManager.RegisterWithUsernameAndPassword(internalUsername, password);
        }
        else
        {
            if (statusField != null) statusField.text = "Logging in...";
            message = await authManager.LoginWithUsernameAndPassword(internalUsername, password);
        }

        // IMPORTANT: Do NOT show message (no success/fail feedback here).
        // We keep showing only "Registering..." / "Logging in..." until scene changes.

        if (message != null && message.ToLower().Contains("success"))
        {
            if (AuthenticationService.Instance.IsSignedIn)
                await HandlePostAuth(type);
            else
                AuthenticationService.Instance.SignedIn += async () => await HandlePostAuth(type);
        }
        else
        {
            // Optional: clear or keep the "Logging in..." text.
            // If you want absolutely no extra feedback, keep it as is.
            // If you prefer clearing it:
            // if (statusField != null) statusField.text = "";
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
        if (statusField != null) statusField.text = "Signing in as guest...";

        isGuest = true;

        displayName = "Guest";
        internalUsername = "guest_" + Guid.NewGuid().ToString("N").Substring(0, 8);

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

    private async Task HandlePostAuth(ButtonType type)
    {
        await PostSignInCommon();
        enabled = true;

        await StartAfterRegisteredLoginAsync(type);
    }
}
