using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the main menu interactions, including navigation to the tutorial and gameplay scenes.
/// </summary>
public class MainMenu : MonoBehaviour
{
    public void OnTutorialButtonClicked()
    {
        SceneManager.LoadScene("TutorialScene"); // Load the tutorial scene
    }
    public void OnPlayButtonClicked()
    {
        Debug.Log("Play button clicked - gameplay scene not implemented yet.");
        // SceneManager.LoadScene("GameScene"); //Will load the gameplay scene when implemented
    }
    public void OnBackToMainButtonClicked()
    {
        SceneManager.LoadScene("MainMenu"); // Load the main menu scene
    }
    public void PhaseOneButtonClicked()
    {
        SceneManager.LoadScene("level1 - israel"); // Load the main menu scene
    }

    public void OnEndLevelOne()
    {
        if (LevelOneState.IsSuccess)
            SceneManager.LoadScene("level2 - china");
        else
            SceneManager.LoadScene("level1 - israel");
    }
}
