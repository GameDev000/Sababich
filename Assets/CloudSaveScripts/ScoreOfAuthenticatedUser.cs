using TMPro;
using Unity.Services.Authentication;
using UnityEngine;
using System.Threading.Tasks;
using Unity.Services.CloudSave.Models;   // Item + GetAs extensions
using UnityEngine.SceneManagement;       // SceneManager (resume to saved scene)

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
 * IMPORTANT CHANGE (focus):
 * - We do NOT auto-load the resume scene inside Initialize().
 * - Instead, after login we show a "Welcome back" screen (gamePanel)
 *   and wait for the player to press a Continue button.
 *   (This matches your requirement: show a message + "pause" before resuming.)
 */
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

    /* ================= AUTHENTICATION ================= */

    // It handles UnityServices.InitializeAsync + login/register
    [SerializeField] private AuthenticationManagerWithPassword authManager;

    /* ================= SABABICH DATA ================= */

    // Sababich data to save in the cloud
    // In this stage we save ONLY the player's progress (where to resume).
    // Key in CloudSave: "resumeScene"
    private string resumeScene = "";

    // FUTURE OPTION:
    // We may also save player's money/coins later.
    // private int money = 0;

    // Assignment requirement: username-only login.
    // We keep a fixed password inside the code (same for all users).
    private const string FIXED_PASSWORD = "SababichFixedPassword2025!";

    // If no resumeScene exists (new user / finished game / cleared progress),
    // we send the player to MainMenu.
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    // Safety: do not allow saving "Login" as resume
    private const string LOGIN_SCENE_NAME = "Login";

    void Awake()
    {
        Debug.Log("ScoreOfAuthenticatedUser Awake");
        // Authentication initialization happens inside AuthenticationManagerWithPassword.Awake()
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

        /* ========== CLOUD LOAD ========== */
        // This is where we DEFINE what we load from the cloud
        var data = await DatabaseManager.LoadData("resumeScene");

        // Read resumeScene safely (default is empty string)
        resumeScene = DatabaseManager.ReadString(data, "resumeScene", "");

        UpdateUI();

        // Optional: save username for debugging (as lecturer did)
        if (usernameInputField != null)
            await DatabaseManager.SaveData(("username", usernameInputField.text));

        enabled = true;

        /* ========== RESUME GAME FLOW ========== */
        // IMPORTANT:
        // We do NOT load the scene automatically here.
        // We wait for the player to click the Continue button.
        //
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

        // FUTURE OPTION:
        // We may also save money later (not required at this stage).
        // await DatabaseManager.SaveData(("money", money));
    }

    // OPTIONAL utility:
    // Call this when the player finished ALL levels (e.g., finished Level3 successfully),
    // so next login will go directly to MainMenu.
    public async void ClearResumeToMainMenu()
    {
        if (!enabled) return;

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
