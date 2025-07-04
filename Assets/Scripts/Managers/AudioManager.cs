﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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

        [Header("Music Settings")] // This is just for the inspector
        [SerializeField] private string currentMusicTitle;
        [SerializeField] private string musicProgress;

        [Header("> Menu Music")]
        [SerializeField] private bool isMenuMusicRandom;
        [SerializeField] private List<AudioClip> onMenuMusics;

        [Header("> Game Music")]
        [SerializeField] private bool isGameMusicRandom;
        [SerializeField] private List<InGameMusic> onGameMusics;

        private MusicType _currentMusicType;
        private MusicSoundType _currentMusicSoundType = MusicSoundType.Calm;
        private AudioSource _audioSource;
        private AudioClip _lastClip;

        private int _menuMusicIndex;
        private int _gameMusicIndex;
        private string _lastGameMusicName;
        private float _lastPlaybackTime;
        private bool _hasPlayedVictoryMusic;

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
            LoadSavedPlaybackInfo();

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
            StartCoroutine(UpdateMusicProgress());
        }

        private void Update() {
            if (_audioSource.time >= _audioSource.clip.length - 0.1f) {
                PlayNextTrackAfterFinished();
            }
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

        private AudioClip GetNextClip(MusicType musicType, MusicSoundType musicSoundType, bool avoidRepeat = true) {
            List<AudioClip> selectedMusics = GetSelectedMusics(musicType, musicSoundType);

            if (selectedMusics == null || selectedMusics.Count == 0) {
                return null;
            }

            bool isMusicRandom = musicType == MusicType.Menu ? isMenuMusicRandom : isGameMusicRandom;
            AudioClip nextClip;

            if (isMusicRandom) {
                nextClip = GetRandomAudioClipAvoidingRepeat(selectedMusics, _lastClip, avoidRepeat);
            } else {
                int musicIdx = musicType == MusicType.Menu
                    ? _menuMusicIndex++ % selectedMusics.Count
                    : _gameMusicIndex++ % selectedMusics.Count;

                nextClip = selectedMusics[musicIdx];
            }

            _lastClip = nextClip;
            return nextClip;
        }

        public void PlayMusic(MusicType musicType, MusicSoundType musicSoundType) {
            if (_hasPlayedVictoryMusic) {
                // After Victory, only menu music can be played
                if (musicType == MusicType.Menu) {
                    PlayNewMusic(MusicType.Menu, MusicSoundType.All);
                }
                return;
            }

            if (musicSoundType == MusicSoundType.Victory) {
                _hasPlayedVictoryMusic = true;
            }


            if (musicType == MusicType.Menu) {
                SaveCurrentPlayback();
            } else if (musicType == MusicType.Game && TryResumePreviousGameMusic(musicSoundType)) {
                return;
            }

            // Normal reproduction if we don't have any previous music saved
            PlayNewMusic(musicType, musicSoundType);
        }

        private bool TryResumePreviousGameMusic(MusicSoundType musicSoundType) {
            if (string.IsNullOrEmpty(_lastGameMusicName)) {
                return false;
            }

            // Check if the last music is still available
            AudioClip existingClip = GetSelectedMusics(MusicType.Game, musicSoundType)
                .FirstOrDefault(c => c != null && c.name == _lastGameMusicName);

            if (existingClip == null) {
                return false;
            }

            _currentMusicType = MusicType.Game;
            _audioSource.clip = existingClip;
            _audioSource.time = _lastPlaybackTime;
            _audioSource.Play();
            currentMusicTitle = existingClip.name;
            _currentMusicSoundType = musicSoundType;
            _lastClip = existingClip;
            SaveCurrentPlayback();

            StopAllCoroutines();
            StartCoroutine(UpdateMusicProgress());
            return true;
        }

        private void PlayNewMusic(MusicType musicType, MusicSoundType musicSoundType) {
            _currentMusicType = musicType;
            _currentMusicSoundType = musicSoundType;
            AudioClip audioClip = GetNextClip(musicType, musicSoundType, false);

            if (audioClip == null) {
                return;
            }

            _audioSource.clip = audioClip;
            _audioSource.time = 0f;
            _audioSource.Play();
            currentMusicTitle = audioClip.name;
            SaveCurrentPlayback();

            if (musicType == MusicType.Game) {
                _lastGameMusicName = audioClip.name;
                _lastPlaybackTime = 0f;
            }

            StopAllCoroutines();
            StartCoroutine(UpdateMusicProgress());
        }

        private void SaveCurrentPlayback() {
            if (_currentMusicType == MusicType.Game && _audioSource.isPlaying) {
                _lastPlaybackTime = _audioSource.time;
                _lastGameMusicName = _audioSource.clip.name;
                PlayerPrefs.SetString(LastGameMusicNameKey, _lastGameMusicName);
                PlayerPrefs.SetFloat(LastPlaybackTimeKey, _lastPlaybackTime);
                PlayerPrefs.Save();
            }
        }

        private void LoadSavedPlaybackInfo() {
            _lastGameMusicName = PlayerPrefs.GetString(LastGameMusicNameKey, string.Empty);
            _lastPlaybackTime = PlayerPrefs.GetFloat(LastPlaybackTimeKey, 0f);
        }

        public void ResetCurrentInGameMusic() {
            _lastGameMusicName = string.Empty;
            _lastPlaybackTime = 0f;
            PlayerPrefs.DeleteKey(LastGameMusicNameKey);
            PlayerPrefs.DeleteKey(LastPlaybackTimeKey);
            PlayerPrefs.Save();
            _hasPlayedVictoryMusic = false;
        }

        private void PlayNextTrackAfterFinished() {
            AudioClip next = GetNextClip(_currentMusicType, _currentMusicSoundType);

            if (next == null) {
                return;
            }

            _audioSource.clip = next;
            _audioSource.time = 0f;
            _audioSource.Play();
            currentMusicTitle = next.name;
            _lastClip = next;
            _lastGameMusicName = next.name;
            _lastPlaybackTime = 0f;
            SaveCurrentPlayback();
        }

        private List<AudioClip> GetSelectedMusics(MusicType musicType, MusicSoundType musicSoundType = MusicSoundType.All) {
            if (musicType == MusicType.Menu) {
                return onMenuMusics;
            }

            if (musicSoundType == MusicSoundType.All) {
                return onGameMusics.ConvertAll(m => m.audioClip);
            }

            return onGameMusics
                .Where(m => m.musicSoundType == musicSoundType)
                .Select(m => m.audioClip)
                .ToList();
        }

        private IEnumerator UpdateMusicProgress() {
            #if !UNITY_EDITOR
            yield return null;
            #endif

            while (true) {
                if (_audioSource.clip != null && _audioSource.isPlaying) {
                    TimeSpan current = TimeSpan.FromSeconds(_audioSource.time);
                    TimeSpan total = TimeSpan.FromSeconds(_audioSource.clip.length);
                    musicProgress = $"{(int)current.TotalMinutes}:{current.Seconds:00} / {(int)total.TotalMinutes}:{total.Seconds:00}";
                } else {
                    musicProgress = string.Empty;
                }

                yield return new WaitForSecondsRealtime(0.5f);
            }
        }

        private static AudioClip GetRandomAudioClipAvoidingRepeat(List<AudioClip> clips, AudioClip lastClip, bool avoidRepeat = true) {
            if (clips == null || clips.Count == 0) {
                return null;
            }

            if (clips.Count == 1 || !clips.Contains(lastClip)) {
                return GetRandomAudioClip(clips);
            }

            if (!avoidRepeat) {
                return GetRandomAudioClip(clips);
            }

            List<AudioClip> filtered = clips.Where(c => c != lastClip).ToList();
            return GetRandomAudioClip(filtered);
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
        All,
        Calm,
        Action,
        Victory
    }

    [Serializable]
    public class InGameMusic {
        public AudioClip audioClip;
        public MusicSoundType musicSoundType;
    }
}