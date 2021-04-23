#pragma warning disable 0649

using UnityEngine;

namespace Watermelon
{
    public class AdsDummyController : MonoBehaviour
    {
        [SerializeField]
        private GameObject bannerObject;

        [Space]
        [SerializeField]
        private GameObject interstitialObject;

        [Space]
        [SerializeField]
        private GameObject rewardedVideoObject;

        private RectTransform bannerRectTransform;

        private AdvertisingHandler.RewardedVideoCallback rewardedVideoCallback;

        private void Awake()
        {
            bannerRectTransform = (RectTransform)bannerObject.transform;
        }

        public void Init(AdsData settings)
        {
            switch (settings.dummyBanner)
            {
                case BannerPosition.Bottom:
                    bannerRectTransform.pivot = new Vector2(0.5f, 0.0f);

                    bannerRectTransform.anchorMin = new Vector2(0.0f, 0.0f);
                    bannerRectTransform.anchorMax = new Vector2(1.0f, 0.0f);

                    bannerRectTransform.anchoredPosition = Vector2.zero;
                    break;
                case BannerPosition.Top:
                    bannerRectTransform.pivot = new Vector2(0.5f, 1.0f);

                    bannerRectTransform.anchorMin = new Vector2(0.0f, 1.0f);
                    bannerRectTransform.anchorMax = new Vector2(1.0f, 1.0f);

                    bannerRectTransform.anchoredPosition = Vector2.zero;
                    break;
            }
        }

        public void SetRewardedVideoCallback(AdvertisingHandler.RewardedVideoCallback rewardedVideoCallback)
        {
            this.rewardedVideoCallback = rewardedVideoCallback;
        }

        public void ShowBanner()
        {
            bannerObject.SetActive(true);
        }

        public void HideBanner()
        {
            bannerObject.SetActive(false);
        }

        public void ShowInterstitial()
        {
            interstitialObject.SetActive(true);
        }

        public void CloseInterstitial()
        {
            interstitialObject.SetActive(false);
        }

        public void ShowRewardedVideo()
        {
            rewardedVideoObject.SetActive(true);
        }

        public void CloseRewardedVideo()
        {
            rewardedVideoObject.SetActive(false);
        }

        #region Buttons
        public void CloseInterstitialButton()
        {
            CloseInterstitial();
        }

        public void CloseRewardedVideoButton()
        {
            if (rewardedVideoCallback != null)
            {
                rewardedVideoCallback.Invoke(false);

                rewardedVideoCallback = null;
            }

            CloseRewardedVideo();
        }

        public void GetRewardButton()
        {
            if (rewardedVideoCallback != null)
            {
                rewardedVideoCallback.Invoke(true);

                rewardedVideoCallback = null;
            }

            CloseRewardedVideo();
        }
        #endregion
    }
}