using UnityEngine;
using UnityEngine.UI;

public class MuteButton : MonoBehaviour
{
    public Image buttonImage;
    public Sprite unmuteSprite;
    public Sprite muteSprite;

    void Start()
    {
        UpdateIcon();
    }
    public void OnPress()
    {
        if (MusicPlayer.Instance != null)
        {
            MusicPlayer.Instance.ToggleSound();
            UpdateIcon();
        }
    }

    void UpdateIcon()
    {
        if (MusicPlayer.Instance != null)
        {
            if (MusicPlayer.Instance.audioSource.mute)
            {
                buttonImage.sprite = muteSprite;
            }
            else
            {
                buttonImage.sprite = unmuteSprite;
            }
        }
    }
}