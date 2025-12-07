using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    public static MusicPlayer Instance;
    public AudioSource audioSource;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        audioSource = GetComponent<AudioSource>();
        DontDestroyOnLoad(this.gameObject);
    }

    public void ToggleSound()
    {
        audioSource.mute = !audioSource.mute;
    }
}