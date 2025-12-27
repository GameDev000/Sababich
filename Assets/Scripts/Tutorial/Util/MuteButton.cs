using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the mute button functionality, allowing the player to toggle sound on and off.
/// </summary>
public class MuteButton : MonoBehaviour
{
    [SerializeField] private Image buttonImage; // Reference to the button's Image component
    [SerializeField] private Sprite unmuteSprite; // Sprite to display when sound is unmuted
    [SerializeField] private Sprite muteSprite; // Sprite to display when sound is muted

    void Start()
    {
        UpdateIcon(); // Initialize the button icon based on the current sound state
    }

    public void OnPress()
    {
        if (MusicPlayer.Instance != null)
        {
            MusicPlayer.Instance.ToggleSound(); // Toggle the sound state
            UpdateIcon(); // Update the button icon accordingly
        }
    }

    /// <summary>
    /// Updates the button icon based on the current mute state of the MusicPlayer.
    /// </summary>
    void UpdateIcon()
    {
        if (MusicPlayer.Instance != null)
        {
            if (MusicPlayer.Instance.IsMuted)
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