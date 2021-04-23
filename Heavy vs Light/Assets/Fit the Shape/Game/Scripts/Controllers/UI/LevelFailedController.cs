using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Watermelon;

public class LevelFailedController : MonoBehaviour
{
    public Watermelon.AudioSettings audioSettings;
    public GameSettings gameSettings;

    public CanvasGroup revive;
    public Image reviveFill;
    public Image reviveHeart;

    [Space]
    public Text continueButtonText;
    public Text continueHeaderText;
    public Text watchAdText;

    [Space]
    public CanvasGroup uiBackground;

    [Space]
    public GameObject levelFailed;
    public Image failedOutline;
    public Image failedBody;
    public Text failedText;
    public Text restartText;
    public CanvasGroup failedNext;

    private bool isRevive = false;
    private bool reward = false;

    private void Start()
    {
        continueButtonText.text = Multilanguage.GetWord("revive.continue_button");
        continueHeaderText.text = Multilanguage.GetWord("revive.continue_header");
        watchAdText.text = Multilanguage.GetWord("revive.watch_ad");

        failedText.text = Multilanguage.GetWord("level_failed.header");
        restartText.text = Multilanguage.GetWord("level_failed.restart");
    }

    public void Show()
    {
        reward = false;
        isRevive = true;
        uiBackground.gameObject.SetActive(true);
        uiBackground.DOFade(1, 0.5f);
        revive.gameObject.SetActive(true);
        revive.DOFade(1, 0.5f);
        reviveFill.fillAmount = 1;
        StartCoroutine(HeartBeat());
    }

    void Update()
    {
        if (isRevive)
        {
            if (!reward)
            {
                reviveFill.fillAmount -= Time.deltaTime  / gameSettings.reviveCountdownTime;
                if (reviveFill.fillAmount <= 0)
                {
                    isRevive = false;
                    ShowLevelFailed();
                }
            }
        }
    }

    private void ShowLevelFailed()
    {
        revive.DOFade(0, 0.5f).OnComplete(delegate
        {
            revive.gameObject.SetActive(false);
            levelFailed.SetActive(true);
            StartCoroutine(LevelFailedBannerInit());
        });
    }

    IEnumerator LevelFailedBannerInit()
    {
        while (failedOutline.fillAmount < 1)
        {
            failedOutline.fillAmount += Time.deltaTime * 5;
            yield return new WaitForFixedUpdate();
        }
        failedText.gameObject.SetActive(true);
        failedText.DOFade(1, 0.01f);
        while (failedBody.fillAmount < 1)
        {
            failedBody.fillAmount += Time.deltaTime * 5;
            yield return new WaitForFixedUpdate();
        }
        failedNext.gameObject.SetActive(true);
        failedNext.DOFade(1, 0.5f);
    }

    IEnumerator HideLevelFailed()
    {
        failedText.DOFade(0, 0.1f);
        failedText.gameObject.SetActive(false);
        while (failedOutline.fillAmount > 0)
        {
            failedOutline.fillAmount -= Time.deltaTime * 5;
            failedBody.fillAmount -= Time.deltaTime * 5;
            yield return new WaitForFixedUpdate();
        }
        failedNext.DOFade(0, 0.5f).OnComplete(delegate
        {
            failedNext.gameObject.SetActive(false);
            levelFailed.SetActive(false);
            GameController.instance.NextLevel(true);
        });
        uiBackground.DOFade(0, 0.5f).OnComplete(delegate
        {
            uiBackground.gameObject.SetActive(false);
        });
    }

    IEnumerator HeartBeat()
    {
        while (isRevive)
        {
            reviveHeart.transform.DOScale(new Vector3(1.1f, 1.1f, 0), 0.1f);
            yield return new WaitForSeconds(0.1f);
            reviveHeart.transform.DOScale(new Vector3(1f, 1f, 0), 0.1f);
            yield return new WaitForSeconds(0.2f);
            reviveHeart.transform.DOScale(new Vector3(1.1f, 1.1f, 0), 0.1f);
            yield return new WaitForSeconds(0.1f);
            reviveHeart.transform.DOScale(new Vector3(1f, 1f, 0), 0.1f);
            yield return new WaitForSeconds(0.5f);
        }
    }

    public void OnNextClick()
    {
        if (GameController.instance.isSound)
        {
            AudioController.PlaySound(audioSettings.sounds.button, AudioController.AudioType.Sound, 0.8f, 1.2f);
        }
        StartCoroutine(HideLevelFailed());
    }

    public void OnContinueClick()
    {
        if (GameController.instance.isSound)
        {
            AudioController.PlaySound(audioSettings.sounds.button, AudioController.AudioType.Sound, 0.8f, 1.2f);
        }
        if (AdsManager.IsRewardBasedVideoLoaded(AdsManager.Settings.rewardedVideoType))
        {
            AdsManager.ShowRewardBasedVideo(AdsManager.Settings.rewardedVideoType, (hasReward) =>
            {
                if (hasReward)
                {
                    revive.DOFade(0, 0.5f).OnComplete(delegate
                    {
                        revive.gameObject.SetActive(false);
                        GameController.instance.Revive();
                    });
                    uiBackground.DOFade(0, 0.5f);
                    reward = true;
                }

                else
                {

                }
            });
        }
        else
        {

        }
    }
}
