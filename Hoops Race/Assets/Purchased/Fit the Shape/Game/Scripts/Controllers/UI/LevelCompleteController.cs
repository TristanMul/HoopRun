#pragma warning disable 649

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Watermelon;

public class LevelCompleteController : MonoBehaviour
{
    public Watermelon.AudioSettings audioSettings;
    [Space]
    [SerializeField]
    LevelDatabase levelDatabase;
    public GameSettings gameSettings;

    [Space]
    [SerializeField]
    private Image completeOutline;
    [SerializeField]
    private Image completeBody;
    [SerializeField]
    private Text completeText;

    [Space]
    [SerializeField]
    private CanvasGroup baitsContainer;

    [Space]
    [SerializeField]
    private CanvasGroup completeReward;
    [SerializeField]
    private Image completeRewardBackground;
    [SerializeField]
    private Text completeRewardText;

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
    public BaitBlock[] baitBlocks;

    public delegate void OnClose();

    private bool isGrowing = true;
    private bool isScaling = false;
    private bool hasReward = false;
    private bool isOpening = false;

    private float degree;
    private int count;

    private void Start()
    {
        completeText.text = Multilanguage.GetWord("level_complete.header");
        nextButtonAdText.text = Multilanguage.GetWord("level_complete.watch_ad");
    }

    private void FixedUpdate()
    {
        if (isOpening)
        {
            var easing = Ease.GetFunction(Ease.Type.SineInOut);
            degree += Time.deltaTime * 0.5f;
            if (degree > 1)
            {
                degree -= 1;
                count++;
            }
            var easingDegree = count + degree; //easing(degree);
            completeRewardBackground.transform.localEulerAngles = new Vector3(0, 0, easingDegree * 20);
            if (degree > 360)
                degree -= 360;
        }

        if (!isScaling)
        {
            isScaling = true;
            if (isGrowing)
            {
                completeRewardBackground.transform.DOScale(1.1f, 0.5f).SetEasing(Ease.Type.BackInOut).OnComplete(delegate
                {
                    isScaling = false;
                    isGrowing = false;
                });
            }
            else
            {
                completeRewardBackground.transform.DOScale(1f, 0.5f).SetEasing(Ease.Type.BackInOut).OnComplete(delegate
                {
                    isScaling = false;
                    isGrowing = true;
                });
            }
        }
    }

    public void Hide()
    {
        isOpening = false;
        StartCoroutine(HideLevelComplete());
    }

    public void Show()
    {
        degree = 0;
        count = 0;
        isOpening = true;
        isGrowing = true;
        isScaling = false;
        completeRewardBackground.transform.localScale = new Vector3(1, 1, 1);
        gameObject.SetActive(true);
        StartCoroutine(InitLevelComplete());
    }

    private IEnumerator HideLevelComplete()
    {
        completeText.DOFade(0, 0.1f);
        completeText.gameObject.SetActive(false);
        while (completeOutline.fillAmount > 0)
        {
            completeOutline.fillAmount -= Time.deltaTime * 5;
            completeBody.fillAmount -= Time.deltaTime * 5;
            yield return new WaitForFixedUpdate();
        }
        baitsContainer.DOFade(0, 0.5f);
        completeReward.DOFade(0, 0.5f);
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

    private IEnumerator InitLevelComplete()
    {
        completeText.DOFade(1f, 0.01f);
        while (completeOutline.fillAmount < 1)
        {
            completeOutline.fillAmount += Time.deltaTime * 5;
            yield return new WaitForFixedUpdate();
        }
        yield return new WaitForSeconds(0.2f);
        completeText.gameObject.SetActive(true);
        while (completeBody.fillAmount < 1)
        {
            completeBody.fillAmount += Time.deltaTime * 5;
            yield return new WaitForFixedUpdate();
        }
        InitBaitContainer();

        if (GameController.instance.catchedBait)
        {
            completeReward.DOFade(1, 0.5f);
            completeRewardText.gameObject.SetActive(true);
            completeRewardText.DOFade(1, 0.1f);
            var jelliesIndex = JelliseIndex(GameController.instance.currentLevel);
            if (jelliesIndex % 3 == 2 && PlayerPrefs.HasKey(GetRealIndex(jelliesIndex - 1) + "_level") && PlayerPrefs.HasKey(GetRealIndex(jelliesIndex - 2) + "_level"))
            {
                completeRewardText.text = "+" + gameSettings.megaBaitReward;
                GameController.instance.AddGems(gameSettings.megaBaitReward);
            }
            else
            {
                completeRewardText.text = "+" + gameSettings.baitReward;
                GameController.instance.AddGems(gameSettings.baitReward);
            }

            nextButtonText.text = Multilanguage.GetWord("level_complete.claim") + " X" + gameSettings.adsRewardMultiplier;
        }
        else
        {
            nextButtonText.text = "+" + gameSettings.missedBaitAdReward;
        }

        nextButton.DOFade(1, 0.5f).OnComplete(delegate
        {
            StartCoroutine(InitLevelCompleteNextButton());
        });
    }

    private int GetRealIndex(int index)
    {
        if (index >= levelDatabase.jellyRushes.Count)
            return -1;
        for (int j = 0; j < levelDatabase.levels.Length; j++)
        {
            if (levelDatabase.levels[j] == levelDatabase.jellyRushes[index])
            {
                return j;
            }
        }
        return 0;
    }

    private int JelliseIndex(int index)
    {
        return levelDatabase.jellyRushes.IndexOf(levelDatabase.levels[GameController.instance.currentLevel]);
    }

    private void InitBaitContainer()
    {

        var animIndex = 0;
        var currentIndex = JelliseIndex(GameController.instance.currentLevel);
        for (int i = 0; i < baitBlocks.Length; i++)
        {
            var index = currentIndex - currentIndex % 3 + i;
            var realIndex = GetRealIndex(index);

            baitBlocks[i].baitBackground.color = new Color(0.89019607843f, 0.89019607843f, 0.89019607843f, 1f);
            baitBlocks[i].baitIcon.gameObject.SetActive(false);
            baitBlocks[i].checkmark.gameObject.SetActive(false);
            if (index < levelDatabase.jellyRushes.Count)
            {
                bool catched = PlayerPrefs.HasKey(realIndex + "_level");

                baitBlocks[i].baitIcon.overrideSprite = levelDatabase.jellyRushes[index].bait.texture;
                baitBlocks[i].baitLockedIcon.overrideSprite = levelDatabase.jellyRushes[index].bait.lockedTexture;
                if (index > currentIndex)
                {
                    baitBlocks[i].baitIcon.gameObject.SetActive(false);
                    baitBlocks[i].baitIcon.color = new Color(0.684f, 0.684f, 0.684f, 1f);
                    baitBlocks[i].checkmark.gameObject.SetActive(false);
                }
                else if (index == currentIndex)
                {
                    animIndex = i;
                }
                else
                {
                    baitBlocks[i].baitBackground.color = GameController.instance.currentPreset.backgroundStart;
                    baitBlocks[i].baitIcon.gameObject.SetActive(true);
                    baitBlocks[i].baitIcon.color = Color.white;
                    baitBlocks[i].checkmark.gameObject.SetActive(catched);
                }
            }
            else
            {
                baitBlocks[i].baitIcon.gameObject.SetActive(false);
                baitBlocks[i].checkmark.gameObject.SetActive(false);
            }
        }
        var block = baitBlocks[animIndex];
        baitsContainer.DOFade(1, 0.5f).OnComplete(delegate
        {
            block.baitBackground.DOColor(GameController.instance.currentPreset.backgroundStart, 0.5f).OnComplete(delegate
            {
                block.baitIcon.gameObject.SetActive(true);
                block.baitIcon.color = Color.white;
                block.baitIcon.DOFade(0, 0.0001f).OnComplete(delegate
                {
                    block.baitIcon.DOFade(1, 0.5f).OnComplete(delegate
                    {
                        bool catched = PlayerPrefs.HasKey(GameController.instance.currentLevel + "_level");
                        if (catched)
                        {
                            block.checkmark.gameObject.SetActive(true);
                            block.checkmark.DOFade(0, 0.0001f).OnComplete(delegate
                            {
                                if (GameController.instance.isSound)
                                {
                                    AudioController.PlaySound(audioSettings.sounds.checkmark, AudioController.AudioType.Sound, 1f, 1.1f);
                                }
                                block.checkmark.DOFade(1, 0.5f);
                            });
                        }
                    });
                });

            });
        });

    }

    private IEnumerator InitLevelCompleteNextButton()
    {
        hasReward = false;
        nextButtonFill.fillAmount = 1;
        while (nextButtonFill.fillAmount > 0)
        {
            if (hasReward)
            {
                nextButtonFill.fillAmount = 0;
                break;
            }
            nextButtonFill.fillAmount -= Time.deltaTime / 3.5f;
            yield return new WaitForFixedUpdate();
        }
        nextButtonText.DOFade(0, 0.5f).OnComplete(delegate
        {
            nextButtonText.text = Multilanguage.GetWord("level_complete.next");
            nextButtonText.DOFade(1, 0.5f);
            nextButtonAdText.DOFade(0, 0.5f);
        });
    }

    public void NextButton()
    {
        if (GameController.instance.isSound)
        {
            AudioController.PlaySound(audioSettings.sounds.button, AudioController.AudioType.Sound, 0.8f, 1.2f);
        }
        if (nextButtonText.text == Multilanguage.GetWord("level_complete.next"))
        {
            Hide();
        }
        else
        {
            if (AdsManager.IsRewardBasedVideoLoaded(AdsManager.Settings.rewardedVideoType))
            {
                AdsManager.ShowRewardBasedVideo(AdsManager.Settings.rewardedVideoType, (hasReward) =>
                {
                    if (hasReward)
                    {
                        if (nextButtonText.text == "+" + gameSettings.missedBaitAdReward)
                        {
                            GameController.instance.AddGems(gameSettings.missedBaitAdReward);
                        }
                        else
                        {
                            GameController.instance.AddGems(int.Parse(completeRewardText.text) * (gameSettings.adsRewardMultiplier - 1));
                            completeRewardText.text = ("+" + int.Parse(completeRewardText.text) * gameSettings.adsRewardMultiplier).ToString();
                        }
                        this.hasReward = true;
                        completeReward.GetComponent<RectTransform>().DOScale(new Vector3(1.1f, 1.1f, 1.1f), 1).OnComplete(delegate
                        {
                            completeReward.GetComponent<RectTransform>().DOScale(new Vector3(1f, 1f, 1f), 1);
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

[System.Serializable]
public struct BaitBlock
{
    public Image baitIcon;
    public Image baitLockedIcon;
    public Image baitBackground;
    public Image checkmark;
}
