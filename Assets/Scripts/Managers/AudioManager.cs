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

        private const float DefaultMasterVolume = 8f;
        private const float DefaultMusicVolume = 6f;
        private const float DefaultSfxVolume = 6f;

        private void Start() {
            float globalVolume = PlayerPrefs.GetFloat(MasterVolumeKey, DefaultMasterVolume);
            float musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, DefaultMusicVolume);
            float sfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, DefaultSfxVolume);

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

        private void SetMasterVolume(float value) {
            audioMixer.SetFloat(MasterVolumeKey, Mathf.Log10(value) * 20);
            PlayerPrefs.SetFloat(MasterVolumeKey, value);
            SaveAudioSettings();
        }

        private void SetMusicVolume(float value) {
            audioMixer.SetFloat(MasterVolumeKey, Mathf.Log10(value) * 20);
            PlayerPrefs.SetFloat(MusicVolumeKey, value);
            SaveAudioSettings();
        }

        private void SetSfxVolume(float value) {
            audioMixer.SetFloat(MasterVolumeKey, Mathf.Log10(value) * 20);
            PlayerPrefs.SetFloat(SfxVolumeKey, value);
            SaveAudioSettings();
        }

        private static void SaveAudioSettings() {
            PlayerPrefs.Save();
        }

        public void ResetAudioSettings() {
            PlayerPrefs.DeleteKey(MasterVolumeKey);
            PlayerPrefs.DeleteKey(MusicVolumeKey);
            PlayerPrefs.DeleteKey(SfxVolumeKey);
            PlayerPrefs.Save();

            masterVolumeSlider.value = DefaultMasterVolume;
            musicSlider.value = DefaultMusicVolume;
            sfxSlider.value = DefaultSfxVolume;
        }
    }
}