#pragma warning disable 0649
#pragma warning disable 0162

#if MODULE_ADMOB
using GoogleMobileAds.Api;
#endif
#if MODULE_UNITYADS
using UnityEngine.Monetization;
using UnityEngine.Advertisements;
#endif

using System;
using System.Collections.Generic;
using UnityEngine;
using Watermelon.Core;

namespace Watermelon
{
    [Define("MODULE_ADMOB")]
    [Define("MODULE_UNITYADS")]
    [Define("MODULE_MAX")]
    public class AdsManager : MonoBehaviour
    {
        private static AdsManager instance;

        [SerializeField]
        private AdsData settings;
        public static AdsData Settings
        {
            get { return instance.settings; }
        }

        [Space]
        [SerializeField]
        private GameObject dummyCanvasPrefab;

        private static bool isInititalized = false;
        public static bool IsInititalized
        {
            get { return isInititalized; }
            set { isInititalized = value; }
        }

        private AdvertisingHandler[] advertisingModules = new AdvertisingHandler[]
        {
            new DummyHandler(AdvertisingModules.Dummy), // Dummy

#if MODULE_ADMOB
            new AdMobHandler(AdvertisingModules.AdMob), // AdMob module
#endif

#if MODULE_UNITYADS
            new UnityAdsHandler(AdvertisingModules.UnityAds), // Unity Ads module
#endif

#if MODULE_MAX
            new MAXHandler(AdvertisingModules.MAX),
#endif
        };

        private static Dictionary<AdvertisingModules, AdvertisingHandler> advertisingLink = new Dictionary<AdvertisingModules, AdvertisingHandler>();
        
        private void Awake()
        {
            if (instance != null)
            {
                Debug.LogWarning("[AdsManager]: Module already exists!");

                Destroy(this);

                return;
            }

            if (settings == null)
            {
                Debug.LogWarning("[AdsManager]: Settings don't exist!");

                Destroy(this);

                return;
            }

            instance = this;
        }

        private void Start()
        {
            if (settings.gdprContainer.enableGDPR)
            {
                if (GDPRController.IsGDPRStateExist())
                {
                    Initialize(GDPRController.GetGDPRState());
                }
            }
            else
            {
                Initialize(false);
            }

            // Inititalize dummy controller
            if (settings.IsDummyEnabled())
            {
                if (dummyCanvasPrefab != null)
                {
                    GameObject dummyCanvas = Instantiate(dummyCanvasPrefab, gameObject.transform);
                    dummyCanvas.transform.position = Vector3.zero;
                    dummyCanvas.transform.localScale = Vector3.one;
                    dummyCanvas.transform.rotation = Quaternion.identity;

                    AdsDummyController adsDummyController = dummyCanvas.GetComponent<AdsDummyController>();
                    adsDummyController.Init(settings);

                    DummyHandler dummyHandler = (DummyHandler)Array.Find(advertisingModules, x => x.ModuleType == AdvertisingModules.Dummy);
                    if(dummyHandler != null)
                        dummyHandler.SetDummyController(adsDummyController);
                }
                else
                {
                    Debug.LogError("[AdsManager]: Dummy controller can't be null!");
                }
            }
        }

        public static void Initialize(bool gdprState)
        {
            if (isInititalized)
                return;

            isInititalized = true;

            // Initialize all advertising modules
            advertisingLink = new Dictionary<AdvertisingModules, AdvertisingHandler>();
            for (int i = 0; i < instance.advertisingModules.Length; i++)
            {
                instance.advertisingModules[i].Init(instance.settings);
                instance.advertisingModules[i].SetGDPR(gdprState);

                advertisingLink.Add(instance.advertisingModules[i].ModuleType, instance.advertisingModules[i]);
            }
        }
        
        public static bool IsModuleActive(AdvertisingModules advertisingModules)
        {
            if (!isInititalized)
                return false;

            if (advertisingModules == AdvertisingModules.Disable)
                return false;

            return advertisingLink.ContainsKey(advertisingModules);
        }

        public static void SetGDPR(bool state)
        {
            if(!isInititalized)
            {
                Debug.LogError("[AdsManager]: Inititialize ads manager!");
            }

            for(int i = 0; i < instance.advertisingModules.Length; i++)
            {
                instance.advertisingModules[i].SetGDPR(state);
            }
        }

        public static void RequestInterstitial(AdvertisingModules advertisingModules)
        {
            if (!IsModuleActive(advertisingModules))
                return;

            if (advertisingLink[advertisingModules].IsInterstitialLoaded())
                return;

            advertisingLink[advertisingModules].RequestInterstitial();
        }

        public static void RequestRewardBasedVideo(AdvertisingModules advertisingModules)
        {
            if (!IsModuleActive(advertisingModules))
                return;

            if (advertisingLink[advertisingModules].IsRewardedVideoLoaded())
                return;

            advertisingLink[advertisingModules].RequestRewardedVideo();
        }

        public static bool IsInterstitialLoaded(AdvertisingModules advertisingModules)
        {
            if (!IsModuleActive(advertisingModules))
                return false;

            return advertisingLink[advertisingModules].IsInterstitialLoaded();
        }

        public static bool IsRewardBasedVideoLoaded(AdvertisingModules advertisingModules)
        {
            if (!IsModuleActive(advertisingModules))
                return false;

            return advertisingLink[advertisingModules].IsRewardedVideoLoaded();
        }

        public static void ShowBanner(AdvertisingModules advertisingModules)
        {
            if (!IsModuleActive(advertisingModules))
                return;

            advertisingLink[advertisingModules].ShowBanner();
        }

        public static void ShowInterstitial(AdvertisingModules advertisingModules)
        {
            if (!IsModuleActive(advertisingModules))
                return;

            if (!advertisingLink[advertisingModules].IsInterstitialLoaded())
                return;

            advertisingLink[advertisingModules].ShowInterstitial();
        }

        public static void ShowRewardBasedVideo(AdvertisingModules advertisingModules, AdvertisingHandler.RewardedVideoCallback callback)
        {
            if (!IsModuleActive(advertisingModules))
                return;

            if (!advertisingLink[advertisingModules].IsRewardedVideoLoaded())
                return;

            advertisingLink[advertisingModules].ShowRewardedVideo(callback);
        }

        public static void DestroyBanner(AdvertisingModules advertisingModules)
        {
            if (!IsModuleActive(advertisingModules))
                return;

            advertisingLink[advertisingModules].DestroyBanner();
        }

        public static void HideBanner(AdvertisingModules advertisingModules)
        {            
            if (!IsModuleActive(advertisingModules))
                return;

            advertisingLink[advertisingModules].HideBanner();
        }
    }
    
    public abstract class AdvertisingHandler
    {
        private AdvertisingModules moduleType;
        public AdvertisingModules ModuleType
        {
            get { return moduleType; }
        }

        protected AdsData adsSettings;
        protected RewardedVideoCallback rewardedVideoCallback;

        public AdvertisingHandler(AdvertisingModules moduleType)
        {
            this.moduleType = moduleType;
        }

        public abstract void Init(AdsData adsSettings);

        public abstract bool IsGDPRRequired();
        public abstract void SetGDPR(bool state);

        public abstract void ShowBanner();
        public abstract void HideBanner();
        public abstract void DestroyBanner();

        public abstract void RequestInterstitial();
        public abstract void ShowInterstitial();
        public abstract bool IsInterstitialLoaded();

        public abstract void RequestRewardedVideo();
        public abstract void ShowRewardedVideo(RewardedVideoCallback callback);
        public abstract bool IsRewardedVideoLoaded();

        public delegate void RewardedVideoCallback(bool hasReward);
    }

    public class DummyHandler : AdvertisingHandler
    {
        private AdsDummyController dummyController;

        public DummyHandler(AdvertisingModules moduleType) : base(moduleType) { }

        public override void Init(AdsData adsSettings)
        {
            this.adsSettings = adsSettings;
        }

        public void SetDummyController(AdsDummyController dummyController)
        {
            this.dummyController = dummyController;
        }

        public override void ShowBanner()
        {
            dummyController.ShowBanner();
        }

        public override void HideBanner()
        {
            dummyController.HideBanner();
        }

        public override void DestroyBanner()
        {
            dummyController.HideBanner();
        }

        public override void RequestInterstitial()
        {

        }

        public override bool IsInterstitialLoaded()
        {
            return true;
        }

        public override void ShowInterstitial()
        {
            dummyController.ShowInterstitial();
        }

        public override void RequestRewardedVideo()
        {

        }

        public override bool IsRewardedVideoLoaded()
        {
            return true;
        }

        public override void ShowRewardedVideo(RewardedVideoCallback callback)
        {
            dummyController.SetRewardedVideoCallback(callback);

            dummyController.ShowRewardedVideo();
        }

        public override void SetGDPR(bool state)
        {
            
        }

        public override bool IsGDPRRequired()
        {
            return false;
        }
    }

#if MODULE_ADMOB
    public class AdMobHandler : AdvertisingHandler
    {
        private BannerView bannerView;
        private InterstitialAd interstitial;
        private RewardBasedVideoAd rewardBasedVideo;

        private AdRequest adRequest;

        private bool isInitialized = false;

        public AdMobHandler(AdvertisingModules moduleType) : base(moduleType) { }

        public override void Init(AdsData adsSettings)
        {
            this.adsSettings = adsSettings;

            Debug.Log("[AdsManager]: AdMob is trying to initialize!");

            // Initialize the Google Mobile Ads SDK.
            MobileAds.Initialize(initStatus =>
            {
                CreateAdRequest();

                isInitialized = true;

                // Get singleton reward based video ad reference.
                rewardBasedVideo = RewardBasedVideoAd.Instance;

                // RewardBasedVideoAd is a singleton, so handlers should only be registered once.
                rewardBasedVideo.OnAdLoaded += HandleRewardBasedVideoLoaded;
                rewardBasedVideo.OnAdFailedToLoad += HandleRewardBasedVideoFailedToLoad;
                rewardBasedVideo.OnAdOpening += HandleRewardBasedVideoOpened;
                rewardBasedVideo.OnAdStarted += HandleRewardBasedVideoStarted;
                rewardBasedVideo.OnAdRewarded += HandleRewardBasedVideoRewarded;
                rewardBasedVideo.OnAdClosed += HandleRewardBasedVideoClosed;
                rewardBasedVideo.OnAdLeavingApplication += HandleRewardBasedVideoLeftApplication;

                RequestRewardedVideo();

                Debug.Log("[AdsManager]: AdMob is initialized!");
            });
        }

        public override void DestroyBanner()
        {
            if (bannerView != null)
                bannerView.Destroy();
        }

        public override void HideBanner()
        {
            if (bannerView != null)
                bannerView.Hide();
        }

        public override void RequestInterstitial()
        {
            if (!isInitialized)
            {
                Debug.LogWarning("[AdsManager]: AdMob isn't initialized.");

                return;
            }

            // Clean up interstitial ad before creating a new one.
            if (interstitial != null)
            {
                interstitial.Destroy();
            }

            // Create an interstitial.
            interstitial = new InterstitialAd(GetInterstitialID());

            // Register for ad events.
            interstitial.OnAdLoaded += HandleInterstitialLoaded;
            interstitial.OnAdFailedToLoad += HandleInterstitialFailedToLoad;
            interstitial.OnAdOpening += HandleInterstitialOpened;
            interstitial.OnAdClosed += HandleInterstitialClosed;
            interstitial.OnAdLeavingApplication += HandleInterstitialLeftApplication;

            // Load an interstitial ad.
            interstitial.LoadAd(adRequest);
        }

        public override void RequestRewardedVideo()
        {
            if (!isInitialized)
            {
                Debug.LogWarning("[AdsManager]: AdMob isn't initialized.");

                return;
            }

            rewardBasedVideo.LoadAd(adRequest, GetRewardedVideoID());
        }

        public override void ShowBanner()
        {
            if (!isInitialized)
            {
                Debug.LogWarning("[AdsManager]: AdMob isn't initialized.");

                return;
            }

            // Clean up banner ad before creating a new one.
            if (bannerView != null)
            {
                bannerView.Destroy();
            }

            AdSize adSize = AdSize.Banner;

            switch (adsSettings.adMobContainer.bannerType)
            {
                case AdsData.AdMobContainer.BannerType.Banner:
                    adSize = AdSize.Banner;
                    break;
                case AdsData.AdMobContainer.BannerType.MediumRectangle:
                    adSize = AdSize.MediumRectangle;
                    break;
                case AdsData.AdMobContainer.BannerType.IABBanner:
                    adSize = AdSize.IABBanner;
                    break;
                case AdsData.AdMobContainer.BannerType.Leaderboard:
                    adSize = AdSize.Leaderboard;
                    break;
                case AdsData.AdMobContainer.BannerType.SmartBanner:
                    adSize = AdSize.SmartBanner;
                    break;
            }

            AdPosition adPosition = AdPosition.Bottom;
            switch (adsSettings.adMobContainer.bannerPosition)
            {
                case BannerPosition.Bottom:
                    adPosition = AdPosition.Bottom;
                    break;
                case BannerPosition.Top:
                    adPosition = AdPosition.Top;
                    break;
            }

            bannerView = new BannerView(GetBannerID(), adSize, adPosition);

            // Register for ad events.
            bannerView.OnAdLoaded += HandleAdLoaded;
            bannerView.OnAdFailedToLoad += HandleAdFailedToLoad;
            bannerView.OnAdOpening += HandleAdOpened;
            bannerView.OnAdClosed += HandleAdClosed;
            bannerView.OnAdLeavingApplication += HandleAdLeftApplication;

            // Load a banner ad.
            bannerView.LoadAd(adRequest);
        }

        public override void ShowInterstitial()
        {
            if (!isInitialized)
            {
                Debug.LogWarning("[AdsManager]: AdMob isn't initialized.");

                return;
            }

            interstitial.Show();
        }

        public override void ShowRewardedVideo(RewardedVideoCallback callback)
        {
            if (!isInitialized)
            {
                Debug.LogWarning("[AdsManager]: AdMob isn't initialized.");

                return;
            }

            if (rewardedVideoCallback != null)
                rewardedVideoCallback = null;

            rewardedVideoCallback = callback;

            rewardBasedVideo.Show();
        }

        public override bool IsInterstitialLoaded()
        {
            return interstitial != null && interstitial.IsLoaded();
        }

        public override bool IsRewardedVideoLoaded()
        {
            return rewardBasedVideo != null && rewardBasedVideo.IsLoaded();
        }

        public override void SetGDPR(bool state)
        {
            CreateAdRequest();
        }
    
        public override bool IsGDPRRequired()
        {
            return true;
        }

        public void CreateAdRequest()
        {
            AdRequest.Builder builder = new AdRequest.Builder();

            if(adsSettings.testMode)
                builder = builder.AddTestDevice("*");

            builder = builder.AddExtra("npa", GDPRController.GetGDPRState() ? "1" : "0");

            adRequest = builder.Build();
        }

    #region Banner callback handlers
        public void HandleAdLoaded(object sender, EventArgs args)
        {
            Debug.Log("[AdsManager]: HandleAdLoaded event received");
        }

        public void HandleAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
        {
            Debug.Log("[AdsManager]: HandleFailedToReceiveAd event received with message: " + args.Message);
        }

        public void HandleAdOpened(object sender, EventArgs args)
        {
            Debug.Log("[AdsManager]: HandleAdOpened event received");
        }

        public void HandleAdClosed(object sender, EventArgs args)
        {
            Debug.Log("[AdsManager]: HandleAdClosed event received");
        }

        public void HandleAdLeftApplication(object sender, EventArgs args)
        {
            Debug.Log("[AdsManager]: HandleAdLeftApplication event received");
        }
    #endregion

    #region Interstitial callback handlers
        public void HandleInterstitialLoaded(object sender, EventArgs args)
        {
            Debug.Log("[AdsManager]: HandleInterstitialLoaded event received");
        }

        public void HandleInterstitialFailedToLoad(object sender, AdFailedToLoadEventArgs args)
        {
            Debug.Log("[AdsManager]: HandleInterstitialFailedToLoad event received with message: " + args.Message);
        }

        public void HandleInterstitialOpened(object sender, EventArgs args)
        {
            Debug.Log("[AdsManager]: HandleInterstitialOpened event received");
        }

        public void HandleInterstitialClosed(object sender, EventArgs args)
        {
            Debug.Log("[AdsManager]: HandleInterstitialClosed event received");
        }

        public void HandleInterstitialLeftApplication(object sender, EventArgs args)
        {
            Debug.Log("[AdsManager]: HandleInterstitialLeftApplication event received");
        }
    #endregion

    #region RewardedVideo callback handlers
        public void HandleRewardBasedVideoLoaded(object sender, EventArgs args)
        {
            Debug.Log("[AdsManager]: HandleRewardBasedVideoLoaded event received");
        }

        public void HandleRewardBasedVideoFailedToLoad(object sender, AdFailedToLoadEventArgs args)
        {
            if (rewardedVideoCallback != null)
            {
                rewardedVideoCallback.Invoke(false);

                rewardedVideoCallback = null;
            }

            Debug.Log("[AdsManager]: HandleRewardBasedVideoFailedToLoad event received with message: " + args.Message);
        }

        public void HandleRewardBasedVideoOpened(object sender, EventArgs args)
        {
            Debug.Log("[AdsManager]: HandleRewardBasedVideoOpened event received");
        }

        public void HandleRewardBasedVideoStarted(object sender, EventArgs args)
        {
            Debug.Log("[AdsManager]: HandleRewardBasedVideoStarted event received");
        }

        public void HandleRewardBasedVideoClosed(object sender, EventArgs args)
        {
            if (rewardedVideoCallback != null)
            {
                rewardedVideoCallback.Invoke(false);

                rewardedVideoCallback = null;
            }

            Debug.Log("[AdsManager]: HandleRewardBasedVideoClosed event received");
        }

        public void HandleRewardBasedVideoRewarded(object sender, Reward args)
        {
            if (rewardedVideoCallback != null)
            {
                rewardedVideoCallback.Invoke(true);

                rewardedVideoCallback = null;
            }

            string type = args.Type;
            double amount = args.Amount;

            Debug.Log("[AdsManager]: HandleRewardBasedVideoRewarded event received for " + amount.ToString() + " " + type);
        }

        public void HandleRewardBasedVideoLeftApplication(object sender, EventArgs args)
        {
            Debug.Log("[AdsManager]: HandleRewardBasedVideoLeftApplication event received");
        }
    #endregion
        
        public string GetBannerID()
        {
#if UNITY_EDITOR
            return "unused";
#elif UNITY_ANDROID
            return adsSettings.adMobContainer.androidBannerID;
#elif UNITY_IOS
            return adsSettings.adMobContainer.IOSBannerID;
#else
            return "unexpected_platform";
#endif
        }

        public string GetInterstitialID()
        {
#if UNITY_EDITOR
            return "unused";
#elif UNITY_ANDROID
            return adsSettings.adMobContainer.androidInterstitialID;
#elif UNITY_IOS
            return adsSettings.adMobContainer.IOSInterstitialID;
#else
            return "unexpected_platform";
#endif
        }

        public string GetRewardedVideoID()
        {
#if UNITY_EDITOR
            return "unused";
#elif UNITY_ANDROID
            return adsSettings.adMobContainer.androidRewardedVideoID;
#elif UNITY_IOS
            return adsSettings.adMobContainer.IOSRewardedVideoID;
#else
            return "unexpected_platform";
#endif
        }
    }
#endif

#if MODULE_UNITYADS
    public class UnityAdsHandler : AdvertisingHandler, IUnityAdsListener
    {
        public UnityAdsHandler(AdvertisingModules moduleType) : base(moduleType) { }

        public override void Init(AdsData adsSettings)
        {
            this.adsSettings = adsSettings;

            string unityAdsAppID = GetUnityAdsAppID();

            Advertisement.AddListener(this);
            Advertisement.Initialize(unityAdsAppID, adsSettings.testMode);

            Advertisement.Banner.SetPosition((UnityEngine.Advertisements.BannerPosition)adsSettings.unityAdsContainer.bannerPosition);

            Debug.Log("[AdsManager]: Unity Ads initialized: " + Advertisement.isInitialized);
            Debug.Log("[AdsManager]: Unity Ads is supported: " + Advertisement.isSupported);
            Debug.Log("[AdsManager]: Unity Ads test mode enabled: " + Advertisement.debugMode);
            Debug.Log("[AdsManager]: Unity Ads version: " + Advertisement.version);
        }

        public override void DestroyBanner()
        {
            Advertisement.Banner.Hide(true);
        }

        public override void HideBanner()
        {
            Advertisement.Banner.Hide(false);
        }

        public override bool IsGDPRRequired()
        {
            return true;
        }

        public override void RequestInterstitial()
        {
            Debug.Log("[AdsManager]: Unity Ads has auto interstitial caching");
        }

        public override void RequestRewardedVideo()
        {
            Debug.Log("[AdsManager]: Unity Ads has auto video caching");
        }

        public override void ShowBanner()
        {
            Advertisement.Banner.Show(GetUnityAdsBannerID());
        }

        public override void ShowInterstitial()
        {
            Advertisement.Show(GetUnityAdsInterstitialID());
        }

        public override void ShowRewardedVideo(RewardedVideoCallback callback)
        {
            if (rewardedVideoCallback != null)
                rewardedVideoCallback = null;

            rewardedVideoCallback = callback;

            Advertisement.Show(GetUnityAdsRewardedVideoID());
        }

        public override bool IsInterstitialLoaded()
        {
#if UNITY_EDITOR
            // Requires to show Unity Ads dummy
            return true;
#else
            return Advertisement.IsReady(GetUnityAdsInterstitialID());
#endif
        }

        public override bool IsRewardedVideoLoaded()
        {
#if UNITY_EDITOR
            // Requires to show Unity Ads dummy
            return true;
#else
            return Advertisement.IsReady(GetUnityAdsRewardedVideoID());
#endif
        }

        public string GetUnityAdsAppID()
        {
#if UNITY_ANDROID
            return adsSettings.unityAdsContainer.androidAppID;
#elif UNITY_IOS
            return adsSettings.unityAdsContainer.IOSAppID;
#else
            return "unexpected_platform";
#endif
        }

        public string GetUnityAdsBannerID()
        {
#if UNITY_EDITOR
            return "unused";
#elif UNITY_ANDROID
            return adsSettings.unityAdsContainer.androidBannerID;
#elif UNITY_IOS
            return adsSettings.unityAdsContainer.IOSBannerID;
#else
            return "unexpected_platform";
#endif
        }

        public string GetUnityAdsInterstitialID()
        {
#if UNITY_EDITOR
            return "unused";
#elif UNITY_ANDROID
            return adsSettings.unityAdsContainer.androidInterstitialID;
#elif UNITY_IOS
            return adsSettings.unityAdsContainer.IOSInterstitialID;
#else
            return "unexpected_platform";
#endif
        }

        public string GetUnityAdsRewardedVideoID()
        {
#if UNITY_EDITOR
            return "unused";
#elif UNITY_ANDROID
            return adsSettings.unityAdsContainer.androidRewardedVideoID;
#elif UNITY_IOS
            return adsSettings.unityAdsContainer.IOSRewardedVideoID;
#else
            return "unexpected_platform";
#endif
        }

        public void OnUnityAdsReady(string placementId)
        {
            Debug.Log("[AdsManager]: OnUnityAdsReady - " + placementId);
        }

        public void OnUnityAdsDidError(string message)
        {
            Debug.Log("[AdsManager]: OnUnityAdsDidError - " + message);
        }

        public void OnUnityAdsDidStart(string placementId)
        {
            Debug.Log("[AdsManager]: OnUnityAdsDidStart - " + placementId);
        }

        public void OnUnityAdsDidFinish(string placementId, UnityEngine.Advertisements.ShowResult showResult)
        {
            Debug.Log("[AdsManager]: OnUnityAdsDidFinish - " + placementId + ". Result - " + showResult);

            bool state = showResult == UnityEngine.Advertisements.ShowResult.Finished;
            
            // Reward the player
            if (rewardedVideoCallback != null)
            {
                rewardedVideoCallback.Invoke(state);

                rewardedVideoCallback = null;
            }
        }
    }
#endif

#if MODULE_MAX
    public class MAXHandler : AdvertisingHandler
    {
        public MAXHandler(AdvertisingModules moduleType) : base(moduleType) { }

        public override void Init(AdsData adsSettings)
        {
            this.adsSettings = adsSettings;

            // Base
            MaxSdkCallbacks.OnSdkInitializedEvent += OnSdkInitialized;

            // Interstitial
            MaxSdkCallbacks.OnInterstitialAdFailedToDisplayEvent += OnInterstitialAdFailedToDisplayEvent;
            MaxSdkCallbacks.OnInterstitialClickedEvent += OnInterstitialClickedEvent;
            MaxSdkCallbacks.OnInterstitialDisplayedEvent += OnInterstitialDisplayedEvent;
            MaxSdkCallbacks.OnInterstitialHiddenEvent += OnInterstitialHiddenEvent;
            MaxSdkCallbacks.OnInterstitialLoadedEvent += OnInterstitialLoadedEvent;
            MaxSdkCallbacks.OnInterstitialLoadFailedEvent += OnInterstitialLoadFailedEvent;

            // Rewarded Video
            MaxSdkCallbacks.OnRewardedAdClickedEvent += OnRewardedAdClickedEvent;
            MaxSdkCallbacks.OnRewardedAdDisplayedEvent += OnRewardedAdDisplayedEvent;
            MaxSdkCallbacks.OnRewardedAdFailedToDisplayEvent += OnRewardedAdFailedToDisplayEvent;
            MaxSdkCallbacks.OnRewardedAdHiddenEvent += OnRewardedAdHiddenEvent;
            MaxSdkCallbacks.OnRewardedAdLoadedEvent += OnRewardedAdLoadedEvent;
            MaxSdkCallbacks.OnRewardedAdLoadFailedEvent += OnRewardedAdLoadFailedEvent;
            MaxSdkCallbacks.OnRewardedAdReceivedRewardEvent += OnRewardedAdReceivedRewardEvent;

            MaxSdk.SetSdkKey(GetSDKKey());
            MaxSdk.InitializeSdk();

            Debug.Log("MAX INIT");
        }

        #region Callbacks
        private void OnSdkInitialized(MaxSdkBase.SdkConfiguration configuration)
        {
            string bannerID = GetBannerID();

            MaxSdk.CreateBanner(bannerID, MaxSdkBase.BannerPosition.BottomCenter);
            MaxSdk.SetBannerBackgroundColor(bannerID, Color.black);

            RequestInterstitial();
            RequestRewardedVideo();

            if (configuration.ConsentDialogState == MaxSdkBase.ConsentDialogState.Applies)
            {
                //if (!GDPRController.IsGDPRStateExist())
                //{
                //    SceneLoader.LoadScene("GDPR");
                //}
                //else
                //{
                //    SceneLoader.LoadScene("Game");
                //}
            }
            else if (configuration.ConsentDialogState == MaxSdkBase.ConsentDialogState.DoesNotApply)
            {
                //SceneLoader.LoadScene("Game");
            }
            else
            {
                //if (!GDPRController.IsGDPRStateExist())
                //{
                //    SceneLoader.LoadScene("GDPR");
                //}
                //else
                //{
                //    SceneLoader.LoadScene("Game");
                //}
            }
        }

        private void OnInterstitialLoadFailedEvent(string arg1, int arg2)
        {
            Debug.Log("OnInterstitialLoadFailedEvent");

            Tween.DelayedCall(3, RequestInterstitial, true, TweenType.Update);
        }

        private void OnInterstitialLoadedEvent(string obj)
        {
            Debug.Log("OnInterstitialLoadedEvent");
        }

        private void OnInterstitialHiddenEvent(string obj)
        {
            Debug.Log("OnInterstitialHiddenEvent");

            RequestInterstitial();
        }

        private void OnInterstitialDisplayedEvent(string obj)
        {
            Debug.Log("OnInterstitialDisplayedEvent");
        }

        private void OnInterstitialClickedEvent(string obj)
        {
            Debug.Log("OnInterstitialClickedEvent");
        }

        private void OnInterstitialAdFailedToDisplayEvent(string arg1, int arg2)
        {
            Debug.Log("OnInterstitialAdFailedToDisplayEvent");

            RequestInterstitial();
        }

        private void OnRewardedAdReceivedRewardEvent(string arg1, MaxSdkBase.Reward arg2)
        {
            Debug.Log("OnRewardedAdReceivedRewardEvent");

            // Reward the player
            if (rewardedVideoCallback != null)
            {
                RewardedVideoCallback videoCallbackTemp = rewardedVideoCallback;

                videoCallbackTemp.Invoke(true);

                rewardedVideoCallback = null;
            }
        }

        private void OnRewardedAdLoadFailedEvent(string arg1, int arg2)
        {
            Debug.Log("OnRewardedAdLoadFailedEvent");

            Tween.DelayedCall(3, RequestRewardedVideo, true, TweenType.Update);
        }

        private void OnRewardedAdLoadedEvent(string obj)
        {
            Debug.Log("OnRewardedAdLoadedEvent");
        }

        private void OnRewardedAdHiddenEvent(string obj)
        {
            Debug.Log("OnRewardedAdHiddenEvent");

            RequestRewardedVideo();
        }

        private void OnRewardedAdFailedToDisplayEvent(string arg1, int arg2)
        {
            Debug.Log("OnRewardedAdFailedToDisplayEvent");

            if (rewardedVideoCallback != null)
            {
                RewardedVideoCallback videoCallbackTemp = rewardedVideoCallback;

                videoCallbackTemp.Invoke(false);

                rewardedVideoCallback = null;
            }
        }

        private void OnRewardedAdDisplayedEvent(string obj)
        {
            Debug.Log("OnRewardedAdDisplayedEvent");
        }

        private void OnRewardedAdClickedEvent(string obj)
        {
            Debug.Log("OnRewardedAdClickedEvent");
        }
        #endregion

        public override void DestroyBanner()
        {
            MaxSdk.DestroyBanner(GetBannerID());
        }

        public override void HideBanner()
        {
            MaxSdk.HideBanner(GetBannerID());
        }

        public override void RequestInterstitial()
        {
            MaxSdk.LoadInterstitial(GetInterstitialID());
        }

        public override void RequestRewardedVideo()
        {
            MaxSdk.LoadRewardedAd(GetRewardedVideoID());
        }

        public override void ShowBanner()
        {
            MaxSdk.ShowBanner(GetBannerID());
        }

        public override void ShowInterstitial()
        {
            MaxSdk.ShowInterstitial(GetInterstitialID());
        }

        public override void ShowRewardedVideo(RewardedVideoCallback callback)
        {
            if (rewardedVideoCallback != null)
                rewardedVideoCallback = null;

            rewardedVideoCallback = callback;

            MaxSdk.ShowRewardedAd(GetRewardedVideoID());
        }

        public override bool IsInterstitialLoaded()
        {
            return MaxSdk.IsInterstitialReady(GetInterstitialID());
        }

        public override bool IsRewardedVideoLoaded()
        {
            return MaxSdk.IsRewardedAdReady(GetRewardedVideoID());
        }

        public string GetSDKKey()
        {
#if UNITY_ANDROID
            return adsSettings.maxContainer.androidSDKKey;
#elif UNITY_IOS
            return adsSettings.maxContainer.iosSDKKey;
#else
            return "unexpected_platform";
#endif
        }

        public string GetBannerID()
        {
#if UNITY_EDITOR
            return "unused";
#elif UNITY_ANDROID
            return adsSettings.maxContainer.androidBannerID;
#elif UNITY_IOS
            return adsSettings.maxContainer.iosBannerID;
#else
            return "unexpected_platform";
#endif
        }

        public string GetInterstitialID()
        {
#if UNITY_EDITOR
            return "unused";
#elif UNITY_ANDROID
            return adsSettings.maxContainer.androidInterstitialID;
#elif UNITY_IOS
            return adsSettings.maxContainer.iosInterstitialID;
#else
            return "unexpected_platform";
#endif
        }

        public string GetRewardedVideoID()
        {
#if UNITY_EDITOR
            return "unused";
#elif UNITY_ANDROID
            return adsSettings.maxContainer.androidRewardedVideoID;
#elif UNITY_IOS
            return adsSettings.maxContainer.iosRewardedVideoID;
#else
            return "unexpected_platform";
#endif
        }

        public override bool IsGDPRRequired()
        {
            if (!MaxSdk.IsInitialized())
                return true;

            switch (MaxSdk.GetConsentDialogState())
            {
                case MaxSdkBase.ConsentDialogState.Applies:
                    return true;
                    break;
                case MaxSdkBase.ConsentDialogState.DoesNotApply:
                    return false;
                    break;
            }

            return true;
        }

        public override void SetGDPR(bool state)
        {
            MaxSdk.SetHasUserConsent(state);
        }
    }
#endif

    public enum AdvertisingModules
    {
        Disable = 0,
        Dummy = 1,
        AdMob = 2,
        UnityAds = 3,
        MAX = 4
    }
}