#pragma warning disable 649

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GradientBackground : MonoBehaviour
{
    [SerializeField]
    private RawImage backgroundImage;

    [Space]
    [SerializeField]
    private Color startColor = Color.white;
    [SerializeField]
    private Color endColor = Color.white;

    [Space]
    [SerializeField]
    private bool initOnAwake = true;

    private Texture2D backgroundTexture;

    private void Awake()
    {
        backgroundImage.color = Color.white;

        backgroundTexture = new Texture2D(1, 9);
        backgroundTexture.wrapMode = TextureWrapMode.Clamp;
        backgroundTexture.filterMode = FilterMode.Bilinear;

        if(initOnAwake)
            SetColor(startColor, endColor);
    }

    public void SetColor(Color color1, Color color2)
    {
        startColor = color1;
        endColor = color2;
        backgroundTexture.SetPixel(0, 0, startColor);
        backgroundTexture.SetPixel(0, 1, Color.Lerp(startColor, endColor, 0.125f));
        backgroundTexture.SetPixel(0, 2, Color.Lerp(startColor, endColor, 0.250f));
        backgroundTexture.SetPixel(0, 3, Color.Lerp(startColor, endColor, 0.375f));
        backgroundTexture.SetPixel(0, 4, Color.Lerp(startColor, endColor, 0.500f));
        backgroundTexture.SetPixel(0, 5, Color.Lerp(startColor, endColor, 0.625f));
        backgroundTexture.SetPixel(0, 6, Color.Lerp(startColor, endColor, 0.750f));
        backgroundTexture.SetPixel(0, 7, Color.Lerp(startColor, endColor, 0.875f));
        backgroundTexture.SetPixel(0, 8, endColor);

        backgroundTexture.Apply();
        backgroundImage.texture = backgroundTexture;
    }

    public bool OverrideColor(Color color1, Color color2){
        
        bool result = startColor == color1 && endColor == color2;
        if(result) return result;

        startColor.r = startColor.r - color1.r > 0.01f ? startColor.r - 0.01f : startColor.r - color1.r < -0.01f ? startColor.r + 0.01f : color1.r;
        startColor.g = startColor.g - color1.g > 0.01f ? startColor.g - 0.01f : startColor.g - color1.g < -0.01f ? startColor.g + 0.01f : color1.g;
        startColor.b = startColor.b - color1.b > 0.01f ? startColor.b - 0.01f : startColor.b - color1.b < -0.01f ? startColor.b + 0.01f : color1.b;

        endColor.r = endColor.r - color2.r > 0.01f ? endColor.r - 0.01f : endColor.r - color2.r < -0.01f ? endColor.r + 0.01f : color2.r;
        endColor.g = endColor.g - color2.g > 0.01f ? endColor.g - 0.01f : endColor.g - color2.g < -0.01f ? endColor.g + 0.01f : color2.g;
        endColor.b = endColor.b - color2.b > 0.01f ? endColor.b - 0.01f : endColor.b - color2.b < -0.01f ? endColor.b + 0.01f : color2.b;

        backgroundTexture.SetPixel(0, 0, startColor);
        backgroundTexture.SetPixel(0, 1, Color.Lerp(startColor, endColor, 0.125f));
        backgroundTexture.SetPixel(0, 2, Color.Lerp(startColor, endColor, 0.250f));
        backgroundTexture.SetPixel(0, 3, Color.Lerp(startColor, endColor, 0.375f));
        backgroundTexture.SetPixel(0, 4, Color.Lerp(startColor, endColor, 0.500f));
        backgroundTexture.SetPixel(0, 5, Color.Lerp(startColor, endColor, 0.625f));
        backgroundTexture.SetPixel(0, 6, Color.Lerp(startColor, endColor, 0.750f));
        backgroundTexture.SetPixel(0, 7, Color.Lerp(startColor, endColor, 0.875f));
        backgroundTexture.SetPixel(0, 8, endColor);

        backgroundTexture.Apply();
        backgroundImage.texture = backgroundTexture;

        return result;
    }

    public static bool TransitionFromTo(Color from, Color to, out Color result){
        result = from;

        result.r = from.r - to.r > 0.01f ? from.r - 0.01f : from.r - to.r < -0.01f ? from.r + 0.01f : to.r;
        result.g = from.g - to.g > 0.01f ? from.g - 0.01f : from.g - to.g < -0.01f ? from.g + 0.01f : to.g;
        result.b = from.b - to.b > 0.01f ? from.b - 0.01f : from.b - to.b < -0.01f ? from.b + 0.01f : to.b;

        return result == to;
    }
}