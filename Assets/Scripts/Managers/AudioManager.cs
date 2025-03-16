using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using static Utils.PlayerPrefsKeys;

namespace Managers {
    public class AudioManager : MonoBehaviour {
        public AudioMixer audioMixer;
        public Slider masterVolumeSlider;
        public Slider musicSlider;
        public Slider sfxSlider;

        private void Start() {
            float globalVolume = PlayerPrefs.GetFloat(MasterVolumeKey, 8f);
            float musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 6f);
            float sfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, 6f);

            if (masterVolumeSlider != null) {
                masterVolumeSlider.value = globalVolume;
                masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
            }

            if (musicSlider != null) {
                musicSlider.value = musicVolume;
                musicSlider.onValueChanged.AddListener(SetMusicVolume);
            }

            if (sfxSlider != null) {
                sfxSlider.value = sfxVolume;
                sfxSlider.onValueChanged.AddListener(SetSfxVolume);
            }

            SetMasterVolume(globalVolume);
            SetMusicVolume(musicVolume);
            SetSfxVolume(sfxVolume);
        }

        public void SetMasterVolume(float value) {
            audioMixer.SetFloat(MasterVolumeKey, Mathf.Log10(value) * 20);
            PlayerPrefs.SetFloat(MasterVolumeKey, value);
        }

        public void SetMusicVolume(float value) {
            audioMixer.SetFloat(MasterVolumeKey, Mathf.Log10(value) * 20);
            PlayerPrefs.SetFloat(MusicVolumeKey, value);
        }

        public void SetSfxVolume(float value) {
            audioMixer.SetFloat(MasterVolumeKey, Mathf.Log10(value) * 20);
            PlayerPrefs.SetFloat(SfxVolumeKey, value);
        }
    }
}