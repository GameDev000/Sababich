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
        SceneManager.LoadScene("level1 - israel");
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

    public void OnEndLevelTwo()
    {
        if (LevelTwoState.IsSuccess)
            SceneManager.LoadScene("level3 - USA");
        else
            SceneManager.LoadScene("level2 - china");
    }

    public void OnEndLevelThree()
    {
        if (LevelThreeState.IsSuccess)
            SceneManager.LoadScene("MainMenu");
        else
            SceneManager.LoadScene("level3 - USA");
    }
}
