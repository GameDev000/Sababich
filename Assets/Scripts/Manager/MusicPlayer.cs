using UnityEngine;
//// <summary>
/// Singleton class that manages background music playback and sound toggling.
/// </summary>
public class MusicPlayer : MonoBehaviour
{
    [SerializeField] public static MusicPlayer Instance;
    public AudioSource audioSource;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this; // Set the singleton instance
        audioSource = GetComponent<AudioSource>(); // Get the AudioSource component
        DontDestroyOnLoad(this.gameObject); // Persist across scenes
    }

    /// <summary>
    /// Toggles the sound on and off by muting or unmuting the AudioSource.
    /// </summary>
    public void ToggleSound()
    {
        audioSource.mute = !audioSource.mute;
    }
}