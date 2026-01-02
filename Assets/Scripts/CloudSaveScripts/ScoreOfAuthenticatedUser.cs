using TMPro;
using Unity.Services.Authentication;
using UnityEngine;
using System.Threading.Tasks;
using Unity.Services.CloudSave.Models;   // Item + GetAs extensions
using UnityEngine.SceneManagement;       // SceneManager (resume to saved scene)
using UnityEngine.UI;                    // for Button

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

    // If true, the player JUST registered now -> Continue button should be plain "Continue".
    private bool justRegistered = false;

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
        // This prevents the "Welcome back" (gamePanel) from appearing before authentication.
        if (signInPanel != null) signInPanel.SetActive(true);
        if (gamePanel != null) gamePanel.SetActive(false);

        // Make sure inputs are editable at the start
        if (usernameInputField != null) usernameInputField.readOnly = false;
        if (passwordInputField != null) passwordInputField.readOnly = false;

        // clear previous status text at start
        if (statusField != null) statusField.text = "";

        // Prevent "flash" of default Continue button text before async cloud load finishes
        if (continueButton != null) continueButton.interactable = false; // not clickable until we know what to show
        if (continueButtonText != null) continueButtonText.text = "";     // optional: remove Inspector default text
    }

    // Create an ASCII-only username for Unity Authentication (Display name may be Hebrew/English)
    private string GenerateInternalUsername()
    {
        return "user_" + System.Guid.NewGuid().ToString("N").Substring(0, 10);
    }

    // Cloud key used to map displayName -> internalUsername
    private string NameMapKey(string disp)
    {
        // Important: use a stable prefix so keys don't collide with other data.
        return $"user_of_{disp}";
    }

    /* ===================================================
     * CALLED AFTER SUCCESSFUL LOGIN / REGISTER
     * =================================================== */
    async void Initialize()
    {
        Debug.Log("Initialize Sababich Cloud State");

        // Switch UI panels
        if (signInPanel != null) signInPanel.SetActive(false);
        if (gamePanel != null) gamePanel.SetActive(true);

        if (usernameInputField != null) usernameInputField.readOnly = true;

        // We keep this line to match the lecturer's UI pattern,
        // but password is not used (it is fixed in code).
        if (passwordInputField != null) passwordInputField.readOnly = true;

        // Sync passed flags (LevelOneState/Two/Three) BEFORE reading resumeScene
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

        // Save displayName + mapping after authentication, so Hebrew login works next time.
        // We do it here because now we are definitely signed-in and CloudSave is available.
        if (!string.IsNullOrEmpty(displayName) && !string.IsNullOrEmpty(internalUsername))
        {
            await DatabaseManager.SaveData(("displayName", displayName));                  // UI/debug
            await DatabaseManager.SaveData((NameMapKey(displayName), internalUsername));  // mapping
        }

        // Now we know if user is NEW or EXISTING -> set the Continue button text ONCE (no flash)
        // "new" should also happen for JUST REGISTERED users (even if resumeScene empty).
        bool isNewUser = justRegistered || string.IsNullOrEmpty(resumeScene) || resumeScene == mainMenuSceneName;

        if (continueButtonText != null)
        {
            // New user (including just registered) -> plain Continue
            // Existing user -> "Welcome back..."
            continueButtonText.text = isNewUser
                ? "המשך"
                : "ברוך שובך,\nהמשך מהמקום שעצרת";
        }

        // Enable the Continue button only after the text is correct
        if (continueButton != null)
        {
            continueButton.interactable = true;
        }

        UpdateUI();

        // Optional: save username for debugging (as lecturer did)
        // save internal username (actual Unity Auth username) for debugging
        if (usernameInputField != null)
            await DatabaseManager.SaveData(("username", internalUsername));

        enabled = true;

        // Now that we are signed in, enable the tracker so it can save scene changes.
        if (progressTracker == null)
            progressTracker = FindObjectOfType<CloudProgressTracker>(); // fallback

        if (progressTracker != null)
        {
            progressTracker.EnableAfterSignIn();
        }
        else
        {
            Debug.LogWarning("ScoreOfAuthenticatedUser: CloudProgressTracker not found. Scene progress won't be auto-saved.");
        }

        // We do NOT load the scene automatically here.
        // We wait for the player to click the Continue button.
        // In the Inspector, connect the Continue button OnClick() to:
        // ScoreOfAuthenticatedUser -> ContinueFromSavedProgress()
        if (statusField != null)
        {
            statusField.gameObject.SetActive(true);

            if (isNewUser)
                statusField.text = "Welcome! Click Continue to go to the main menu...";
            else
                statusField.text = "Welcome back! Click Continue to resume where you stopped...";
        }

        // Reset flag so future logins behave normally
        justRegistered = false;
    }

    /* ===================================================
     * AUTHENTICATION BUTTON HANDLING
     * =================================================== */
    enum ButtonType { REGISTER, LOGIN }

    async Task OnButtonClicked(ButtonType type)
    {
        // Display name can be Hebrew/English
        displayName = usernameInputField != null ? usernameInputField.text.Trim() : "";
        string password = FIXED_PASSWORD;

        if (string.IsNullOrEmpty(displayName))
        {
            if (statusField != null) statusField.text = "Please enter username";
            return;
        }

        string message;

        if (type == ButtonType.REGISTER)
        {
            // New user -> generate an ASCII-only internal username for Unity Authentication
            internalUsername = GenerateInternalUsername();
            justRegistered = true;

            if (statusField != null) statusField.text = "Registering...";
            message = await authManager.RegisterWithUsernameAndPassword(internalUsername, password);
        }
        else
        {
            // Existing user login by Hebrew/English display name:
            // we look up the internal username from Cloud Save mapping.
            justRegistered = false;

            if (statusField != null) statusField.text = "Looking up user...";

            var lookup = await DatabaseManager.LoadData(NameMapKey(displayName));
            internalUsername = DatabaseManager.ReadString(lookup, NameMapKey(displayName), "");

            if (string.IsNullOrEmpty(internalUsername))
            {
                if (statusField != null) statusField.text = "User not found. Please register first.";
                return;
            }

            if (statusField != null) statusField.text = "Logging in...";
            message = await authManager.LoginWithUsernameAndPassword(internalUsername, password);
        }

        if (statusField != null) statusField.text = message;

        /* ========== AUTHENTICATION SUCCESS ========== */
        if (message.ToLower().Contains("success"))
        {
            if (AuthenticationService.Instance.IsSignedIn)
            {
                HandlePostAuth(type);
            }
            else
            {
                AuthenticationService.Instance.SignedIn += () => HandlePostAuth(type);
            }
        }
    }

    // Connected to a NEW UI button: "Guest"
    // Guest WILL be saved to cloud (anonymous auth) because it still has PlayerId.
    public async void OnGuestButtonClicked()
    {
        if (statusField != null) statusField.text = "Signing in as guest...";

        // Guest has no displayName typed; use a nice UI name and a stable mapping key
        displayName = "Guest";
        internalUsername = "guest_" + System.Guid.NewGuid().ToString("N").Substring(0, 8); // not used by anonymous auth
        justRegistered = true; // treat guest like "new user" for button text
        string message = await authManager.LoginAnonymously();
        if (statusField != null) statusField.text = message;

        if (message.ToLower().Contains("success"))
        {
            if (AuthenticationService.Instance.IsSignedIn)
            {
                Initialize();
            }
            else
            {
                AuthenticationService.Instance.SignedIn += Initialize;
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
     * CONTINUE BUTTON (Login Welcome Screen)
     * =================================================== */

    // Hook this to a UI button on the Login scene (inside gamePanel).
    // It loads the saved scene or MainMenu (if new/finished).
    public void ContinueFromSavedProgress()
    {
        if (!enabled) return;

        // Prevent continuing if user somehow isn't signed in yet
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            if (statusField != null) statusField.text = "Please sign in first...";
            Debug.LogWarning("ContinueFromSavedProgress: not signed in yet.");
            return;
        }

        GoToResumeOrMainMenu();
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

        resumeScene = mainMenuSceneName;

        /* ========== CLOUD SAVE ========== */
        await DatabaseManager.SaveData(("resumeScene", resumeScene));
        UpdateUI();
    }

    // Decides where to send the player after they click Continue:
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

    void HandlePostAuth(ButtonType type)
    {
        // Register → play intro video
        if (type == ButtonType.REGISTER)
        {
            SceneManager.LoadScene(introVideoSceneName);
            return;
        }

        // Login → skip intro, continue normally
        Initialize();
    }
}
