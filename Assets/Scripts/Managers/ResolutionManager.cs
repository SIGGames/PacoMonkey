using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static Configuration.GameConfig;
using static Utils.PlayerPrefsKeys;

namespace Managers {
    public class ResolutionManager : MonoBehaviour {
        public static ResolutionManager Instance { get; private set; }

        [SerializeField] private int screenWidth = DefaultScreenWidth;
        [SerializeField] private int screenHeight = DefaultScreenHeight;
        [SerializeField] private bool fullScreen = DefaultFullScreen;
        [SerializeField] private int vSyncCount = VSyncCount;
        [SerializeField] private int frameRate = FrameRate;
        [SerializeField, Range(1f, 10f)] private float currentBrightness = 10f;
        [SerializeField] private Image brightnessImage;
        [SerializeField] private Sprite deathSequenceSprite;

        private void Awake() {
            if (Instance == null) {
                Instance = this;
            } else {
                Destroy(gameObject);
            }
        }

        private void Start() {
            int width = PlayerPrefs.GetInt(ScreenWidthKey, screenWidth);
            int height = PlayerPrefs.GetInt(ScreenHeightKey, screenHeight);
            bool isFullscreen = PlayerPrefs.GetInt(FullScreenKey, fullScreen ? 1 : 0) == 1;
            float brightness = PlayerPrefs.GetFloat(BrightnessKey, currentBrightness);

            SetResolution(width, height, isFullscreen);
            SetBrightness(brightness);

            QualitySettings.vSyncCount = vSyncCount;
            Application.targetFrameRate = frameRate;
        }

        private static void SaveResolutionSettings(int width, int height, bool isFullscreen) {
            PlayerPrefs.SetInt(ScreenWidthKey, width);
            PlayerPrefs.SetInt(ScreenHeightKey, height);
            PlayerPrefs.SetInt(FullScreenKey, isFullscreen ? 1 : 0);
            PlayerPrefs.Save();
        }

        public static void SetResolution(int width, int height, bool isFullscreen) {
            Screen.SetResolution(width, height, isFullscreen);
            SaveResolutionSettings(width, height, isFullscreen);
        }

        public void SetVSyncCount(int count) {
            vSyncCount = count;
            QualitySettings.vSyncCount = count;
            PlayerPrefs.SetInt(VSyncCountKey, count);
            PlayerPrefs.Save();
        }

        public void SetBrightness(float brightness) {
            currentBrightness = brightness;
            // Normalize the brightness value so it's not too dark or too bright
            float normalizedBrightness = 1 - Mathf.Lerp(0.1f, 1f, brightness / 10);
            brightnessImage.color = new Color(0, 0, 0, normalizedBrightness);

            PlayerPrefs.SetFloat(BrightnessKey, brightness);
            PlayerPrefs.Save();
        }

        public void StartDeathSequence() {
            StartCoroutine(DeathSequenceCoroutine());
        }

        private IEnumerator DeathSequenceCoroutine() {
            // Death sequence animation aberration
            const float duration = 4f;
            float time = 0f;

            CameraManager.Instance.SetProgressiveZoom(duration);

            Image tempImage = Instantiate(brightnessImage, brightnessImage.transform.parent);
            tempImage.gameObject.name = "DeathSequenceImage";
            if (deathSequenceSprite != null) {
                tempImage.sprite = deathSequenceSprite;
            }

            // Just in case the brightness image is not active
            brightnessImage.gameObject.SetActive(true);
            tempImage.gameObject.SetActive(true);

            // Lerp from transparent red to opaque black
            while (time < duration) {
                time += Time.deltaTime;
                float t = time / duration;
                brightnessImage.color = Color.Lerp(
                    new Color(1, 0, 0, 0),
                    new Color(0, 0, 0, 1),
                    t
                );
                yield return null;
            }

            // Resetting everything
            Destroy(tempImage.gameObject);
            CameraManager.Instance.ResetCamera();
            SetBrightness(currentBrightness);
        }
    }
}