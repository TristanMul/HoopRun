using UnityEngine;
using Watermelon.Core;

namespace Watermelon
{
    [SetupTab("Audio", texture = "icon_audio")]
    [CreateAssetMenu(fileName = "Audio Settings", menuName = "Settings/Audio Settings")]
    public class AudioSettings : ScriptableObject
    {
        [System.Serializable]
        public class Sounds
        {
            public AudioClip toggle;
            public AudioClip turn1;
            public AudioClip turn2;
            public AudioClip obstacle;
            public AudioClip gem;
            public AudioClip finish;
            public AudioClip button;
            public AudioClip bait;
            public AudioClip explosion;
            public AudioClip checkmark;
        }

        [System.Serializable]
        public class Vibrations
        {
            public int shortVibration;
            public int longVibration;
        }
        
        public bool isMusicEnabled = true;
        public bool isAudioEnabled = true;
        
        public AudioClip[] musicAudioClips;
        
        public Sounds sounds;
        public Vibrations vibrations;
    }
}