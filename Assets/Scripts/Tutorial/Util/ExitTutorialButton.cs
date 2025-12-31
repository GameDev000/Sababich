using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitTutorialButton : MonoBehaviour
{
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    // Optional: if you have a tutorial manager that needs to stop logic
    [SerializeField] private MonoBehaviour tutorialManagerToDisable;

    public void OnUnderstoodClicked()
    {
        // Stop tutorial logic (arrows/text) if provided
        if (tutorialManagerToDisable != null)
            tutorialManagerToDisable.enabled = false;

        // Load MainMenu
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
