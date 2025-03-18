using Configuration;
using Enums;
using UnityEngine;
using System;
using static Utils.PlayerPrefsKeys;

namespace Localization {
    public class LocalizationManager : MonoBehaviour {
        public static LocalizationManager Instance { get; private set; }

        [SerializeField] private LocalizationData localizationData;
        public Language currentLanguage = GameConfig.DefaultLanguage;

        private Language _previousLanguage;
        public event Action OnLanguageChanged;

        private void Awake() {
            if (Instance == null) {
                Instance = this;
            } else {
                Destroy(gameObject);
            }
        }

        private void Start() {
            // Gets the language from the player preferences or sets the default language based on the system language
            if (!PlayerPrefs.HasKey(LanguageKey)) {
                currentLanguage = Application.systemLanguage switch {
                    SystemLanguage.Catalan => Language.Catalan,
                    SystemLanguage.Spanish => Language.Spanish,
                    SystemLanguage.English => Language.English,
                    _ => GameConfig.DefaultLanguage
                };
                PlayerPrefs.SetInt(LanguageKey, (int)currentLanguage);
                PlayerPrefs.Save();
            } else {
                currentLanguage = (Language)PlayerPrefs.GetInt(LanguageKey, (int)currentLanguage);
            }
        }

        public string GetLocalizedText(string key) {
            string text = localizationData.GetText(key, currentLanguage);

            if (string.IsNullOrEmpty(text) || text.Contains("MISSING")) {
                Debug.LogWarning($"Missing text for key: {key}");
            }

            return text;
        }

        private void Update() {
            if (_previousLanguage != currentLanguage) {
                _previousLanguage = currentLanguage;
                ChangeLanguage();
            }
        }

        public void SetLanguage(string language) {
            currentLanguage = language.ToLower() switch {
                "catalan" => Language.Catalan,
                "spanish" => Language.Spanish,
                "english" => Language.English,
                _ => currentLanguage
            };
        }

        private void ChangeLanguage() {
            PlayerPrefs.SetInt(LanguageKey, (int)currentLanguage);
            PlayerPrefs.Save();

            OnLanguageChanged?.Invoke();
        }
    }
}