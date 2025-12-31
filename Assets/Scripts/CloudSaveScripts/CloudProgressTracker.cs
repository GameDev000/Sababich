using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.Authentication; // Check if player is signed in
using Unity.Services.Core;          // Check if Unity Services finished initializing

/// <summary>
/// Saves the name of the CURRENT scene to the cloud.
/// This lets the game "resume" from the last scene the player reached.
/// </summary>
public class CloudProgressTracker : MonoBehaviour
{
    // When this script becomes active, start listening to scene-load events.
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // When this script becomes inactive, stop listening (prevents double calls / memory leaks).
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Unity calls this automatically every time a new scene finishes loading.
    private async void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 1) Safety: don't use cloud/auth systems before Unity Services is ready.
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            Debug.Log("[CloudProgressTracker] Unity Services not ready yet -> not saving.");
            return;
        }

        // 2) Safety: Cloud Save works only after the player is signed in.
        // Otherwise we get errors like "Player user is missing".
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            Debug.Log("[CloudProgressTracker] Player not signed in -> not saving.");
            return;
        }

        // 3) Important rule: never save progress while on the Login scene.
        if (scene.name == "Login")
            return;

        // 4) Save the current scene name to the cloud.
        // Key: "resumeScene"
        await DatabaseManager.SaveData(("resumeScene", scene.name));

        Debug.Log($"[CloudProgressTracker] Saved resumeScene = '{scene.name}'");
    }

    /// <summary>
    /// Call this ONLY when the player finishes the whole game (e.g., after level 3).
    /// We set resumeScene to MainMenu so next time they start from the menu.
    /// </summary>
    public async void MarkGameCompleted()
    {
        // Same safety checks as above.
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            Debug.Log("[CloudProgressTracker] Unity Services not ready -> cannot mark completed.");
            return;
        }

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            Debug.Log("[CloudProgressTracker] Player not signed in -> cannot mark completed.");
            return;
        }

        await DatabaseManager.SaveData(("resumeScene", "MainMenu"));
        Debug.Log("[CloudProgressTracker] Game completed -> resumeScene = 'MainMenu'");
    }

    /// <summary>
    /// Helper for your Login script:
    /// after a successful sign-in, enable this script so it starts saving progress.
    /// </summary>
    public void EnableAfterSignIn()
    {
        enabled = true;
        Debug.Log("[CloudProgressTracker] Enabled after sign-in.");
    }
}
