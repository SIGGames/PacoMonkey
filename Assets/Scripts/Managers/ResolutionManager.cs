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
        [SerializeField] public Image brightnessImage;
        [SerializeField] private GameObject backgroundImage;

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
            Instance?.StretchBackgroundToFullScreen();
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

        private void StretchBackgroundToFullScreen() {
            if (backgroundImage == null) {
                return;
            }

            SpriteRenderer spriteRenderer = backgroundImage.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null || spriteRenderer.sprite == null || Camera.main == null) {
                return;
            }

            if (Application.platform == RuntimePlatform.WebGLPlayer) {
                StartCoroutine(StretchNextFrame(spriteRenderer));
            } else {
                ApplyScaling(spriteRenderer);
            }
        }

        private IEnumerator StretchNextFrame(SpriteRenderer spriteRenderer) {
            yield return null;
            ApplyScaling(spriteRenderer);
        }

        private void ApplyScaling(SpriteRenderer spriteRenderer) {
            if (Camera.main == null) {
                return;
            }

            float cameraHeight = Camera.main.orthographicSize * 2f;
            float cameraWidth = cameraHeight * Screen.width / Screen.height;

            float spriteWidth = spriteRenderer.sprite.bounds.size.x;
            float spriteHeight = spriteRenderer.sprite.bounds.size.y;

            float scaleX = cameraWidth / spriteWidth;
            float scaleY = cameraHeight / spriteHeight;
            float scale = Mathf.Max(scaleX, scaleY);

            backgroundImage.transform.localScale = new Vector3(scale, scale, 1f);
        }

        public void ResetBrightness() {
            currentBrightness = PlayerPrefs.GetFloat(BrightnessKey, currentBrightness);
            SetBrightness(currentBrightness);
        }
    }
}