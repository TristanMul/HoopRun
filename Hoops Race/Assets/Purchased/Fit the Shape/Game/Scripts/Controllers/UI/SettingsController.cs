using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Watermelon;

public class SettingsController : MonoBehaviour
{
    public CanvasGroup uiBackground;
    public Watermelon.AudioSettings audioSettings;

    [Space]
    public Image settingsContainer;
    public Image closeButtonSettings;

    [Space]
    public CanvasGroup hapticGroup;
    public CanvasGroup soundGroup;
    public CanvasGroup musicGroup;
    public CanvasGroup removeAdsGroup;

    [Space]
    public Text hapticText;
    public Text soundText;
    public Text musicText;
    public Text removeAdsText;
    public Text settingsText;
    public Text closeText;

    [Space]
    public Toggle hapticToggle;
    public Toggle soundToggle;
    public Toggle musicToggle;

    [Space]
    public Image hapticToggleBackground;
    public Image soundToggleBackground;
    public Image musicToggleBackground;

    private bool isOpening = false;


    void Start()
    {
        settingsText.text = Multilanguage.GetWord("settings.settings");
        hapticText.text = Multilanguage.GetWord("settings.haptic");
        soundText.text = Multilanguage.GetWord("settings.sound");
        musicText.text = Multilanguage.GetWord("settings.music");
        removeAdsText.text = Multilanguage.GetWord("settings.remove_ads");
        closeText.text = Multilanguage.GetWord("default.close");
    }

    public void OpenSettings()
    {
        isOpening = true;
        StartCoroutine(SettingsInit());
    }

    IEnumerator SettingsInit()
    {
        settingsText.DOFade(0f, 0.00001f);
        uiBackground.DOFade(1, 0.25f);
        settingsContainer.gameObject.SetActive(true);
        settingsContainer.rectTransform.DOLocalMove(new Vector3(0, -200, 0), 0.25f).OnComplete(delegate
        {
            settingsContainer.rectTransform.DOLocalMove(new Vector3(0, 0, 0), 0.12f).SetEasing(Ease.Type.BackOut);
        });
        yield return new WaitForSeconds(0.2f);
        closeButtonSettings.gameObject.SetActive(true);
        closeButtonSettings.rectTransform.DOLocalMove(new Vector3(0, -540, 0), 0.3f).OnComplete(delegate
        {
            closeButtonSettings.rectTransform.DOLocalMove(new Vector3(0, -420, 0), 0.2f).SetEasing(Ease.Type.BackOut);
        });
        yield return new WaitForSeconds(0.2f);

        settingsText.DOFade(1f, 0.15f);

        hapticGroup.DOFade(1f, 0.15f);
        soundGroup.DOFade(1f, 0.15f);
        musicGroup.DOFade(1f, 0.15f);
        removeAdsGroup.DOFade(1f, 0.15f);
    }

    public delegate void AfterClose();

    public void CloseSettings(AfterClose afterClose)
    {
        isOpening = false;
        settingsContainer.rectTransform.DOLocalMove(new Vector3(0, -1500, 0), 0.3f);
        closeButtonSettings.rectTransform.DOLocalMove(new Vector3(0, -1920, 0), 0.3f);
        uiBackground.DOFade(0, 0.3f).OnComplete(delegate
        {
            afterClose();
        });
        settingsText.DOFade(0f, 0.2f);
        hapticGroup.DOFade(0f, 0.2f);
        soundGroup.DOFade(0, 0.2f);
        musicGroup.DOFade(0f, 0.2f);
        removeAdsGroup.DOFade(0f, 0.2f);
    }

    public void ChangeHapticSettings()
    {
        GameController.instance.isHaptic = !hapticToggle.isOn;
        PlayerPrefs.SetInt("isHaptic", GameController.instance.isHaptic ? 1 : 0);
        if (GameController.instance.isSound && isOpening)
        {
            AudioController.PlaySound(audioSettings.sounds.toggle, AudioController.AudioType.Sound, 1);
        }
    }

    public void ChangeSoundSettings()
    {
        GameController.instance.isSound = !soundToggle.isOn;
        PlayerPrefs.SetInt("isSound", GameController.instance.isSound ? 1 : 0);
        if (GameController.instance.isSound && isOpening)
        {
            AudioController.PlaySound(audioSettings.sounds.toggle, AudioController.AudioType.Sound, 1);
        }
    }

    public void ChangeMusicSettings()
    {
        GameController.instance.isMusic = !musicToggle.isOn;
        PlayerPrefs.SetInt("isMusic", GameController.instance.isMusic ? 1 : 0);
        if (GameController.instance.isSound && isOpening)
        {
            AudioController.PlaySound(audioSettings.sounds.toggle, AudioController.AudioType.Sound, 1);
        }
    }

    public void Background(Color start, Color end)
    {

        hapticToggle.isOn = !GameController.instance.isHaptic;
        hapticToggleBackground.color = end;

        soundToggle.isOn = !GameController.instance.isSound;
        soundToggleBackground.color = end;

        musicToggle.isOn = !GameController.instance.isMusic;
        musicToggleBackground.color = end;
    }

    public void RemoveAds()
    {
        if (GameController.instance.isSound)
        {
            AudioController.PlaySound(audioSettings.sounds.button, AudioController.AudioType.Sound, 0.8f, 1.2f);
        }
    }
}