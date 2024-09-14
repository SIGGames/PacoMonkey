using UnityEngine;

namespace Configuration {
    public class ResolutionManager : MonoBehaviour {
        [SerializeField] private int screenWidth = GlobalConfiguration.ScreenWidth;
        [SerializeField] private int screenHeight = GlobalConfiguration.ScreenHeight;
        [SerializeField] private bool fullScreen = GlobalConfiguration.FullScreen;

        void Start() {
            int width = PlayerPrefs.GetInt("ScreenWidth", screenWidth);
            int height = PlayerPrefs.GetInt("ScreenHeight", screenHeight);
            bool isFullscreen = PlayerPrefs.GetInt("FullScreen", fullScreen ? 1 : 0) == 1;

            Screen.SetResolution(width, height, isFullscreen);
        }

        public void SaveResolutionSettings(int width, int height, bool isFullscreen) {
            PlayerPrefs.SetInt("ScreenWidth", width);
            PlayerPrefs.SetInt("ScreenHeight", height);
            PlayerPrefs.SetInt("FullScreen", isFullscreen ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}