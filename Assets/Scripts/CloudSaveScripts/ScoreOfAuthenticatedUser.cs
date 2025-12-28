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
 * */
public class ScoreOfAuthenticatedUser : MonoBehaviour
{
    /* ================= UI REFERENCES ================= */

    // Username input (shown to the player)
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

    void Awake()
    {
        Debug.Log("ScoreOfAuthenticatedUser Awake");
        // Authentication initialization happens inside AuthenticationManagerWithPassword.Awake()

        //  Ensure tracker doesn't try to save before sign-in (extra safety)
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

        var data = await DatabaseManager.LoadData("resumeScene");

        // Read resumeScene safely (default is empty string)
        resumeScene = DatabaseManager.ReadString(data, "resumeScene", "");

        // Now we know if user is NEW or EXISTING -> set the Continue button text ONCE (no flash)
        bool isNewUser = string.IsNullOrEmpty(resumeScene);

        if (continueButtonText != null)
        {
            continueButtonText.text = isNewUser
                ? "נמשיך"
                : "ברוך שובך,\nנמשיך מהמקום שעצרת";
        }

        //  Enable the Continue button only after the text is correct
        if (continueButton != null)
        {
            continueButton.interactable = true;
        }

        UpdateUI();

        if (usernameInputField != null)
            await DatabaseManager.SaveData(("username", usernameInputField.text));

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

            // If player has no saved progress OR saved progress points to MainMenu -> MainMenu.
            if (string.IsNullOrEmpty(resumeScene) || resumeScene == mainMenuSceneName)
                statusField.text = "Welcome! Click Continue to go to the main menu...";
            else
                statusField.text = "Welcome back! Click Continue to resume where you stopped...";
        }
    }

    /* ===================================================
     * AUTHENTICATION BUTTON HANDLING
     * =================================================== */
    enum ButtonType { REGISTER, LOGIN }

    async Task OnButtonClicked(ButtonType type)
    {
        string username = usernameInputField != null ? usernameInputField.text : "";
        string password = FIXED_PASSWORD;

        if (string.IsNullOrEmpty(username))
        {
            if (statusField != null) statusField.text = "Please enter username";
            return;
        }

        string message;

        if (type == ButtonType.REGISTER)
        {
            if (statusField != null) statusField.text = "Registering...";
            message = await authManager.RegisterWithUsernameAndPassword(username, password);
        }
        else
        {
            if (statusField != null) statusField.text = "Logging in...";
            message = await authManager.LoginWithUsernameAndPassword(username, password);
        }

        if (statusField != null) statusField.text = message;

        /* ========== AUTHENTICATION SUCCESS ========== */
        if (message.ToLower().Contains("success"))
        {
            // Only proceed if actually signed in
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
            textField.text = $"Resume Scene: {shown}";
        }
    }
}
