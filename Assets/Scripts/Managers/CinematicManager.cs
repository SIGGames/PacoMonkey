using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Enums;
using UnityEngine;
using UnityEngine.UI;

namespace Managers {
    public class CinematicManager : MonoBehaviour {
        public static CinematicManager Instance { get; private set; }

        [SerializeField] private List<CinematicConfig> cinematicConfigs;

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
            // TODO: Remove this
            if (Input.GetKeyDown(KeyCode.F2)) {
                StartCinematic(Cinematic.NewGame);
            }

            if (Input.GetKeyDown(KeyCode.F3)) {
                StartCinematic(Cinematic.Death);
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

            // Hide HUD if needed
            if (config.hideHUD) {
                StartCoroutine(HideHud(GetCinematicDuration(config)));
            }

            if (config.progressiveZoom) {
                CameraManager.Instance.SetProgressiveZoom(cinematicDuration, config.cameraZoomMultiplier);
            }

            if (config.showFadeIn) {
                yield return StartCoroutine(FadeIn(config, cinematicDuration));
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

        [Header("Hide HUD while cinematic is playing")]
        public bool hideHUD;
    }
}