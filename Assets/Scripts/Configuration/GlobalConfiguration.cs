namespace Configuration {
    public static class GlobalConfiguration {
        // Player configuration
        public static float playerSpeed = 5.0f;
        public static float playerJumpForce = 12.0f;
        public static int playerMaxHealth = 100;

        // Enemy configuration
        public static float enemySpeed = 3.0f;
        public static int enemyMaxHealth = 50;

        // Game configuration
        public static int maxLives = 3;
        public static float gravityScale = 1.0f;

        // Audio configuration
        public static float masterVolume = 1.0f;
        public static float musicVolume = 0.8f;
        public static float sfxVolume = 0.7f;

        // Environment configuration
        public static float windSpeed = 2.0f;

        // Flags
        public static bool isDebugMode = false;
        public static bool isGodMode = false;
    }
}