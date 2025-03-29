using Configuration;
using UnityEditor;

namespace Editor.Tools {
    public static class UpdateGameVersion {
        [MenuItem("Tools/Update Game Version")]
        public static void UpdateVersion() {
            PlayerSettings.bundleVersion = GameConfig.GameVersion;
            AssetDatabase.SaveAssets();
        }
    }
}