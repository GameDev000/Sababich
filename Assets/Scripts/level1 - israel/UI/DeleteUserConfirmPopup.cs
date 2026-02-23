using UnityEngine;
using UnityEngine.UI;

public class DeleteUserConfirmPopup : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject confirmPanel; // ConfirmDeletePanel
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    private AuthenticationManagerWithPassword auth;

    private void Awake()
    {
        // Find the auth manager (it should persist across scenes per your DontDestroyOnLoad)
        auth = Object.FindFirstObjectByType<AuthenticationManagerWithPassword>();

        // Panel hidden by default
        if (confirmPanel != null)
            confirmPanel.SetActive(false);

        // Hook buttons once
        if (yesButton != null) yesButton.onClick.AddListener(OnYesClicked);
        if (noButton != null) noButton.onClick.AddListener(OnNoClicked);
    }

    // Call this from the Delete button OnClick()
    public void OnDeleteButtonClicked()
    {
        if (confirmPanel != null)
            confirmPanel.SetActive(true);
    }

    private async void OnYesClicked()
    {
        // Hide popup immediately
        if (confirmPanel != null)
            confirmPanel.SetActive(false);

        if (auth == null)
        {
            Debug.LogError("AuthenticationManagerWithPassword not found.");
            return;
        }

        var msg = await auth.DeleteCurrentUserFromCloudAndSignOut();
        Debug.Log(msg);


        UnityEngine.SceneManagement.SceneManager.LoadScene("Login");
    }

    private void OnNoClicked()
    {
        if (confirmPanel != null)
            confirmPanel.SetActive(false);
    }
}