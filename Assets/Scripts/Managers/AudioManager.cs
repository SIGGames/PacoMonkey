using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using static Utils.PlayerPrefsKeys;

namespace Managers {
    public class AudioManager : MonoBehaviour {
        public AudioMixer audioMixer;
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private TMP_InputField masterVolumeInput;
        [SerializeField] private TMP_InputField musicVolumeInput;
        [SerializeField] private TMP_InputField sfxVolumeInput;

        private const float DefaultMasterVolume = 8f;
        private const float DefaultMusicVolume = 6f;
        private const float DefaultSfxVolume = 6f;
        private const float MinVolume = 0f;
        private const float MaxVolume = 10f;

        private void Awake() {
            if (masterVolumeSlider == null || musicSlider == null || sfxSlider == null) {
                Debug.LogError("One or more sliders are not assigned in the inspector");
                enabled = false;
            }

            if (masterVolumeInput == null || musicVolumeInput == null || sfxVolumeInput == null) {
                Debug.LogError("One or more input fields are not assigned in the inspector");
                enabled = false;
            }
        }

        private void Start() {
            float globalVolume = PlayerPrefs.GetFloat(MasterVolumeKey, DefaultMasterVolume);
            float musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, DefaultMusicVolume);
            float sfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, DefaultSfxVolume);

            InitializeSliders(globalVolume, musicVolume, sfxVolume);
            InitializeInputs(globalVolume, musicVolume, sfxVolume);
        }

        private void InitializeSliders(float globalVolume, float musicVolume, float sfxVolume) {
            masterVolumeSlider.value = globalVolume;
            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
            masterVolumeSlider.onValueChanged.AddListener(delegate {
                UpdateInputFromSlider(masterVolumeSlider, masterVolumeInput);
            });

            musicSlider.value = musicVolume;
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
            musicSlider.onValueChanged.AddListener(delegate { UpdateInputFromSlider(musicSlider, musicVolumeInput); });

            sfxSlider.value = sfxVolume;
            sfxSlider.onValueChanged.AddListener(SetSfxVolume);
            sfxSlider.onValueChanged.AddListener(delegate { UpdateInputFromSlider(sfxSlider, sfxVolumeInput); });
        }

        private void InitializeInputs(float globalVolume, float musicVolume, float sfxVolume) {
            masterVolumeInput.text = globalVolume.ToString(CultureInfo.InvariantCulture);
            masterVolumeInput.onValueChanged.AddListener(delegate { OnMasterVolumeInputChanged(); });

            musicVolumeInput.text = musicVolume.ToString(CultureInfo.InvariantCulture);
            musicVolumeInput.onValueChanged.AddListener(delegate { OnMusicVolumeInputChanged(); });

            sfxVolumeInput.text = sfxVolume.ToString(CultureInfo.InvariantCulture);
            sfxVolumeInput.onValueChanged.AddListener(delegate { OnSfxVolumeInputChanged(); });
        }

        private void SetMasterVolume(float value) {
            audioMixer.SetFloat(MasterVolumeKey, GetNormalizedVolume(value));
            PlayerPrefs.SetFloat(MasterVolumeKey, value);
            SaveAudioSettings();
        }

        private void SetMusicVolume(float value) {
            audioMixer.SetFloat(MusicVolumeKey, GetNormalizedVolume(value));
            PlayerPrefs.SetFloat(MusicVolumeKey, value);
            SaveAudioSettings();
        }

        private void SetSfxVolume(float value) {
            audioMixer.SetFloat(SfxVolumeKey, GetNormalizedVolume(value));
            PlayerPrefs.SetFloat(SfxVolumeKey, value);
            SaveAudioSettings();
        }

        private static float GetNormalizedVolume(float value) {
            // On min volume mute the sound
            if (Math.Abs(value - MinVolume) < 0.01f) {
                return -80f;
            }
            // Convert volume to decibels
            return Mathf.Log10(Mathf.Clamp(value, MinVolume, MaxVolume)) * 20f;
        }

        private static void SaveAudioSettings() {
            PlayerPrefs.Save();
        }

        private void OnMasterVolumeInputChanged() {
            if (float.TryParse(masterVolumeInput.text, out float value)) {
                value = Mathf.Clamp(value, MinVolume, MaxVolume);
                masterVolumeInput.text = value.ToString(CultureInfo.InvariantCulture);
                SetMasterVolume(value);
            }
        }

        private void OnMusicVolumeInputChanged() {
            if (float.TryParse(musicVolumeInput.text, out float value)) {
                value = Mathf.Clamp(value, MinVolume, MaxVolume);
                musicVolumeInput.text = value.ToString(CultureInfo.InvariantCulture);
                SetMusicVolume(value);
            }
        }

        private void OnSfxVolumeInputChanged() {
            if (float.TryParse(sfxVolumeInput.text, out float value)) {
                value = Mathf.Clamp(value, MinVolume, MaxVolume);
                sfxVolumeInput.text = value.ToString(CultureInfo.InvariantCulture);
                SetSfxVolume(value);
            }
        }

        private static void UpdateInputFromSlider(Slider slider, TMP_InputField inputField) {
            inputField.text = slider.value.ToString(CultureInfo.InvariantCulture);
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