using Configuration;
using Enums;
using UnityEngine;
using System;
using System.Text.RegularExpressions;
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

        public bool HasKey(string key) {
            return localizationData.HasKey(key);
        }

        public string GetLocalizedText(string key, bool debugIfMissing = true) {
            string text = localizationData.GetText(key.ToLower(), currentLanguage);

            if (debugIfMissing && (string.IsNullOrEmpty(text) || text.Contains("MISSING"))) {
                Debug.LogWarning($"Missing text for key: {key}");
            }

            return FormatText(text);
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

        private static string FormatText(string input) {
            // \n: New line
            // \t: Tab
            // **text**: Bold
            // *text*: Italic
            // \url{url}{text}: Link
            input = input.Replace("\\n", "\n");
            input = input.Replace("\\t", "\t");
            input = Regex.Replace(input, @"\*\*(.*?)\*\*", "<b>$1</b>");
            input = Regex.Replace(input, @"(?<!\*)\*(?!\*)(.*?)\*(?!\*)", "<i>$1</i>");
            input = Regex.Replace(input, @"\\url\{(.*?)\}\{(.*?)\}", "<link=\"$1\"><color=#0077FF><u>$2</u></color></link>");
            return input;
        }
    }
}