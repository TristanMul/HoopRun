#pragma warning disable 649

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Watermelon;


public class GameController : MonoBehaviour
{
    public static GameController instance;
    public Watermelon.AudioSettings audioSettings;

    [SerializeField]
    private LevelDatabase levelDatabase;

    [SerializeField]
    private LevelController levelController;

    [SerializeField]
    private PlayerController playerController;

    [SerializeField]
    private CameraController cameraController;

    [SerializeField]
    private BaitController baitController;

    [SerializeField]
    private GradientBackground gradientBackground;

    [SerializeField]
    private UIController uiController;

    public GameSettings gameSettings;

    public GameStage gameStage;

    public Material obstacleBody;
    public Material obstacleSurface;

    public Material railBody;
    public Material railSurface;
    public Material railPart;

    public Material platformBody;
    public Material platformSurface;

    public Skin skin;

    public ColorPreset currentPreset;
    public ColorPreset nextPreset;
    public ColorPreset transitionPreset;

    public bool shopOpen = false;
    public bool settingsOpen = false;
    public bool presetTransition;
    public bool baitFinished = false;

    public int currentSkin;

    [System.NonSerialized]
    public Dictionary<string, List<Skin>> skins;

    [System.NonSerialized]
    public int currentLevel = -1;

    [System.NonSerialized]
    public int turboLevel = 0;

    [System.NonSerialized]
    public int gemsCount = 0;

    [System.NonSerialized]
    public bool isHaptic;
    [System.NonSerialized]
    public bool isSound;
    [System.NonSerialized]
    public bool isMusic;

    private Color turboStartColor;
    private Color turboEndColor;
    private bool hasToChange = false;

    private float lastInterstShowTime;

    private void Awake()
    {
        shopOpen = false;
        settingsOpen = false;
        instance = this;
        levelDatabase.Init();
        uiController.Init();
        uiController.StartGame();
    }

    public void StartGame()
    {
        gameStage = GameStage.GAME;
        levelController.SwapObstacle(0);
        baitController.StartGame();
        AddGems(0);
        uiController.Play(delegate
        { });
    }

    public void NextLevel(bool replay)
    {
        if (!replay)
        {
            currentLevel++;
            PlayerPrefs.SetInt("Level", currentLevel);
        }
        
        if (levelDatabase.levels.Length == currentLevel)
        {
            currentLevel = 0;
        }

        InitLevel();
    }

    public void ChangeTurbo()
    {
        if (GetCurrentLevelType() == Level.LevelType.GEM_RUSH)
            return;

        uiController.SetTurboLevel(turboLevel / 5f);
        Color grayStart = new Color(0.5f, 0.5f, 0.5f);
        Color grayEnd = new Color(0.7f, 0.7f, 0.7f);

        if (turboLevel != 5)
        {
            turboStartColor = currentPreset.backgroundStart + (grayStart - currentPreset.backgroundStart) / 5 * turboLevel;
            turboEndColor = currentPreset.backgroundFinish + (grayEnd - currentPreset.backgroundFinish) / 5 * turboLevel;
            if (turboLevel == 0 && nextPreset == levelDatabase.turboPreset)
            {
                presetTransition = true;
                nextPreset = currentPreset;
                transitionPreset = levelDatabase.turboPreset.Copy();
                //ConfirmPreset(currentPreset);
            }
        }
        else
        {
            transitionPreset = currentPreset.Copy();
            nextPreset = levelDatabase.turboPreset;
            presetTransition = true;
            turboStartColor = levelDatabase.turboPreset.backgroundStart;
            turboEndColor = levelDatabase.turboPreset.backgroundFinish;
            //ConfirmPreset(levelDatabase.turboPreset);
        }

        hasToChange = true;
    }

    private void Start()
    {
        GameSettingsPrefs.Set("no_ads", false);

        if (PlayerPrefs.HasKey("Level"))
        {
            currentLevel = PlayerPrefs.GetInt("Level");
        }
        else
        {
            currentLevel = 0;
            PlayerPrefs.SetInt("Level", currentLevel);
        }

        if (PlayerPrefs.HasKey("isHaptic"))
            isHaptic = PlayerPrefs.GetInt("isHaptic") == 1;
        if (PlayerPrefs.HasKey("isSound"))
            isSound = PlayerPrefs.GetInt("isSound") == 1;
        if (PlayerPrefs.HasKey("isMusic"))
            isMusic = PlayerPrefs.GetInt("isMusic") == 1;

        if (PlayerPrefs.HasKey("currentSkin"))
        {
            currentSkin = PlayerPrefs.GetInt("currentSkin");
        }
        else
        {
            currentSkin = 0;
            PlayerPrefs.SetInt("currentSkin", 0);
        }

        PlayerPrefs.SetInt(levelDatabase.skins[currentSkin].group + " " + levelDatabase.skins[currentSkin].name + "_SKIN", 1);

        if (PlayerPrefs.HasKey("gemsCount"))
        {
            gemsCount = PlayerPrefs.GetInt("gemsCount");
        }
        else
        {
            PlayerPrefs.SetInt("gemsCount", gameSettings.basicGemsAmount);
            gemsCount = gameSettings.basicGemsAmount;
        }

        InitSkin();
        InitSkins();
        InitLevel();

        uiController.SetLevel();

        AdsManager.RequestInterstitial(AdsManager.Settings.interstitialType);
        AdsManager.RequestRewardBasedVideo(AdsManager.Settings.rewardedVideoType);

        if (!GameSettingsPrefs.Get<bool>("no_ads"))
        {
            AdsManager.ShowBanner(AdsManager.Settings.bannerType);
        }

        if (GameController.instance.isMusic)
        {
            AudioController.PlayRandomMusic();
        }
    }

    public void InitSkin()
    {
        var parent = GameObject.Find("PlayerAnim");
        if (parent.transform.childCount != 0)
            Destroy(parent.transform.GetChild(0).gameObject);

        skin = levelDatabase.skins[currentSkin];

        var gameObject = Instantiate(skin.prefab);
        gameObject.transform.SetParent(parent.transform);
        gameObject.transform.localPosition = Vector3.zero;
        gameObject.transform.localRotation = new Quaternion();

    }

    public Level.LevelType GetCurrentLevelType()
    {
        return levelDatabase.levels[currentLevel].type;
    }

    public int CatchedGems()
    {
        return playerController.catchedGems;
    }

    public void AddGems(int amount)
    {
        gemsCount += amount;
        PlayerPrefs.SetInt("gemsCount", gemsCount);
        uiController.AddGems();
        if (GetCurrentLevelType() == Level.LevelType.GEM_RUSH && gameStage == GameStage.GAME)
        {
            uiController.SetGemRushText(playerController.catchedGems.ToString() + "/" + GemsInLevel());
        }
    }

    public int GemsInLevel()
    {
        return levelController.GemsInLevel();
    }

    private void InitSkins()
    {
        skins = new Dictionary<string, List<Skin>>();
        foreach (Skin skin in levelDatabase.skins)
        {
            if (!skins.ContainsKey(skin.group))
                skins.Add(skin.group, new List<Skin>());
            skins[skin.group].Add(skin);
        }
    }

    private void ConfirmPreset(ColorPreset preset, bool with)
    {
        obstacleBody.SetColor(Shader.PropertyToID("_main_color"), preset.obstacleBody);
        obstacleSurface.SetColor(Shader.PropertyToID("_main_color"), preset.obstacleSurface);

        railBody.SetColor(Shader.PropertyToID("_main_color"), preset.railBody);
        railSurface.SetColor(Shader.PropertyToID("_main_color"), preset.railSurface);

        platformBody.SetColor(Shader.PropertyToID("_main_color"), preset.platformBody);
        platformSurface.SetColor("_BaseColor", preset.platformSurface);

        uiController.BackgroundInit(preset.backgroundStart, preset.backgroundFinish, with);
    }

    private void InitLevel()
    {
        if (levelDatabase.levels[currentLevel].type == Level.LevelType.JELLY_RUSH)
        {
            currentPreset = levelDatabase.colorPresets[Random.Range(0, levelDatabase.colorPresets.Length)];
        }
        else
        {
            currentPreset = levelDatabase.gemRushPreset;
        }


        gradientBackground.SetColor(currentPreset.backgroundStart, currentPreset.backgroundFinish);

        ConfirmPreset(currentPreset, true);

        uiController.StartGame();

        turboLevel = 0;

        levelController.Init(levelDatabase.levels[currentLevel]);
        levelController.VisualizeLevel(currentPreset);
        gameStage = GameStage.START;
        playerController.Init();
        baitController.Init();
        cameraController.Init();
        baitFinished = false;

        if(lastInterstShowTime + gameSettings.interstitialShowingDelay <= Time.realtimeSinceStartup)
        {
            lastInterstShowTime = Time.realtimeSinceStartup;
            AdsManager.ShowInterstitial(AdsManager.Settings.interstitialType);
        }
    }

    public void Revive()
    {
        turboLevel = 0;
        playerController.Revive();
        gameStage = GameStage.GAME;
        ConfirmPreset(currentPreset, false);
        uiController.Play(delegate
        {
            ChangeTurbo();
        });
    }

    public void ReloadSkin()
    {
        InitSkin();
        playerController.InitProjection();
    }

    public void Finish()
    {
        turboLevel = 0;
        ChangeTurbo();
        if (catchedBait)
            PlayerPrefs.SetInt(currentLevel + "_level", 1);
        uiController.InitTexture(currentPreset.backgroundStart, currentPreset.backgroundFinish);
        uiController.WinGame();
        gameStage = GameStage.FINISH;
    }

    private void FixedUpdate()
    {
        if (hasToChange)
        {
            hasToChange = !gradientBackground.OverrideColor(turboStartColor, turboEndColor);
        }
        if (presetTransition)
        {

            var isFinished = GradientBackground.TransitionFromTo(transitionPreset.obstacleBody, nextPreset.obstacleBody, out transitionPreset.obstacleBody);
            isFinished &= GradientBackground.TransitionFromTo(transitionPreset.obstacleSurface, nextPreset.obstacleSurface, out transitionPreset.obstacleSurface);
            isFinished &= GradientBackground.TransitionFromTo(transitionPreset.railBody, nextPreset.railBody, out transitionPreset.railBody);
            isFinished &= GradientBackground.TransitionFromTo(transitionPreset.railSurface, nextPreset.railSurface, out transitionPreset.railSurface);
            isFinished &= GradientBackground.TransitionFromTo(transitionPreset.railPart, nextPreset.railPart, out transitionPreset.railPart);
            isFinished &= GradientBackground.TransitionFromTo(transitionPreset.platformBody, nextPreset.platformBody, out transitionPreset.platformBody);
            isFinished &= GradientBackground.TransitionFromTo(transitionPreset.platformSurface, nextPreset.platformSurface, out transitionPreset.platformSurface);

            ConfirmPreset(transitionPreset, false);

            if (isFinished)
            {
                //transitionPreset
                presetTransition = false;
            }
        }
    }

    public bool catchedBait = false;

    public void UpdatePlayerPosition(float percent)
    {
        uiController.SetPlayerPosition(percent);
    }

    public void UpdateBaitPosition(float percent)
    {
        if (!catchedBait)
        {
            uiController.SetBaitPosition(percent);
        }
        else
        {
            uiController.SetBaitPosition(-100);
        }
    }

    public void GameOver()
    {
        GameController.instance.gameStage = GameStage.GAME_OVER;
        uiController.GameOver();
    }

    public Bait GetBait()
    {
        return levelController.GetBait();
    }

    public void PlayBaitParticles()
    {
        baitController.PlayParticles();
    }
}

public enum GameStage
{
    START, GAME, PAUSE, FINISH, GAME_OVER
}