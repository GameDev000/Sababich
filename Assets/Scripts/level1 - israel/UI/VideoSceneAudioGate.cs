using UnityEngine;

public class VideoSceneAudioGate : MonoBehaviour
{
    private void Start()
    {
        if (MusicPlayer.Instance != null)
            MusicPlayer.Instance.PauseMusic();
    }

    private void OnDestroy()
    {
        if (MusicPlayer.Instance != null)
            MusicPlayer.Instance.ResumeMusic();
    }
}
