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
}
