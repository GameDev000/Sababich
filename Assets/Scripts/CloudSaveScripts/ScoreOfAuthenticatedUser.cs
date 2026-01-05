using TMPro;
using Unity.Services.Authentication;
using UnityEngine;
using System.Threading.Tasks;
using Unity.Services.CloudSave.Models;   // Item + GetAs extensions
using UnityEngine.SceneManagement;       // SceneManager (resume to saved scene)
using UnityEngine.UI;                    // for Button

// For hashing displayName into ASCII-only key
using System.Security.Cryptography;
using System.Text;
// CHANGE: for Normalize(FormC)
using System;

/*
 * This script is the BRIDGE between:
 * 1) Authentication (username + password)
 * 2) Cloud Save (what is saved/loaded)
 * 3) The game logic (Sababich)
 *
 * Based directly on the lecturer's example: ScoreOfAuthenticatedUser
 *
 * === Sababich adaptation ===
 * - We do NOT save "score". We save ONLY "resumeScene" (where the player stopped),
 *   so after login we can continue from the same place:
 *   Tutorial / Level1 / Level2 / Level3 / their EndScenes (lose screens).
 * - If resumeScene is empty OR the player already finished everything -> go to MainMenu.
 *
 * === Hebrew + English username support ===
 * - The user may type a Display Name in Hebrew/English.
 * - Unity Authentication "username" must be ASCII, so we create/use an internal username.
 * - We store a mapping in Cloud Save: "user_of_<displayName>" -> "<internalUsername>"
 */
public class ScoreOfAuthenticatedUser : MonoBehaviour
{
    /* ================= UI REFERENCES ================= */

    // Username input (shown to the player) - can be Hebrew/English
    [SerializeField] private TMP_InputField usernameInputField;

    // In this assignment we authenticate with USERNAME ONLY, so we do NOT read from this field.
    // We keep the field for UI consistency with the lecturer example.
    [SerializeField] private TMP_InputField passwordInputField;

    // Status messages: "Registering...", "Login successful", errors, etc.
    [SerializeField] private TextMeshProUGUI statusField;

    // Panels
    // signInPanel: the login/registration screen
    // gamePanel: a "welcome back / resuming..." screen (after successful login)
    [SerializeField] private GameObject signInPanel;
    [SerializeField] private GameObject gamePanel;

    // Debug text to show loaded data / "resuming..." message
    [SerializeField] private TextMeshProUGUI textField;

    // Continue button + its TMP text (to prevent "flash" of wrong text before async load finishes)
    // CHANGE: We do not use Continue anymore, but we keep refs so scene won't break if they still exist
    [SerializeField] private Button continueButton;
    [SerializeField] private TextMeshProUGUI continueButtonText;

    /* ================= AUTHENTICATION ================= */

    // It handles UnityServices.InitializeAsync + login/register
    [SerializeField] private AuthenticationManagerWithPassword authManager;

    /* ================= SABABICH DATA ================= */

    // Sababich data to save in the cloud
    // In this stage we save ONLY the player's progress (where to resume).
    // Key in CloudSave: "resumeScene"
    private string resumeScene = "";

    // Assignment requirement: username-only login.
    // We keep a fixed password inside the code (same for all users).
    private const string FIXED_PASSWORD = "SababichFixedPassword2025!";

    // If no resumeScene exists (new user / finished game / cleared progress),
    // we send the player to MainMenu.
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    // Safety: do not allow saving "Login" as resume
    private const string LOGIN_SCENE_NAME = "Login";

    [SerializeField] private CloudProgressTracker progressTracker;

    // Optional reference Inspector to sync Level states from cloud after login
    [SerializeField] private CloudStateSync cloudStateSync;

    // Name of Intro Video scene
    [SerializeField] private string introVideoSceneName = "IntroVideoScene";

    // ================== Hebrew display name + internal username ==================

    // Display name typed by the player (can be Hebrew/English). Used for UI + Cloud Save.
    private string displayName = "";

    // Internal ASCII username used for Unity Authentication.
    private string internalUsername = "";

    // If true, the player JUST registered now -> was used for button text
    private bool justRegistered = false;

    // CHANGE: Guest flag (we do NOT save any guest data to cloud)
    private bool isGuest = false;

    void Awake()
    {
        Debug.Log("ScoreOfAuthenticatedUser Awake");
        // Authentication initialization happens inside AuthenticationManagerWithPassword.Awake()

        // Ensure tracker doesn't try to save before sign-in (extra safety)
        if (progressTracker != null)
            progressTracker.enabled = false;
    }

    void Start()
    {
        // On Play (before any login/register), we MUST show ONLY the sign-in panel.
        if (signInPanel != null) signInPanel.SetActive(true);

        // CHANGE: gamePanel/continue are no longer used, keep hidden
        if (gamePanel != null) gamePanel.SetActive(false);

        // Make sure inputs are editable at the start
        if (usernameInputField != null) usernameInputField.readOnly = false;
        if (passwordInputField != null) passwordInputField.readOnly = false;

        // clear previous status text at start
        if (statusField != null) statusField.text = "";

        // CHANGE: We do not use Continue anymore
        if (continueButton != null) continueButton.interactable = false;
        if (continueButtonText != null) continueButtonText.text = "";
    }

    // Hash any displayName (Hebrew/English) to an ASCII-only key
    private string HashToHex(string input)
    {
        using (var sha = SHA256.Create())
        {
            byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return System.BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }
    }

    // CHANGE: Deterministic internal username from displayName (Hebrew/English OK)
    // This is the key fix for: "login again by the same name should work".
    private string InternalFromDisplayName(string disp)
    {
        if (string.IsNullOrWhiteSpace(disp)) return "";

        // Normalize so unicode variations won't create different users
        // Also trim, and make English case-insensitive
        string normalized = disp.Trim().Normalize(NormalizationForm.FormC).ToLowerInvariant();

        string hex = HashToHex(normalized);

        // Unity Auth username must be ASCII
        return "user_" + hex.Substring(0, 10);
    }

    // Cloud key used to map displayName -> internalUsername
    private string NameMapKey(string disp)
    {
        // Cloud Save keys cannot be Hebrew. So we store mapping under an ASCII-only hashed key.
        string safe = HashToHex(disp.Trim().Normalize(NormalizationForm.FormC));
        return $"user_of_{safe}";
    }

    // CHANGE: Only registered/logged-in users are allowed to save to cloud. Guest must never save.
    private bool AllowCloudSave()
    {
        if (isGuest) return false;
        if (!AuthenticationService.Instance.IsSignedIn) return false;
        return true;
    }

    // This runs after successful auth for register and login
    private async Task PostSignInCommon()
    {
        // Save displayName + mapping immediately after sign-in
        if (!string.IsNullOrEmpty(displayName) && !string.IsNullOrEmpty(internalUsername))
        {
            if (AllowCloudSave())
            {
                await DatabaseManager.SaveData(("displayName", displayName));
                await DatabaseManager.SaveData((NameMapKey(displayName), internalUsername)); // mapping (SAFE KEY)
                await DatabaseManager.SaveData(("username", internalUsername));
            }
            // Guest: no cloud save
        }

        // Enable tracker immediately after sign-in
        if (progressTracker == null)
            progressTracker = FindObjectOfType<CloudProgressTracker>();

        if (progressTracker != null)
        {
            // Guest: keep tracker disabled to prevent any cloud writes
            if (!isGuest)
                progressTracker.EnableAfterSignIn();
            else
                progressTracker.enabled = false;
        }
        else
        {
            Debug.LogWarning("ScoreOfAuthenticatedUser: CloudProgressTracker not found. Scene progress won't be auto-saved.");
        }
    }

    /* ===================================================
     * START GAME IMMEDIATELY AFTER AUTH (NO CONTINUE)
     * =================================================== */

    // CHANGE: This replaces the old "welcome/continue" flow.
    // After login/register we immediately decide where to go and load a scene.
    private async Task StartAfterRegisteredLoginAsync(ButtonType type)
    {
        // Lock UI edits
        if (usernameInputField != null) usernameInputField.readOnly = true;
        if (passwordInputField != null) passwordInputField.readOnly = true;

        // (Optional) hide sign-in UI to avoid flicker
        if (signInPanel != null) signInPanel.SetActive(false);
        if (gamePanel != null) gamePanel.SetActive(false);

        // Register → play intro video
        if (type == ButtonType.REGISTER)
        {
            SceneManager.LoadScene(introVideoSceneName);
            return;
        }

        // Login → load cloud state + resumeScene, then go
        if (cloudStateSync == null)
            cloudStateSync = FindObjectOfType<CloudStateSync>();

        if (cloudStateSync != null)
        {
            await cloudStateSync.SyncStatesFromCloud();
        }
        else
        {
            Debug.LogWarning("[ScoreOfAuthenticatedUser] CloudStateSync not found -> keeping local Level states.");
        }

        // Load resumeScene from cloud
        var data = await DatabaseManager.LoadData("resumeScene");
        resumeScene = DatabaseManager.ReadString(data, "resumeScene", "");

        UpdateUI();

        GoToResumeOrMainMenu();
    }

    // CHANGE: Guest starts immediately, no cloud, no resume.
    private void StartAfterGuestLogin()
    {
        if (signInPanel != null) signInPanel.SetActive(false);
        if (gamePanel != null) gamePanel.SetActive(false);

        // Guest always starts fresh
        SceneManager.LoadScene(mainMenuSceneName);
    }

    /* ===================================================
     * AUTHENTICATION BUTTON HANDLING
     * =================================================== */
    enum ButtonType { REGISTER, LOGIN }

    async Task OnButtonClicked(ButtonType type)
    {
        // CHANGE: not guest when using Register/Login buttons
        isGuest = false;

        // Display name can be Hebrew/English
        displayName = usernameInputField != null ? usernameInputField.text.Trim() : "";
        string password = FIXED_PASSWORD;

        if (string.IsNullOrEmpty(displayName))
        {
            if (statusField != null) statusField.text = "Please enter username";
            return;
        }

        // CHANGE: Deterministic internal username so the same name logs into the same account
        internalUsername = InternalFromDisplayName(displayName);

        string message;

        if (type == ButtonType.REGISTER)
        {
            justRegistered = true;

            if (statusField != null) statusField.text = "Registering...";
            message = await authManager.RegisterWithUsernameAndPassword(internalUsername, password);
        }
        else
        {
            justRegistered = false;

            if (statusField != null) statusField.text = "Logging in...";
            message = await authManager.LoginWithUsernameAndPassword(internalUsername, password);
        }

        if (statusField != null) statusField.text = message;

        /* ========== AUTHENTICATION SUCCESS ========== */
        if (message.ToLower().Contains("success"))
        {
            if (AuthenticationService.Instance.IsSignedIn)
            {
                await HandlePostAuth(type);
            }
            else
            {
                // SignedIn event is not async, so wrap with async lambda
                AuthenticationService.Instance.SignedIn += async () => await HandlePostAuth(type);
            }
        }
    }

    // Connected to a NEW UI button: "Guest"
    public async void OnGuestButtonClicked()
    {
        if (statusField != null) statusField.text = "Signing in as guest...";

        // CHANGE: mark guest session
        isGuest = true;

        // Guest has no displayName typed; use a nice UI name
        displayName = "Guest";
        internalUsername = "guest_" + System.Guid.NewGuid().ToString("N").Substring(0, 8); // not used by anonymous auth
        justRegistered = true;

        string message = await authManager.LoginAnonymously();
        if (statusField != null) statusField.text = message;

        if (message.ToLower().Contains("success"))
        {
            if (AuthenticationService.Instance.IsSignedIn)
            {
                // Guest: this will NOT save anything due to AllowCloudSave() == false
                await PostSignInCommon();

                // CHANGE: start immediately (no continue)
                StartAfterGuestLogin();
            }
            else
            {
                AuthenticationService.Instance.SignedIn += async () =>
                {
                    // Guest: this will NOT save anything due to AllowCloudSave() == false
                    await PostSignInCommon();

                    // CHANGE: start immediately (no continue)
                    StartAfterGuestLogin();
                };
            }
        }
    }

    // Connected to UI buttons
    public async void OnSignUpButtonClicked()
    {
        await OnButtonClicked(ButtonType.REGISTER);
    }

    public async void OnSignInButtonClicked()
    {
        await OnButtonClicked(ButtonType.LOGIN);
    }

    /* ===================================================
     * SABABICH GAME ACTIONS
     * =================================================== */

    // Called by AutoSaveResumeScene / CloudProgressTracker (recommended) OR manually to store where the player is now.
    // This should be called from Tutorial/Level/EndScene, so the player resumes from there next time.
    public async void SaveResumeScene(string sceneName)
    {
        if (!enabled) return;

        // Prevent Cloud Save calls before authentication
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            Debug.LogWarning("SaveResumeScene: not signed in -> ignoring.");
            return;
        }

        // CHANGE: Guest should not save to cloud
        if (isGuest)
        {
            // Guest mode: NO cloud save
            return;
        }

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("SaveResumeScene: sceneName is empty - ignoring.");
            return;
        }

        // Do not overwrite progress with Login scene.
        if (sceneName == LOGIN_SCENE_NAME)
        {
            Debug.Log("SaveResumeScene: ignoring Login scene.");
            return;
        }

        resumeScene = sceneName;

        /* ========== CLOUD SAVE ========== */
        // This is where we DEFINE what is saved to the cloud
        await DatabaseManager.SaveData(("resumeScene", resumeScene));
    }

    // Call this when the player finished ALL levels (e.g., finished Level3 successfully),
    // so next login will go directly to MainMenu.
    public async void ClearResumeToMainMenu()
    {
        if (!enabled) return;

        // Prevent Cloud Save calls before authentication
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            Debug.LogWarning("ClearResumeToMainMenu: not signed in -> ignoring.");
            return;
        }

        // CHANGE: Guest should not save to cloud
        if (isGuest)
        {
            // Guest mode: NO cloud save
            return;
        }

        resumeScene = mainMenuSceneName;

        /* ========== CLOUD SAVE ========== */
        await DatabaseManager.SaveData(("resumeScene", resumeScene));
        UpdateUI();
    }

    // Decides where to send the player after login:
    // - If resumeScene exists -> load it
    // - Else -> load MainMenu
    private void GoToResumeOrMainMenu()
    {
        string target = string.IsNullOrEmpty(resumeScene) ? mainMenuSceneName : resumeScene;

        // If the saved scene is not in Build Settings, Unity will fail to load it.
        // In that case, fall back to MainMenu.
        if (!IsSceneInBuildSettings(target))
        {
            Debug.LogWarning($"Saved scene '{target}' is not in Build Settings. Falling back to '{mainMenuSceneName}'.");
            target = mainMenuSceneName;
        }

        Debug.Log($"Loading scene: {target}");
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

    void UpdateUI()
    {
        if (textField != null)
        {
            // Debug/UX text:
            // show what we loaded from the cloud so it's easy to verify during testing.
            string shown = string.IsNullOrEmpty(resumeScene) ? "(none -> MainMenu)" : resumeScene;

            // Include display name for clarity while testing
            string shownName = string.IsNullOrEmpty(displayName) ? "(no displayName)" : displayName;

            textField.text = $"DisplayName: {shownName}\nResume Scene: {shown}";
        }
    }

    // CHANGE: HandlePostAuth is now async and starts immediately (no Continue screen)
    private async Task HandlePostAuth(ButtonType type)
    {
        // Always save mapping + enable tracker right after sign-in (registered only)
        await PostSignInCommon();

        // Enable this script after sign-in (so SaveResumeScene can run)
        enabled = true;

        // CHANGE: start immediately (registered flow)
        await StartAfterRegisteredLoginAsync(type);
    }
}
