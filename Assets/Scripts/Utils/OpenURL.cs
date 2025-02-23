using UnityEngine;

namespace Utils {
    public class OpenURL : MonoBehaviour {
        public static void OpenProvidedUrl(string url) {
            Application.OpenURL(url);
        }

        public static void OpenGithubUrl() {
            Application.OpenURL("https://github.com/SIGGames/PacoMonkey");
        }

        public static void OpenWebsiteUrl() {
            Application.OpenURL("https://siggames.cat/");
        }

        public static void OpenDiscordUrl() {
            Application.OpenURL("https://discord.gg/XAQUEgqP7x");
        }

        public static void OpenInstagramUrl() {
            Application.OpenURL("https://www.instagram.com/siggames.official/");
        }

        public static void OpenLinkedinUrl() {
            Application.OpenURL("https://www.linkedin.com/company/siggames/");
        }

        public static void OpenTikTokUrl() {
            // TODO: Add TikTok URL
            Application.OpenURL("");
        }
    }
}