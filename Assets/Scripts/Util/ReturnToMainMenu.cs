using UnityEngine;
// שורה זו קריטית - היא מאפשרת לנו לנהל סצנות
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";


    public void ReturnToMainMenu()
    {
        if (string.IsNullOrEmpty(mainMenuSceneName))
        {
            Debug.LogError("Main menu scene name is not set.");
            return;
        }

        SceneManager.LoadScene(mainMenuSceneName);
        
        Debug.Log("Returning to main menu: " + mainMenuSceneName);
    }
}