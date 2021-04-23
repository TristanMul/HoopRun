#pragma warning disable 0649 

using UnityEngine;
using UnityEngine.UI;
using Watermelon;

public class AudioToggleButton : MonoBehaviour
{
    [SerializeField]
    public Image graphic;

    [Space]
    [SerializeField]
    private Sprite soundsOnIcon;
    [SerializeField]
    private Sprite musicOffIcon;
    [SerializeField]
    private Sprite audioOffIcon;

    [Space]
    [SerializeField]
    private Color activeColor = Color.white;
    [SerializeField]
    private Color disableColor = Color.white;

    private State state = State.SoundsOn;

    private void Start()
    {
        if(AudioController.GetMusicVolume() == 1.0f)
        {
            state = State.SoundsOn;
            graphic.sprite = soundsOnIcon;
            graphic.color = activeColor;
        }
        else
        {
            if (AudioController.GetSoundVolume() == 1.0f)
            {
                state = State.MusicOff;
                graphic.sprite = musicOffIcon;
                graphic.color = activeColor;
            }
            else
            {
                state = State.AudioOff;
                graphic.sprite = audioOffIcon;
                graphic.color = disableColor;
            }
        }
    }

    public void SwitchState()
    {
        switch(state)
        {
            case State.SoundsOn:
                graphic.sprite = musicOffIcon;
                graphic.color = activeColor;

                AudioController.SetMusicVolume(0.0f);

                state = State.MusicOff;
                break;
            case State.MusicOff:
                graphic.sprite = audioOffIcon;
                graphic.color = disableColor;

                AudioController.SetSoundVolume(0.0f);

                state = State.AudioOff;
                break;
            case State.AudioOff:
                graphic.sprite = soundsOnIcon;
                graphic.color = activeColor;

                AudioController.SetMusicVolume(1.0f);
                AudioController.SetSoundVolume(1.0f);

                state = State.SoundsOn;
                break;
        }
    }

    private enum State
    {
        SoundsOn = 0,
        MusicOff = 1,
        AudioOff = 2
    }
}
