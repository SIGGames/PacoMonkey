using UnityEngine;
using static Configuration.GameConfig;
using static Utils.PlayerPrefsKeys;

namespace Managers {
    public class ResolutionManager : MonoBehaviour {
        [SerializeField] private int screenWidth = DefaultScreenWidth;
        [SerializeField] private int screenHeight = DefaultScreenHeight;
        [SerializeField] private bool fullScreen = DefaultFullScreen;
        [SerializeField] private int vSyncCount = VSyncCount;
        [SerializeField] private int frameRate = FrameRate;
        [SerializeField, Range(0f, 10f)] private float currentBrightness = 5f;
        [SerializeField, Range(0f, 10f)] private float currentContrast = 1f;

        private void Start() {
            int width = PlayerPrefs.GetInt(ScreenWidthKey, screenWidth);
            int height = PlayerPrefs.GetInt(ScreenHeightKey, screenHeight);
            bool isFullscreen = PlayerPrefs.GetInt(FullScreenKey, fullScreen ? 1 : 0) == 1;
            float brightness = PlayerPrefs.GetFloat(BrightnessKey, currentBrightness);
            float contrast = PlayerPrefs.GetFloat(ContrastKey, currentContrast);

            SetResolution(width, height, isFullscreen);
            SetBrightness(brightness);
            SetContrast(contrast);

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

        public void SetContrast(float contrast) {
            currentContrast = contrast;
            QualitySettings.antiAliasing = (int)contrast;
            PlayerPrefs.SetFloat(ContrastKey, contrast);
            PlayerPrefs.Save();
        }

        public void SetBrightness(float brightness) {
            currentBrightness = brightness;
            PlayerPrefs.SetFloat(BrightnessKey, brightness);
            PlayerPrefs.Save();
        }
    }
}