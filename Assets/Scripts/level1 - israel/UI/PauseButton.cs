using UnityEngine;
using UnityEngine.UI;

public class PauseButton : MonoBehaviour
{
    [Header("Button Icon")]
    [SerializeField] private Image buttonImage;
    [SerializeField] private Sprite pauseSprite;
    [SerializeField] private Sprite resumeSprite;

    private void Awake()
    {
        if (buttonImage == null)
            buttonImage = GetComponent<Image>();
    }

    private void OnEnable()
    {
        UpdateIcon();
    }

    public void Toggle()
    {
        if (PauseManager.Instance == null) return;

        PauseManager.Instance.TogglePause();
        UpdateIcon();
    }

    public void UpdateIcon()
    {
        if (PauseManager.Instance == null || buttonImage == null) return;

        bool paused = PauseManager.Instance.IsPaused;
        buttonImage.sprite = paused ? resumeSprite : pauseSprite;
    }
}
