using UnityEngine;
using UnityEngine.Tilemaps;
using static Configuration.GameConfig;
using static Utils.PlayerPrefsKeys;

namespace Managers {
    public class ResolutionManager : MonoBehaviour {
        [SerializeField] private int screenWidth = DefaultScreenWidth;
        [SerializeField] private int screenHeight = DefaultScreenHeight;
        [SerializeField] private bool fullScreen = DefaultFullScreen;
        [SerializeField] private int vSyncCount = VSyncCount;
        [SerializeField] private int frameRate = FrameRate;
        [SerializeField, Range(0f, 10f)] private float currentBrightness = 10f;

        private SpriteRenderer[] _sprites;
        private Tilemap[] _tilemaps;

        private void Start() {
            int width = PlayerPrefs.GetInt(ScreenWidthKey, screenWidth);
            int height = PlayerPrefs.GetInt(ScreenHeightKey, screenHeight);
            bool isFullscreen = PlayerPrefs.GetInt(FullScreenKey, fullScreen ? 1 : 0) == 1;
            float brightness = PlayerPrefs.GetFloat(BrightnessKey, currentBrightness);
            _sprites = FindObjectsOfType<SpriteRenderer>(true);
            _tilemaps = FindObjectsOfType<Tilemap>(true);

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
            // Normalize the brightness value to be between 0.4 and 1 so it's not too dark or too bright
            float normalizedBrightness = Mathf.Lerp(4f, 10f, brightness / 10f) / 10f;
            // This is kinda inefficient, but it's the only way to change the brightness of the sprites and tilemaps without using an external shader
            foreach (SpriteRenderer sprite in _sprites) {
                sprite.color = new Color(normalizedBrightness, normalizedBrightness, normalizedBrightness, sprite.color.a);
            }

            foreach (Tilemap tilemap in _tilemaps) {
                tilemap.color = new Color(normalizedBrightness, normalizedBrightness, normalizedBrightness, tilemap.color.a);
            }

            PlayerPrefs.SetFloat(BrightnessKey, brightness);
            PlayerPrefs.Save();
        }
    }
}