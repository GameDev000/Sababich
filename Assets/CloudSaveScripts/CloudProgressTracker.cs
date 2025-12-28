using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.Authentication; // only save after sign-in

/*
 * Saves the CURRENT scene name to Cloud Save whenever a scene is loaded.
 * This guarantees we can resume into endScenes / mid-level scenes exactly as required.
 */
public class CloudProgressTracker : MonoBehaviour
{
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private async void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //  Prevent Cloud Save calls before authentication (avoids: Player ID is missing)
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            Debug.Log("[CloudProgressTracker] Not signed in yet -> skipping save.");
            return;
        }

        // Don't overwrite progress while on Login (otherwise you'll always resume to Login)
        if (scene.name == "Login") return;

        // If you also don't want to overwrite when user is in MainMenu, comment out the next line.
        // If (scene.name == "MainMenu") return;

        await DatabaseManager.SaveData(("resumeScene", scene.name));
        Debug.Log($"[CloudProgressTracker] Saved resumeScene='{scene.name}'");
    }

    // Call this ONLY when the player finished level 3 successfully.
    public async void MarkGameCompleted()
    {
        // Prevent Cloud Save calls before authentication (extra safety)
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            Debug.Log("[CloudProgressTracker] Not signed in yet -> cannot mark completed.");
            return;
        }

        await DatabaseManager.SaveData(("resumeScene", "MainMenu"));
        Debug.Log("[CloudProgressTracker] Game completed -> resumeScene='MainMenu'");
    }

    //  Small helper so the Login script can enable it after successful sign-in.
    public void EnableAfterSignIn()
    {
        enabled = true;
        Debug.Log("[CloudProgressTracker] Enabled after sign-in.");
    }
}
