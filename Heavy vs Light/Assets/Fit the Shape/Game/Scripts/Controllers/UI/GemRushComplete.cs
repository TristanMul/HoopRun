#pragma warning disable 649

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Watermelon;

public class GemRushComplete : MonoBehaviour
{
    public Watermelon.AudioSettings audioSettings;
    public GameSettings gameSettings;
    [Space]
    [SerializeField]
    private Image rushOutline;
    [SerializeField]
    private Image rushBody;
    [SerializeField]
    private Text rushText;

    [SerializeField]
    private Text collectedText;

    [Space]
    [SerializeField]
    private CanvasGroup rushReward;
    [SerializeField]
    private Text rushRewardText;

    [Space]
    [SerializeField]
    private CanvasGroup nextButton;
    [SerializeField]
    private Image nextButtonFill;
    [SerializeField]
    private Text nextButtonText;
    [SerializeField]
    private Text nextButtonAdText;

    [Space]
    public CanvasGroup uiBackground;

    private bool reward = false;

    private void Start()
    {
        rushText.text = Multilanguage.GetWord("gem_rush_complete.gem_rush") + "\n" + Multilanguage.GetWord("gem_rush_complete.complete");
        nextButtonAdText.text = Multilanguage.GetWord("gem_rush_complete.watch_ad");
        collectedText.text = Multilanguage.GetWord("gem_rush_complete.collected");
    }

    public void Show(int gems)
    {
        gameObject.SetActive(true);
        StartCoroutine(InitGemRushComplete(gems));
    }

    private IEnumerator InitGemRushComplete(int gems)
    {
        rushRewardText.text = gems.ToString();
        nextButtonFill.fillAmount = 1;
        rushText.DOFade(1f, 0.01f);
        while (rushOutline.fillAmount < 1)
        {
            rushOutline.fillAmount += Time.deltaTime * 5;
            yield return new WaitForFixedUpdate();
        }
        yield return new WaitForSeconds(0.2f);
        rushText.gameObject.SetActive(true);
        while (rushBody.fillAmount < 1)
        {
            rushBody.fillAmount += Time.deltaTime * 5;
            yield return new WaitForFixedUpdate();
        }
        //InitBaitContainer();
        rushReward.DOFade(1, 0.5f);

        nextButton.DOFade(1, 0.5f);

        nextButtonText.text = Multilanguage.GetWord("gem_rush_complete.claim") + " X" + gameSettings.adsRewardMultiplier;

        nextButton.DOFade(1, 0.5f).OnComplete(delegate
        {

            StartCoroutine(InitGemRushCompleteNextButton());
        });
    }

    private IEnumerator InitGemRushCompleteNextButton()
    {
        reward = false;
        nextButtonFill.fillAmount = 1;
        while (nextButtonFill.fillAmount > 0)
        {
            if (reward)
            {
                nextButtonFill.fillAmount = 0;
                break;
            }
            nextButtonFill.fillAmount -= Time.deltaTime / 3f;
            yield return new WaitForFixedUpdate();
        }
        nextButtonText.DOFade(0, 0.5f).OnComplete(delegate
        {
            nextButtonText.text = Multilanguage.GetWord("gem_rush_complete.next");
            nextButtonText.DOFade(1, 0.5f);
            nextButtonAdText.DOFade(0, 0.5f);
        });
    }

    private IEnumerator HidGemRushComplete()
    {
        rushText.DOFade(0, 0.1f);
        rushText.gameObject.SetActive(false);
        while (rushOutline.fillAmount > 0)
        {
            rushOutline.fillAmount -= Time.deltaTime * 5;
            rushBody.fillAmount -= Time.deltaTime * 5;
            yield return new WaitForFixedUpdate();
        }
        rushReward.DOFade(0, 0.5f);
        nextButton.DOFade(0, 0.5f).OnComplete(delegate
        {
            nextButtonAdText.DOFade(1, 0.01f);
            nextButtonFill.fillAmount = 1;
            gameObject.SetActive(false);
            GameController.instance.NextLevel(false);
        });
        uiBackground.DOFade(0, 0.5f).OnComplete(delegate
        {
            uiBackground.gameObject.SetActive(false);
        });
    }

    public void NextButton()
    {
        if (GameController.instance.isSound)
        {
            AudioController.PlaySound(audioSettings.sounds.button, AudioController.AudioType.Sound, 0.8f, 1.2f);
        }
        if (nextButtonText.text == Multilanguage.GetWord("gem_rush_complete.next"))
        {
            StartCoroutine(HidGemRushComplete());
        }
        else
        {
            if (AdsManager.IsRewardBasedVideoLoaded(AdsManager.Settings.rewardedVideoType))
            {
                AdsManager.ShowRewardBasedVideo(AdsManager.Settings.rewardedVideoType, (hasReward) =>
                {

                    if (hasReward)
                    {
                        GameController.instance.AddGems(int.Parse(rushRewardText.text) * (gameSettings.adsRewardMultiplier - 1));
                        rushRewardText.text = (int.Parse(rushRewardText.text) * gameSettings.adsRewardMultiplier).ToString();
                        reward = true;
                        rushReward.GetComponent<RectTransform>().DOScale(new Vector3(1.1f, 1.1f, 1.1f), 1).OnComplete(delegate
                        {
                            rushReward.GetComponent<RectTransform>().DOScale(new Vector3(1f, 1f, 1f), 1);
                        });
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
}