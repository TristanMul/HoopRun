using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Watermelon;

public class ShopController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Watermelon.AudioSettings audioSettings;
    public LevelDatabase levelDatabase;
    public GameSettings gameSettings;
    public UIController uiController;
    private Image skinsContainer;

    private CanvasGroup skinsContainerCanvas;

    public CanvasGroup uiBackgroundCanvasGroup;
    private Image closeButton;

    [Space]
    private CanvasGroup dotsContainer;
    private CanvasGroup customizationText;
    private CanvasGroup leftSkinsContainer;
    private CanvasGroup rightSkinsContainer;
    private CanvasGroup titleGroup;

    [Space]
    public Text randomUnlockTextLeft;
    public Text randomUnlockTextCenter;
    public Text randomUnlockTextRight;

    public Text randomTextLeft;
    public Text randomTextCenter;
    public Text randomTextRight;
    public Image randomUnlockBackgroundLeft;
    public Image randomUnlockBackgroundCenter;
    public Image randomUnlockBackgroundRight;

    [Space]
    public Text watchAdTextLeft;
    public Text watchAdTextCenter;
    public Text watchAdTextRight;
    public Text watchAdRewardTextLeft;
    public Text watchAdRewardTextCenter;
    public Text watchAdRewardTextRight;
    public Image watchAdBackgroundLeft;
    public Image watchAdBackgroundCenter;
    public Image watchAdBackgroundRight;

    [Space]
    public GameObject mainSkinsCamerasContainer;
    public GameObject sideSkinsCamerasContainer;

    [Space]
    public Text closeText;
    public Text customizeText;

    [Space]
    public float dragMultiplier = 50;

    private List<Image> dotsList;
    private List<Skin> pageSkins;

    private Color gradientStart;
    private Color gradientEnd;
    List<int> closedSkins;
 
    private bool unlocking = false;

    private bool newDrag = true;
    private bool leftNew = false;
    private bool border = false;
    private bool swaped = false;
    private bool borderLeft = false;

    private float transparencyPerPixel = 0.80f / 800f;
    private float MAX_LEFT = 200;
    private float lastDrag = 0;

    private int groupCost = 500;

    private int currentDot = 0;
    private int amountOfDots = 0;


    private void Start()
    {
        watchAdTextLeft.text = Multilanguage.GetWord("shop.watch_ad");
        watchAdTextCenter.text = Multilanguage.GetWord("shop.watch_ad");
        watchAdTextRight.text = Multilanguage.GetWord("shop.watch_ad");

        randomTextLeft.text = Multilanguage.GetWord("shop.random") + "\n" + Multilanguage.GetWord("shop.unlock");
        randomTextCenter.text = Multilanguage.GetWord("shop.random") + "\n" + Multilanguage.GetWord("shop.unlock");
        randomTextRight.text = Multilanguage.GetWord("shop.random") + "\n" + Multilanguage.GetWord("shop.unlock");

        closeText.text = Multilanguage.GetWord("default.close");
        customizeText.text = Multilanguage.GetWord("shop.customize");

        watchAdRewardTextLeft.text = "+" + gameSettings.adsReward;
        watchAdRewardTextCenter.text = "+" + gameSettings.adsReward;
        watchAdRewardTextRight.text = "+" + gameSettings.adsReward;
    }

    public void UnlockRandom()
    {
        if (GameController.instance.isSound)
        {
            AudioController.PlaySound(audioSettings.sounds.button, AudioController.AudioType.Sound, 0.8f, 1.2f);
        }

        unlocking = true;

        if (GameController.instance.gemsCount >= groupCost && closedSkins.Count != 0)
        {
            if (closedSkins.Count == 1)
            {
                OpenSkin(closedSkins[0]);
            }
            else
            {
                StartCoroutine(Rulette());
            }
            GameController.instance.AddGems(-groupCost);
        }
    }

    public void AddGems()
    {
        if (GameController.instance.isSound)
        {
            AudioController.PlaySound(audioSettings.sounds.button, AudioController.AudioType.Sound, 0.8f, 1.2f);
        }

        AdsManager.ShowRewardBasedVideo(AdsManager.Settings.rewardedVideoType, (hasReward) =>
        {
            if (hasReward)
            {
                GameController.instance.AddGems(gameSettings.adsReward);
            }
        });
    }

    IEnumerator Rulette()
    {
        float delay = 0.05f;
        int index = 0;
        Color prevColor = Color.white;
        Image prevImage = null;
        Image frame;

        do
        {
            var newIndex = Random.Range(0, closedSkins.Count);
            while (newIndex == index)
            {
                newIndex = Random.Range(0, closedSkins.Count);
            }
            index = newIndex;
            if (prevImage != null)
            {
                prevImage.color = prevColor;
            }

            frame = skinsContainer.transform.GetChild(closedSkins[index]).Find("Frame").GetComponent<Image>();
            prevImage = frame;
            prevColor = frame.color;
            frame.color = Color.red;
            delay *= 1.1f;
            yield return new WaitForSeconds(delay);
        } while (delay <= 0.5f);

        frame.color = prevColor;
        OpenSkin(closedSkins[index]);
        unlocking = false;
    }

    private void OpenSkin(int index)
    {
        var skin = GetSkinsPage(currentDot)[index];
        PlayerPrefs.SetInt(skin.group + " " + skin.name + "_SKIN", 1);
        var globalIndex = 0;

        for (int i = 0; i < levelDatabase.skins.Length; i++)
        {
            if (skin.name == levelDatabase.skins[i].name && skin.group == levelDatabase.skins[i].group)
            {
                globalIndex = i;
                break;
            }
        }


        GameController.instance.currentSkin = globalIndex;
        GameController.instance.skin = skin;
        PlayerPrefs.SetInt("currentSkin", globalIndex);
        SetUpPage(mainSkinsCamerasContainer, skinsContainerCanvas, currentDot);
        GameController.instance.ReloadSkin();
    }

    public void Init()
    {
        skinsContainer = transform.Find("Skins Container").GetComponent<Image>();
        skinsContainerCanvas = skinsContainer.GetComponent<CanvasGroup>();
        closeButton = transform.Find("Close Button").GetComponent<Image>();

        dotsContainer = transform.Find("Dots Container").GetComponent<CanvasGroup>();
        customizationText = transform.Find("Customize Text").GetComponent<CanvasGroup>();
        leftSkinsContainer = transform.Find("Left Skins Container").GetComponent<CanvasGroup>();
        rightSkinsContainer = transform.Find("Right Skins Container").GetComponent<CanvasGroup>();
        titleGroup = skinsContainer.transform.Find("Group Title").GetComponent<CanvasGroup>();

        dotsList = new List<Image>();
    }

    public void CloseShop()
    {
        if (GameController.instance.isSound)
        {
            //AudioController.PlaySound(audioSettings.sounds.settings, AudioController.AudioType.Sound, 0.8f, 1f);  
        }

        if (GameController.instance.isSound)
        {
            AudioController.PlaySound(audioSettings.sounds.button, AudioController.AudioType.Sound, 0.8f, 1f);
        }

        skinsContainer.rectTransform.DOLocalMove(new Vector3(0, -1500, 0), 0.3f);
        closeButton.rectTransform.DOLocalMove(new Vector3(0, -2040, 0), 0.3f);

        uiBackgroundCanvasGroup.DOFade(0, 0.3f).OnComplete(delegate
        {
            uiController.CloseShop();
        });

        dotsContainer.DOFade(0f, 0.15f);
        customizationText.DOFade(0f, 0.15f);
        leftSkinsContainer.DOFade(0f, 0.15f);
        rightSkinsContainer.DOFade(0f, 0.15f);
        titleGroup.DOFade(0f, 0.15f);

        foreach (Transform skin in skinsContainer.transform)
        {
            //skin.gameObject.SetActive(true);
            var canvasGroup = skin.GetComponent<CanvasGroup>();
            canvasGroup.DOFade(0f, 0.15f);
        }
    }

    private List<Skin> GetSkinsPage(int page)
    {
        var result = new List<Skin>();
        var skins = GameController.instance.skins;
        var currentAmount = 0;

        foreach (var key in skins.Keys)
        {
            var dotsInGroup = Mathf.CeilToInt(skins[key].Count / 6f);
            currentAmount += dotsInGroup;
            if (page < currentAmount)
            {

                for (int i = (currentAmount - page - 1) * 6; i < (currentAmount - page) * 6; i++)
                {
                    if (i == skins[key].Count)
                        break;
                    result.Add(skins[key][i]);
                }
                break;
            }
        }

        return result;
    }

    public IEnumerator ShopInit()
    {
        var selectedSkin = levelDatabase.skins[GameController.instance.currentSkin];
        var selectedGroup = selectedSkin.group;
        currentDot = 0;

        amountOfDots = 0;
        var skins = GameController.instance.skins;

        foreach (var key in skins.Keys)
        {
            var dotsInGroup = Mathf.CeilToInt(skins[key].Count / 6f);
            amountOfDots += dotsInGroup;
            if (skins[key].Contains(selectedSkin))
            {
                var index = skins[key].IndexOf(selectedSkin);
                if (index != 0)
                {
                    currentDot = amountOfDots - (dotsInGroup - Mathf.CeilToInt(index / 6f)) - 1;
                }
                else
                {
                    currentDot = amountOfDots - dotsInGroup;
                }

            }
        }

        var startingDotX = -35 * (amountOfDots - 1);
        dotsList = new List<Image>();

        for (int i = 0; i < amountOfDots; i++)
        {
            Transform dot;
            if (i < 5)
            {
                dot = dotsContainer.transform.GetChild(i);

            }
            else
            {
                dot = Instantiate(dotsContainer.transform.GetChild(i));
                dot.SetParent(dotsContainer.transform);
            }
            dot.localPosition = new Vector3(startingDotX + i * 70, 0, 0);
            dot.gameObject.SetActive(true);
            var image = dot.GetComponent<Image>();

            if (currentDot == i)
            {
                image.color = new Color(1, 1, 1, 1);
            }
            else
            {
                image.color = new Color(1, 1, 1, 0.39f);
            }
            dotsList.Add(image);
        }

        for (int i = amountOfDots; i < 5; i++)
        {
            var dot = dotsContainer.transform.GetChild(i);
            dot.gameObject.SetActive(false);
        }

        SetUpPage(mainSkinsCamerasContainer, skinsContainerCanvas, currentDot);

        uiBackgroundCanvasGroup.DOFade(1, 0.25f);

        skinsContainer.rectTransform.DOLocalMove(new Vector3(0, -200, 0), 0.25f).OnComplete(delegate
        {
            skinsContainer.rectTransform.DOLocalMove(new Vector3(0, 0, 0), 0.12f).SetEasing(Ease.Type.BackOut);
        });
        yield return new WaitForSeconds(0.2f);

        closeButton.rectTransform.DOLocalMove(new Vector3(0, -700, 0), 0.3f).OnComplete(delegate
        {
            closeButton.rectTransform.DOLocalMove(new Vector3(0, -580, 0), 0.1f).SetEasing(Ease.Type.BackOut);
        });
        yield return new WaitForSeconds(0.1f);

        if (currentDot != 0)
        {
            leftSkinsContainer.gameObject.SetActive(true);
            leftSkinsContainer.DOFade(0.2f, 0.2f);
        }

        if (currentDot != amountOfDots - 1)
        {
            rightSkinsContainer.gameObject.SetActive(true);
            rightSkinsContainer.DOFade(0.2f, 0.2f);
        }

        titleGroup.gameObject.SetActive(true);
        dotsContainer.gameObject.SetActive(true);
        customizationText.gameObject.SetActive(true);
        dotsContainer.DOFade(1f, 0.2f);
        customizationText.DOFade(1f, 0.2f);

        titleGroup.DOFade(1f, 0.2f);
        yield return new WaitForSeconds(0.2f);

        foreach (Transform skin in skinsContainer.transform)
        {
            //skin.gameObject.SetActive(true);
            var canvasGroup = skin.GetComponent<CanvasGroup>();
            canvasGroup.DOFade(1f, 0.2f);
            yield return new WaitForSeconds(0.04f);
        }
    }

    public void SelectSkin(int skinId)
    {
        var skin = GetSkinsPage(currentDot)[skinId];
        if (skin != GameController.instance.skin)
        {
            GameController.instance.skin = skin;
            var globalIndex = 0;
            for (int i = 0; i < levelDatabase.skins.Length; i++)
            {
                if (skin.name == levelDatabase.skins[i].name && skin.group == levelDatabase.skins[i].group)
                {
                    globalIndex = i;
                    break;
                }
            }

            GameController.instance.currentSkin = globalIndex;
            PlayerPrefs.SetInt("currentSkin", globalIndex);
            SetUpPage(mainSkinsCamerasContainer, skinsContainerCanvas, currentDot);
            GameController.instance.ReloadSkin();
        }
    }

    public void SetBackground(Color gradientStart, Color gradientEnd)
    {
        this.gradientStart = gradientStart;
        this.gradientEnd = gradientEnd;

        foreach (Transform skin in skinsContainer.transform)
        {
            if (skin.name.StartsWith("Skin"))
            {
                skin.Find("Background").GetComponent<Image>().color = gradientStart;
                skin.Find("Question Mark").GetComponent<Image>().color = gradientEnd;
                skin.Find("Frame").GetComponent<Image>().color = gradientEnd;
            }
        }

        foreach (Transform skin in leftSkinsContainer.transform)
        {
            if (skin.name.StartsWith("Skin"))
            {
                skin.Find("Background").GetComponent<Image>().color = gradientStart;
                skin.Find("Question Mark").GetComponent<Image>().color = gradientEnd;
                skin.Find("Frame").GetComponent<Image>().color = gradientEnd;
            }
        }

        foreach (Transform skin in rightSkinsContainer.transform)
        {
            if (skin.name.StartsWith("Skin"))
            {
                skin.Find("Background").GetComponent<Image>().color = gradientStart;
                skin.Find("Question Mark").GetComponent<Image>().color = gradientEnd;
                skin.Find("Frame").GetComponent<Image>().color = gradientEnd;
            }
        }

        randomUnlockBackgroundLeft.color = gradientEnd;
        randomUnlockBackgroundCenter.color = gradientEnd;
        randomUnlockBackgroundRight.color = gradientEnd;
        watchAdBackgroundLeft.color = gradientEnd;
        watchAdBackgroundCenter.color = gradientEnd;
        watchAdBackgroundRight.color = gradientEnd;

        randomUnlockTextLeft.color = gradientStart;
        randomUnlockTextCenter.color = gradientStart;
        randomUnlockTextRight.color = gradientStart;
        watchAdTextLeft.color = gradientStart;
        watchAdTextCenter.color = gradientStart;
        watchAdTextRight.color = gradientStart;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        border = true;
        newDrag = true;
        swaped = false;

        lastDrag = 0;
        if (eventData.delta.x > 0 && currentDot != 0)
        {
            SetUpPage(sideSkinsCamerasContainer, leftSkinsContainer, currentDot - 1);
            leftNew = true;
            border = false;
        }
        else if (eventData.delta.x < 0 && currentDot != amountOfDots - 1)
        {
            SetUpPage(sideSkinsCamerasContainer, rightSkinsContainer, currentDot + 1);
            border = false;
            leftNew = false;

        }
        else
        {
            borderLeft = eventData.delta.x > 0;
        }
    }

    private void DisablePage(GameObject camerasContainer, CanvasGroup skinsContainer)
    {
        foreach (Transform child in camerasContainer.transform)
        {
            if (child.childCount != 0)
            {
                Destroy(child.GetChild(0).gameObject);
            }
            child.gameObject.SetActive(false);
        }
        camerasContainer.SetActive(false);

        foreach (Transform child in skinsContainer.transform)
        {
            if (child.name.StartsWith("Skin"))
            {
                child.Find("Frame").GetComponent<Image>().color = gradientEnd;
            }

            child.gameObject.SetActive(false);
        }
    }

    private void SetUpPage(GameObject camerasContainer, CanvasGroup skinsContainer, int page)
    {
        var selectedSkin = levelDatabase.skins[GameController.instance.currentSkin];
        var newSkins = GetSkinsPage(page);
        var text = skinsContainer.transform.Find("Group Title");
        text.gameObject.SetActive(true);
        text.GetChild(0).GetComponent<Text>().text = newSkins[0].group.ToUpper();

        if (skinsContainer == skinsContainerCanvas)
        {
            for (int i = 0; i < levelDatabase.skinGroups.Length; i++)
            {
                if (newSkins[0].group == levelDatabase.skinGroups[i])
                {
                    groupCost = levelDatabase.skinGroupCosts[i];
                    randomUnlockTextCenter.text = groupCost.ToString();
                    break;
                }
            }
        }

        var shopButtons = skinsContainer.transform.Find("Shop Buttons");
        shopButtons.gameObject.SetActive(true);

        camerasContainer.SetActive(true);
        closedSkins = new List<int>();

        for (int i = 0; i < newSkins.Count; i++)
        {
            var skinContainer = skinsContainer.transform.GetChild(i);
            skinContainer.gameObject.SetActive(true);
            var skin = newSkins[i];
            var skinCamera = camerasContainer.transform.GetChild(i);

            if (PlayerPrefs.HasKey(skin.group + " " + skin.name + "_SKIN"))
            {
                skinContainer.Find("Frame").GetComponent<Image>().color = gradientEnd;
                if (skin == selectedSkin)
                {
                    skinContainer.Find("Frame").GetComponent<Image>().color = Color.red;
                }
                skinContainer.Find("Skin Image").gameObject.SetActive(true);
                skinContainer.Find("Question Mark").gameObject.SetActive(false);
                skinCamera.gameObject.SetActive(true);
                skinsContainer.transform.GetChild(i).Find("Background").GetComponent<Image>().color = gradientStart;

                if (skinCamera.childCount != 0)
                {
                    Destroy(skinCamera.GetChild(0).gameObject);
                }
                var skinPrefab = Instantiate(skin.prefab);
                var skinParent = Instantiate(new GameObject("Parent"));

                skinParent.transform.SetParent(skinCamera);
                skinPrefab.transform.SetParent(skinParent.transform);

                skinParent.transform.localPosition = new Vector3(0, -0.45f, 1.75f);
                skinParent.transform.localRotation = Quaternion.Euler(0, -20, 0);
            }
            else
            {
                closedSkins.Add(i);
                skinContainer.Find("Skin Image").gameObject.SetActive(false);
                skinContainer.Find("Question Mark").gameObject.SetActive(true);
                skinsContainer.transform.GetChild(i).Find("Background").GetComponent<Image>().color = gradientStart;
                skinCamera.gameObject.SetActive(false);
            }
        }
        for (int i = newSkins.Count; i < 6; i++)
        {
            Debug.Log(i);
            var skinContainer = skinsContainer.transform.GetChild(i);
            skinContainer.gameObject.SetActive(false);
        }
    }

    public void OnDrag(PointerEventData data)
    {
        if (unlocking)
            return;
        var drag = data.delta.x;
        drag += drag / 5f;
        lastDrag = data.delta.x;
        ;
        var newPos = new Vector3(skinsContainer.transform.localPosition.x + drag, 0, 0);
        if (currentDot == 0 && drag > 0)
        {
            if (newPos.x <= MAX_LEFT)
            {
                DragPositionX(drag);
            }
            else
            {
                DragToPositionX(MAX_LEFT);
            }
        }
        else if (currentDot == amountOfDots - 1 && drag < 0)
        {
            if (newPos.x >= -MAX_LEFT)
            {
                DragPositionX(drag);
            }
            else
            {
                DragToPositionX(-MAX_LEFT);
            }
        }
        else
        {
            if (!border && !swaped)
            {
                if (skinsContainer.transform.localPosition.x < -MAX_LEFT / 2)
                {
                    Swap(false);
                }
                else if (skinsContainer.transform.localPosition.x > MAX_LEFT / 2)
                {
                    Swap(true);
                }
            }
            DragPositionX(drag);
        }
    }

    private void Swap(bool left)
    {
        if (left)
        {
            DisablePage(mainSkinsCamerasContainer, skinsContainerCanvas);
            DisablePage(sideSkinsCamerasContainer, rightSkinsContainer);

            SetUpPage(sideSkinsCamerasContainer, rightSkinsContainer, currentDot);
            SetUpPage(mainSkinsCamerasContainer, skinsContainerCanvas, currentDot - 1);

            rightSkinsContainer.alpha = skinsContainerCanvas.alpha;
            skinsContainerCanvas.alpha = leftSkinsContainer.alpha;

            rightSkinsContainer.transform.localPosition = new Vector3(skinsContainer.transform.localPosition.x, 0);
            skinsContainer.transform.localPosition = new Vector3(skinsContainer.transform.localPosition.x - 800, 0);
            leftSkinsContainer.transform.localPosition = new Vector3(skinsContainer.transform.localPosition.x - 800, 0);
            currentDot--;
            if (currentDot != 0)
            {
                leftSkinsContainer.alpha = 0.2f;
            }
            else
            {
                leftSkinsContainer.alpha = 0.0f;
            }
        }
        else
        {
            DisablePage(mainSkinsCamerasContainer, skinsContainerCanvas);
            DisablePage(sideSkinsCamerasContainer, leftSkinsContainer);

            SetUpPage(sideSkinsCamerasContainer, leftSkinsContainer, currentDot);
            SetUpPage(mainSkinsCamerasContainer, skinsContainerCanvas, currentDot + 1);

            leftSkinsContainer.transform.localPosition = new Vector3(skinsContainer.transform.localPosition.x, 0);
            skinsContainer.transform.localPosition = new Vector3(skinsContainer.transform.localPosition.x + 800, 0);
            rightSkinsContainer.transform.localPosition = new Vector3(skinsContainer.transform.localPosition.x + 800, 0);
            currentDot++;
            leftSkinsContainer.alpha = skinsContainerCanvas.alpha;
            skinsContainerCanvas.alpha = rightSkinsContainer.alpha;
            if (currentDot != amountOfDots - 1)
            {
                rightSkinsContainer.alpha = 0.2f;
            }
            else
            {
                rightSkinsContainer.alpha = 0.0f;
            }
        }
        swaped = true;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        newDrag = false;
        StartCoroutine(ReturPosToStart());

    }

    IEnumerator ReturPosToStart()
    {
        float diff = skinsContainer.transform.localPosition.x;
        do
        {

            if (newDrag)
            {
                break;
            }
            if (diff <= 0.5f && diff >= -0.5f)
            {
                diff = 0;
            }
            else
            {
                diff *= 0.8f;
            }
            DragPositionX((diff - skinsContainer.transform.localPosition.x));

            diff = skinsContainer.transform.localPosition.x;
            yield return new WaitForFixedUpdate();
        } while (diff != 0);


        skinsContainerCanvas.alpha = 1;
        if (currentDot != 0)
        {
            leftSkinsContainer.alpha = 0.2f;
        }
        else
        {
            leftSkinsContainer.alpha = 0f;
        }

        if (currentDot != amountOfDots - 1)
        {
            rightSkinsContainer.alpha = 0.2f;
        }
        else
        {
            rightSkinsContainer.alpha = 0f;
        }

        for (int i = 0; i < dotsList.Count; i++)
        {
            var color = dotsList[i].color;
            if (currentDot == i)
            {
                color.a = 1;
            }
            else
            {
                color.a = 0.2f;
            }
            dotsList[i].color = color;
        }

    }

    private void DragPositionX(float amount)
    {
        var scaledAmount = amount * Time.deltaTime * dragMultiplier;
        skinsContainer.transform.localPosition = new Vector3(skinsContainer.transform.localPosition.x + scaledAmount,
                skinsContainer.transform.localPosition.y,
                skinsContainer.transform.localPosition.z);

        if (rightSkinsContainer.gameObject.activeSelf)
        {
            rightSkinsContainer.transform.localPosition = new Vector3(rightSkinsContainer.transform.localPosition.x + scaledAmount,
                    rightSkinsContainer.transform.localPosition.y,
                    rightSkinsContainer.transform.localPosition.z);
        }

        if (leftSkinsContainer.gameObject.activeSelf)
        {
            leftSkinsContainer.transform.localPosition = new Vector3(leftSkinsContainer.transform.localPosition.x + scaledAmount,
                    leftSkinsContainer.transform.localPosition.y,
                    leftSkinsContainer.transform.localPosition.z);
        }


        var absAmount = Mathf.Abs(scaledAmount);
        if (currentDot == amountOfDots - 1 && border)
            absAmount *= -1;
        if (swaped)
            absAmount *= -1;

        if (scaledAmount > 0)
        {
            if (amountOfDots == 1)
            {
                if (borderLeft)
                {
                    skinsContainerCanvas.alpha += transparencyPerPixel * absAmount;
                }
                else
                {
                    skinsContainerCanvas.alpha -= transparencyPerPixel * absAmount;
                }

            }
            else if (leftNew == border)
            {
                skinsContainerCanvas.alpha += transparencyPerPixel * absAmount;
            }
            else
            {

                skinsContainerCanvas.alpha -= transparencyPerPixel * absAmount;
            }


            if (currentDot != amountOfDots - 1 && (leftNew || !swaped))
            {
                rightSkinsContainer.alpha -= transparencyPerPixel * Mathf.Abs(scaledAmount);
            }
            if (currentDot != 0 && (!leftNew || !swaped))
            {
                leftSkinsContainer.alpha += transparencyPerPixel * Mathf.Abs(scaledAmount);
            }

        }
        else
        {
            if (amountOfDots == 1)
            {
                if (borderLeft)
                {
                    skinsContainerCanvas.alpha -= transparencyPerPixel * absAmount;
                }
                else
                {
                    skinsContainerCanvas.alpha += transparencyPerPixel * absAmount;
                }

            }
            else
            if (leftNew == border)
            {
                skinsContainerCanvas.alpha -= transparencyPerPixel * absAmount;
            }
            else
            {
                skinsContainerCanvas.alpha += transparencyPerPixel * absAmount;
            }

            if (currentDot != amountOfDots - 1 && (leftNew || !swaped))
            {
                rightSkinsContainer.alpha += transparencyPerPixel * Mathf.Abs(scaledAmount);
            }
            if (currentDot != 0 && (!leftNew || !swaped))
            {
                leftSkinsContainer.alpha -= transparencyPerPixel * Mathf.Abs(scaledAmount);
            }
        }

        var transparency = transparencyPerPixel * Mathf.Abs(scaledAmount);
        if (swaped)
            transparency *= -1;

        if (!border)
        {
            if (leftNew)
            {

                Color currentDotColor = dotsList[currentDot].color;
                Color leftDotColor;
                if (swaped)
                {
                    leftDotColor = dotsList[currentDot + 1].color;
                }
                else
                {
                    leftDotColor = dotsList[currentDot - 1].color;
                }
                if (scaledAmount > 0)
                {
                    currentDotColor.a -= transparency;
                    leftDotColor.a += transparency;
                }
                else
                {
                    currentDotColor.a += transparency;
                    leftDotColor.a -= transparency;
                }
                dotsList[currentDot].color = currentDotColor;
                if (swaped)
                {
                    dotsList[currentDot + 1].color = leftDotColor;
                }
                else
                {
                    dotsList[currentDot - 1].color = leftDotColor;
                }
            }
            else
            {
                var currentDotColor = dotsList[currentDot].color;
                Color rightDotColor;
                if (swaped)
                {
                    rightDotColor = dotsList[currentDot - 1].GetComponent<Image>().color;
                }
                else
                {
                    rightDotColor = dotsList[currentDot + 1].color;
                }
                if (scaledAmount > 0)
                {
                    currentDotColor.a += transparency;
                    rightDotColor.a -= transparency;
                }
                else
                {
                    currentDotColor.a -= transparency;
                    rightDotColor.a += transparency;
                }
                dotsContainer.transform.GetChild(currentDot).GetComponent<Image>().color = currentDotColor;
                if (swaped)
                {
                    dotsList[currentDot - 1].color = rightDotColor;
                }
                else
                {
                    dotsList[currentDot + 1].color = rightDotColor;
                }
            }
        }
    }

    private void DragToPositionX(float pos)
    {
        skinsContainer.transform.localPosition = new Vector3(pos,
                skinsContainer.transform.localPosition.y,
                skinsContainer.transform.localPosition.z);

        if (rightSkinsContainer.gameObject.activeSelf)
        {
            rightSkinsContainer.transform.localPosition = new Vector3(pos + 800,
                    rightSkinsContainer.transform.localPosition.y,
                    rightSkinsContainer.transform.localPosition.z);
        }

        if (leftSkinsContainer.gameObject.activeSelf)
        {
            leftSkinsContainer.transform.localPosition = new Vector3(pos - 800,
                    leftSkinsContainer.transform.localPosition.y,
                    leftSkinsContainer.transform.localPosition.z);
        }
    }
}