#pragma warning disable 649

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Watermelon;

public class StartController : MonoBehaviour
{
    [SerializeField]
    private CanvasGroup startCanvas;

    [Space]
    public RectTransform handBackgroung;
    public Transform handTopArrow;
    public Transform handImage;
    public Transform handLine;

    [Space]
    public Text settingsText;
    public Text customizationText;
    public Text slideText;

    private void Start()
    {
        settingsText.text = Multilanguage.GetWord("start.settings");
        customizationText.text = Multilanguage.GetWord("start.customization");
        slideText.text = Multilanguage.GetWord("start.slide") + "\n" + Multilanguage.GetWord("start.up&down");
    }

    public void StartGame()
    {
        startCanvas.DOFade(1, 0.5f).OnComplete(delegate
        {
            StartCoroutine(ScaleStart());
        });
    }

    public IEnumerator ScaleStart()
    {
        while (gameObject.activeSelf)
        {
            handBackgroung.DOSize(new Vector3(250, 220, 0), 0.5f).SetEasing(Ease.Type.BackInOut);

            handLine.DOScaleX(0.5f, 0.5f).SetEasing(Ease.Type.BackInOut);

            handTopArrow.DOLocalMove(new Vector3(0, 25, 0), 0.5f).SetEasing(Ease.Type.BackInOut);
            handImage.DOLocalMove(new Vector3(10, -50, 0), 0.5f).SetEasing(Ease.Type.BackInOut);

            yield return new WaitForSeconds(1);

            if (!gameObject.activeSelf)
                break;

            handBackgroung.DOSize(new Vector3(200, 340, 0), 0.5f).SetEasing(Ease.Type.BackInOut);

            handLine.DOScaleX(1f, 0.5f).SetEasing(Ease.Type.BackInOut);

            handTopArrow.DOLocalMove(new Vector3(0, 140, 0), 0.5f).SetEasing(Ease.Type.BackInOut);
            handImage.DOLocalMove(new Vector3(10, 65, 0), 0.5f).SetEasing(Ease.Type.BackInOut);

            yield return new WaitForSeconds(1);
        }
    }
}