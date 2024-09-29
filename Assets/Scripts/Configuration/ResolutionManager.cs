using UnityEngine;

namespace Configuration {
    public class ResolutionManager : MonoBehaviour {
        [SerializeField] private int screenWidth = GlobalConfiguration.DefaultScreenWidth;
        [SerializeField] private int screenHeight = GlobalConfiguration.DefaultScreenHeight;
        [SerializeField] private bool fullScreen = GlobalConfiguration.DefaultFullScreen;
        [SerializeField] private int vSyncCount = GlobalConfiguration.VSyncCount;
        [SerializeField] private int frameRate = GlobalConfiguration.FrameRate;

        void Start() {
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