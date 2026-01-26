using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneButton : MonoBehaviour
{
    [Header("Target Scene")]
    [SerializeField] private string sceneName;

    [Header("Optional")]
    [SerializeField] private bool resetPauseBeforeLoad = true;

    public void Load()
    {
        if (resetPauseBeforeLoad)
        {
            Time.timeScale = 1f;
            AudioListener.pause = false;

        }

        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError($"{nameof(LoadSceneButton)}: sceneName is empty on {gameObject.name}");
            return;
        }

        SceneManager.LoadScene(sceneName);
    }
}
