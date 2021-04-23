#pragma warning disable 649

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Watermelon;
using Watermelon.Core;

public class UIController : MonoBehaviour
{
    public Watermelon.AudioSettings audioSettings;
    public ShopController shopController;

    [SerializeField]
    private CanvasGroup gameCanvas;

    [SerializeField]
    private GameObject settingsCanvas;
    [SerializeField]
    private GameObject defaultCanvas;
    [SerializeField]
    private GameObject shopCanvas;

    [Space]
    [SerializeField]
    private GemRushComplete gemRushCompleteController;
    [SerializeField]
    private LevelCompleteController levelCompleteController;
    [SerializeField]
    private LevelFailedController levelFailedController;
    [SerializeField]
    private GameCanvasController gameCanvasController;
    [SerializeField]
    private StartController startController;
    [SerializeField]
    private SettingsController settingsController;

    [Space]
    [SerializeField]
    private CanvasGroup startCanvas;

    public RawImage uiBackgroundImage;
    private Texture2D backgroundTexture;

    [Space]
    public Text gemRushText;

    [Space]
    public Text gemsText;
    public Text levelsText;

    private Color gradientStart;
    private Color gradientEnd;

    public delegate void AfterPlay();

    public void SetGemRushText(string text)
    {
        gemRushText.text = text;
    }

    public void AddGems()
    {
        gemsText.text = GameController.instance.gemsCount.ToString();
    }

    public void SetLevel()
    {
        if (GameController.instance.GetCurrentLevelType() == Level.LevelType.GEM_RUSH)
        {
            levelsText.text = Multilanguage.GetWord("default.gem_rush");
        }
        else
        {
            levelsText.text = Multilanguage.GetWord("default.level") + " " + (GameController.instance.currentLevel + 1);
        }

    }

    public void Init()
    {
        shopController.Init();
    }

    public void Play(AfterPlay afterPlay)
    {
        startCanvas.DOFade(0, 0.5f).OnComplete(delegate
        {
            startCanvas.gameObject.SetActive(false);
            gameCanvasController.Play();
            afterPlay();
        });
    }

    public void StartGame()
    {
        startCanvas.gameObject.SetActive(true);
        gameCanvas.gameObject.SetActive(false);
        shopCanvas.SetActive(false);
        settingsCanvas.SetActive(false);
        uiBackgroundImage.color = Color.white;
        startController.StartGame();
    }

    public void OpenShop()
    {
        if (GameController.instance.isSound)
        {
            //AudioController.PlaySound(audioSettings.sounds.settings, AudioController.AudioType.Sound, 0.8f, 1f);  
        }

        if (GameController.instance.isSound)
        {
            AudioController.PlaySound(audioSettings.sounds.button, AudioController.AudioType.Sound, 0.8f, 1.2f);
        }

        uiBackgroundImage.gameObject.SetActive(true);
        startCanvas.gameObject.SetActive(false);
        gameCanvas.gameObject.SetActive(false);
        shopCanvas.SetActive(true);
        StartCoroutine(shopController.ShopInit());
        GameController.instance.shopOpen = true;
    }

    public void CloseShop()
    {
        uiBackgroundImage.gameObject.SetActive(false);
        startCanvas.gameObject.SetActive(true);
        gameCanvas.gameObject.SetActive(false);
        shopCanvas.SetActive(false);
        StartCoroutine(startController.ScaleStart());
        GameController.instance.shopOpen = false;
    }

    public void OpenSettings()
    {
        if (GameController.instance.isSound)
        {
            AudioController.PlaySound(audioSettings.sounds.button, AudioController.AudioType.Sound, 0.8f, 1.2f);
        }
        uiBackgroundImage.gameObject.SetActive(true);
        startCanvas.gameObject.SetActive(false);
        gameCanvas.gameObject.SetActive(false);
        settingsCanvas.SetActive(true);
        settingsController.OpenSettings();
        GameController.instance.settingsOpen = true;
    }

    public void CloseSettings()
    {
        if (GameController.instance.isSound)
        {
            AudioController.PlaySound(audioSettings.sounds.button, AudioController.AudioType.Sound, 0.8f, 1f);
        }
        settingsController.CloseSettings(delegate
        {
            uiBackgroundImage.gameObject.SetActive(false);
            startCanvas.gameObject.SetActive(true);
            gameCanvas.gameObject.SetActive(false);
            settingsCanvas.SetActive(false);
            StartCoroutine(startController.ScaleStart());
            GameController.instance.settingsOpen = false;
        });
    }

    public void BackgroundInit(Color start, Color end, bool with)
    {
        gradientStart = start;
        gradientEnd = end;

        // Start Canvas
        var customizeButton = startCanvas.transform.Find("Customize Button Container");
        customizeButton.Find("Shadow").GetComponent<Image>().color = gradientStart;
        customizeButton.Find("Outline").GetComponent<Image>().color = gradientEnd;
        customizeButton.Find("Text").GetComponent<Text>().color = gradientEnd;

        var settingsButton = startCanvas.transform.Find("Settings Button Container");
        settingsButton.Find("Shadow").GetComponent<Image>().color = gradientStart;
        settingsButton.Find("Outline").GetComponent<Image>().color = gradientEnd;
        settingsButton.Find("Text").GetComponent<Text>().color = gradientEnd;

        var handComponent = startCanvas.transform.Find("Hand Component");
        handComponent.Find("Text").GetComponent<Text>().color = gradientEnd;

        // Default data
        var levelContainer = defaultCanvas.transform.Find("Level Container");
        levelContainer.Find("Text").GetComponent<Text>().text = "LEVEL " + (GameController.instance.currentLevel + 1);

        var gemContainer = defaultCanvas.transform.Find("Gem Container");
        gemContainer.Find("Text").GetComponent<Text>().text = GameController.instance.gemsCount.ToString();

        settingsController.Background(gradientStart, gradientEnd);
        shopController.SetBackground(gradientStart, gradientEnd);

        backgroundTexture = new Texture2D(1, 9);
        backgroundTexture.wrapMode = TextureWrapMode.Clamp;
        backgroundTexture.filterMode = FilterMode.Bilinear;
        start.a = 0.8f;
        end.a = 0.8f;

        if (with)
            InitTexture(start, end);
    }

    public void GameOver()
    {
        gameCanvas.DOFade(0, 0.5f).OnComplete(delegate
        {
            gameCanvas.gameObject.SetActive(false);
            levelFailedController.Show();
        });
    }

    public void WinGame()
    {
        StartCoroutine(FinishGameCoroutine());
    }

    private IEnumerator FinishGameCoroutine()
    {
        yield return new WaitForSeconds(0.5f);

        if (GameController.instance.isSound)
        {
            AudioController.PlaySound(audioSettings.sounds.finish, AudioController.AudioType.Sound, 1.3f, 1f);
        }

        yield return new WaitForSeconds(0.5f);

        uiBackgroundImage.gameObject.SetActive(true);
        uiBackgroundImage.GetComponent<CanvasGroup>().DOFade(1, 0.5f);
        gameCanvas.DOFade(0, 0.5f).OnComplete(delegate
        {
            gameCanvas.gameObject.SetActive(false);
            if (GameController.instance.GetCurrentLevelType() == Level.LevelType.JELLY_RUSH)
            {
                levelCompleteController.Show();
            }
            else
            {
                gemRushCompleteController.Show(GameController.instance.CatchedGems());
            }

        });
    }

    public void InitTexture(Color color1, Color color2)
    {
        color1.a = 0.8f;
        color2.a = 0.8f;
        backgroundTexture.SetPixel(0, 0, color1);
        backgroundTexture.SetPixel(0, 1, Color.Lerp(color1, color2, 0.125f));
        backgroundTexture.SetPixel(0, 2, Color.Lerp(color1, color2, 0.250f));
        backgroundTexture.SetPixel(0, 3, Color.Lerp(color1, color2, 0.375f));
        backgroundTexture.SetPixel(0, 4, Color.Lerp(color1, color2, 0.500f));
        backgroundTexture.SetPixel(0, 5, Color.Lerp(color1, color2, 0.625f));
        backgroundTexture.SetPixel(0, 6, Color.Lerp(color1, color2, 0.750f));
        backgroundTexture.SetPixel(0, 7, Color.Lerp(color1, color2, 0.875f));
        backgroundTexture.SetPixel(0, 8, color2);
        backgroundTexture.Apply();
        uiBackgroundImage.texture = backgroundTexture;
    }

    public void SetBaitPosition(float percent)
    {
        gameCanvasController.SetBaitPosition(percent);
    }

    public void SetPlayerPosition(float percent)
    {
        gameCanvasController.SetPlayerPosition(percent);
    }

    public void SetTurboLevel(float percent)
    {
        gameCanvasController.SetTurboLevel(percent);
    }
}