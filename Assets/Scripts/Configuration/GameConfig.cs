using Enums;

namespace Configuration {
    public static class GameConfig {
        public const string GameVersion = "v0.10.0";

        public const Language DefaultLanguage = Language.Catalan;

        // Player Config
        public const bool IsGodMode = false;

        // Map Config
        public const float MaxMapY = 300f;
        public const float MinMapY = -300f;

        // Screen Config
        public const int DefaultScreenWidth = 640;
        public const int DefaultScreenHeight = 360;
        public const bool DefaultFullScreen = true;
        public const int VSyncCount = 0;
        public const int FrameRate = 12;
    }
}