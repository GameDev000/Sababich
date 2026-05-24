using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class TransitionVideoPlayer : MonoBehaviour
{
    [Header("Video")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private string videoUrlOverride = "";
    [SerializeField] private string fileName = "China.mp4";

    [Header("UI")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button skipButton;

    [Header("After Video")]
    [SerializeField] private string nextSceneName = "level1.2 - china";

    private bool isLeaving = false;
    private bool hasStarted = false;
    private bool previewFrameShown = false;
    private bool isPreparing = false;

    private void Awake()
    {
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();

        if (startButton != null)
            startButton.onClick.AddListener(StartVideo);

        if (skipButton != null)
            skipButton.onClick.AddListener(Skip);
    }

    private void Start()
    {
        if (startButton != null) startButton.gameObject.SetActive(true);
        if (skipButton != null) skipButton.gameObject.SetActive(false);

        videoPlayer.source = VideoSource.Url;
        if (!string.IsNullOrWhiteSpace(videoUrlOverride))
            videoPlayer.url = videoUrlOverride.Trim();
        else
            videoPlayer.url = $"{Application.streamingAssetsPath}/{fileName}";
        videoPlayer.isLooping = false;
        videoPlayer.playOnAwake = false;
        videoPlayer.waitForFirstFrame = true;
        videoPlayer.sendFrameReadyEvents = true;

        videoPlayer.loopPointReached += OnVideoFinished;
        videoPlayer.prepareCompleted += OnPrepared;
        videoPlayer.errorReceived += OnVideoError;
        videoPlayer.frameReady += OnFrameReady;

        videoPlayer.Stop();
        isPreparing = true;
        videoPlayer.Prepare();
    }

    public void StartVideo()
    {
        if (isLeaving || hasStarted) return;
        hasStarted = true;

        if (startButton != null) startButton.gameObject.SetActive(false);
        if (skipButton != null)
        {
            skipButton.gameObject.SetActive(true);
            skipButton.interactable = true;
        }

        if (videoPlayer.isPrepared)
        {
            videoPlayer.Play();
            return;
        }

        isPreparing = true;
        videoPlayer.Prepare();
    }

    private void OnPrepared(VideoPlayer vp)
    {
        if (isLeaving || !isPreparing) return;
        isPreparing = false;

        if (hasStarted) { vp.Play(); return; }

        previewFrameShown = false;
        vp.Play();
    }

    private void OnFrameReady(VideoPlayer vp, long frameIdx)
    {
        if (hasStarted || previewFrameShown) return;
        previewFrameShown = true;
        vp.Pause();
    }

    private void OnVideoFinished(VideoPlayer vp) => GoNext();

    private void OnVideoError(VideoPlayer vp, string msg)
    {
        Debug.LogError($"[TransitionVideoPlayer] Video error: {msg}");
        GoNext();
    }

    private void Skip() => GoNext();

    private void GoNext()
    {
        if (isLeaving) return;
        isLeaving = true;

        if (skipButton != null) skipButton.interactable = false;
        if (startButton != null) startButton.interactable = false;
        if (videoPlayer != null) videoPlayer.Stop();

        SceneManager.LoadScene(nextSceneName);
    }

    private void OnDestroy()
    {
        if (videoPlayer == null) return;
        videoPlayer.loopPointReached -= OnVideoFinished;
        videoPlayer.prepareCompleted -= OnPrepared;
        videoPlayer.errorReceived -= OnVideoError;
        videoPlayer.frameReady -= OnFrameReady;
    }
}
