#pragma warning disable 0649

using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class AdsManagerExampleScript : MonoBehaviour
    {
        private Vector2 scrollView;

        [SerializeField]
        private Text logText;

        [Space]
        [SerializeField]
        private Text bannerTitleText;
        [SerializeField]
        private Button[] bannerButtons;

        [Space]
        [SerializeField]
        private Text interstitialTitleText;
        [SerializeField]
        private Button[] interstitialButtons;

        [Space]
        [SerializeField]
        private Text rewardVideoTitleText;
        [SerializeField]
        private Button[] rewardVideoButtons;

        private AdsData settings;

        private void Awake()
        {
            Application.logMessageReceived += Log;
        }

        private void OnDestroy()
        {
            Application.logMessageReceived -= Log;
        }

        private void Start()
        {
            settings = AdsManager.Settings;

            logText.text = string.Empty;

            bannerTitleText.text = string.Format("Banner ({0})", settings.bannerType.ToString());
            if(settings.bannerType == AdvertisingModules.Disable)
            {
                for(int i = 0; i < bannerButtons.Length; i++)
                {
                    bannerButtons[i].interactable = false; 
                }
            }

            interstitialTitleText.text = string.Format("Interstitial ({0})", settings.interstitialType.ToString());
            if (settings.interstitialType == AdvertisingModules.Disable)
            {
                for (int i = 0; i < interstitialButtons.Length; i++)
                {
                    interstitialButtons[i].interactable = false;
                }
            }

            rewardVideoTitleText.text = string.Format("Rewarded Video ({0})", settings.rewardedVideoType.ToString());
            if (settings.rewardedVideoType == AdvertisingModules.Disable)
            {
                for (int i = 0; i < rewardVideoButtons.Length; i++)
                {
                    rewardVideoButtons[i].interactable = false;
                }
            }
        }

        private void Log(string condition, string stackTrace, LogType type)
        {
            logText.text = logText.text.Insert(0, condition + "\n");
        }

        private void Log(string condition)
        {
            logText.text = logText.text.Insert(0, condition + "\n");
        }

        #region Buttons
        public void ShowBannerButton()
        {
            AdsManager.ShowBanner(settings.bannerType);
        }

        public void HideBannerButton()
        {
            AdsManager.HideBanner(settings.bannerType);
        }

        public void DestroyBannerButton()
        {
            AdsManager.DestroyBanner(settings.bannerType);
        }

        public void InterstitialStatusButton()
        {
            Log("[AdsManager]: Interstitial " + (AdsManager.IsInterstitialLoaded(settings.interstitialType) ? "is loaded" : "isn't loaded"));
        }

        public void RequestInterstitialButton()
        {
            AdsManager.RequestInterstitial(settings.interstitialType);
        }

        public void ShowInterstitialButton()
        {
            AdsManager.ShowInterstitial(settings.interstitialType);
        }

        public void RewardedVideoStatusButton()
        {
            Log("[AdsManager]: Rewarded video " + (AdsManager.IsRewardBasedVideoLoaded(settings.rewardedVideoType) ? "is loaded" : "isn't loaded"));
        }

        public void RequestRewardedVideoButton()
        {
            AdsManager.RequestRewardBasedVideo(settings.rewardedVideoType);
        }

        public void ShowRewardedVideoButton()
        {
            AdsManager.ShowRewardBasedVideo(settings.rewardedVideoType, (hasReward) =>
            {
                if(hasReward)
                {
                    Log("[AdsManager]: Reward are received");
                }
                else
                {
                    Log("[AdsManager]: Reward aren't received");
                }
            });
        }
        #endregion
    }
}