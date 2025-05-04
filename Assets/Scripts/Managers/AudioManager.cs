using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using static Utils.PlayerPrefsKeys;
using Random = UnityEngine.Random;

namespace Managers {
    public class AudioManager : MonoBehaviour {
        public static AudioManager Instance { get; private set; }

        public AudioMixer audioMixer;

        [Header("Audio Components")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private TMP_InputField masterVolumeInput;
        [SerializeField] private TMP_InputField musicVolumeInput;
        [SerializeField] private TMP_InputField sfxVolumeInput;

        [Header("Music Settings")]
        [SerializeField] private string currentMusicTitle;
        [SerializeField] private List<AudioClip> onMenuMusics;
        [SerializeField] private List<InGameMusic> onGameMusics;

        private MusicType _currentMusicType;
        private AudioSource _audioSource;

        private int _menuMusicIndex;
        private int _gameMusicIndex;

        [Header("Volume Settings")]
        [SerializeField, Range(MinVolume, MaxVolume)]
        private float currentMasterVolume = DefaultMasterVolume;

        [SerializeField, Range(MinVolume, MaxVolume)]
        private float currentMusicVolume = DefaultMusicVolume;

        [SerializeField, Range(MinVolume, MaxVolume)]
        private float currentSfxVolume = DefaultSfxVolume;

        [SerializeField, Range(0, 1)]
        private float volumeReducer = 1;

        private const float DefaultMasterVolume = 8f;
        private const float DefaultMusicVolume = 6f;
        private const float DefaultSfxVolume = 6f;
        private const float MinVolume = 0f;
        private const float MaxVolume = 10f;

        private void Awake() {
            if (Instance == null) {
                Instance = this;
            } else {
                Destroy(gameObject);
            }

            if (masterVolumeSlider == null || musicSlider == null || sfxSlider == null) {
                Debug.LogError("One or more sliders are not assigned in the inspector");
                enabled = false;
            }

            if (masterVolumeInput == null || musicVolumeInput == null || sfxVolumeInput == null) {
                Debug.LogError("One or more input fields are not assigned in the inspector");
                enabled = false;
            }

            _audioSource = GetComponent<AudioSource>();
        }

        private void Start() {
            float globalVolume = PlayerPrefs.GetFloat(MasterVolumeKey, DefaultMasterVolume);
            float musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, DefaultMusicVolume);
            float sfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, DefaultSfxVolume);

            InitializeSliders(globalVolume, musicVolume, sfxVolume);
            InitializeInputs(globalVolume, musicVolume, sfxVolume);

            SetMasterVolume(globalVolume);
            SetMusicVolume(musicVolume);
            SetSfxVolume(sfxVolume);
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
            currentMasterVolume = value;
            audioMixer.SetFloat(MasterVolumeKey, GetNormalizedVolume(value));
            PlayerPrefs.SetFloat(MasterVolumeKey, value);
            SaveAudioSettings();

            // Update music and sfx volume since they are dependent on the master volume
            SetMusicVolume(currentMusicVolume);
            SetSfxVolume(currentSfxVolume);
        }

        private void SetMusicVolume(float value) {
            float scaledVolume = value * currentMasterVolume / MaxVolume;

            audioMixer.SetFloat(MusicVolumeKey, GetNormalizedVolume(scaledVolume));
            currentMusicVolume = value;
            PlayerPrefs.SetFloat(MusicVolumeKey, value);
            SaveAudioSettings();
        }

        private void SetSfxVolume(float value) {
            float scaledVolume = value * currentMasterVolume / MaxVolume;

            audioMixer.SetFloat(SfxVolumeKey, GetNormalizedVolume(scaledVolume));
            currentSfxVolume = value;
            PlayerPrefs.SetFloat(SfxVolumeKey, value);
            SaveAudioSettings();
        }

        private float GetNormalizedVolume(float value) {
            // On min volume mute the sound
            if (Math.Abs(value - MinVolume) < 0.01f) {
                return -80f;
            }

            // Convert volume to decibels
            return Mathf.Log10(Mathf.Clamp(value, MinVolume, MaxVolume)) * 20f * volumeReducer;
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

            currentMasterVolume = DefaultMasterVolume;
            currentMusicVolume = DefaultMusicVolume;
            currentSfxVolume = DefaultSfxVolume;
        }

        public void DisableCarrets() {
            if (masterVolumeInput == null || musicVolumeInput == null || sfxVolumeInput == null) {
                return;
            }

            TMP_SelectionCaret masterVolumeCaret = masterVolumeInput.GetComponentInChildren<TMP_SelectionCaret>();
            TMP_SelectionCaret musicVolumeCaret = musicVolumeInput.GetComponentInChildren<TMP_SelectionCaret>();
            TMP_SelectionCaret sfxVolumeCaret = sfxVolumeInput.GetComponentInChildren<TMP_SelectionCaret>();

            if (masterVolumeCaret != null) {
                masterVolumeCaret.enabled = false;
            }

            if (musicVolumeCaret != null) {
                musicVolumeCaret.enabled = false;
            }

            if (sfxVolumeCaret != null) {
                sfxVolumeCaret.enabled = false;
            }
        }

        public void PlayMusic(MusicType musicType, bool random = true) {
            _currentMusicType = musicType;

            List<AudioClip> selectedMusics = musicType == MusicType.Menu ? onMenuMusics : onGameMusics.ConvertAll(m => m.audioClip);
            AudioClip audioClip;

            if (random) {
                audioClip = GetRandomAudioClip(selectedMusics);
            } else {
                int index = musicType == MusicType.Menu
                    ? _menuMusicIndex++ % selectedMusics.Count
                    : _gameMusicIndex++ % selectedMusics.Count;
                audioClip = selectedMusics[index];
            }

            if (audioClip != null) {
                _audioSource.clip = audioClip;
                currentMusicTitle = audioClip.name;
                _audioSource.Play();

                StopAllCoroutines();
                StartCoroutine(PlayNextAfterDelay(audioClip.length));
            }
        }

        private IEnumerator PlayNextAfterDelay(float delay) {
            yield return new WaitForSecondsRealtime(delay + 0.1f);
            PlayMusic(_currentMusicType);
        }

        public static AudioClip GetRandomAudioClip(List<AudioClip> audioClips) {
            if (audioClips == null || audioClips.Count == 0) {
                return null;
            }

            int randomIndex = Random.Range(0, audioClips.Count);
            return audioClips[randomIndex];
        }
    }

    [Serializable]
    public enum MusicType {
        Menu,
        Game
    }

    [Serializable]
    public enum MusicSoundType {
        Calm,
        Action,
    }

    [Serializable]
    public class InGameMusic {
        public AudioClip audioClip;
        public MusicSoundType musicSoundType;
    }
}