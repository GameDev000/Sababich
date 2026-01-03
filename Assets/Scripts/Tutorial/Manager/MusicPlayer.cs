using UnityEngine;

/// <summary>
/// Singleton class that manages background music playback and sound toggling.
/// </summary>
public class MusicPlayer : MonoBehaviour
{
    public static MusicPlayer Instance;

    [System.Serializable]
    public class Track
    {
        public string id;
        public string displayName;
        public AudioClip clip;
    }

    private const string PREF_TRACK = "music_selected_track";
    private const string PREF_MUTED = "music_muted";

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    [Header("Tracks")]
    [SerializeField] private Track[] tracks;

    public bool IsMuted => audioSource != null && audioSource.mute;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        bool muted = PlayerPrefs.GetInt(PREF_MUTED, 0) == 1;
        if (audioSource != null) audioSource.mute = muted;

        string savedId = PlayerPrefs.GetString(PREF_TRACK, GetDefaultTrackId());
        SetTrack(savedId, playImmediately: true);
    }

    public void ToggleSound()
    {
        if (audioSource == null) return;

        audioSource.mute = !audioSource.mute;
        PlayerPrefs.SetInt(PREF_MUTED, audioSource.mute ? 1 : 0);
        PlayerPrefs.Save();
        if (!audioSource.mute && audioSource.clip != null && !audioSource.isPlaying)
            audioSource.Play();
    }



    public void SetTrack(string id, bool playImmediately = true)
    {
        if (audioSource == null) return;

        Track t = FindTrack(id);
        if (t == null || t.clip == null) return;

        audioSource.loop = true;

        if (audioSource.clip != t.clip)
            audioSource.clip = t.clip;

        if (playImmediately && !audioSource.mute && !audioSource.isPlaying)
            audioSource.Play();

        PlayerPrefs.SetString(PREF_TRACK, id);
        PlayerPrefs.Save();
    }




    public string GetCurrentTrackId()
    {
        return PlayerPrefs.GetString(PREF_TRACK, GetDefaultTrackId());
    }

    private Track FindTrack(string id)
    {
        if (tracks == null) return null;
        foreach (var t in tracks)
            if (t != null && t.id == id) return t;
        return null;
    }

    private string GetDefaultTrackId()
    {
        return (tracks != null && tracks.Length > 0 && tracks[0] != null) ? tracks[0].id : "";
    }
    public Track[] GetTracks() => tracks;


    public void PauseMusic()
    {
        if (audioSource == null) return;
        if (audioSource.isPlaying) audioSource.Pause();
    }

    public void ResumeMusic()
    {
        if (audioSource == null) return;
        if (!audioSource.mute && audioSource.clip != null && !audioSource.isPlaying)
            audioSource.UnPause();
    }

    public void StopMusic()
    {
        if (audioSource == null) return;
        audioSource.Stop();
    }


}
