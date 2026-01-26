using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class IntroVideoPlayer : MonoBehaviour
{
    [Header("Video")]
    [SerializeField] private VideoPlayer videoPlayer;

    [Tooltip("If set, we will use this URL (recommended for itch/WebGL). Example: https://itamar000git.github.io/sababich-video/intro1.mp4")]
    [SerializeField] private string videoUrlOverride = "";

    [Tooltip("Used only if videoUrlOverride is empty. Must exist inside StreamingAssets.")]
    [SerializeField] private string fileName = "intro.mp4";

    [Header("UI")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button skipButton;

    [Header("After Video")]
    [SerializeField] private string nextSceneName = "MainMenu";

    private bool isLeaving = false;
    private bool isPreparing = false;
    private bool hasStarted = false;

    //for showing a preview frame before clicking start
    private bool previewFrameShown = false;

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
        // Default UI state: show Start, hide Skip (optional)
        if (startButton != null) startButton.gameObject.SetActive(true);
        if (skipButton != null) skipButton.gameObject.SetActive(false);

        videoPlayer.source = VideoSource.Url;
        videoPlayer.isLooping = false;
        videoPlayer.playOnAwake = false;

        //helps avoid black flashes when starting playback
        videoPlayer.waitForFirstFrame = true;

        //we need this to catch the first decoded frame and pause on it
        videoPlayer.sendFrameReadyEvents = true;

        //For itch/WebGL, prefer a real HTTPS URL (GitHub Pages).
        //StreamingAssetsPath (works in editor/standalone, often fails on itch).
        if (!string.IsNullOrWhiteSpace(videoUrlOverride))
            videoPlayer.url = videoUrlOverride.Trim();
        else
            videoPlayer.url = $"{Application.streamingAssetsPath}/{fileName}";

        Debug.Log($"[IntroVideoPlayer] Video URL: {videoPlayer.url}");

        // Use named handlers (not lambdas) so we can unsubscribe safely.
        videoPlayer.loopPointReached += OnVideoFinished;
        videoPlayer.prepareCompleted += OnPrepared;
        videoPlayer.errorReceived += OnVideoError;

        //pause on the first frame to show a "thumbnail" preview
        videoPlayer.frameReady += OnFrameReady;

        videoPlayer.Stop();

        //preload now so the first frame can be shown behind the Start button
        isPreparing = true;
        Debug.Log("[IntroVideoPlayer] Preparing video for preview...");
        videoPlayer.Prepare();
    }

    public void StartVideo()
    {
        if (isLeaving) return;
        if (hasStarted) return;

        hasStarted = true;

        // Hide Start, show Skip (optional)
        if (startButton != null)
            startButton.gameObject.SetActive(false);

        if (skipButton != null)
        {
            skipButton.gameObject.SetActive(true);
            skipButton.interactable = true;
        }

        // If already prepared (we preloaded), just play.
        if (videoPlayer.isPrepared)
        {
            videoPlayer.Play();
            return;
        }

        // Fallback: if somehow not prepared yet, prepare now
        isPreparing = true;
        Debug.Log("[IntroVideoPlayer] Preparing video...");
        videoPlayer.Prepare();
    }

    private void OnPrepared(VideoPlayer vp)
    {
        if (isLeaving) return;
        if (!isPreparing) return;

        isPreparing = false;

        // If user already pressed Start -> play normally
        if (hasStarted)
        {
            Debug.Log("[IntroVideoPlayer] Prepared. Starting Play().");
            vp.Play();
            return;
        }

        // Otherwise, we are in "preview" mode: play until first frame, then pause in OnFrameReady
        previewFrameShown = false;
        Debug.Log("[IntroVideoPlayer] Prepared for preview. Grabbing first frame...");
        vp.Play(); // OnFrameReady will pause on first frame
    }

    private void OnFrameReady(VideoPlayer vp, long frameIdx)
    {
        // Only for preview (before clicking Start)
        if (hasStarted) return;
        if (previewFrameShown) return;

        previewFrameShown = true;
        vp.Pause(); // leaves the first frame rendered as the background
        Debug.Log("[IntroVideoPlayer] Preview frame shown (paused on first frame).");
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        GoNext();
    }

    private void OnVideoError(VideoPlayer vp, string msg)
    {
        Debug.LogError($"[IntroVideoPlayer] Video error: {msg}");
        GoNext();
    }

    private void Skip()
    {
        GoNext();
    }

    private void GoNext()
    {
        if (isLeaving) return;
        isLeaving = true;

        if (skipButton != null) skipButton.interactable = false;
        if (startButton != null) startButton.interactable = false;

        if (videoPlayer != null)
            videoPlayer.Stop();

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
