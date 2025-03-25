using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Localization {
    public class LocalizedTextUpdater : MonoBehaviour {
        [SerializeField] private string textKey;
        private TextMeshProUGUI _textComponent;
        private Text _text; // Fallback for TextMeshProUGUI

        private void Awake() {
            if (_textComponent == null) {
                _textComponent = GetComponent<TextMeshProUGUI>();
            }

            if (_textComponent == null) {
                _text = GetComponent<Text>();
            }

            if (_textComponent == null) {
                Debug.LogError("Text component not found: " + name);
            }
        }

        private void Start() {
            UpdateText();
        }

        private void OnEnable() {
            if (LocalizationManager.Instance != null) {
                LocalizationManager.Instance.OnLanguageChanged += UpdateText;
                // Force update since the text might have changed while the object was disabled
                UpdateText();
            }
        }

        private void OnDisable() {
            if (LocalizationManager.Instance != null) {
                LocalizationManager.Instance.OnLanguageChanged -= UpdateText;
            }
        }

        private void UpdateText() {
            if (LocalizationManager.Instance != null) {
                if (_textComponent != null) {
                    _textComponent.text = LocalizationManager.Instance.GetLocalizedText(textKey);
                } else if (_text != null) {
                    _text.text = LocalizationManager.Instance.GetLocalizedText(textKey);
                }
            }
        }
    }
}