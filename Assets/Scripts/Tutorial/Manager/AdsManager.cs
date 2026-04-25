using UnityEngine;
using UnityEngine.Advertisements;
using System;
using System.Collections;

public class AdsManager : MonoBehaviour,
    IUnityAdsInitializationListener,
    IUnityAdsLoadListener,
    IUnityAdsShowListener
{
    public static AdsManager Instance { get; private set; }

    [Header("Unity Ads Game IDs")]
    [SerializeField] private string androidGameId = "6050145";
    // אם בעתיד תוציא גם iOS:
    [SerializeField] private string iosGameId = "6050144";

    [Header("Placement IDs (must match Unity Dashboard)")]
    [SerializeField] private string interstitialPlacementIdAndroid = "Interstitial_Android";
    [SerializeField] private string rewardedPlacementIdAndroid = "Rewarded_Android";
    [SerializeField] private string bannerPlacementIdAndroid = "Banner_Android";

    [Header("Behavior")]
    [Tooltip("If true, loads ads on init and keeps them warm.")]
    [SerializeField] private bool preloadOnInit = true;

    [Tooltip("Minimum seconds between interstitials (safety).")]
    [SerializeField] private float interstitialCooldownSeconds = 45f;

    [Tooltip("Retry delay when a load fails.")]
    [SerializeField] private float loadRetryDelaySeconds = 10f;

    [Header("Ads Switch")]
    [SerializeField] private bool adsEnabled = false;

    private bool isInitialized;

    private bool interstitialLoaded;
    private bool rewardedLoaded;
    private bool bannerLoaded;

    private bool interstitialLoading;
    private bool rewardedLoading;
    private bool bannerLoading;

    private float lastInterstitialShowTime = -999f;

    private Action onInterstitialClosed;
    private Action<bool> onRewardedFinished; // true = rewarded completed

    private string GameId
    {
        get
        {
#if UNITY_IOS
            return iosGameId;
#else
            return androidGameId;
#endif
        }
    }

    private string InterstitialPlacementId
    {
        get
        {
#if UNITY_IOS
            // אם תגדיר iOS placements בעתיד, תחליף כאן
            return "Interstitial_iOS";
#else
            return interstitialPlacementIdAndroid;
#endif
        }
    }

    private string RewardedPlacementId
    {
        get
        {
#if UNITY_IOS
            return "Rewarded_iOS";
#else
            return rewardedPlacementIdAndroid;
#endif
        }
    }

    private string BannerPlacementId
    {
        get
        {
#if UNITY_IOS
            return "Banner_iOS";
#else
            return bannerPlacementIdAndroid;
#endif
        }
    }

    private bool TestMode
    {
        get
        {
#if UNITY_EDITOR
            return true;                // תמיד טסט בעורך
#else
            return Debug.isDebugBuild;   // Development Build = טסט, Release לחנות = false
#endif
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (adsEnabled)
        {
            InitializeAds();
        }
        else
        {
            Debug.Log("[Ads] Ads are disabled.");
        }
    }

    private void InitializeAds()
    {
        if (isInitialized) return;

        if (string.IsNullOrWhiteSpace(GameId))
        {
            Debug.LogWarning("[Ads] GameId is empty. Ads will be disabled.");
            return;
        }

        Debug.Log($"[Ads] Initializing Unity Ads. TestMode={TestMode}, GameId={GameId}");
        Advertisement.Initialize(GameId, TestMode, this);
    }

    // =========================
    // Public API
    // =========================

    /// <summary>
    /// Warm up ad inventory (call after scene load / at app start).
    /// </summary>
    public void PreloadAll()
    {
        if (!isInitialized) return;

        LoadInterstitial();
        LoadRewarded();
        LoadBanner();
    }

    public void ShowInterstitialThen(Action afterAd)
    {
        if (!adsEnabled)
        {
            afterAd?.Invoke();
            return;
        }

        onInterstitialClosed = afterAd;

        if (!CanShowInterstitialNow())
        {
            FinishInterstitialFlow();
            return;
        }

        interstitialLoaded = false;
        lastInterstitialShowTime = Time.realtimeSinceStartup;

        Debug.Log("[Ads] Showing interstitial...");
        Advertisement.Show(InterstitialPlacementId, this);
    }

    public void ShowRewardedThen(Action<bool> onFinished)
    {
        if (!adsEnabled)
        {
            onFinished?.Invoke(false);
            return;
        }

        onRewardedFinished = onFinished;

        if (!isInitialized || !rewardedLoaded)
        {
            FinishRewardedFlow(false);
            return;
        }

        rewardedLoaded = false;

        Debug.Log("[Ads] Showing rewarded...");
        Advertisement.Show(RewardedPlacementId, this);
    }

    public void ShowBanner()
    {
        if (!adsEnabled) return;

        if (!isInitialized) return;

        if (!bannerLoaded)
        {
            LoadBanner();
            return;
        }

        Debug.Log("[Ads] Banner is marked loaded. (If you use Banner API, call Banner.Show here.)");
    }

    // =========================
    // Internal Loaders
    // =========================

    private void LoadInterstitial()
    {
        if (!isInitialized || interstitialLoading || interstitialLoaded) return;

        interstitialLoading = true;
        Debug.Log("[Ads] Loading interstitial...");
        Advertisement.Load(InterstitialPlacementId, this);
    }

    private void LoadRewarded()
    {
        if (!isInitialized || rewardedLoading || rewardedLoaded) return;

        rewardedLoading = true;
        Debug.Log("[Ads] Loading rewarded...");
        Advertisement.Load(RewardedPlacementId, this);
    }

    private void LoadBanner()
    {
        // NOTE: Banner is API-specific. If you rely on a separate Banner API,
        // keep the "loaded" signal in that path. Here we at least keep a placeholder.
        if (!isInitialized || bannerLoading || bannerLoaded) return;

        bannerLoading = true;
        Debug.Log("[Ads] Loading banner (via placement load)...");
        Advertisement.Load(BannerPlacementId, this);
    }

    private bool CanShowInterstitialNow()
    {
        if (!isInitialized) return false;
        if (!interstitialLoaded) return false;

        float now = Time.realtimeSinceStartup;
        if (now - lastInterstitialShowTime < interstitialCooldownSeconds) return false;

        return true;
    }

    private void RetryLoad(string placementId)
    {
        StartCoroutine(RetryLoadCoroutine(placementId));
    }

    private IEnumerator RetryLoadCoroutine(string placementId)
    {
        yield return new WaitForSecondsRealtime(loadRetryDelaySeconds);

        if (!isInitialized) yield break;

        if (placementId == InterstitialPlacementId) LoadInterstitial();
        else if (placementId == RewardedPlacementId) LoadRewarded();
        else if (placementId == BannerPlacementId) LoadBanner();
    }

    // =========================
    // Finish flows
    // =========================

    private void FinishInterstitialFlow()
    {
        try { onInterstitialClosed?.Invoke(); }
        finally { onInterstitialClosed = null; }

        // warm next
        LoadInterstitial();
    }

    private void FinishRewardedFlow(bool rewarded)
    {
        try { onRewardedFinished?.Invoke(rewarded); }
        finally { onRewardedFinished = null; }

        // warm next
        LoadRewarded();
    }

    // =========================
    // Initialization callbacks
    // =========================

    public void OnInitializationComplete()
    {
        isInitialized = true;
        Debug.Log("[Ads] Initialized OK");

        if (preloadOnInit)
            PreloadAll();
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        isInitialized = false;
        Debug.LogWarning($"[Ads] Init failed: {error} - {message}");
    }

    // =========================
    // Load callbacks
    // =========================

    public void OnUnityAdsAdLoaded(string placementId)
    {
        if (placementId == InterstitialPlacementId)
        {
            interstitialLoading = false;
            interstitialLoaded = true;
            Debug.Log("[Ads] Interstitial loaded.");
            return;
        }

        if (placementId == RewardedPlacementId)
        {
            rewardedLoading = false;
            rewardedLoaded = true;
            Debug.Log("[Ads] Rewarded loaded.");
            return;
        }

        if (placementId == BannerPlacementId)
        {
            bannerLoading = false;
            bannerLoaded = true;
            Debug.Log("[Ads] Banner loaded (placement).");
            return;
        }

        // unknown placement
        Debug.Log($"[Ads] Loaded unknown placement: {placementId}");
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        Debug.LogWarning($"[Ads] Load failed: {placementId} - {error} - {message}");

        if (placementId == InterstitialPlacementId)
        {
            interstitialLoading = false;
            interstitialLoaded = false;
            RetryLoad(placementId);
            return;
        }

        if (placementId == RewardedPlacementId)
        {
            rewardedLoading = false;
            rewardedLoaded = false;
            RetryLoad(placementId);
            return;
        }

        if (placementId == BannerPlacementId)
        {
            bannerLoading = false;
            bannerLoaded = false;
            RetryLoad(placementId);
            return;
        }
    }

    // =========================
    // Show callbacks
    // =========================

    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        Debug.LogWarning($"[Ads] Show failed: {placementId} - {error} - {message}");

        if (placementId == InterstitialPlacementId)
        {
            FinishInterstitialFlow();
            return;
        }

        if (placementId == RewardedPlacementId)
        {
            FinishRewardedFlow(false);
            return;
        }
    }

    public void OnUnityAdsShowStart(string placementId) { }
    public void OnUnityAdsShowClick(string placementId) { }

    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        if (placementId == InterstitialPlacementId)
        {
            FinishInterstitialFlow();
            return;
        }

        if (placementId == RewardedPlacementId)
        {
            bool rewarded = (showCompletionState == UnityAdsShowCompletionState.COMPLETED);
            FinishRewardedFlow(rewarded);
            return;
        }
    }
}