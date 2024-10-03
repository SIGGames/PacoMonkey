using UnityEngine;

namespace Utils {
    public class OpenURL : MonoBehaviour {
        public string websiteAddress;

        public void OpenURLOnClick() {
            Application.OpenURL(websiteAddress);
        }

        public static void OpenProvidedUrl(string url) {
            Application.OpenURL(url);
        }

        public static void OpenGithubUrl() {
            Application.OpenURL("https://github.com/SIGGames/PacoMonkey");
        }

        public static void OpenWebsiteUrl() {
            Application.OpenURL("https://siggames-official.vercel.app/");
        }

        public static void OpenDiscordUrl() {
            // TODO: Add Discord URL
            Application.OpenURL("");
        }

        public static void OpenInstagramUrl() {
            // TODO: Add Instagram URL
            Application.OpenURL("");
        }

        public static void OpenLinkedinUrl() {
            // TODO: Add LinkedIn URL
            Application.OpenURL("");
        }

        public static void OpenTikTokUrl() {
            // TODO: Add TikTok URL
            Application.OpenURL("");
        }
    }
}