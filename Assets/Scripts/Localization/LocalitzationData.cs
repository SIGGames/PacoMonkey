using System;
using System.Collections.Generic;
using System.Linq;
using Enums;
using UnityEngine;

namespace Localization {
    [CreateAssetMenu(menuName = "Localization/Localization Data", fileName = "LocalizationData")]
    public class LocalizationData : ScriptableObject {
        [Serializable]
        public class LocalizedText {
            public string key;
            public string catalan;
            public string spanish;
            public string english;
        }

        public List<LocalizedText> texts = new();

        public string GetText(string key, Language language) {
            foreach (var text in texts.Where(text => text.key == key)) {
                return language switch {
                    Language.Catalan => text.catalan,
                    Language.Spanish => text.spanish,
                    Language.English => text.english,
                    _ => text.english
                };
            }

            return $"[MISSING: {key}]";
        }

        public bool HasKey(string key) {
            return texts.Any(text => text.key == key);
        }
    }
}