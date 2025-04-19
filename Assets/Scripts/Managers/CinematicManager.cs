using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Enums;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Managers {
    public class CinematicManager : MonoBehaviour {
        public static CinematicManager Instance { get; private set; }

        [SerializeField] private List<CinematicConfig> cinematicConfigs;

        [Header("Cinematic Configurations")]
        [SerializeField] private GameObject timerGameObject;
        [SerializeField] private GameObject level1GameObject;
        [SerializeField] private GameObject level2GameObject;

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

        private void Update() {
            // TODO: Remove this debug code
            if (Input.GetKeyDown(KeyCode.F2)) {
                StartCinematic(Cinematic.Ending);
            }

            if (Input.GetKeyDown(KeyCode.F3)) {
                StopTimer();
            }
        }

        public void StartCinematic(Cinematic cinematic) {
            CinematicConfig config = cinematicConfigs.Find(cinematicConfig => cinematicConfig.cinematic == cinematic);
            if (config != null) {
                StartCoroutine(PlayCinematic(config));
            }
        }

        private IEnumerator PlayCinematic(CinematicConfig config) {
            if (config.waitBeforeStart) {
                yield return new WaitForSeconds(config.waitDuration);
            }
            float cinematicDuration = GetCinematicDuration(config);

            if (config.hideHUD) {
                StartCoroutine(HideHud(GetCinematicDuration(config)));
            }

            if (config.progressiveZoom) {
                CameraManager.Instance.SetProgressiveZoom(cinematicDuration, config.cameraZoomMultiplier);
            }

            if (config.showFadeIn) {
                yield return StartCoroutine(FadeIn(config, cinematicDuration));
            }

            if (config.cinematic == Cinematic.Ending) {
                level1GameObject.SetActive(false);
                level2GameObject.SetActive(true);
            }

            if (config.showTimer) {
                // Start the timer coroutine only if there is not already one running
                _activeTimerCoroutine ??= StartCoroutine(ShowTimer(config));
            }
        }

        private static IEnumerator FadeIn(CinematicConfig config, float cinematicDuration) {
            Image fadeImage = ResolutionManager.Instance.brightnessImage;
            if (fadeImage == null) {
                yield break;
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
                float t = Mathf.Pow(timer / cinematicDuration, 1f / Mathf.Clamp(config.fadeInBias, 0.01f, 1f));
                fadeImage.color = Color.Lerp(config.fadeInStartColor, config.fadeInEndColor, t);
                timer += Time.deltaTime;
                yield return null;
            }

            // Resetting everything
            fadeImage.color = config.fadeInEndColor;
            ResolutionManager.Instance.ResetBrightness();
            Destroy(tempImage.gameObject);
        }

        private static float GetCinematicDuration(CinematicConfig config) {
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
            float timeRemaining = config.timerDuration;
            float threshold = config.timerDuration * config.lowTimerPercentage;

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
        }

        private static IEnumerator StopRumbleAfterDelay(float delay) {
            yield return new WaitForSeconds(delay);
            if (Gamepad.current != null) {
                Gamepad.current.SetMotorSpeeds(0f, 0f);
            }
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

        [Header("Fade In")]
        public bool showFadeIn;
        [Range(0.01f, 1f)]
        public float fadeInBias = 0.5f;
        public Color fadeInStartColor = new(0, 0, 0, 0); // Transparent
        public Color fadeInEndColor = new(0, 0, 0, 1); // Opaque black
        public Sprite fadeInSprite;

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
    }
}