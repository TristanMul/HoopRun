#pragma warning disable 0649 

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Watermelon
{
    public class GDPRController : MonoBehaviour
    {
        public GameObject panelObject;

        private const string PREFS_NAME = "GDPR";

        private void Awake()
        {
            if (AdsManager.Settings.gdprContainer.enableGDPR)
            {
                panelObject.SetActive(!IsGDPRStateExist());
            }
            else
            {
                panelObject.SetActive(false);
            }
        }

        public void OpenPrivacyLink()
        {
            Application.OpenURL(AdsManager.Settings.gdprContainer.privacyLink);
        }

        public void SetGDPRState(bool state)
        {
            if(AdsManager.IsInititalized)
            {
                AdsManager.SetGDPR(state);
            }
            else
            {
                AdsManager.Initialize(state);
            }

            PlayerPrefs.SetInt(PREFS_NAME, state ? 1 : 0);

            Close();
        }

        public void Open()
        {
            panelObject.SetActive(true);
        }

        public void Close()
        {
            panelObject.SetActive(false);
        }

        public static bool GetGDPRState()
        {
            if (PlayerPrefs.HasKey(PREFS_NAME))
            {
                return PlayerPrefs.GetInt(PREFS_NAME) == 1 ? true : false;
            }

            return false;
        }

        public static bool IsGDPRStateExist()
        {
            return PlayerPrefs.HasKey(PREFS_NAME);
        }
    }
}