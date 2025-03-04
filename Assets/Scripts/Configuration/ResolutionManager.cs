using UnityEngine;
using static Configuration.GameConfig;

namespace Configuration {
    public class ResolutionManager : MonoBehaviour {
        [SerializeField] private int screenWidth = DefaultScreenWidth;
        [SerializeField] private int screenHeight = DefaultScreenHeight;
        [SerializeField] private bool fullScreen = DefaultFullScreen;
        [SerializeField] private int vSyncCount = VSyncCount;
        [SerializeField] private int frameRate = FrameRate;

        private void Start() {
            int width = PlayerPrefs.GetInt("ScreenWidth", screenWidth);
            int height = PlayerPrefs.GetInt("ScreenHeight", screenHeight);
            bool isFullscreen = PlayerPrefs.GetInt("FullScreen", fullScreen ? 1 : 0) == 1;

            Screen.SetResolution(width, height, isFullscreen);

            QualitySettings.vSyncCount = vSyncCount;
            Application.targetFrameRate = frameRate;
        }

        public void SaveResolutionSettings(int width, int height, bool isFullscreen) {
            PlayerPrefs.SetInt("ScreenWidth", width);
            PlayerPrefs.SetInt("ScreenHeight", height);
            PlayerPrefs.SetInt("FullScreen", isFullscreen ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}