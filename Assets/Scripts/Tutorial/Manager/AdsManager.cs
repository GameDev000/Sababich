using UnityEngine;
using UnityEngine.Advertisements;
using System;

public class AdsManager : MonoBehaviour,
    IUnityAdsInitializationListener,
    IUnityAdsLoadListener,
    IUnityAdsShowListener
{
    [Header("Unity Ads IDs")]
    [SerializeField] private string androidGameId = "6050145";
    [SerializeField] private string interstitialPlacementId = "Interstitial_Android";

    [Header("Debug")]
    [SerializeField] private bool testMode = true;

    public static AdsManager Instance { get; private set; }

    private Action onAdClosed;
    private bool isInitialized = false;
    private bool isLoading = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        Initialize();
    }

    private void Initialize()
    {
        if (isInitialized) return;

        if (string.IsNullOrWhiteSpace(androidGameId))
        {
            Debug.LogWarning("[Ads] androidGameId is empty!");
            return;
        }

        Advertisement.Initialize(androidGameId, testMode, this);
    }

    private void LoadInterstitial()
    {
        if (!isInitialized || isLoading) return;

        isLoading = true;
        Advertisement.Load(interstitialPlacementId, this);
    }

    public void ShowInterstitialThen(Action afterAd)
    {
        onAdClosed = afterAd;

        // אם לא מאותחל → לא חוסמים משחק
        if (!isInitialized)
        {
            FinishAdFlow();
            return;
        }

        // Legacy: אין IsReady. ננסה Show; אם לא נטען תקבל OnUnityAdsShowFailure ואז נמשיך.
        Advertisement.Show(interstitialPlacementId, this);
    }

    private void FinishAdFlow()
    {
        try { onAdClosed?.Invoke(); }
        finally { onAdClosed = null; }

        // לטעון לפעם הבאה
        LoadInterstitial();
    }

    // =======================
    // Initialization callbacks
    // =======================
    public void OnInitializationComplete()
    {
        isInitialized = true;
        Debug.Log("[Ads] Initialized");
        LoadInterstitial();
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.LogWarning($"[Ads] Init failed: {error} - {message}");
        // לא חוסמים משחק
        isInitialized = false;
    }

    // ================
    // Load callbacks
    // ================
    public void OnUnityAdsAdLoaded(string placementId)
    {
        isLoading = false;
        Debug.Log($"[Ads] Loaded: {placementId}");
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        isLoading = false;
        Debug.LogWarning($"[Ads] Load failed: {placementId} - {error} - {message}");
    }

    // ================
    // Show callbacks
    // ================
    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        Debug.LogWarning($"[Ads] Show failed: {placementId} - {error} - {message}");
        // גם אם נכשל – ממשיכים למשחק
        FinishAdFlow();
    }

    public void OnUnityAdsShowStart(string placementId) { }

    public void OnUnityAdsShowClick(string placementId) { }

    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        // נסגר/הסתיים → ממשיכים
        FinishAdFlow();
    }
}