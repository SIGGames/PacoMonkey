using UnityEngine;
using UnityEngine.Serialization;

namespace Configuration {
    public class GlobalConfiguration : MonoBehaviour {
        // Screen configuration
        public const int ScreenWidth = 1920;
        public const int ScreenHeight = 1080;
        public const bool FullScreen = true;

        // Player configuration
        public static float playerSpeed = 5.0f;
        public static float playerJumpForce = 12.0f;
        public static int playerMaxHealth = 100;

        // Enemy configuration
        public static float enemySpeed = 3.0f;
        public static int enemyMaxHealth = 50;

        // Health configuration
        public const int DefaultHp = 100;
        public const int MaxHp = 100;
        public const int DefaultLives = 1;
        public const int MaxLives = 1;
        public const int DefaultHpIncrement = 1;
        public const int DefaultHpDecrement = 1;

        // Game configuration
        public const float GravityScale = 1.0f;

        // Audio configuration
        public static float masterVolume = 1.0f;
        public static float musicVolume = 0.8f;
        public static float sfxVolume = 0.7f;

        // Environment configuration
        public static float windSpeed = 2.0f;

        // Flags
        public static bool isDebugMode = false;
        public const bool IsGodMode = false;
    }
}