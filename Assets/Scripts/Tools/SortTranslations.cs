using System.Linq;
using Localization;
using UnityEditor;

namespace Tools {
    public static class SortTranslations {
        [MenuItem("Tools/Sort Translations")]
        public static void SortTranslationsData() {
            const string assetPath = "Assets/Resources/Translations.asset";

            LocalizationData translationsData = AssetDatabase.LoadAssetAtPath<LocalizationData>(assetPath);
            if (translationsData == null) {
                return;
            }

            translationsData.texts = translationsData.texts
                .OrderBy(t => t.key)
                .ToList();

            EditorUtility.SetDirty(translationsData);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}