using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Watermelon;

public class GameCanvasController : MonoBehaviour
{
    public CanvasGroup gameCanvas;

    public Image turboImage;
    public Text turboText;
    public CanvasGroup turboBar;
    public Text gemRushText;
    public CanvasGroup gemRushBar;
    public GameObject playerBar;
    public RectTransform baitIndicator;
    public RectTransform playerIndicator;

    public Image baitBackground;
    public Image baitImage;
    public Image playerBackground;
    public Image playerImage;

    private void Start()
    {
        turboText.text = Multilanguage.GetWord("game.turbo");
    }

    public void Play()
    {
        gameCanvas.gameObject.SetActive(true);
        gameCanvas.DOFade(1, 0.5f);
        if (GameController.instance.GetCurrentLevelType() == Level.LevelType.JELLY_RUSH)
        {
            baitImage.overrideSprite = GameController.instance.GetBait().texture;
            //playerBackground.color = GameController.instance.skin.color;
            //playerImage.color = GameController.instance.skin.color;
            playerBar.SetActive(true);
            gemRushBar.gameObject.SetActive(false);
            turboBar.gameObject.SetActive(true);
            turboBar.DOFade(0, 0.01f);
        }
        else
        {
            playerBar.SetActive(false);
            gemRushBar.gameObject.SetActive(true);
            turboBar.gameObject.SetActive(false);
            turboBar.DOFade(0, 0.01f);
        }
    }

    IEnumerator FillTurbo(float to)
    {
        var diff = turboImage.fillAmount - to;
        while (diff != 0)
        {
            if (diff > 0)
            {
                turboImage.fillAmount -= Time.deltaTime;
                diff = turboImage.fillAmount - to;
                if (diff < 0)
                {
                    diff = 0;
                    turboImage.fillAmount = to;
                }
            }
            else
            {
                turboImage.fillAmount += Time.deltaTime;
                diff = turboImage.fillAmount - to;
                if (diff > 0)
                {
                    diff = 0;
                    turboImage.fillAmount = to;
                }
            }
            yield return new WaitForFixedUpdate();
        }
    }

    public void SetTurboLevel(float percent)
    {
        if (turboImage.fillAmount == 0)
        {
            turboBar.DOFade(1, 0.5f);
        }
        else if (percent == 0)
        {
            turboBar.DOFade(0, 0.5f);
        }
        StartCoroutine(FillTurbo(percent));
    }

    public void SetBaitPosition(float percent)
    {
        var posX = percent * 400 - 200;
        baitIndicator.localPosition = new Vector3(posX, -50, 0);
    }

    public void SetPlayerPosition(float percent)
    {
        var posX = percent * 400 - 200;
        playerIndicator.localPosition = new Vector3(posX, -50, 0);
    }
}