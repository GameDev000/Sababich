using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>
/// Plays an introductory video at the start of the game, with an option to skip.
/// Upon completion or skipping, transitions to the specified next scene.
/// </summary>
public class IntroVideoPlayer : MonoBehaviour
{
    [Header("Video")]
    [SerializeField] private VideoPlayer videoPlayer; // Assign in Inspector or get in Awake
    [SerializeField] private string fileName = "intro.mp4"; // Video file in StreamingAssets

    [Header("UI")]
    [SerializeField] private Button skipButton; // Assign in Inspector

    [Header("After Video")]
    [SerializeField] private string nextSceneName = "MainMenu"; // Scene to load after video

    private bool isLeaving = false;

    private void Awake()
    {
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();

        if (skipButton != null)
            skipButton.onClick.AddListener(Skip);
    }

    /// <summary>
    /// Starts playing the intro video and sets up the event to transition scenes upon completion.
    /// </summary>
    private void Start()
    {
        videoPlayer.source = VideoSource.Url; // Load from URL (file path)
        videoPlayer.url = Path.Combine(Application.streamingAssetsPath, fileName); // Full path to video file
        videoPlayer.isLooping = false; // Do not loop the video

        videoPlayer.loopPointReached += _ => GoNext(); // Event when video ends

        videoPlayer.Play(); // Start playing the video
    }

    private void Skip()
    {
        GoNext();
    }

    /// <summary>
    /// Transitions to the next scene after the video ends or is skipped.
    /// Prevents multiple calls if already leaving.
    /// </summary>
    private void GoNext()
    {
        if (isLeaving) return;
        isLeaving = true; // Mark as leaving to prevent re-entry

        if (skipButton != null)
            skipButton.interactable = false; // Disable skip button

        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= _ => GoNext();// Unsubscribe event
            videoPlayer.Stop();
        }

        SceneManager.LoadScene(nextSceneName); // Load the next scene
    }
}
