using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class ChinaTransitionVideoPlayer : MonoBehaviour
{
    [Header("Video")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private string fileName = "China.mp4";

    [Header("UI")]
    [SerializeField] private Button skipButton;

    [Header("After Video")]
    [SerializeField] private string nextSceneName = "level1.2 - china";

    private bool isLeaving = false;

    private void Awake()
    {
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();

        if (skipButton != null)
            skipButton.onClick.AddListener(Skip);
    }

    private void Start()
    {
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = $"{Application.streamingAssetsPath}/{fileName}";
        videoPlayer.isLooping = false;
        videoPlayer.playOnAwake = false;
        videoPlayer.waitForFirstFrame = true;

        videoPlayer.loopPointReached += OnVideoFinished;
        videoPlayer.errorReceived += OnVideoError;

        videoPlayer.Play();
    }

    private void OnVideoFinished(VideoPlayer vp) => GoNext();

    private void OnVideoError(VideoPlayer vp, string msg)
    {
        Debug.LogError($"[ChinaTransitionVideoPlayer] Video error: {msg}");
        GoNext();
    }

    private void Skip() => GoNext();

    private void GoNext()
    {
        if (isLeaving) return;
        isLeaving = true;

        if (skipButton != null) skipButton.interactable = false;
        if (videoPlayer != null) videoPlayer.Stop();

        SceneManager.LoadScene(nextSceneName);
    }

    private void OnDestroy()
    {
        if (videoPlayer == null) return;
        videoPlayer.loopPointReached -= OnVideoFinished;
        videoPlayer.errorReceived -= OnVideoError;
    }
}
