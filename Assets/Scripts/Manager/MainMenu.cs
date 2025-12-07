using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void OnTutorialButtonClicked()
    {
        SceneManager.LoadScene("TutorialScene");
    }
    public void OnPlayButtonClicked()
    {
        Debug.Log("Play button clicked - gameplay scene not implemented yet.");
        // בהמשך: SceneManager.LoadScene("GameScene");
    }
}
