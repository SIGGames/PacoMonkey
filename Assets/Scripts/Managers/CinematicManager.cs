using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Controllers;
using Enums;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using PlayerInputManager = PlayerInput.PlayerInputManager;

namespace Managers {
    public class CinematicManager : MonoBehaviour {
        public static CinematicManager Instance { get; private set; }

        public List<CinematicConfig> cinematicConfigs;

        [Header("Extra Components")]
        [SerializeField] private GameObject timerGameObject;
        [SerializeField] private RectTransform timerHandTransform;

        private Cinematic? _currentCinematic;
        private Coroutine _activeTimerCoroutine;
        private Color _originalTextColor;
        private bool _hasTriggeredVibration;

        private void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start() {
            // Ensure there are no duplicate Cinematic entries
            HashSet<Cinematic> uniqueCinematics = new();
            foreach (CinematicConfig config in cinematicConfigs.Where(config => !uniqueCinematics.Add(config.cinematic))) {
                Debug.LogError($"Duplicate cinematic entry found: {config.cinematic}");
            }
        }

        public void StartCinematic(Cinematic cinematic, bool forceOverride = false) {
            if (_currentCinematic != null && !forceOverride) {
                // Cinematic was ignored because another one is already playing
                return;
            }

            CinematicConfig config = cinematicConfigs.Find(cinematicConfig => cinematicConfig.cinematic == cinematic);
            if (config != null) {
                _currentCinematic = cinematic;
                StartCoroutine(ResetCurrentCinematic(GetCinematicDuration(config, true)));
                StartCoroutine(PlayCinematic(config));
            }
        }

        private IEnumerator PlayCinematic(CinematicConfig config) {
            if (_activeTimerCoroutine != null) {
                StopTimer();
            }

            if (config.waitBeforeStart) {
                yield return new WaitForSeconds(config.waitDuration);
            }

            float cinematicDuration = GetCinematicDuration(config);

            if (config.hideHUD) {
                StartCoroutine(HideHud(GetCinematicDuration(config)));
            }

            if (config.instanceEnemies) {
                CharacterManager.InstanceEnemies();
            }

            if (config.progressiveZoom) {
                CameraManager.Instance.SetProgressiveZoom(cinematicDuration, config.cameraZoomMultiplier);
            }

            if (config.showFadeIn) {
                StartCoroutine(FadeIn(config, cinematicDuration));
            }

            if (config.changeCharacter) {
                CharacterManager.Instance.SetCharacter(config.characterToChange);
            }

            if (config.setLevel) {
                LevelManager.Instance.SetLevel(config.levelToSet);
            }

            if (config.showTimer) {
                // Start the timer coroutine only if there is not already one running
                _activeTimerCoroutine ??= StartCoroutine(ShowTimer(config));
            }

            if (config.enablePanel) {
                EnablePanel(config.panel, config.cinematicDuration);
            }

            if (config.openGameMenu) {
                StartCoroutine(OpenGameMenu(config.gameMenuPanel, config.cinematicDuration));
            }

            if (config.resetGameProgress) {
                StartCoroutine(ResetGameProgress(config.cinematicDuration));
            }
        }

        private static IEnumerator FadeIn(CinematicConfig config, float cinematicDuration) {
            Image fadeImage = ResolutionManager.Instance.brightnessImage;
            if (fadeImage == null) {
                yield break;
            }

            MetaGameController.isUIMenusDisabled = true;

            if (config.disablePlayerPositionWhileFadeIn) {
                PlayerInputManager.Instance.InputActions.PlayerControls.Move.Disable();
            }

            Image tempImage = Instantiate(fadeImage, fadeImage.transform.parent);
            tempImage.gameObject.name = config.cinematic + "FadeImage";

            if (config.fadeInSprite != null) {
                tempImage.sprite = config.fadeInSprite;
            }

            // Just in case the image is not active
            fadeImage.gameObject.SetActive(true);
            tempImage.gameObject.SetActive(true);

            // Lerp from start color to end color
            float timer = 0f;
            while (timer < cinematicDuration) {
                float t = Mathf.Pow(timer / cinematicDuration, Mathf.Clamp(config.fadeInBias, 0.01f, 10f));
                fadeImage.color = Color.Lerp(config.fadeInStartColor, config.fadeInEndColor, t);
                timer += Time.deltaTime;
                yield return null;
            }

            // Resetting everything
            fadeImage.color = config.fadeInEndColor;
            ResolutionManager.Instance.ResetBrightness();
            Destroy(tempImage.gameObject);
            MetaGameController.isUIMenusDisabled = false;
            if (config.disablePlayerPositionWhileFadeIn) {
                PlayerInputManager.Instance.InputActions.PlayerControls.Move.Enable();
            }

            if (config.disableFollowCameraOnFinishFadeIn) {
                CameraManager.Instance.FollowAndLookAt(null);
            }
        }

        private IEnumerator ResetCurrentCinematic(float time) {
            yield return new WaitForSeconds(time);
            _currentCinematic = null;
        }

        private static float GetCinematicDuration(CinematicConfig config, bool useTimer = false) {
            if (useTimer && config.showTimer) {
                return config.timerDuration;
            }

            return config.cinematic == Cinematic.Death
                ? CharacterManager.Instance.currentCharacterRespawnTime
                : config.cinematicDuration;
        }

        private static IEnumerator HideHud(float duration) {
            GameObject hud = GameObject.Find("InGameInterface");
            if (hud == null) {
                yield break;
            }

            hud.SetActive(false);
            yield return new WaitForSeconds(duration);
            hud.SetActive(true);
        }

        private IEnumerator ShowTimer(CinematicConfig config) {
            if (timerGameObject == null) {
                Debug.LogError("Timer GameObject not assigned");
                yield break;
            }

            TextMeshProUGUI timerText = timerGameObject.GetComponentInChildren<TextMeshProUGUI>();
            _originalTextColor = timerText.color;
            timerGameObject.SetActive(true);
            StartCoroutine(AnimateTimerPopup());
            float timerDuration = GetTimerDuration(config);
            float timeRemaining = timerDuration;

            // Set up the initial position for the clock hand
            if (timerHandTransform != null) {
                float initialAngle = Mathf.Lerp(0f, 360f, 1f - (timeRemaining / timerDuration));
                timerHandTransform.localRotation = Quaternion.Euler(0, 0, initialAngle);
            }

            float threshold = timerDuration * config.lowTimerPercentage;

            while (timeRemaining > 0f) {
                timeRemaining -= Time.deltaTime;
                int secondsTotal = Mathf.CeilToInt(timeRemaining);
                if (secondsTotal > 60) {
                    int minutes = secondsTotal / 60;
                    int seconds = secondsTotal % 60;
                    timerText.text = $"{minutes:00}:{seconds:00}";
                } else {
                    timerText.text = secondsTotal.ToString();
                }

                if (timeRemaining <= threshold) {
                    timerText.color = config.lowTimerColor;

                    if (!_hasTriggeredVibration && Gamepad.current != null) {
                        Gamepad.current.SetMotorSpeeds(config.rumbleIntensityOnLowTime, config.rumbleIntensityOnLowTime);
                        _hasTriggeredVibration = true;
                        StartCoroutine(StopRumbleAfterDelay(config.rumbleDurationOnLowTime));
                    }
                }

                // Timer hand rotation
                if (timerHandTransform != null) {
                    float angle = Mathf.Lerp(0f, 360f, 1f - (timeRemaining / timerDuration));
                    timerHandTransform.localRotation = Quaternion.Euler(0, 0, angle);
                }

                yield return null;
            }

            // Timer finished
            if (config.killPlayerOnFinishTime) {
                CharacterManager.Instance.currentPlayerController.KillPlayer();
            }

            ResetTimerUI(timerText);
        }

        private IEnumerator AnimateTimerPopup(float duration = 0.25f) {
            RectTransform rect = timerGameObject.GetComponent<RectTransform>();
            Vector3 initialScale = Vector3.zero;
            Vector3 finalScale = Vector3.one;

            float timer = 0f;
            rect.localScale = initialScale;

            while (timer < duration) {
                float t = timer / duration;
                float scale = Mathf.SmoothStep(0f, 1f, t);
                rect.localScale = new Vector3(scale, scale, 1f);
                timer += Time.deltaTime;
                yield return null;
            }

            rect.localScale = finalScale;
        }

        public void StopTimer() {
            if (_activeTimerCoroutine != null) {
                StopCoroutine(_activeTimerCoroutine);
                _activeTimerCoroutine = null;
            }

            ResetTimerUI();
        }

        private void ResetTimerUI(TextMeshProUGUI timerText = null) {
            if (timerGameObject != null) {
                if (timerText == null) {
                    timerText = timerGameObject.GetComponentInChildren<TextMeshProUGUI>();
                }
                timerText.color = _originalTextColor;
                timerGameObject.SetActive(false);
            }
            _hasTriggeredVibration = false;

            if (Gamepad.current != null) {
                Gamepad.current.SetMotorSpeeds(0f, 0f);
            }

            if (timerHandTransform != null) {
                timerHandTransform.localRotation = Quaternion.identity;
            }
        }

        private void EnablePanel(GameObject panel, float duration) {
            if (panel == null) {
                return;
            }
            panel.SetActive(true);

            // Hide the text after the cinematic duration
            StartCoroutine(DisablePanel(panel, duration));
        }

        private static IEnumerator DisablePanel(GameObject panel, float duration) {
            yield return new WaitForSeconds(duration);
            panel.SetActive(false);
        }

        private static IEnumerator OpenGameMenu(Panel panelToOpen, float duration) {
            yield return new WaitForSeconds(duration);
            MetaGameController.Instance.OpenGameMenu(panelToOpen);
        }

        private static IEnumerator ResetGameProgress(float duration) {
            yield return new WaitForSeconds(duration);
            GameController.Instance.NewGame();
        }

        private static IEnumerator StopRumbleAfterDelay(float delay) {
            yield return new WaitForSeconds(delay);
            if (Gamepad.current != null) {
                Gamepad.current.SetMotorSpeeds(0f, 0f);
            }
        }

        public static float GetTimerDuration(CinematicConfig config) {
            return config.timerDuration * (1 / DifficultyManager.Instance.GetPlayerDifficultyMultiplier(DifficultyManager.Instance.currentDifficulty));
        }
    }

    [Serializable]
    public class CinematicConfig {
        public Cinematic cinematic;

        [Tooltip("Duration of the cinematic in seconds (ignored for Death)")]
        [Range(0.01f, 10f)]
        public float cinematicDuration = 1f;

        [Header("Wait Before Start")]
        public bool waitBeforeStart;
        [Range(0.01f, 10f)]
        public float waitDuration = 1f;

        [Header("Character")]
        public bool changeCharacter;
        public Character characterToChange;

        [Header("Enemies")]
        public bool instanceEnemies;

        [Header("Level")]
        public bool setLevel;
        public Level levelToSet;

        [Header("Fade In")]
        public bool showFadeIn;
        [Range(0.01f, 10f)]
        public float fadeInBias = 0.5f;
        public Color fadeInStartColor = new(0, 0, 0, 0); // Transparent
        public Color fadeInEndColor = new(0, 0, 0, 1); // Opaque black
        public Sprite fadeInSprite;
        public bool disableFollowCameraOnFinishFadeIn;
        public bool disablePlayerPositionWhileFadeIn;

        [Header("Progressive Zoom")]
        public bool progressiveZoom;
        [Range(0.01f, 10f)]
        public float cameraZoomMultiplier = 1f;

        [Header("Timer")]
        public bool showTimer;
        [Range(0.01f, 1000f)]
        public float timerDuration = 60f;
        [Range(0.01f, 1f)]
        public float lowTimerPercentage = 0.2f;
        public Color lowTimerColor = Color.red;
        [Range(0.01f, 1f)]
        public float rumbleIntensityOnLowTime = 0.5f;
        [Range(0.01f, 2f)]
        public float rumbleDurationOnLowTime = 0.5f;
        public bool killPlayerOnFinishTime;

        [Header("Hide HUD while cinematic is playing")]
        public bool hideHUD;

        [Header("Panel")]
        public bool enablePanel;
        public GameObject panel;

        [Header("Open Game Menu")]
        public bool openGameMenu;
        public Panel gameMenuPanel;

        [Header("Game Progress")]
        public bool resetGameProgress;
    }
}